using System;
using System.Collections.Generic;

namespace TwitchBot
{
    [Serializable]
    public class SaveData
    {
        public List<string> BLACKLIST = new List<string>();
        public uint CRASH_COUNTER;
        public char PREFIX;

        public SaveData() {
            BLACKLIST = Bot.blacklist;
            CRASH_COUNTER = Bot.crashCounter;
            PREFIX = Bot.prefix;
        }
    }
}
