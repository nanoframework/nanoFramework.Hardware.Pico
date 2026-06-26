//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace nanoFramework.Hardware.Pico.Pio
{
    /// <summary>PIO hardware version. Determines which instructions/features are legal.</summary>
    public enum PioVersion
    {
        /// <summary>RP2040 (PIO v0).</summary>
        Rp2040 = 0,
        /// <summary>RP2350 (PIO v1): adds extended MOV, FIFO PUTGET, GPIO base window.</summary>
        Rp2350 = 1,
    }

    /// <summary>Destination operand. The legal subset depends on the instruction (validated at emit time).</summary>
    public enum PioDest
    {
        /// <summary>The pins mapped to the instruction's pin group.</summary>
        Pins = 0,
        /// <summary>The scratch register X.</summary>
        X = 1,
        /// <summary>The scratch register Y.</summary>
        Y = 2,
        /// <summary>Discards the data (writes nowhere).</summary>
        Null = 3,
        /// <summary>The pin direction registers for the mapped pins.</summary>
        PinDirs = 4,
        /// <summary>The program counter (an unconditional jump).</summary>
        Pc = 5,
        /// <summary>The status value (shares the encoding of <see cref="Pc"/>).</summary>
        Status = 5,
        /// <summary>The input shift register (ISR).</summary>
        Isr = 6,
        /// <summary>The output shift register (OSR).</summary>
        Osr = 7,
        /// <summary>Executes the value as an instruction (shares the encoding of <see cref="Osr"/>).</summary>
        Exec = 7,
    }

    /// <summary>Source operand. The legal subset depends on the instruction (validated at emit time).</summary>
    public enum PioSrc
    {
        /// <summary>The pins mapped to the instruction's pin group.</summary>
        Pins = 0,
        /// <summary>The scratch register X.</summary>
        X = 1,
        /// <summary>The scratch register Y.</summary>
        Y = 2,
        /// <summary>All zeroes.</summary>
        Null = 3,
        /// <summary>The status value (all-ones or all-zeroes per the configured status source).</summary>
        Status = 5,
        /// <summary>The input shift register (ISR).</summary>
        Isr = 6,
        /// <summary>The output shift register (OSR).</summary>
        Osr = 7,
    }

    /// <summary>MOV operation applied to the source before writing the destination.</summary>
    public enum PioMovOp
    {
        /// <summary>No operation — the source is copied unchanged.</summary>
        None = 0,
        /// <summary>Bitwise NOT ("!" / "~").</summary>
        Invert = 1,
        /// <summary>Bit reverse ("::").</summary>
        BitReverse = 2,
    }

    /// <summary>JMP condition (3-bit field).</summary>
    public enum PioCondition
    {
        /// <summary>Always jump (unconditional).</summary>
        Always = 0,
        /// <summary>!X — X is zero.</summary>
        XZero = 1,
        /// <summary>X-- — X is non-zero prior to decrement, then decrement.</summary>
        XPostDec = 2,
        /// <summary>!Y — Y is zero.</summary>
        YZero = 3,
        /// <summary>Y-- — Y is non-zero prior to decrement, then decrement.</summary>
        YPostDec = 4,
        /// <summary>X != Y.</summary>
        XNotEqualY = 5,
        /// <summary>PIN — the JMP pin is high.</summary>
        Pin = 6,
        /// <summary>!OSRE — output shift register is not empty.</summary>
        OsrNotEmpty = 7,
    }

    /// <summary>WAIT polarity source.</summary>
    public enum PioWaitSource
    {
        /// <summary>Absolute GPIO index.</summary>
        Gpio = 0,
        /// <summary>Pin relative to the state machine's IN base.</summary>
        Pin = 1,
        /// <summary>IRQ flag.</summary>
        Irq = 2,
    }

    /// <summary>Shift direction for the ISR/OSR (and autopush/autopull defaults).</summary>
    public enum PioShiftDir
    {
        /// <summary>Shift towards the most-significant bit (left).</summary>
        Left = 0,
        /// <summary>Shift towards the least-significant bit (right).</summary>
        Right = 1,
    }

    /// <summary>
    /// FIFO join mode (SHIFTCTRL FJOIN). Values match the SDK's <c>pio_fifo_join</c>. The
    /// PUT/GET modes are RP2350 (PIO v1) only and turn the RX FIFO into a random-access
    /// register file addressed by the indexed RX-FIFO MOV instructions.
    /// </summary>
    public enum PioFifoJoin
    {
        /// <summary>Separate TX (depth 4) and RX (depth 4) FIFOs.</summary>
        None = 0,
        /// <summary>TX FIFO depth 8, RX disabled.</summary>
        Tx = 1,
        /// <summary>RX FIFO depth 8, TX disabled.</summary>
        Rx = 2,
        /// <summary>(v1) TX depth 4; RX storage is the processor-writable "get" register file.</summary>
        TxGet = 4,
        /// <summary>(v1) TX depth 4; RX storage is the processor-readable "put" register file.</summary>
        TxPut = 8,
        /// <summary>(v1) TX depth 4; RX storage is the SM-only "put"/"get" register file.</summary>
        PutGet = 12,
    }

    /// <summary>FIFO selected by the EXECCTRL status source for the <c>mov ..., status</c> instruction.</summary>
    public enum PioMovStatusSel
    {
        /// <summary>Status is all-ones when the TX FIFO level is below N.</summary>
        TxLevel = 0,
        /// <summary>Status is all-ones when the RX FIFO level is below N.</summary>
        RxLevel = 1,
    }
}
