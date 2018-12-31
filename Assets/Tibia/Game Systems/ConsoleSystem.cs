using Assets.Tibia.ClassicNetwork;
using Assets.Tibia.UI.GameInterface;
using Game.DAO;
using Game.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public class ConsoleSystem
    {
        internal static void OpenPrivateChannel(string creatureName)
        {
            
        }

        internal static void ExcludeFromOwnChannel(string creatureName)
        {
            
        }

        internal static void InviteToOwnChannel(string creatureName)
        {
            
        }

        internal static bool GetOwnPrivateTab()
        {
            return false;
        }

        internal static void AddText(string text, MessageSettingsObject msgtype, string tab)
        {
            var ui = GameObject.FindObjectOfType<UIConsole>();
            ui.AddText(ui.Channels.Where(e => e.Value == tab).Select(e=>e.Key).FirstOrDefault(), text, msgtype.Color);
        }
        

        internal static void Talk(Game.DAO.MessageMode mode, int channelId, string reciever, string message)
        {
            GameServer.Instance.SendTalk(mode, channelId, reciever, message);
        }

        internal static bool IsIgnored(string creatureName)
        {
            return false;
        }

        internal static void AddIgnoredPlayer(string creatureName)
        {
            
        }

        internal static void RemoveIgnoredPlayer(string creatureName)
        {
            
        }

        internal static void ProcessChannelList(List<Tuple<int, string>> channelList)
        {
           
        }

        internal static void ProcessTalk(string name, int level, MessageMode mode, string text, int channelId, Vector3 pos)
        {
            var ui = GameObject.FindObjectOfType<UIConsole>();
            var speakType = UIConsole.SpeakTypes[mode];

            if (!speakType.HideInConsole)
                ui.AddText(channelId, ApplyMessagePrefixies(name, level, text), speakType.GetColor());

            UITextController.Instance.PoolText(Map.Current.GetTile(pos).RealPosition, text, speakType.GetColor(), 1);
        }

        static string ApplyMessagePrefixies(string name, int level, string message)
        {
            if (PlayerPrefs.HasKey("showLevelsInConsole") && level > 0)
                message = name + " [" + level + "]: " + message;
            else
                message = name + ": " + message;
            return message;
        }

        internal static void ProcessOpenChannel(int channelId, string name)
        {
            var ui = GameObject.FindObjectOfType<UIConsole>();
            ui.AddChannel(channelId, name);
            ui.ChannelsTabs.Last().Value.isOn = true;

        }

        internal static void ProcessOpenPrivateChannel(string name)
        {
            
        }
    }
}
