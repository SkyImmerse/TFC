using Assets.Tibia.DAO;
using Game.DAO;
using GameClient.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.ClassicNetwork
{
    public partial class GameServer
    {
        public void SendExtendedOpcode(byte opcode, string buffer)
        {
            if (EnableSendExtendedOpcode)
            {
                var msg = new OutputMessage(Connection);
                msg.AddU8((byte)ClientOpcodes.ClientExtendedOpcode);
                msg.AddU8(opcode);
                msg.AddString(buffer);
                Connection.Send(msg);
            }
            else
            {
                Debug.LogError(string.Format("Unable to send extended opcode {0}, extended opcodes are not enabled",
                    opcode));
            }
        }
        
        public void SendEnterGame()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientEnterGame);
            Connection.Send(msg);
        }

        public void SendLogout()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientLeaveGame);
            Connection.Send(msg);
        }

        public void SendPing()
        {
            if (FeatureManager.GetFeature(GameFeature.GameExtendedClientPing))
                SendExtendedOpcode(2, "");
            else
            {
                var msg = new OutputMessage(Connection);
                msg.AddU8((byte)ClientOpcodes.ClientPing);
                Connection.Send(msg);
            }
        }

        public void SendPingBack()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientPingBack);
            Connection.Send(msg);
        }

        public void SendAutoWalk(List<Direction> path)
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientAutoWalk);
            msg.AddU8((byte)path.Count);
            foreach (var dir in path)
            {
                byte @byte;
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (dir)
                {
                    case Direction.East:
                        @byte = 1;
                        break;
                    case Direction.NorthEast:
                        @byte = 2;
                        break;
                    case Direction.North:
                        @byte = 3;
                        break;
                    case Direction.NorthWest:
                        @byte = 4;
                        break;
                    case Direction.West:
                        @byte = 5;
                        break;
                    case Direction.SouthWest:
                        @byte = 6;
                        break;
                    case Direction.South:
                        @byte = 7;
                        break;
                    case Direction.SouthEast:
                        @byte = 8;
                        break;
                    default:
                        @byte = 0;
                        break;
                }
                msg.AddU8(@byte);
            }
            Connection.Send(msg);
        }

        public void SendWalkNorth()
        {
            Debug.Log("SendWalkNorth");
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientWalkNorth);
            Connection.Send(msg);
        }

        public void SendWalkEast()
        {
            Debug.Log("SendWalkEast");
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientWalkEast);
            Connection.Send(msg);
        }

        public void SendWalkSouth()
        {
            Debug.Log("SendWalkSouth");
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientWalkSouth);
            Connection.Send(msg);
        }

        public void SendWalkWest()
        {
            Debug.Log("SendWalkWest");
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientWalkWest);
            Connection.Send(msg);
        }

        public void SendStop()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientStop);
            Connection.Send(msg);
        }

        public void SendWalkNorthEast()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientWalkNorthEast);
            Connection.Send(msg);
        }

        public void SendWalkSouthEast()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientWalkSouthEast);
            Connection.Send(msg);
        }

        public void SendWalkSouthWest()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientWalkSouthWest);
            Connection.Send(msg);
        }

        public void SendWalkNorthWest()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientWalkNorthWest);
            Connection.Send(msg);
        }

        public void SendTurnNorth()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientTurnNorth);
            Connection.Send(msg);
        }

        public void SendTurnEast()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientTurnEast);
            Connection.Send(msg);
        }

        public void SendTurnSouth()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientTurnSouth);
            Connection.Send(msg);
        }

        public void SendTurnWest()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientTurnWest);
            Connection.Send(msg);
        }

        public void SendEquipItem(int itemId, int countOrSubType)
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientEquipItem);
            msg.AddU16((ushort)itemId);
            msg.AddU8((byte)countOrSubType);
            Connection.Send(msg);
        }

        public void SendMove(Vector3 fromPos, int thingId, int stackpos, Vector3 toPos, int count)
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientMove);
            AddPosition(msg, fromPos);
            msg.AddU16((ushort)thingId);
            msg.AddU8((byte)stackpos);
            AddPosition(msg, toPos);
            msg.AddU8((byte)count);
            Connection.Send(msg);
        }

        public void SendInspectNpcTrade(int itemId, int count)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientInspectNpcTrade);
            msg.AddU16((ushort)itemId);
            msg.AddU8((byte)count);
            Connection.Send(msg);
        }

        public void SendBuyItem(int itemId, int subType, int amount, bool ignoreCapacity, bool buyWithBackpack)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientBuyItem);
            msg.AddU16((ushort)itemId);
            msg.AddU8((byte)subType);
            msg.AddU8((byte)amount);
            msg.AddU8(ignoreCapacity ? (byte)0x01 : (byte)0x00);
            msg.AddU8(buyWithBackpack ? (byte)0x01 : (byte)0x00);
            Connection.Send(msg);
        }

        public void SendSellItem(int itemId, int subType, int amount, bool ignoreEquipped)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientSellItem);
            msg.AddU16((ushort)itemId);
            msg.AddU8((byte)subType);
            if (FeatureManager.GetFeature(GameFeature.GameDoubleShopSellAmount))
                msg.AddU16((ushort)amount);
            else
                msg.AddU8((byte)amount);
            msg.AddU8((byte)(ignoreEquipped ? 0x01 : 0x00));
            Connection.Send(msg);
        }

        public void SendCloseNpcTrade()
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientCloseNpcTrade);
            Connection.Send(msg);
        }

        public void SendRequestTrade(Vector3 pos, int thingId, int stackpos, uint creatureId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRequestTrade);
            AddPosition(msg, pos);
            msg.AddU16((ushort)thingId);
            msg.AddU8((byte)stackpos);
            msg.AddU32(creatureId);
            Connection.Send(msg);
        }

        public void SendInspectTrade(bool counterOffer, int index)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientInspectTrade);
            msg.AddU8((byte)(counterOffer ? 0x01 : 0x00));
            msg.AddU8((byte)index);
            Connection.Send(msg);
        }

        public void SendAcceptTrade()
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientAcceptTrade);
            Connection.Send(msg);
        }

        public void SendRejectTrade()
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRejectTrade);
            Connection.Send(msg);
        }

        public void SendUseItem(Vector3 position, int itemId, int stackpos, int index)
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientUseItem);
            AddPosition(msg, position);
            msg.AddU16((ushort)itemId);
            msg.AddU8((byte)stackpos);
            msg.AddU8((byte)index);
            Connection.Send(msg);
        }

        public void SendUseItemWith(Vector3 fromPos, int itemId, int fromStackPos, Vector3 toPos, int toThingId,
            int toStackPos)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientUseItemWith);
            AddPosition(msg, fromPos);
            msg.AddU16((ushort)itemId);
            msg.AddU8((byte)fromStackPos);
            AddPosition(msg, toPos);
            msg.AddU16((ushort)toThingId);
            msg.AddU8((byte)toStackPos);
            Connection.Send(msg);
        }

        public void SendUseOnCreature(Vector3 pos, int thingId, int stackpos, uint creatureId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientUseOnCreature);
            AddPosition(msg, pos);
            msg.AddU16((ushort)thingId);
            msg.AddU8((byte)stackpos);
            msg.AddU32(creatureId);
            Connection.Send(msg);
        }

        public void SendRotateItem(Vector3 pos, int thingId, int stackpos)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRotateItem);
            AddPosition(msg, pos);
            msg.AddU16((ushort)thingId);
            msg.AddU8((byte)stackpos);
            Connection.Send(msg);
        }

        public void SendCloseContainer(int containerId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientCloseContainer);
            msg.AddU8((byte)containerId);
            Connection.Send(msg);
        }

        public void SendUpContainer(int containerId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientUpContainer);
            msg.AddU8((byte)containerId);
            Connection.Send(msg);
        }

        public void SendEditText(uint id, string text)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientEditText);
            msg.AddU32(id);
            msg.AddString(text);
            Connection.Send(msg);
        }

        public void SendEditList(uint id, int doorId, string text)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientEditList);
            msg.AddU8((byte)doorId);
            msg.AddU32(id);
            msg.AddString(text);
            Connection.Send(msg);
        }

        public void SendLook(Vector3 position, int thingId, int stackpos)
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientLook);
            AddPosition(msg, position);
            msg.AddU16((ushort)thingId);
            msg.AddU8((byte)stackpos);
            Connection.Send(msg);
        }

        public void SendLookCreature(UInt32 creatureId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientLookCreature);
            msg.AddU32(creatureId);
            Connection.Send(msg);
        }

        public void SendTalk(MessageMode mode, int channelId, string receiver, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (message.Length > 255)
            {
                Debug.LogError("message too large");
                return;
            }

            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientTalk);
            msg.AddU8(MessageModeTranslator.translateMessageModeToServer(mode));

            switch (mode)
            {
                case MessageMode.PrivateTo:
                case MessageMode.GamemasterPrivateTo:
                case MessageMode.RVRAnswer:
                    msg.AddString(receiver);
                    break;
                case MessageMode.Channel:
                case MessageMode.ChannelHighlight:
                case MessageMode.ChannelManagement:
                case MessageMode.GamemasterChannel:
                    msg.AddU16((ushort)channelId);
                    break;
            }

            msg.AddString(message);
            Connection.Send(msg);
        }

        public void SendRequestChannels()
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRequestChannels);
            Connection.Send(msg);
        }

        public void SendJoinChannel(int channelId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientJoinChannel);
            msg.AddU16((ushort)channelId);
            Connection.Send(msg);
        }

        public void SendLeaveChannel(int channelId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientLeaveChannel);
            msg.AddU16((ushort)channelId);
            Connection.Send(msg);
        }

        public void SendOpenPrivateChannel(string receiver)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientOpenPrivateChannel);
            msg.AddString(receiver);
            Connection.Send(msg);
        }

        public void SendOpenRuleViolation(string reporter)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientOpenRuleViolation);
            msg.AddString(reporter);
            Connection.Send(msg);
        }

        public void SendCloseRuleViolation(string reporter)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientCloseRuleViolation);
            msg.AddString(reporter);
            Connection.Send(msg);
        }

        public void SendCancelRuleViolation()
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientCancelRuleViolation);
            Connection.Send(msg);
        }

        public void SendCloseNpcChannel()
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientCloseNpcChannel);
            Connection.Send(msg);
        }

        public void SendChangeFightModes(FightModes fightMode, ChaseModes chaseMode, bool safeFight, PVPModes pvpMode)
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientChangeFightModes);
            msg.AddU8((byte)fightMode);
            msg.AddU8((byte)chaseMode);
            msg.AddU8(safeFight ? (byte)0x01 : (byte)0x00);
            if (FeatureManager.GetFeature(GameFeature.GamePVPMode))
                msg.AddU8((byte)pvpMode);
            Connection.Send(msg);
        }

        public void SendAttack(uint creatureId, uint seq)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientAttack);
            msg.AddU32(creatureId);
            if (FeatureManager.GetFeature(GameFeature.GameAttackSeq))
                msg.AddU32(seq);
            Connection.Send(msg);
        }

        public void SendFollow(int creatureId, int seq)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientFollow);
            msg.AddU32((uint)creatureId);
            if (FeatureManager.GetFeature(GameFeature.GameAttackSeq))
                msg.AddU32((uint)seq);
            Connection.Send(msg);
        }

        public void SendInviteToParty(uint creatureId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientInviteToParty);
            msg.AddU32(creatureId);
            Connection.Send(msg);
        }

        public void SendJoinParty(uint creatureId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientJoinParty);
            msg.AddU32(creatureId);
            Connection.Send(msg);
        }

        public void SendRevokeInvitation(uint creatureId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRevokeInvitation);
            msg.AddU32(creatureId);
            Connection.Send(msg);
        }

        public void SendPassLeadership(uint creatureId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientPassLeadership);
            msg.AddU32(creatureId);
            Connection.Send(msg);
        }

        public void SendLeaveParty()
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientLeaveParty);
            Connection.Send(msg);
        }

        public void SendShareExperience(bool active)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientShareExperience);
            msg.AddU8((byte)(active ? 0x01 : 0x00));
            if (Config.ClientVersion < 910)
                msg.AddU8(0);
            Connection.Send(msg);
        }

        public void SendOpenOwnChannel()
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientOpenOwnChannel);
            Connection.Send(msg);
        }

        public void SendInviteToOwnChannel(string name)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientInviteToOwnChannel);
            msg.AddString(name);
            Connection.Send(msg);
        }

        public void SendExcludeFromOwnChannel(string name)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientExcludeFromOwnChannel);
            msg.AddString(name);
            Connection.Send(msg);
        }

        public void SendCancelAttackAndFollow()
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientCancelAttackAndFollow);
            Connection.Send(msg);
        }

        public void SendRefreshContainer(int containerId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRefreshContainer);
            msg.AddU8((byte)containerId);
            Connection.Send(msg);
        }

        public void SendRequestOutfit()
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRequestOutfit);
            Connection.Send(msg);
        }

        public void SendChangeOutfit(Outfit outfit)
        {
            var msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientChangeOutfit);
            if (FeatureManager.GetFeature(GameFeature.GameLooktypeU16))
                msg.AddU16((byte)outfit.LookTypeId);
            else
                msg.AddU8((byte)outfit.LookTypeId);
            msg.AddU8((byte)outfit.HeadColorByte);
            msg.AddU8((byte)outfit.BodyColorByte);
            msg.AddU8((byte)outfit.LegsColorByte);
            msg.AddU8((byte)outfit.FeetColorByte);
            if (FeatureManager.GetFeature(GameFeature.GamePlayerAddons))
                msg.AddU8((byte)outfit.Addons);
            if (FeatureManager.GetFeature(GameFeature.GamePlayerMounts))
                msg.AddU16((byte)outfit.Mount);
            Connection.Send(msg);
        }

        public void SendMountStatus(bool mount)
        {
            if (FeatureManager.GetFeature(GameFeature.GamePlayerMounts))
            {
                OutputMessage msg = new OutputMessage(Connection);
                msg.AddU8((byte)ClientOpcodes.ClientMount);
                msg.AddU8((byte)(mount ? 1 : 0));
                Connection.Send(msg);
            }
            else
            {
                Debug.LogError("ProtocolGame::sendMountStatus does not support the current protocol.");
            }
        }

        public void SendAddVip(string name)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientAddVip);
            msg.AddString(name);
            Connection.Send(msg);
        }

        public void SendRemoveVip(uint playerId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRemoveVip);
            msg.AddU32(playerId);
            Connection.Send(msg);
        }

        public void SendEditVip(uint playerId, string description, int iconId, bool notifyLogin)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientEditVip);
            msg.AddU32(playerId);
            msg.AddString(description);
            msg.AddU32((uint)iconId);
            msg.AddU8((byte)(notifyLogin ? 1 : 0));
            Connection.Send(msg);
        }

        public void SendBugReport(string comment)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientBugReport);
            msg.AddString(comment);
            Connection.Send(msg);
        }

        public void SendRuleViolation(string target, int reason, int action, string comment, string statement,
            int statementId, bool ipBanishment)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRuleViolation);
            msg.AddString(target);
            msg.AddU8((byte)reason);
            msg.AddU8((byte)action);
            msg.AddString(comment);
            msg.AddString(statement);
            msg.AddU16((ushort)statementId);
            msg.AddU8((byte)(ipBanishment ? 1 : 0));
            Connection.Send(msg);
        }

        public void SendDebugReport(string a, string b, string c, string d)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientDebugReport);
            msg.AddString(a);
            msg.AddString(b);
            msg.AddString(c);
            msg.AddString(d);
            Connection.Send(msg);
        }

        public void SendRequestQuestLog()
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRequestQuestLog);
            Connection.Send(msg);
        }

        public void SendRequestQuestLine(int questId)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRequestQuestLine);
            msg.AddU16((ushort)questId);
            Connection.Send(msg);
        }

        public void SendNewNewRuleViolation(int reason, int action, string characterName, string comment, string translation)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientNewRuleViolation);
            msg.AddU8((byte)reason);
            msg.AddU8((byte)action);
            msg.AddString(characterName);
            msg.AddString(comment);
            msg.AddString(translation);
            Connection.Send(msg);
        }

        public void SendRequestItemInfo(int itemId, int subType, int index)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRequestItemInfo);
            msg.AddU8((byte)subType);
            msg.AddU16((ushort)itemId);
            msg.AddU8((byte)index);
            Connection.Send(msg);
        }

        public void SendAnswerModalDialog(int dialog, int button, int choice)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientAnswerModalDialog);
            msg.AddU32((uint)dialog);
            msg.AddU8((byte)button);
            msg.AddU8((byte)choice);
            Connection.Send(msg);
        }

        public void SendBrowseField(Vector3 position)
        {
            if (!FeatureManager.GetFeature(GameFeature.GameBrowseField))
                return;

            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientBrowseField);
            AddPosition(msg, position);
            Connection.Send(msg);
        }

        public void SendSeekInContainer(int cid, int index)
        {
            if (!FeatureManager.GetFeature(GameFeature.GameContainerPagination))
                return;

            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientSeekInContainer);
            msg.AddU8((byte)cid);
            msg.AddU16((ushort)index);
            Connection.Send(msg);
        }

        public void SendBuyStoreOffer(int offerId, int productType, string name)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientBuyStoreOffer);
            msg.AddU32((uint)offerId);
            msg.AddU8((byte)productType);

            if (productType == (int)StoreProductTypes.ProductTypeNameChange)
                msg.AddString(name);

            Connection.Send(msg);
        }

        public void SendRequestTransactionHistory(int page, int entriesPerPage)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRequestTransactionHistory);
            if (Config.ClientVersion <= 1096)
            {
                msg.AddU16((ushort)page);
                msg.AddU32((uint)entriesPerPage);
            }
            else
            {
                msg.AddU32((uint)page);
                msg.AddU8((byte)entriesPerPage);
            }

            Connection.Send(msg);
        }

        public void SendRequestStoreOffers(string categoryName, int serviceType)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientRequestStoreOffers);

            if (FeatureManager.GetFeature(GameFeature.GameIngameStoreServiceType))
            {
                msg.AddU8((byte)serviceType);
            }
            msg.AddString(categoryName);

            Connection.Send(msg);
        }

        public void SendOpenStore(int serviceType, string category)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientOpenStore);

            if (FeatureManager.GetFeature(GameFeature.GameIngameStoreServiceType))
            {
                msg.AddU8((byte)serviceType);
                msg.AddString(category);
            }

            Connection.Send(msg);
        }

        public void SendTransferCoins(string recipient, int amount)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientTransferCoins);
            msg.AddString(recipient);
            msg.AddU16((ushort)amount);
            Connection.Send(msg);
        }

        public void SendOpenTransactionHistory(int entriesPerPage)
        {
            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientOpenTransactionHistory);
            msg.AddU8((byte)entriesPerPage);

            Connection.Send(msg);
        }

        public void SendChangeMapAwareRange(int xrange, int yrange)
        {
            if (!FeatureManager.GetFeature(GameFeature.GameChangeMapAwareRange))
                return;

            OutputMessage msg = new OutputMessage(Connection);
            msg.AddU8((byte)ClientOpcodes.ClientChangeMapAwareRange);
            msg.AddU8((byte)xrange);
            msg.AddU8((byte)yrange);
            Connection.Send(msg);
        }

        public void AddPosition(OutputMessage msg, Vector3 position)
        {
            msg.AddU16((ushort)position.x);
            msg.AddU16((ushort)position.y);
            msg.AddU8((byte)position.z);
        }
    }
}
