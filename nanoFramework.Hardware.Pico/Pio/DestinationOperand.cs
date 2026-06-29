//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace nanoFramework.Hardware.Pico.Pio
{
    /// <summary>Destination operand. The legal subset depends on the instruction (validated at emit time).</summary>
    public enum DestinationOperand
    {
        /// <summary>The pins mapped to the instruction's pin group.</summary>
        Pins = 0,
        /// <summary>The scratch register X.</summary>
        RegisterX = 1,
        /// <summary>The scratch register Y.</summary>
        RegisterY = 2,
        /// <summary>Discards the data (writes nowhere).</summary>
        DiscardsData = 3,
        /// <summary>The pin direction registers for the mapped pins.</summary>
        PinDirs = 4,
        /// <summary>The program counter (an unconditional jump).</summary>
        Pc = 5,
        /// <summary>The status value (shares the encoding of <see cref="Pc"/>).</summary>
        Status = 5,
        /// <summary>The input shift register (ISR).</summary>
        InputShiftRegister = 6,
        /// <summary>The output shift register (OSR).</summary>
        OutputShiftRegister = 7,
        /// <summary>Executes the value as an instruction (shares the encoding of <see cref="OutputShiftRegister"/>).</summary>
        Executes = 7,
    }
}
