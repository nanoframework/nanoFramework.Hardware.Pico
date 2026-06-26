//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace nanoFramework.Hardware.Pico.Pio
{
    /// <summary>Construction-time options for <see cref="PioAssembler"/>.</summary>
    public sealed class PioAssemblerOptions
    {
        /// <summary>Target PIO version. RP2350 unlocks v1-only instructions.</summary>
        public PioVersion Version = PioVersion.Rp2040;

        /// <summary>Side-set value-bit count (0..5). Reduces the per-instruction delay range.</summary>
        public int SideSetCount = 0;

        /// <summary>When true, side-set is optional and an enable bit is reserved.</summary>
        public bool SideSetOpt = false;

        /// <summary>When true, side-set drives PINDIRS instead of PINS.</summary>
        public bool SideSetPinDirs = false;
    }
}
