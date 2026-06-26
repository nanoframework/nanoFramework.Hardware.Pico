//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace nanoFramework.Hardware.Pico.Pio
{
    /// <summary>
    /// A jump target. Obtain one from <see cref="PioAssembler.DefineLabel"/>, place it
    /// with <see cref="PioAssembler.MarkLabel"/>, and reference it from JMP — forward
    /// references are resolved at <see cref="PioAssembler.Build"/> time.
    /// </summary>
    public sealed class PioLabel
    {
        internal PioLabel(int id)
        {
            Id = id;
            Address = -1;
        }

        internal int Id { get; }

        /// <summary>Program offset where the label was marked, or -1 if not yet bound.</summary>
        internal int Address { get; set; }

        /// <summary>True once the label has been placed in the instruction stream.</summary>
        public bool IsBound { get { return Address >= 0; } }
    }
}
