using System.Collections.Generic;
using System.Linq;

namespace Game.DAO
{
    public static class MessageModeTranslator
    {
        public static Dictionary<MessageMode, byte> messageModesMap = new Dictionary<MessageMode, byte>();

        public static void buildMessageModesMap(int version)
        {
            messageModesMap.Clear();


            if (version >= 1094)
            {
                messageModesMap[MessageMode.Mana] = 43;
            }


            if (version >= 1055)
            {
                // might be 1054

                messageModesMap[MessageMode.None] = 0;

                messageModesMap[MessageMode.Say] = 1;

                messageModesMap[MessageMode.Whisper] = 2;

                messageModesMap[MessageMode.Yell] = 3;

                messageModesMap[MessageMode.PrivateFrom] = 4;

                messageModesMap[MessageMode.PrivateTo] = 5;

                messageModesMap[MessageMode.ChannelManagement] = 6;

                messageModesMap[MessageMode.Channel] = 7;

                messageModesMap[MessageMode.ChannelHighlight] = 8;

                messageModesMap[MessageMode.Spell] = 9;

                messageModesMap[MessageMode.NpcFromStartBlock] = 10;

                messageModesMap[MessageMode.NpcFrom] = 11;

                messageModesMap[MessageMode.NpcTo] = 12;

                messageModesMap[MessageMode.GamemasterBroadcast] = 13;

                messageModesMap[MessageMode.GamemasterChannel] = 14;

                messageModesMap[MessageMode.GamemasterPrivateFrom] = 15;

                messageModesMap[MessageMode.GamemasterPrivateTo] = 16;

                messageModesMap[MessageMode.Login] = 17;

                messageModesMap[MessageMode.Warning] = 18; // Admin

                messageModesMap[MessageMode.Game] = 19;

                messageModesMap[MessageMode.GameHighlight] = 20;

                messageModesMap[MessageMode.Failure] = 21;

                messageModesMap[MessageMode.Look] = 22;

                messageModesMap[MessageMode.DamageDealed] = 23;

                messageModesMap[MessageMode.DamageReceived] = 24;

                messageModesMap[MessageMode.Heal] = 25;

                messageModesMap[MessageMode.Exp] = 26;

                messageModesMap[MessageMode.DamageOthers] = 27;

                messageModesMap[MessageMode.HealOthers] = 28;

                messageModesMap[MessageMode.ExpOthers] = 29;

                messageModesMap[MessageMode.Status] = 30;

                messageModesMap[MessageMode.Loot] = 31;

                messageModesMap[MessageMode.TradeNpc] = 32;

                messageModesMap[MessageMode.Guild] = 33;

                messageModesMap[MessageMode.PartyManagement] = 34;

                messageModesMap[MessageMode.Party] = 35;

                messageModesMap[MessageMode.BarkLow] = 36;

                messageModesMap[MessageMode.BarkLoud] = 37;

                messageModesMap[MessageMode.Report] = 38;

                messageModesMap[MessageMode.HotkeyUse] = 39;

                messageModesMap[MessageMode.TutorialHint] = 40;

                messageModesMap[MessageMode.Thankyou] = 41;

                messageModesMap[MessageMode.Market] = 42;
            }
            else if (version >= 1036)
            {
                for (int i = (int)MessageMode.None; i <= (int)MessageMode.BeyondLast; ++i)
                {
                    if (i >= (int)MessageMode.NpcTo)
                        messageModesMap[(MessageMode)i] = (byte)(i + 1);
                    else

                        messageModesMap[(MessageMode)i] = (byte)i;
                }
            }
            else if (version >= 900)
            {
                for (int i = (byte)MessageMode.None; i <= (byte)MessageMode.BeyondLast; ++i)

                    messageModesMap[(MessageMode)i] = (byte)i;
            }
            else if (version >= 861)
            {
                messageModesMap[MessageMode.None] = 0;

                messageModesMap[MessageMode.Say] = 1;

                messageModesMap[MessageMode.Whisper] = 2;

                messageModesMap[MessageMode.Yell] = 3;

                messageModesMap[MessageMode.NpcTo] = 4;

                messageModesMap[MessageMode.NpcFrom] = 5;

                messageModesMap[MessageMode.PrivateFrom] = 6;

                messageModesMap[MessageMode.PrivateTo] = 6;

                messageModesMap[MessageMode.Channel] = 7;

                messageModesMap[MessageMode.ChannelManagement] = 8;

                messageModesMap[MessageMode.GamemasterBroadcast] = 9;

                messageModesMap[MessageMode.GamemasterChannel] = 10;

                messageModesMap[MessageMode.GamemasterPrivateFrom] = 11;

                messageModesMap[MessageMode.GamemasterPrivateTo] = 11;

                messageModesMap[MessageMode.ChannelHighlight] = 12;

                messageModesMap[MessageMode.MonsterSay] = 13;

                messageModesMap[MessageMode.MonsterYell] = 14;

                messageModesMap[MessageMode.Warning] = 15;

                messageModesMap[MessageMode.Game] = 16;

                messageModesMap[MessageMode.Login] = 17;

                messageModesMap[MessageMode.Status] = 18;

                messageModesMap[MessageMode.Look] = 19;

                messageModesMap[MessageMode.Failure] = 20;

                messageModesMap[MessageMode.Blue] = 21;

                messageModesMap[MessageMode.Red] = 22;
            }
            else if (version >= 840)
            {
                messageModesMap[MessageMode.None] = 0;

                messageModesMap[MessageMode.Say] = 1;

                messageModesMap[MessageMode.Whisper] = 2;

                messageModesMap[MessageMode.Yell] = 3;

                messageModesMap[MessageMode.NpcTo] = 4;

                messageModesMap[MessageMode.NpcFromStartBlock] = 5;

                messageModesMap[MessageMode.PrivateFrom] = 6;

                messageModesMap[MessageMode.PrivateTo] = 6;

                messageModesMap[MessageMode.Channel] = 7;

                messageModesMap[MessageMode.ChannelManagement] = 8;

                messageModesMap[MessageMode.RVRChannel] = 9;

                messageModesMap[MessageMode.RVRAnswer] = 10;

                messageModesMap[MessageMode.RVRContinue] = 11;

                messageModesMap[MessageMode.GamemasterBroadcast] = 12;

                messageModesMap[MessageMode.GamemasterChannel] = 13;

                messageModesMap[MessageMode.GamemasterPrivateFrom] = 14;

                messageModesMap[MessageMode.GamemasterPrivateTo] = 14;

                messageModesMap[MessageMode.ChannelHighlight] = 15;

                // 16, 17 ??

                messageModesMap[MessageMode.Red] = 18;

                messageModesMap[MessageMode.MonsterSay] = 19;

                messageModesMap[MessageMode.MonsterYell] = 20;

                messageModesMap[MessageMode.Warning] = 21;

                messageModesMap[MessageMode.Game] = 22;

                messageModesMap[MessageMode.Login] = 23;

                messageModesMap[MessageMode.Status] = 24;

                messageModesMap[MessageMode.Look] = 25;

                messageModesMap[MessageMode.Failure] = 26;

                messageModesMap[MessageMode.Blue] = 27;
            }
            else if (version >= 760)
            {
                messageModesMap[MessageMode.None] = 0;

                messageModesMap[MessageMode.Say] = 1;

                messageModesMap[MessageMode.Whisper] = 2;

                messageModesMap[MessageMode.Yell] = 3;

                messageModesMap[MessageMode.PrivateFrom] = 4;

                messageModesMap[MessageMode.PrivateTo] = 4;

                messageModesMap[MessageMode.Channel] = 5;

                messageModesMap[MessageMode.RVRChannel] = 6;

                messageModesMap[MessageMode.RVRAnswer] = 7;

                messageModesMap[MessageMode.RVRContinue] = 8;

                messageModesMap[MessageMode.GamemasterBroadcast] = 9;

                messageModesMap[MessageMode.GamemasterChannel] = 10;

                messageModesMap[MessageMode.GamemasterPrivateFrom] = 11;

                messageModesMap[MessageMode.GamemasterPrivateTo] = 11;

                messageModesMap[MessageMode.ChannelHighlight] = 12;

                // 13, 14, 15 ??

                messageModesMap[MessageMode.MonsterSay] = 16;

                messageModesMap[MessageMode.MonsterYell] = 17;

                messageModesMap[MessageMode.Warning] = 18;

                messageModesMap[MessageMode.Game] = 19;

                messageModesMap[MessageMode.Login] = 20;

                messageModesMap[MessageMode.Status] = 21;

                messageModesMap[MessageMode.Look] = 22;

                messageModesMap[MessageMode.Failure] = 23;

                messageModesMap[MessageMode.Blue] = 24;

                messageModesMap[MessageMode.Red] = 25;
            }
        }


        public static MessageMode translateMessageModeFromServer(byte mode)
        {
            var it = messageModesMap.Where((pair => pair.Value == mode));
            var keyValuePairs = it as KeyValuePair<MessageMode, byte>[] ?? it.ToArray();

            return keyValuePairs.Any() ? keyValuePairs.First().Key : MessageMode.Invalid;
        }

        public static byte translateMessageModeToServer(MessageMode mode)
        {

            if (mode < 0 || mode >= MessageMode.LastMessage)

                return (byte)MessageMode.Invalid;

            var it = messageModesMap.Where(pair => pair.Key == mode);

            var keyValuePairs = it as IList<KeyValuePair<MessageMode, byte>> ?? it.ToList();
            return keyValuePairs.Any() ? (byte)keyValuePairs.First().Value : (byte)MessageMode.Invalid;
        }
    }
}