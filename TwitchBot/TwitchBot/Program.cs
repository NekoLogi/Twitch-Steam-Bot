using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub.Models.Responses;
using SteamKit2;


namespace TwitchBot
{
    public class Program
    {
        static void Main(string[] args) {
            Bot bot = new Bot();
            while (true) {
                if (bot.channel != null) {
                    bot.Info();
                    Thread.Sleep(1800000);
                }
                Thread.Sleep(500);
            }
        }
    }

    public class Bot
    {
        TwitchClient client;

        public static uint crashCounter;
        public static List<string> blacklist = new List<string>();
        public static char prefix = '!';
        static bool cooldown = true;

        public string channel;


        public Bot() {
            ConnectionCredentials credentials = new ConnectionCredentials("nekologi_", "tkdd6huajjrjwntjxpx3ufzjtpd7y0");
            var clientOptions = new ClientOptions {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, "whiteshino");

            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnWhisperReceived += Client_OnWhisperReceived;
            client.OnNewSubscriber += Client_OnNewSubscriber;
            client.OnConnected += Client_OnConnected;

            client.Connect();
        }

        private void Client_OnLog(object sender, OnLogArgs e) {
            Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e) {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e) {
            SaveSystem.Load();

            Console.WriteLine("Bot started!");
            client.SendMessage(channel = e.Channel, "/me Bot started!");
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e) {
            if (e.ChatMessage.Message.StartsWith(prefix.ToString())) {

                #region Global
                if (!blacklist.Contains(e.ChatMessage.Username)) {
                    if (e.ChatMessage.Message.Contains($"{prefix}help")) {                              // Help
                        client.SendMessage(channel, $"/me {prefix} = Prefix.");
                        client.SendMessage(channel, $"Global:");
                        client.SendMessage(channel, $"/me help = Alle Befehle und der Prefix.");
                        client.SendMessage(channel, $"/me prime = Twitch Prime Werbung");
                        client.SendMessage(channel, $"VIP Only:");
                        client.SendMessage(channel, $"/me ups = CrashCounter.");
                        client.SendMessage(channel, $"MOD Only:");
                        client.SendMessage(channel, $"/me reset = Setze CrashCounter zurück.");
                        client.SendMessage(channel, $"/me bls = Blacklist. [{prefix}bls <Username>]");
                        client.SendMessage(channel, $"/me debls = Blacklist Remove. [{prefix}debls <Username>]");
                        client.SendMessage(channel, $"/me fix = Neuer Prefix. [{prefix}fix <Symbol>]");

                        Console.WriteLine($"{prefix} = Prefix.");
                        Console.WriteLine($"Global:");
                        Console.WriteLine($"help = Alle Befehle und der Prefix.");
                        Console.WriteLine($"/me prime = Twitch Prime Werbung");
                        Console.WriteLine($"VIP Only:");
                        Console.WriteLine($"ups = CrashCounter.");
                        Console.WriteLine($"MOD Only:");
                        Console.WriteLine($"reset = Setze CrashCounter zurück.");
                        Console.WriteLine($"bls = Blacklist. [{prefix}bls <Username>]");
                        Console.WriteLine($"debls = Blacklist Remove. [{prefix}debls <Username>]");
                        Console.WriteLine($"fix = Neuer Prefix. [{prefix}fix <Symbol>]");

                    } else if (e.ChatMessage.Message.Contains($"{prefix}prime")) {                       // Twitch Prime
                        client.SendMessage(channel, $"/me Twitch Prime is not a Crime!");
                        client.SendMessage(channel, $"/me Subscribe jetzt und bekomme eine Goldene Toilettenpapierrolle Gratis!");

                        Console.WriteLine($"Twitch Prime is not a Crime!\nSubscribe jetzt und bekomme eine Goldene Toilettenpapierrolle Gratis!");
                    }
                }
                #endregion

                #region VIP Only
                if (!blacklist.Contains(e.ChatMessage.Username) && e.ChatMessage.IsVip || e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster) {

                    if (cooldown && e.ChatMessage.Message.Contains($"{prefix}ups")) {               // CrashCounter
                        crashCounter++;

                        client.SendMessage(channel, $"/me CrashCounter: {crashCounter}");
                        Console.WriteLine($"CrashCounter: {crashCounter}");

                        SaveSystem.Save();
                        Task.Run(Cooldown);
                    }
                }
                #endregion

                #region MOD Only
                if (!blacklist.Contains(e.ChatMessage.Username) && e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster) {

                    if (e.ChatMessage.Message.Contains($"{prefix}reset")) {                         // Reset CrashCounter
                        crashCounter = 0;

                        client.SendMessage(channel, $"/me CrashCounter: {crashCounter}");
                        Console.WriteLine($"CrashCounter: {crashCounter}");

                        SaveSystem.Save();

                    } else if (e.ChatMessage.Message.Contains($"{prefix}fix")) {                    // Change Prefix
                        string[] message = e.ChatMessage.Message.Split(' ');
                        prefix = message[1].ToCharArray()[0];

                        client.SendMessage(channel, $"/me {e.ChatMessage.Username} hat Prefix auf {message[1]} geändert.");
                        Console.WriteLine($"{e.ChatMessage.Username} hat Prefix auf {message[1]} geändert.");

                        SaveSystem.Save();

                    } else if (e.ChatMessage.Message.Contains($"{prefix}bls")) {                    // Blacklist
                        string[] message = e.ChatMessage.Message.Split(' ');
                        blacklist.Add(message[1]);

                        client.SendMessage(channel, $"/me {e.ChatMessage.Username} hat {message[1]} in die Blacklist hinzugefügt.");
                        Console.WriteLine($"{e.ChatMessage.Username} hat {message[1]} in die Blacklist hinzugefügt.");

                        SaveSystem.Save();

                    } else if (e.ChatMessage.Message.Contains($"{prefix}debls")) {                  // Remove User from Blacklist
                        string[] message = e.ChatMessage.Message.Split(' ');
                        blacklist.Remove(message[1]);

                        client.SendMessage(channel, $"/me {e.ChatMessage.Username} hat {message[1]} aus der Blacklist entfernt.");
                        Console.WriteLine($"{e.ChatMessage.Username} hat {message[1]} aus der Blacklist entfernt.");

                        SaveSystem.Save();
                    }
                }
                #endregion
            }
        }

        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e) {
            if (e.WhisperMessage.Username == "my_friend")
                client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }

        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e) {
            if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
                client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
            else
                client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
        }

        public void Cooldown() {
            cooldown = false;
            Thread.Sleep(3000);
            cooldown = true;
        }

        public void Info() {
            client.SendMessage(channel, $"/me Unter {prefix}help könnt ihr euch Befehle für den Chat Anschauen!");
        }
    }
}
