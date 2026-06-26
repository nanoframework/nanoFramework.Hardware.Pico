//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Runtime.CompilerServices;

namespace nanoFramework.Hardware.Pico.Pio
{
    /// <summary>
    /// One of a PIO block's four state machines. Initialize it from a
    /// <see cref="PioStateMachineConfig"/>, enable it, and exchange words through the
    /// TX/RX FIFOs. Disposing releases the SM claim.
    /// </summary>
    public sealed class PioStateMachine : IDisposable
    {
        private readonly PioBlock _block;
        private readonly int _sm;
        private bool _disposed;
        private bool _enabled;

        /// <summary>Initializes a new instance of the <see cref="PioStateMachine"/> class.</summary>
        internal PioStateMachine(PioBlock block, int sm)
        {
            _block = block;
            _sm = sm;
        }

        /// <summary>State machine index (0..3).</summary>
        public int Index
        {
            get
            {
                return _sm;
            }
        }

        /// <summary>
        /// Configures and resets the state machine to start executing at
        /// <paramref name="offset"/> (maps to <c>pio_sm_init</c>). The configuration is
        /// flattened to a blob and rebuilt into a <c>pio_sm_config</c> natively.
        /// </summary>
        public void Init(uint offset, PioStateMachineConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            NativeInit(_block.Index, _sm, (int)offset, config.ToBlob());
        }

        /// <summary>
        /// Enables or disables the state machine (maps to <c>pio_sm_set_enabled</c>). The getter
        /// reflects the last value set through this API.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                NativeSetEnabled(_block.Index, _sm, value);
            }
        }

        /// <summary>Pushes a word into the TX FIFO, yielding to other threads until there is room.</summary>
        public void Put(uint value)
        {
            while (NativeTxFull(_block.Index, _sm))
            {
                System.Threading.Thread.Sleep(0);
            }

            NativePutBlocking(_block.Index, _sm, value);
        }

        /// <summary>Pops a word from the RX FIFO, yielding to other threads until one is available.</summary>
        public uint Get()
        {
            while (NativeRxEmpty(_block.Index, _sm))
            {
                System.Threading.Thread.Sleep(0);
            }

            return NativeGetBlocking(_block.Index, _sm);
        }

        /// <summary>True when the TX FIFO cannot accept another word.</summary>
        public bool IsTxFull()
        {
            return NativeTxFull(_block.Index, _sm);
        }

        /// <summary>True when the RX FIFO has no words to read.</summary>
        public bool IsRxEmpty()
        {
            return NativeRxEmpty(_block.Index, _sm);
        }

        /// <summary>Reads the number of words currently queued in the TX FIFO (depth depends on FIFO join).</summary>
        public uint GetTxLevel()
        {
            return NativeTxLevel(_block.Index, _sm);
        }

        /// <summary>Reads the number of words currently queued in the RX FIFO (depth depends on FIFO join).</summary>
        public uint GetRxLevel()
        {
            return NativeRxLevel(_block.Index, _sm);
        }

        /// <summary>Reads the state machine's current program counter (instruction-memory offset 0..31).</summary>
        public uint GetProgramCounter()
        {
            return NativeGetPc(_block.Index, _sm);
        }

        /// <summary>
        /// Attempts to push a word into the TX FIFO without blocking. Returns <c>false</c> (and writes
        /// nothing) when the FIFO is full, so callers can poll or do other work instead of stalling the
        /// CLR thread the way <see cref="Put"/> does. Safe against the FIFO state changing under it: the
        /// check and the write are a single uninterrupted managed step on the cooperative CLR.
        /// </summary>
        public bool TryPut(uint value)
        {
            if (NativeTxFull(_block.Index, _sm))
            {
                return false;
            }

            NativePutBlocking(_block.Index, _sm, value);
            return true;
        }

        /// <summary>
        /// Attempts to pop a word from the RX FIFO without blocking. Returns <c>false</c> (and sets
        /// <paramref name="value"/> to 0) when the FIFO is empty, instead of blocking like <see cref="Get"/>.
        /// </summary>
        public bool TryGet(out uint value)
        {
            if (NativeRxEmpty(_block.Index, _sm))
            {
                value = 0;
                return false;
            }

            value = NativeGetBlocking(_block.Index, _sm);
            return true;
        }

        /// <summary>Clears this state machine's TX and RX FIFOs (maps to <c>pio_sm_clear_fifos</c>).</summary>
        public void ClearFifos()
        {
            NativeClearFifos(_block.Index, _sm);
        }

        /// <summary>
        /// Drains any words left in the TX FIFO (maps to <c>pio_sm_drain_tx_fifo</c>): useful before
        /// reconfiguring or restarting so a stale half-streamed frame is not emitted.
        /// </summary>
        public void DrainTxFifo()
        {
            NativeDrainTxFifo(_block.Index, _sm);
        }

        /// <summary>
        /// Restarts the state machine's internal state — ISR/OSR, shift counters, delay/clock phase
        /// (maps to <c>pio_sm_restart</c>). Does not touch the FIFOs or the program counter.
        /// </summary>
        public void Restart()
        {
            NativeRestart(_block.Index, _sm);
        }

        /// <summary>
        /// Restarts this state machine's clock divider so its phase realigns with other SMs started at
        /// the same time (maps to <c>pio_sm_clkdiv_restart</c>).
        /// </summary>
        public void ClockDivRestart()
        {
            NativeClkDivRestart(_block.Index, _sm);
        }

        /// <summary>Changes the clock divider (1.0 .. 65536.0) live and restarts the divider phase.</summary>
        /// <param name="div">The clock divider, 1.0 to 65536.0.</param>
        /// <exception cref="ArgumentException"><paramref name="div"/> is outside the 1.0 .. 65536.0 range.</exception>
        public void SetClockDivisor(float div)
        {
            if (div < 1.0f || div > 65536.0f)
            {
                throw new ArgumentException();
            }

            int intPart = (int)div;
            int frac = (int)((div - intPart) * 256.0f + 0.5f);
            if (frac > 255)
            {
                frac = 0;
                intPart += 1;
            }

            NativeSetClockDivisor(_block.Index, _sm, intPart >= 65536 ? 0 : intPart, frac);
        }

        /// <summary>
        /// Immediately executes a single instruction on the state machine, out of band, without
        /// advancing the program counter (maps to <c>pio_sm_exec</c>). Encode the 16-bit instruction
        /// with <see cref="PioEncoder"/> (or take a word from an assembled <see cref="PioProgram"/>).
        /// </summary>
        public void Exec(ushort instruction)
        {
            NativeExec(_block.Index, _sm, instruction);
        }

        /// <summary>
        /// Sets the direction (output/input) of <paramref name="count"/> consecutive pins starting at
        /// <paramref name="basePin"/> for this state machine (maps to
        /// <c>pio_sm_set_consecutive_pindirs</c>). Pins an SM drives with OUT/SET/side-set must be set
        /// as outputs; combine with <see cref="PioBlock.InitGpio"/>, which routes them to the block.
        /// </summary>
        public void SetConsecutivePinDirs(int basePin, int count, bool output)
        {
            // native side enforces the per-chip GPIO ceiling
            if (basePin < 0 || basePin > 47 || count < 0 || basePin + count > 48)
            {
                throw new ArgumentOutOfRangeException(nameof(basePin));
            }

            NativeSetConsecutivePinDirs(_block.Index, _sm, basePin, count, output);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the SM claim if the instance is collected without an explicit Dispose.</summary>
        ~PioStateMachine()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _enabled = false;
            NativeSetEnabled(_block.Index, _sm, false);
            NativeUnclaim(_block.Index, _sm);
        }

        // ---- native interop (implemented in nf-interpreter) ---------------

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeInit(int block, int sm, int offset, uint[] configBlob);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeSetEnabled(int block, int sm, bool enabled);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativePutBlocking(int block, int sm, uint value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern uint NativeGetBlocking(int block, int sm);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool NativeTxFull(int block, int sm);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool NativeRxEmpty(int block, int sm);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeUnclaim(int block, int sm);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeSetConsecutivePinDirs(int block, int sm, int basePin, int count, bool output);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeClearFifos(int block, int sm);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeDrainTxFifo(int block, int sm);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeRestart(int block, int sm);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeClkDivRestart(int block, int sm);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeExec(int block, int sm, ushort instruction);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern uint NativeTxLevel(int block, int sm);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern uint NativeRxLevel(int block, int sm);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern uint NativeGetPc(int block, int sm);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeSetClockDivisor(int block, int sm, int clkDivInt, int clkDivFrac);
    }
}
