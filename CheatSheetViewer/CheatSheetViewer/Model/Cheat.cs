﻿#region

using System.Collections.Generic;

#endregion

namespace CheatSheetViewerApp.Model
{
    public class Cheat
    {
        public string Caption { get; }
        public List<string> Entries { get; } = new List<string>();

        public Cheat(string caption)
        {
            Caption = caption;
        }
    }
}