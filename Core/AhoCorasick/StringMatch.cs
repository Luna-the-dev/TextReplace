﻿namespace TextReplace.Core.AhoCorasick
{
    public class StringMatch
    {
        public string Text { get; private set; }
        public int Position { get; private set; }
        public StringMatch(string txt, int pos)
        {
            Text = txt;
            Position = pos;
        }
    }
}
