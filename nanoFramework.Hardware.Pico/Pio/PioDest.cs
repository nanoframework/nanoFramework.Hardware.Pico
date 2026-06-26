//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace nanoFramework.Hardware.Pico.Pio
{
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
}
