using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.DAO;
using TMPro;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    class TextMessageSystem
    {
        internal static void ProcessTextMessage(MessageMode mode, string text)
        {
            if (mode == MessageMode.Invalid) return;

            var msgtype = MessageTypes[mode];

            if (msgtype == MessageSettings["none"]) return;

            PlayerPrefs.SetInt("showInfoMessagesInConsole", 1);
            PlayerPrefs.SetInt("showEventMessagesInConsole", 1);
            PlayerPrefs.SetInt("showStatusMessagesInConsole", 1);


            if (!string.IsNullOrEmpty(msgtype.ConsoleTab) && (!string.IsNullOrEmpty(msgtype.ConsoleOption) && PlayerPrefs.HasKey(msgtype.ConsoleOption)))
            {
                ConsoleSystem.AddText(text, msgtype, msgtype.ConsoleTab);
            }

            if (!string.IsNullOrEmpty(msgtype.ScreenTarget))
            {
                var label = GameObject.Find(msgtype.ScreenTarget).GetComponent<TMP_Text>();
                label.text = text;
                label.color = msgtype.Color;
                MicroTasks.DelayedTask((e) => label.text = "", CalculateVisibleTime(text)/1000f);
            }
        }

        internal static void ProcessCloseChannel(int channelId)
        {
            
        }

        internal static void ProcessOpenOwnPrivateChannel(int channelId, string name)
        {
            
        }

        internal static void ProcessOpenPrivateChannel(string name)
        {
            
        }

        internal static void ProcessOpenChannel(int channelId, string name)
        {
            
        }

        internal static void ProcessChannelList(List<Tuple<int, string>> channelList)
        {
            
        }

        internal static void ProcessTalk(string name, int level, MessageMode mode, string text, int channelId, Vector3 pos)
        {
            //Debug.Log("[" + name + "] [" + mode + "] " + channelId + " " + pos + " " + text )
        }

        internal static void ProcessEditText(uint id, int itemId, int maxLength, string text, string writer, string date)
        {
            
        }

        internal static void ProcessEditList(uint id, int doorId, string text)
        {
            
        }

        static readonly Dictionary<string, MessageSettingsObject> MessageSettings = new Dictionary<string, MessageSettingsObject>() {
            { "none"            , new MessageSettingsObject() {} },
            { "consoleRed"      , new MessageSettingsObject() { Color = Color.red,    ConsoleTab="Default" } },
            { "consoleOrange"   , new MessageSettingsObject() { Color = new Color(1, 0.8f, 0), ConsoleTab="Default" } },
            { "consoleBlue"     , new MessageSettingsObject() { Color = Color.blue,   ConsoleTab="Default" } },
            { "centerRed"       , new MessageSettingsObject() { Color = Color.red,    ConsoleTab="Server Log", ScreenTarget="lowCenterLabel" } },
            { "centerGreen"     , new MessageSettingsObject() { Color = Color.green,  ConsoleTab="Server Log", ScreenTarget="highCenterLabel",   ConsoleOption="showInfoMessagesInConsole" } },
            { "centerWhite"     , new MessageSettingsObject() { Color = Color.white,  ConsoleTab="Server Log", ScreenTarget="middleCenterLabel", ConsoleOption="showEventMessagesInConsole" } },
            { "bottomWhite"     , new MessageSettingsObject() { Color = Color.white,  ConsoleTab="Server Log", ScreenTarget="statusLabel",       ConsoleOption="showEventMessagesInConsole" } },
            { "status"          , new MessageSettingsObject() { Color = Color.white,  ConsoleTab="Server Log", ScreenTarget="statusLabel",       ConsoleOption="showStatusMessagesInConsole" } },
            { "statusSmall"     , new MessageSettingsObject() { Color = Color.white,                           ScreenTarget="statusLabel" } },
            { "private"         , new MessageSettingsObject() { Color = new Color(0.8f, 0.8f, 1),                       ScreenTarget="privateLabel"  }}
        };

        static readonly Dictionary<MessageMode, MessageSettingsObject> MessageTypes = new Dictionary<MessageMode, MessageSettingsObject>() {
            { MessageMode.MonsterSay, MessageSettings["consoleOrange"] },
            { MessageMode.MonsterYell, MessageSettings["consoleOrange"] },
            { MessageMode.BarkLow, MessageSettings["consoleOrange"] },
            { MessageMode.BarkLoud, MessageSettings["consoleOrange"] },
            { MessageMode.Failure, MessageSettings["statusSmall"] },
            { MessageMode.Login, MessageSettings["bottomWhite"] },
            { MessageMode.Game, MessageSettings["centerWhite"] },
            { MessageMode.Status, MessageSettings["status"] },
            { MessageMode.Warning, MessageSettings["centerRed"] },
            { MessageMode.Look, MessageSettings["centerGreen"] },
            { MessageMode.Loot, MessageSettings["centerGreen"] },
            { MessageMode.Red, MessageSettings["consoleRed"] },
            { MessageMode.Blue, MessageSettings["consoleBlue"] },
            { MessageMode.PrivateFrom, MessageSettings["consoleBlue"] },

            { MessageMode.GamemasterBroadcast, MessageSettings["consoleRed"] },

            { MessageMode.DamageDealed, MessageSettings["status"] },
            { MessageMode.DamageReceived, MessageSettings["status"] },
            { MessageMode.Heal, MessageSettings["status"] },
            { MessageMode.Exp, MessageSettings["status"] },

            { MessageMode.DamageOthers, MessageSettings["none"] },
            { MessageMode.HealOthers, MessageSettings["none"] },
            { MessageMode.ExpOthers, MessageSettings["none"] },

            { MessageMode.TradeNpc, MessageSettings["centerWhite"] },
            { MessageMode.Guild, MessageSettings["centerWhite"] },
            { MessageMode.Party, MessageSettings["centerGreen"] },
            { MessageMode.PartyManagement, MessageSettings["centerWhite"] },
            { MessageMode.TutorialHint, MessageSettings["centerWhite"] },
            { MessageMode.Market, MessageSettings["centerWhite"] },
            { MessageMode.BeyondLast, MessageSettings["centerWhite"] },
            { MessageMode.Report, MessageSettings["consoleRed"] },
            { MessageMode.HotkeyUse, MessageSettings["centerGreen"] },

            { MessageMode.Private, MessageSettings["private"] }
        };


        public static float CalculateVisibleTime(string text)
        {
            return Mathf.Max(text.Length * 100, 4000);
        }

    }

    public class MessageSettingsObject
    {
        internal string ScreenTarget;

        public MessageSettingsObject()
        {
        }

        public Color Color { get; set; }
        public string ConsoleTab { get; set; }
        public string ConsoleOption { get; set; }
    }
}
