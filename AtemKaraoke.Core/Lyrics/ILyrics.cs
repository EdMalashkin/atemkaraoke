﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtemKaraoke.Core
{
    public interface ILyrics
    {
        string Save();
        void Send();
        void Select(VerseFile newVerse);
    }
}
