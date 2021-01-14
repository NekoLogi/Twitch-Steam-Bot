using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TwitchBot
{
    public static class SaveSystem
    {
        public static void Save() {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream("../data/bot.save", FileMode.Create);

            SaveData data = new SaveData();

            formatter.Serialize(stream, data);
            stream.Close();

            Console.WriteLine("-------------\nData Saved!\n-------------");
        }

        public static void Load() {
            if (File.Exists("../data/bot.save")) {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream("../data/bot.save", FileMode.Open);

                SaveData data = formatter.Deserialize(stream) as SaveData;
                stream.Close();

                Bot.blacklist = data.BLACKLIST;
                Bot.crashCounter = data.CRASH_COUNTER;
                Bot.prefix = data.PREFIX;

                Console.WriteLine("-------------\nData Loaded!\n-------------");

            } else {
                Console.WriteLine("Save file not found in /data/bot.save");
            }

        }
    }
}
