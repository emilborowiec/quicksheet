﻿#region

using System.Collections.Generic;

#endregion

namespace CheatSheetViewerApp.Model
{
    public class Section
    {
        public string Name { get; }
        public List<Cheat> Cheats { get; } = new List<Cheat>();

        public Section(string name)
        {
            Name = name;
        }
    }
}