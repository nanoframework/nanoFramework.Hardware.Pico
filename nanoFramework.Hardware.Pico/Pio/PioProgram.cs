//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.Hardware.Pico.Pio
{
    /// <summary>
    /// A fully assembled PIO program: the encoded instructions plus the metadata the
    /// SDK carries in <c>pio_program_t</c> and applies through <c>pio_sm_config</c>
    /// (wrap, side-set, shift defaults, origin). Produced by <see cref="PioAssembler.Build"/>.
    /// </summary>
    public sealed class PioProgram
    {
        /// <summary>Encoded 16-bit instruction words (length &lt;= 32).</summary>
        public ushort[] Instructions { get; private set; }

        /// <summary>Number of instructions.</summary>
        public int Length
        {
            get
            {
                return Instructions.Length;
            }
        }

        /// <summary>PC value that wraps back to <see cref="WrapTarget"/> (last instruction by default).</summary>
        public int Wrap { get; private set; }

        /// <summary>PC value wrapped to (0 by default).</summary>
        public int WrapTarget { get; private set; }

        /// <summary>Fixed load offset 0..31, or -1 when relocatable.</summary>
        public sbyte Origin { get; private set; }

        /// <summary>Side-set value-bit count (excludes the optional enable bit).</summary>
        public int SideSetCount { get; private set; }

        /// <summary>Whether side-set is optional (an extra enable bit is reserved).</summary>
        public bool SideSetOpt { get; private set; }

        /// <summary>Whether side-set targets PINDIRS instead of PINS.</summary>
        public bool SideSetPinDirs { get; private set; }

        /// <summary>OUT/pull shift direction default.</summary>
        public PioShiftDir OutShiftDir { get; private set; }

        /// <summary>Whether autopull is enabled by default.</summary>
        public bool AutoPull { get; private set; }

        /// <summary>Autopull threshold in bits (1..32).</summary>
        public int PullThreshold { get; private set; }

        /// <summary>IN/push shift direction default.</summary>
        public PioShiftDir InShiftDir { get; private set; }

        /// <summary>Whether autopush is enabled by default.</summary>
        public bool AutoPush { get; private set; }

        /// <summary>Autopush threshold in bits (1..32).</summary>
        public int PushThreshold { get; private set; }

        /// <summary>Minimum PIO version required to run this program.</summary>
        public PioVersion Version { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="PioProgram"/> class.</summary>
        internal PioProgram(
            ushort[] instructions,
            int wrap,
            int wrapTarget,
            sbyte origin,
            int sideSetCount,
            bool sideSetOpt,
            bool sideSetPinDirs,
            PioShiftDir outShiftDir,
            bool autoPull,
            int pullThreshold,
            PioShiftDir inShiftDir,
            bool autoPush,
            int pushThreshold,
            PioVersion version)
        {
            Instructions = instructions;
            Wrap = wrap;
            WrapTarget = wrapTarget;
            Origin = origin;
            SideSetCount = sideSetCount;
            SideSetOpt = sideSetOpt;
            SideSetPinDirs = sideSetPinDirs;
            OutShiftDir = outShiftDir;
            AutoPull = autoPull;
            PullThreshold = pullThreshold;
            InShiftDir = inShiftDir;
            AutoPush = autoPush;
            PushThreshold = pushThreshold;
            Version = version;
        }

        /// <summary>
        /// Wraps an externally produced opcode array (for example, the output of the
        /// stand-alone <c>pioasm</c> tool) together with its metadata, so precompiled
        /// programs and inline-assembled ones share the same load/config path.
        /// </summary>
        /// <param name="instructions">The encoded 16-bit PIO instructions (1..32).</param>
        /// <param name="wrapTarget">The wrap-target instruction index.</param>
        /// <param name="wrap">The wrap instruction index.</param>
        /// <returns>A program wrapping the supplied instructions.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instructions"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="instructions"/> is empty or longer than 32 instructions.</exception>
        public static PioProgram FromEncoded(ushort[] instructions, int wrapTarget, int wrap)
        {
            if (instructions == null)
            {
                throw new ArgumentNullException(nameof(instructions));
            }

            if (instructions.Length == 0 || instructions.Length > 32)
            {
                throw new ArgumentException();
            }

            return new PioProgram(
                instructions,
                wrap,
                wrapTarget,
                -1,
                0, false, false,
                PioShiftDir.Right, false, 32,
                PioShiftDir.Right, false, 32,
                PioVersion.Rp2040);
        }
    }
}
