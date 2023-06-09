﻿using Ryujinx.Common.Utilities;

namespace Ryujinx.Graphics.Vic.Types
{
    struct PipeConfig
    {
#pragma warning disable CS0169, CS0649
        private long _word0;
        private long _word1;
#pragma warning restore CS0169, CS0649

        public int DownsampleHoriz => (int)_word0.Extract(0, 11);
        public int DownsampleVert => (int)_word0.Extract(16, 11);
    }
}
