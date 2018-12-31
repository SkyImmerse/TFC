using Assets.Tibia.DAO;
using Game.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Assets.Tibia.UI.GameInterface
{
    public class UIConsole : MonoBehaviour
    {
        public GameObject MessagePrefab;
        public GameObject TabPrefab;
        public GameObject TabBodyPrefab;

        public Transform TabAnchor;
        public Transform TabBodyAnchor;

        public Dictionary<int, UIVisibleToggle> ChannelsBody = new Dictionary<int, UIVisibleToggle>();
        public Dictionary<int, Toggle> ChannelsTabs = new Dictionary<int, Toggle>();
        public Dictionary<int, string> Channels = new Dictionary<int, string>();

        public int ActiveChannelId;
        public TMPro.TMP_InputField InputField;

        void Start()
        {
            ResetController();
        }
        public void FixedUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                SendField();
            }
        }

        private void SendField()
        {
            if (InputField.text != string.Empty)
            {
                var message = InputField.text;

                int channel = ActiveChannelId;
                var chatCommandSayMode = "";
                var chatCommandPrivate = "";
                var chatCommandPrivateReady = "";
                //when talking on server log, the message goes to default channel
                if (ActiveChannelId == -200)
                {
                    ChannelsTabs.First().Value.isOn = true;
                    channel = 0;
                }

                // player used yell command
                var chatCommandMessage = Regex.Match(message, "^%#[y|Y] (.*)");
                if (chatCommandMessage.Success)
                {
                    chatCommandSayMode = "yell";
                    channel = 0;
                    message = chatCommandMessage.Value;
                }

                //player used whisper
                chatCommandMessage = Regex.Match(message, "^%#[w|W] (.*)");
                if (chatCommandMessage.Success)
                {
                    chatCommandSayMode = "whisper";
                    channel = 0;
                    message = chatCommandMessage.Value;
                }

                //player say
                chatCommandMessage = Regex.Match(message, "^%#[s|S] (.*)");
                if (chatCommandMessage.Success)
                {
                    chatCommandSayMode = "say";
                    channel = 0;
                    message = chatCommandMessage.Value;
                }

                //player red talk on channel
                chatCommandMessage = Regex.Match(message, "^%#[c|C] (.*)");
                if (chatCommandMessage.Success)
                {
                    chatCommandSayMode = "channelRed";
                    message = chatCommandMessage.Value;
                }

                //player broadcast
                chatCommandMessage = Regex.Match(message, "^%#[b|B] (.*)");
                if (chatCommandMessage.Success)
                {
                    chatCommandSayMode = "broadcast";
                    message = chatCommandMessage.Value;
                    channel = 0;
                }

                if (message.Length == 0) return;

                if(chatCommandSayMode == "")
                {
                    chatCommandSayMode = "say";
                }
                if(channel < 0)
                {
                    channel = 0;
                }
                ConsoleSystem.Talk(SpeakTypesSettings[chatCommandSayMode].Type, channel, "", message);

                InputField.text = "";
            }
        }

        public void AddText(int channelId, string message, Color color)
        {
            if(channelId == 0) channelId = -100;
            var beforeScroll = ChannelsBody[channelId].GetComponentInChildren<ScrollRect>().verticalNormalizedPosition;
                var text = GameObject.Instantiate(MessagePrefab, ChannelsBody[channelId].transform.GetComponentInChildren<VerticalLayoutGroup>().transform);
            text.GetComponent<TMP_Text>().text = message;
            text.GetComponent<TMP_Text>().color = color;

            if (beforeScroll == 0)
                ChannelsBody[channelId].GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 0;
        }

        public void AddChannel(int id, string name)
        {
            var tab = GameObject.Instantiate(TabPrefab, TabAnchor);
            tab.GetComponentInChildren<TMP_Text>().text = name;
            tab.GetComponent<Toggle>().onValueChanged = new Toggle.ToggleEvent();
            tab.GetComponent<Toggle>().onValueChanged.AddListener((val) =>
            {
                ToggleTab(id);
            });
            ChannelsTabs.Add(id, tab.GetComponent<Toggle>());

            var body = GameObject.Instantiate(TabBodyPrefab, TabBodyAnchor);
            ChannelsBody.Add(id, body.GetComponent<UIVisibleToggle>());

            Channels.Add(id, name);
        }

        internal void ResetController()
        {
            ClearChannels();
            ChannelsTabs.Last().Value.isOn = true;
        }

        private void ToggleTab(int id)
        {
            var tab = ChannelsTabs[id];
            if (tab.isOn)
            {
                foreach (var item in ChannelsTabs.Where(e => e.Key != id))
                {
                    item.Value.isOn = false;
                    ColorUtility.TryParseHtmlString("#A4A4A4", out Color c);
                    item.Value.transform.GetComponentInChildren<TMP_Text>().color = c;
                }
                foreach (var item in ChannelsBody.Where(e => e.Key != id))
                {
                    item.Value.Visible = false;
                }

                ChannelsTabs[id].transform.GetComponentInChildren<TMP_Text>().color = Color.white;
                ChannelsBody[id].Visible = true;
                ActiveChannelId = id;
            }
        }

        internal void ClearChannels()
        {
            foreach (Transform item in TabAnchor)
            {
                GameObject.Destroy(item.gameObject);
            }
            foreach (Transform item in TabBodyAnchor)
            {
                GameObject.Destroy(item.gameObject);
            }

            ChannelsBody.Clear();
            ChannelsTabs.Clear();
            Channels.Clear();

            AddChannel(-200, "Server Log");
            AddChannel(-100, "Default");
        }

        public class SpeakType
        {
            public MessageMode Type;
            public string Color;
            public bool Private;
            public bool NpcChat;
            public bool HideInConsole;
            public Color GetColor()
            {
                ColorUtility.TryParseHtmlString(Color, out Color i);
                return i;
            }
        }

        public static Dictionary<string, SpeakType> SpeakTypesSettings = new Dictionary<string, SpeakType>() {
          { "none", new SpeakType() {}},
          { "say", new SpeakType() { Type = MessageMode.Say, Color = "#FFFF00" }},
          { "whisper", new SpeakType() { Type = MessageMode.Whisper, Color = "#FFFF00" }},
          { "yell", new SpeakType() { Type = MessageMode.Yell, Color = "#FFFF00" }},
          { "broadcast", new SpeakType() { Type = MessageMode.GamemasterBroadcast, Color = "#F55E5E" }},
          { "private", new SpeakType() { Type = MessageMode.PrivateTo, Color = "#5FF7F7", Private = true }},
          { "privateRed", new SpeakType() { Type = MessageMode.GamemasterPrivateTo, Color = "#F55E5E", Private = true }},
          { "privatePlayerToPlayer", new SpeakType() { Type = MessageMode.PrivateTo, Color = "#9F9DFD", Private = true }},
          { "privatePlayerToNpc", new SpeakType() { Type = MessageMode.NpcTo, Color = "#9F9DFD", Private = true, NpcChat = true }},
          { "privateNpcToPlayer", new SpeakType() { Type = MessageMode.NpcFrom, Color = "#5FF7F7", Private = true, NpcChat = true }},
          { "channelYellow", new SpeakType() { Type = MessageMode.Channel, Color = "#FFFF00" }},
          { "channelWhite", new SpeakType() { Type = MessageMode.ChannelManagement, Color = "#FFFFFF" }},
          { "channelRed", new SpeakType() { Type = MessageMode.GamemasterChannel, Color = "#F55E5E" }},
          { "channelOrange", new SpeakType() { Type = MessageMode.ChannelHighlight, Color = "#FE6500" }},
          { "monsterSay", new SpeakType() { Type = MessageMode.MonsterSay, Color = "#FE6500", HideInConsole = true}},
          { "monsterYell", new SpeakType() { Type = MessageMode.MonsterYell, Color = "#FE6500", HideInConsole = true}},
          { "rvrAnswerFrom", new SpeakType() { Type = MessageMode.RVRAnswer, Color = "#FE6500" }},
          { "rvrAnswerTo", new SpeakType() { Type = MessageMode.RVRAnswer, Color = "#FE6500" }},
          { "rvrContinue", new SpeakType() { Type = MessageMode.RVRContinue, Color = "#FFFF00" }},
        };

        public static Dictionary<MessageMode, SpeakType> SpeakTypes = new Dictionary<MessageMode, SpeakType>() {
          { MessageMode.Say, SpeakTypesSettings["say"] },
          { MessageMode.Whisper, SpeakTypesSettings["whisper"] },
          { MessageMode.Yell, SpeakTypesSettings["yell"] },
          { MessageMode.GamemasterBroadcast, SpeakTypesSettings["broadcast"] },
          { MessageMode.PrivateFrom, SpeakTypesSettings["private"] },
          { MessageMode.GamemasterPrivateFrom, SpeakTypesSettings["privateRed"] },
          { MessageMode.NpcTo, SpeakTypesSettings["privatePlayerToNpc"] },
          { MessageMode.NpcFrom, SpeakTypesSettings["privateNpcToPlayer"] },
          { MessageMode.Channel, SpeakTypesSettings["channelYellow"] },
          { MessageMode.ChannelManagement, SpeakTypesSettings["channelWhite"] },
          { MessageMode.GamemasterChannel, SpeakTypesSettings["channelRed"] },
          { MessageMode.ChannelHighlight, SpeakTypesSettings["channelOrange"] },
          { MessageMode.MonsterSay, SpeakTypesSettings["monsterSay"] },
          { MessageMode.MonsterYell, SpeakTypesSettings["monsterYell"] },
          { MessageMode.RVRChannel, SpeakTypesSettings["channelWhite"] },
          { MessageMode.RVRContinue, SpeakTypesSettings["rvrContinue"] },
          { MessageMode.RVRAnswer, SpeakTypesSettings["rvrAnswerFrom"] },
          { MessageMode.NpcFromStartBlock, SpeakTypesSettings["privateNpcToPlayer"] },

          // ignored types
          { MessageMode.Spell, SpeakTypesSettings["say"] },
          { MessageMode.BarkLow, SpeakTypesSettings["monsterSay"] },
          { MessageMode.BarkLoud, SpeakTypesSettings["monsterYell"] },
        };

    }
}
