//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.Hardware.Pico.Pio
{
    /// <summary>
    /// Stateless helpers that build the 16-bit PIO opcodes. The base-opcode methods
    /// emit the instruction without the shared delay/side-set field, which is layered
    /// on afterwards by <see cref="PackDelaySideSet"/> once the program's side-set
    /// configuration is known.
    /// </summary>
    public static class PioEncoder
    {
        internal const int OpJmp = 0x0000;
        internal const int OpWait = 0x2000;
        internal const int OpIn = 0x4000;
        internal const int OpOut = 0x6000;
        internal const int OpPush = 0x8000;
        internal const int OpPull = 0x8080;
        internal const int OpMov = 0xA000;
        internal const int OpIrq = 0xC000;
        internal const int OpSet = 0xE000;

        /// <summary>JMP &lt;cond&gt; &lt;addr&gt;. Address is an absolute program offset 0..31.</summary>
        public static ushort Jmp(PioCondition condition, int address)
        {
            return (ushort)(OpJmp | (((int)condition & 0x7) << 5) | (address & 0x1F));
        }

        /// <summary>WAIT &lt;polarity&gt; &lt;source&gt; &lt;index&gt;.</summary>
        public static ushort Wait(bool polarity, PioWaitSource source, int index)
        {
            return (ushort)(OpWait | (polarity ? 0x80 : 0) | (((int)source & 0x3) << 5) | (index & 0x1F));
        }

        /// <summary>IN &lt;source&gt;, &lt;bitCount&gt; (1..32; 32 encodes as 0).</summary>
        public static ushort In(PioSrc source, int bitCount)
        {
            return (ushort)(OpIn | (((int)source & 0x7) << 5) | (bitCount & 0x1F));
        }

        /// <summary>OUT &lt;dest&gt;, &lt;bitCount&gt; (1..32; 32 encodes as 0).</summary>
        public static ushort Out(PioDest dest, int bitCount)
        {
            return (ushort)(OpOut | (((int)dest & 0x7) << 5) | (bitCount & 0x1F));
        }

        /// <summary>PUSH [iffull] [block].</summary>
        public static ushort Push(bool ifFull, bool block)
        {
            return (ushort)(OpPush | (ifFull ? 0x40 : 0) | (block ? 0x20 : 0));
        }

        /// <summary>PULL [ifempty] [block].</summary>
        public static ushort Pull(bool ifEmpty, bool block)
        {
            return (ushort)(OpPull | (ifEmpty ? 0x40 : 0) | (block ? 0x20 : 0));
        }

        /// <summary>MOV &lt;dest&gt;, [op] &lt;src&gt;.</summary>
        public static ushort Mov(PioDest dest, PioMovOp op, PioSrc src)
        {
            return (ushort)(OpMov | (((int)dest & 0x7) << 5) | (((int)op & 0x3) << 3) | ((int)src & 0x7));
        }

        // ---- RP2350 (PIO v1) indexed RX-FIFO MOV --------------------------
        // Encoded in the PUSH/PULL opcode space (0b100) with arg2 != 0: bit4 of arg2 marks the
        // v1 form, bit3 selects a fixed index (else the Y register), and arg1 bit2 the direction.

        /// <summary>(v1) MOV RXFIFO[index], ISR — write ISR to RX FIFO slot 0..3.</summary>
        public static ushort MovToRxFifo(int index) => (ushort)(OpPush | 0x18 | (index & 0x3));

        /// <summary>(v1) MOV RXFIFO[Y], ISR — write ISR to the RX FIFO slot selected by Y.</summary>
        public static ushort MovToRxFifoIndexedY() => (ushort)(OpPush | 0x10);

        /// <summary>(v1) MOV OSR, RXFIFO[index] — read RX FIFO slot 0..3 into OSR.</summary>
        public static ushort MovFromRxFifo(int index) => (ushort)(OpPush | 0x80 | 0x18 | (index & 0x3));

        /// <summary>(v1) MOV OSR, RXFIFO[Y] — read the RX FIFO slot selected by Y into OSR.</summary>
        public static ushort MovFromRxFifoIndexedY() => (ushort)(OpPush | 0x80 | 0x10);

        /// <summary>IRQ [clear] [wait] &lt;index&gt;.</summary>
        public static ushort Irq(bool clear, bool wait, int index)
        {
            return (ushort)(OpIrq | (clear ? 0x40 : 0) | (wait ? 0x20 : 0) | (index & 0x1F));
        }

        /// <summary>SET &lt;dest&gt;, &lt;value&gt; (0..31).</summary>
        public static ushort Set(PioDest dest, int value)
        {
            return (ushort)(OpSet | (((int)dest & 0x7) << 5) | (value & 0x1F));
        }

        /// <summary>NOP is an alias for MOV Y, Y.</summary>
        public static ushort Nop()
        {
            return Mov(PioDest.Y, PioMovOp.None, PioSrc.Y);
        }

        /// <summary>
        /// Number of delay bits available given the side-set configuration.
        /// The 5-bit [12:8] field is split: side-set value bits (plus one enable bit
        /// when optional) take the top, the remaining low bits are delay.
        /// </summary>
        public static int DelayBits(int sideSetCount, bool sideSetOpt)
        {
            return 5 - sideSetCount - (sideSetOpt ? 1 : 0);
        }

        /// <summary>
        /// Layers the delay and optional side-set value onto a base opcode.
        /// Mirrors pico-sdk pio_encode_delay / pio_encode_sideset[_opt].
        /// </summary>
        /// <param name="baseBits">Opcode from one of the encode methods (no delay/side field).</param>
        /// <param name="delay">Delay cycles (0..2^delayBits-1).</param>
        /// <param name="sideValue">Side-set value to assert.</param>
        /// <param name="sideUsed">Whether this instruction asserts a side-set value.</param>
        /// <param name="sideSetCount">Side-set value-bit count (excludes the opt enable bit).</param>
        /// <param name="sideSetOpt">Whether side-set is optional for this program.</param>
        public static ushort PackDelaySideSet(
            ushort baseBits,
            int delay,
            int sideValue,
            bool sideUsed,
            int sideSetCount,
            bool sideSetOpt)
        {
            int delayBits = DelayBits(sideSetCount, sideSetOpt);
            int field = delay & ((1 << delayBits) - 1);

            if (sideUsed && sideSetCount > 0)
            {
                if (sideSetOpt)
                {
                    field |= 0x10; // enable bit at top of the 5-bit field (instr bit 12)
                }

                field |= (sideValue & ((1 << sideSetCount) - 1)) << delayBits;
            }

            return (ushort)(baseBits | (field << 8));
        }
    }
}
