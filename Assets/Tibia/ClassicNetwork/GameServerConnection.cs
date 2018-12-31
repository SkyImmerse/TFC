using Assets.Tibia.ClassicNetwork.Account;
using Assets.Tibia.DAO;
using Assets.Tibia.DAO.Extensions;
using Assets.Tibia.Game_Systems;
using Assets.Tibia.UI.GameInterface;
using Game.DAO;
using Game.Graphics;
using GameClient;
using GameClient.Network;
using GameClient.Network.Protocol;
using SkyImmerseEngine.Graphics;
using SkyImmerseEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace Assets.Tibia.ClassicNetwork
{
    public partial class GameServer
    {
        public static readonly GameServer Instance = new GameServer();

        public TCPClient Connection;
        public TCPCmdHandler Protocol;
        private static bool EnableSendExtendedOpcode;
        public string CurrentCharacterName = null;

        #region Events 
        public static event Action EnterGame;
        public static event Action ConnectionSuccussfull;
        #endregion

        public static Dictionary<GameServerOpcodes, bool> debugOpcodeTable = new Dictionary<GameServerOpcodes, bool>()
        {
            // WORKING
            {GameServerOpcodes.GameServerLoginOrPendingState, false},
            // NOT TESTED
            {GameServerOpcodes.GameServerGMActions, true},
            // WORKING
            {GameServerOpcodes.GameServerEnterGame, false},
            // NOT TESTED
            {GameServerOpcodes.GameServerUpdateNeeded, true},
            // NOT TESTED
            {GameServerOpcodes.GameServerLoginError, true},
            // NOT TESTED
            {GameServerOpcodes.GameServerLoginAdvice, true},
            // NOT TESTED
            {GameServerOpcodes.GameServerLoginWait, true},
            // NOT TESTED
            {GameServerOpcodes.GameServerLoginSuccess, true},
            // NOT TESTED
            {GameServerOpcodes.GameServerLoginToken, true},
            // NOT TESTED
            {GameServerOpcodes.GameServerStoreButtonIndicators, true},
            // WORKING
            {GameServerOpcodes.GameServerPingBack, false},
            // WORKING
            {GameServerOpcodes.GameServerPing, false},
            // WORKING
            {GameServerOpcodes.GameServerChallenge, false},
            // TODO
            {GameServerOpcodes.GameServerDeath, true},

            // WORKING
            {GameServerOpcodes.GameServerFirstGameOpcode, false},


            // NOT TESTED
            {GameServerOpcodes.GameServerChangeMapAwareRange, false},

            // WORKING
            {GameServerOpcodes.GameServerFullMap, false},
            // WORKING
            {GameServerOpcodes.GameServerMapTopRow, false},
            // WORKING
            {GameServerOpcodes.GameServerMapRightRow, false},
            // WORKING
            {GameServerOpcodes.GameServerMapBottomRow, false},
            // WORKING
            {GameServerOpcodes.GameServerMapLeftRow, false},
            // WORKING
            {GameServerOpcodes.GameServerUpdateTile, false},
            // WORKING
            {GameServerOpcodes.GameServerCreateOnMap, false},
            // WORKING
            {GameServerOpcodes.GameServerChangeOnMap, false},
            // WORKING
            {GameServerOpcodes.GameServerDeleteOnMap, false},
            // WIP
            {GameServerOpcodes.GameServerMoveCreature, false},
            // TODO
            {GameServerOpcodes.GameServerOpenContainer, true},
            // TODO
            {GameServerOpcodes.GameServerCloseContainer, true},
             // TODO
            {GameServerOpcodes.GameServerCreateContainer, true},
             // TODO
            {GameServerOpcodes.GameServerChangeInContainer, true},
             // TODO
            {GameServerOpcodes.GameServerDeleteInContainer, true},
             // TODO
            {GameServerOpcodes.GameServerSetInventory, true},
             // TODO
            {GameServerOpcodes.GameServerDeleteInventory, true},
             // TODO
            {GameServerOpcodes.GameServerOpenNpcTrade, true},
             // TODO
            {GameServerOpcodes.GameServerPlayerGoods, true},
             // TODO
            {GameServerOpcodes.GameServerCloseNpcTrade, true},
             // TODO
            {GameServerOpcodes.GameServerOwnTrade, true},
             // TODO
            {GameServerOpcodes.GameServerCounterTrade, true},
             // TODO
            {GameServerOpcodes.GameServerCloseTrade, true},
             // WORKING
            {GameServerOpcodes.GameServerAmbient, false},
             // WIP
            {GameServerOpcodes.GameServerGraphicalEffect, true},
             // TODO
            {GameServerOpcodes.GameServerTextEffect, true},
            // WIP
            {GameServerOpcodes.GameServerMissleEffect, true},
             // WORKING
            {GameServerOpcodes.GameServerMarkCreature, false},
             // WORKING
            {GameServerOpcodes.GameServerTrappers, false},
             // WORKING
            {GameServerOpcodes.GameServerCreatureHealth, false},
              // WORKING
            {GameServerOpcodes.GameServerCreatureLight, false},
             // WORKING
            {GameServerOpcodes.GameServerCreatureOutfit, false},
             // WORKING
            {GameServerOpcodes.GameServerCreatureSpeed, false},
              // WORKING
            {GameServerOpcodes.GameServerCreatureSkull, false},
              // WORKING
            {GameServerOpcodes.GameServerCreatureParty, false},
             // WORKING
            {GameServerOpcodes.GameServerCreatureUnpass, false},
             // WORKING
            {GameServerOpcodes.GameServerCreatureMarks, false},
             // WORKING
            {GameServerOpcodes.GameServerPlayerHelpers, false},
             // WORKING
            {GameServerOpcodes.GameServerCreatureType, false},
             // TODO
            {GameServerOpcodes.GameServerEditText, true},
            // TODO
            {GameServerOpcodes.GameServerEditList, true},
            // TODO
            {GameServerOpcodes.GameServerBlessings, true},
            {GameServerOpcodes.GameServerPreset, true},
            // NOT WORKING
            {GameServerOpcodes.GameServerPremiumTrigger, true},
             // WORKING
            {GameServerOpcodes.GameServerPlayerDataBasic, false},
            // WORKING
            {GameServerOpcodes.GameServerPlayerData, true},
            // WORKING
            {GameServerOpcodes.GameServerPlayerSkills, true},
            // WORKING
            {GameServerOpcodes.GameServerPlayerState, true},
            // WORKING
            {GameServerOpcodes.GameServerClearTarget, true},
            // WORKING
            {GameServerOpcodes.GameServerPlayerModes, true},
            // WORKING
            {GameServerOpcodes.GameServerSpellDelay, true},
            // WORKING
            {GameServerOpcodes.GameServerSpellGroupDelay, true},
            // TODO
            {GameServerOpcodes.GameServerMultiUseDelay, true},
            // TODO
            {GameServerOpcodes.GameServerSetStoreDeepLink, true},
            // WORKING
            {GameServerOpcodes.GameServerTalk, true},
            // WORKING
            {GameServerOpcodes.GameServerChannels, false},
            // WORKING
            {GameServerOpcodes.GameServerOpenChannel, false},
            // WORKING
            {GameServerOpcodes.GameServerOpenPrivateChannel, false},
            // WORKING
            {GameServerOpcodes.GameServerRuleViolationChannel, true},
            // NOT WORKING
            { GameServerOpcodes.GameServerRuleViolationRemove, true},
            // NOT WORKING
            {GameServerOpcodes.GameServerRuleViolationCancel, true},
            // NOT WORKING
            { GameServerOpcodes.GameServerRuleViolationLock, true},
            // WORKING
            {GameServerOpcodes.GameServerOpenOwnChannel, true},
            // WORKING
            {GameServerOpcodes.GameServerCloseChannel, true},
            // WORKING
            {GameServerOpcodes.GameServerTextMessage, true},
            // WORKING
            {GameServerOpcodes.GameServerCancelWalk, true},
            // WORKING
            {GameServerOpcodes.GameServerWalkWait, true},
            // WORKING
            {GameServerOpcodes.GameServerUnjustifiedStats, true},
            // WORKING
            {GameServerOpcodes.GameServerPvpSituations, true},
            // WORKING
            {GameServerOpcodes.GameServerFloorChangeUp, true},
            // WORKING
            {GameServerOpcodes.GameServerFloorChangeDown, true},
            // WORKING
            {GameServerOpcodes.GameServerChooseOutfit, true},
            // WORKING
            {GameServerOpcodes.GameServerVipAdd, true},
            // WORKING
            {GameServerOpcodes.GameServerVipState, true},
            // WORKING
            {GameServerOpcodes.GameServerVipLogout, true},
            // WORKING
            {GameServerOpcodes.GameServerTutorialHint, true},
            // WORKING
            {GameServerOpcodes.GameServerAutomapFlag, true},
            // WORKING
            {GameServerOpcodes.GameServerCoinBalance, true},
            // WORKING
            {GameServerOpcodes.GameServerStoreError, true},
            // WORKING
            {GameServerOpcodes.GameServerRequestPurchaseData, true},
            // WORKING
            {GameServerOpcodes.GameServerQuestLog, true},
            // WORKING
            {GameServerOpcodes.GameServerQuestLine, true},
            {GameServerOpcodes.GameServerCoinBalanceUpdating, true},
            {GameServerOpcodes.GameServerChannelEvent, true},
            {GameServerOpcodes.GameServerItemInfo, true},
            {GameServerOpcodes.GameServerPlayerInventory, true},
            {GameServerOpcodes.GameServerMarketEnter, true},
            {GameServerOpcodes.GameServerMarketLeave, true},
            {GameServerOpcodes.GameServerMarketDetail, true},
            {GameServerOpcodes.GameServerMarketBrowse, true},
            {GameServerOpcodes.GameServerModalDialog, true},
            {GameServerOpcodes.GameServerStore, true},
            {GameServerOpcodes.GameServerStoreOffers, true},
            {GameServerOpcodes.GameServerStoreTransactionHistory, true},
            {GameServerOpcodes.GameServerStoreCompletePurchase, true},
        };

        public GameServer()
        {

            Connection = new TCPClient();
            Connection.OnConnect += GameServer_OnConnect;
            Protocol = new TCPCmdHandler();
            Connection.SocketEvent += Protocol.ProcessCmd;

            Init();
        }

        public bool Connect(string host, int port)
        {
            Connection.IsRun = true;

            return Connection.Connect(host, port);
        }

        private void GameServer_OnConnect()
        {
            ConnectionSuccussfull?.Invoke();
        }

        public void Init()
        {
            Protocol.IsSkipWorker += (opcode, msg) =>
            {
                if (!string.IsNullOrEmpty(CurrentCharacterName))
                {
                    if (opcode != (int)GameServerOpcodes.GameServerChallenge)
                    {
                        return true;
                    }
                }
                #if UNITY_EDITOR
                if (debugOpcodeTable.ContainsKey((GameServerOpcodes)opcode) && debugOpcodeTable[(GameServerOpcodes)opcode])
                    Debug.Log((GameServerOpcodes)opcode);
                #endif
                return false;
            };

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerLoginOrPendingState, (TCPClient client, InputMessage msg) => {
                if (FeatureManager.GetFeature(GameFeature.GameLoginPending))
                {
                    ParsePendingGame(msg);
                }
                else
                {
                    ParseLogin(msg);
                }
            });

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPingBack, (TCPClient client, InputMessage msg) => ParsePingBack(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerGMActions, (TCPClient client, InputMessage msg) => ParseGmActions(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerUpdateNeeded, (TCPClient client, InputMessage msg) => ParseUpdateNeeded(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerLoginError, (TCPClient client, InputMessage msg) => ParseLoginError(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerLoginAdvice, (TCPClient client, InputMessage msg) => ParseLoginAdvice(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerLoginWait, (TCPClient client, InputMessage msg) => ParseLoginWait(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerLoginToken, (TCPClient client, InputMessage msg) => ParseLoginToken(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPing, (TCPClient client, InputMessage msg) => ParsePing(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerChallenge, (TCPClient client, InputMessage msg) => ParseChallenge(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerDeath, (TCPClient client, InputMessage msg) => ParseDeath(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerFullMap, (TCPClient client, InputMessage msg) => ParseMapDescription(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerMapTopRow, (TCPClient client, InputMessage msg) => ParseMapMoveNorth(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerMapRightRow, (TCPClient client, InputMessage msg) => ParseMapMoveEast(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerMapBottomRow, (TCPClient client, InputMessage msg) => ParseMapMoveSouth(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerMapLeftRow, (TCPClient client, InputMessage msg) => ParseMapMoveWest(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerUpdateTile, (TCPClient client, InputMessage msg) => ParseUpdateTile(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCreateOnMap, (TCPClient client, InputMessage msg) => ParseTileAddThing(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerChangeOnMap, (TCPClient client, InputMessage msg) => ParseTileTransformThing(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerDeleteOnMap, (TCPClient client, InputMessage msg) => ParseTileRemoveThing(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerMoveCreature, (TCPClient client, InputMessage msg) => ParseMoveCreature(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerOpenContainer, (TCPClient client, InputMessage msg) => ParseOpenContainer(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCloseContainer, (TCPClient client, InputMessage msg) => ParseCloseContainer(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCreateContainer, (TCPClient client, InputMessage msg) => ParseContainerAddItem(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerChangeInContainer, (TCPClient client, InputMessage msg) => ParseContainerUpdateItem(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerDeleteInContainer, (TCPClient client, InputMessage msg) => ParseContainerRemoveItem(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerSetInventory, (TCPClient client, InputMessage msg) => ParseAddInventoryItem(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerDeleteInventory, (TCPClient client, InputMessage msg) => ParseRemoveInventoryItem(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerOpenNpcTrade, (TCPClient client, InputMessage msg) => ParseOpenNpcTrade(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPlayerGoods, (TCPClient client, InputMessage msg) => ParsePlayerGoods(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCloseNpcTrade, (TCPClient client, InputMessage msg) => ParseCloseNpcTrade(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerOwnTrade, (TCPClient client, InputMessage msg) => ParseOwnTrade(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCounterTrade, (TCPClient client, InputMessage msg) => ParseCounterTrade(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCloseTrade, (TCPClient client, InputMessage msg) => ParseCloseTrade(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerAmbient, Callback: (TCPClient client, InputMessage msg) => ParseWorldLight(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerGraphicalEffect, (TCPClient client, InputMessage msg) => ParseMagicEffect(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerTextEffect, (TCPClient client, InputMessage msg) => ParseAnimatedText(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerMissleEffect, (TCPClient client, InputMessage msg) => ParseDistanceMissile(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerMarkCreature, (TCPClient client, InputMessage msg) => ParseCreatureMark(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerTrappers, (TCPClient client, InputMessage msg) => ParseTrappers(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCreatureHealth, (TCPClient client, InputMessage msg) => ParseCreatureHealth(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCreatureLight, (TCPClient client, InputMessage msg) => ParseCreatureLight(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCreatureOutfit, (TCPClient client, InputMessage msg) => ParseCreatureOutfit(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCreatureSpeed, (TCPClient client, InputMessage msg) => ParseCreatureSpeed(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCreatureSkull, (TCPClient client, InputMessage msg) => ParseCreatureSkulls(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCreatureParty, (TCPClient client, InputMessage msg) => ParseCreatureShields(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCreatureUnpass, (TCPClient client, InputMessage msg) => ParseCreatureUnpass(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerEditText, (TCPClient client, InputMessage msg) => ParseEditText(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerEditList, (TCPClient client, InputMessage msg) => ParseEditList(msg));

            // PROTOCOL>=1038
            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPremiumTrigger, (TCPClient client, InputMessage msg) => ParsePremiumTrigger(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPlayerData, (TCPClient client, InputMessage msg) => ParsePlayerStats(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPlayerSkills, (TCPClient client, InputMessage msg) => ParsePlayerSkills(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPlayerState, (TCPClient client, InputMessage msg) => ParsePlayerState(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerClearTarget, (TCPClient client, InputMessage msg) => ParsePlayerCancelAttack(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPlayerModes, (TCPClient client, InputMessage msg) => ParsePlayerModes(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerTalk, (TCPClient client, InputMessage msg) => ParseTalk(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerChannels, (TCPClient client, InputMessage msg) => ParseChannelList(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerOpenChannel, (TCPClient client, InputMessage msg) => ParseOpenChannel(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerOpenPrivateChannel, (TCPClient client, InputMessage msg) => ParseOpenPrivateChannel(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerRuleViolationChannel, (TCPClient client, InputMessage msg) => ParseRuleViolationChannel(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerRuleViolationRemove, (TCPClient client, InputMessage msg) => ParseRuleViolationRemove(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerRuleViolationCancel, (TCPClient client, InputMessage msg) => ParseRuleViolationCancel(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerRuleViolationLock, (TCPClient client, InputMessage msg) => ParseRuleViolationLock(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerOpenOwnChannel, (TCPClient client, InputMessage msg) => ParseOpenOwnPrivateChannel(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCloseChannel, (TCPClient client, InputMessage msg) => ParseCloseChannel(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerTextMessage, (TCPClient client, InputMessage msg) => ParseTextMessage(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCancelWalk, (TCPClient client, InputMessage msg) => ParseCancelWalk(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerWalkWait, (TCPClient client, InputMessage msg) => ParseWalkWait(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerFloorChangeUp, (TCPClient client, InputMessage msg) => ParseFloorChangeUp(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerFloorChangeDown, (TCPClient client, InputMessage msg) => ParseFloorChangeDown(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerChooseOutfit, (TCPClient client, InputMessage msg) => ParseOpenOutfitWindow(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerVipAdd, (TCPClient client, InputMessage msg) => ParseVipAdd(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerVipState, (TCPClient client, InputMessage msg) => ParseVipState(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerVipLogout, (TCPClient client, InputMessage msg) => ParseVipLogout(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerTutorialHint, (TCPClient client, InputMessage msg) => ParseTutorialHint(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerAutomapFlag, (TCPClient client, InputMessage msg) => ParseAutomapFlag(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerQuestLog, (TCPClient client, InputMessage msg) => ParseQuestLog(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerQuestLine, (TCPClient client, InputMessage msg) => ParseQuestLine(msg));

            // PROTOCOL>=870
            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerSpellDelay, (TCPClient client, InputMessage msg) => ParseSpellCooldown(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerSpellGroupDelay, (TCPClient client, InputMessage msg) => ParseSpellGroupCooldown(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerMultiUseDelay, (TCPClient client, InputMessage msg) => ParseMultiUseCooldown(msg));

            // PROTOCOL>=910
            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerChannelEvent, (TCPClient client, InputMessage msg) => ParseChannelEvent(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerItemInfo, (TCPClient client, InputMessage msg) => ParseItemInfo(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPlayerInventory, (TCPClient client, InputMessage msg) => ParsePlayerInventory(msg));

            // PROTOCOL>=950
            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPlayerDataBasic, (TCPClient client, InputMessage msg) => ParsePlayerInfo(msg));

            // PROTOCOL>=970
            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerModalDialog, (TCPClient client, InputMessage msg) => ParseModalDialog(msg));

            // PROTOCOL>=980
            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerLoginSuccess, (TCPClient client, InputMessage msg) => ParseLogin(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerEnterGame, (TCPClient client, InputMessage msg) => ParseEnterGame(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPlayerHelpers, (TCPClient client, InputMessage msg) => ParsePlayerHelpers(msg));

            // PROTOCOL>=1000
            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCreatureMarks, (TCPClient client, InputMessage msg) => ParseCreaturesMark(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCreatureType, (TCPClient client, InputMessage msg) => ParseCreatureType(msg));

            // PROTOCOL>=1055
            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerBlessings, (TCPClient client, InputMessage msg) => ParseBlessings(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerUnjustifiedStats, (TCPClient client, InputMessage msg) => ParseUnjustifiedStats(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPvpSituations, (TCPClient client, InputMessage msg) => ParsePvpSituations(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerPreset, (TCPClient client, InputMessage msg) => ParsePreset(msg));

            // PROTOCOL>=1080
            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCoinBalanceUpdating, (TCPClient client, InputMessage msg) => ParseCoinBalanceUpdating(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerCoinBalance, (TCPClient client, InputMessage msg) => ParseCoinBalance(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerRequestPurchaseData, (TCPClient client, InputMessage msg) => ParseRequestPurchaseData(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerStoreCompletePurchase, (TCPClient client, InputMessage msg) => ParseCompleteStorePurchase(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerStoreOffers, (TCPClient client, InputMessage msg) => ParseStoreOffers(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerStoreTransactionHistory, (TCPClient client, InputMessage msg) => ParseStoreTransactionHistory(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerStoreError, (TCPClient client, InputMessage msg) => ParseStoreError(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerStore, (TCPClient client, InputMessage msg) => ParseStore(msg));

            // PROTOCOL>=1097
            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerStoreButtonIndicators, (TCPClient client, InputMessage msg) => ParseStoreButtonIndicators(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerSetStoreDeepLink, (TCPClient client, InputMessage msg) => ParseSetStoreDeepLink(msg));

            // otclient ONLY
            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerExtendedOpcode, (TCPClient client, InputMessage msg) => ParseExtendedOpcode(msg));

            Protocol.RegisterCmd((int)GameServerOpcodes.GameServerChangeMapAwareRange, (TCPClient client, InputMessage msg) => ParseChangeMapAwareRange(msg));
        }

        public static void SendCharacterPacket(string characterName, uint challengeTimestamp, byte challengeRandom)
        {

            var msg = new OutputMessage(GameServer.Instance.Connection);

            msg.AddU8((byte)ClientOpcodes.ClientPendingGame);
            msg.AddU16((ushort)Config.ClientVersion);
            msg.AddU16((ushort)Config.ProtocolVersion);

            if (FeatureManager.GetFeature(GameFeature.GameClientVersion))
                msg.AddU32((uint)Config.ClientVersion);

            if (FeatureManager.GetFeature(GameFeature.GameContentRevision))
            {
                msg.AddU16(0);
            }

            if (FeatureManager.GetFeature(GameFeature.GamePreviewState))
                msg.AddU8(0);

            int offset = msg.GetMessageSize();
            // first RSA byte must be 0
            msg.AddU8(0);

            if (FeatureManager.GetFeature(GameFeature.GameLoginPacketEncryption))
            {
                // xtea key
                msg.AddU32((uint)GameServer.Instance.Connection.XteaKey[0]);
                msg.AddU32((uint)GameServer.Instance.Connection.XteaKey[1]);
                msg.AddU32((uint)GameServer.Instance.Connection.XteaKey[2]);
                msg.AddU32((uint)GameServer.Instance.Connection.XteaKey[3]);

                msg.AddU8(0); // is gm set?
            }

            if (FeatureManager.GetFeature(GameFeature.GameSessionKey))
            {
                msg.AddString(LoginServer.Instance.SessionKey);
                msg.AddString(characterName);
            }
            else
            {
                if (FeatureManager.GetFeature(GameFeature.GameAccountNames))
                    msg.AddString(LoginServer.Instance.CurrentLogin);
                else
                    msg.AddU32(0);

                msg.AddString(characterName);
                msg.AddString(LoginServer.Instance.CurrentPassword);

                if (FeatureManager.GetFeature(GameFeature.GameAuthenticator))
                    msg.AddString(""); //_mAuthenticatorToken
            }

            if (FeatureManager.GetFeature(GameFeature.GameChallengeOnLogin))
            {
                msg.AddU32(challengeTimestamp);
                msg.AddU8(challengeRandom);
            }

            var extended = "";
            if (!string.IsNullOrEmpty(extended))
                msg.AddString(extended);

            // complete the bytes for rsa encryption with zeros
            var paddingBytes = Rsa.RsaGetSize() - (msg.GetMessageSize() - offset);
            msg.AddPaddingBytes(paddingBytes);

            // encrypt with RSA
            if (FeatureManager.GetFeature(GameFeature.GameLoginPacketEncryption))
                msg.EncryptRsa();

            if (FeatureManager.GetFeature(GameFeature.GameProtocolChecksum))
                GameServer.Instance.Connection.ChecksumEnabled = true;

            GameServer.Instance.Connection.Send(msg);

            if (FeatureManager.GetFeature(GameFeature.GameLoginPacketEncryption))
                GameServer.Instance.Connection.XteaEncryptionEnabled = true;
        }

        private static void ParseLoginError(InputMessage msg)
        {
            var error = msg.GetString();
            Debug.Log(error);
        }

        public static void ParseLogin(InputMessage msg)
        {
            uint playerId = msg.GetU32();
            int serverBeat = msg.GetU16();

            if (FeatureManager.GetFeature(GameFeature.GameNewSpeedLaw))
            {
                double speedA = msg.GetDouble();
                double speedB = msg.GetDouble();
                double speedC = msg.GetDouble();
                LocalPlayer.Current.SetSpeedFormula(speedA, speedB, speedC);
            }
            bool canReportBugs = msg.GetU8() != 0;

            if (Config.ClientVersion >= 1054)
                msg.GetU8(); // can change pvp frame option

            if (Config.ClientVersion >= 1058)
            {
                int expertModeEnabled = msg.GetU8();
                //GameState.SetExpertPvpMode(expertModeEnabled != 0);
            }

            if (FeatureManager.GetFeature(GameFeature.GameIngameStore))
            {
                // URL to ingame store images
                msg.GetString();

                // premium coin package size
                // e.g you can only buy packs of 25, 50, 75, .. coins in the market
                msg.GetU16();
            }

            LocalPlayer.Current.DatId = (int)(playerId);
            PingSystem.ServerBeat = serverBeat;
            //LocalPlayer.Current.SetCanReportBugs(canReportBugs);

        }

        public static void ParsePendingGame(InputMessage msg)
        {
            //set player to pending game state
            //GameState.ProcessPendingGame();
            GameServer.Instance.SendEnterGame();
        }

        public static void ParseEnterGame(InputMessage msg)
        {
            //set player to entered game state
            PingSystem.Start();
            EnterGame?.Invoke();
        }

        public static void ParseStoreButtonIndicators(InputMessage msg)
        {
            msg.GetU8(); // unknown
            msg.GetU8(); // unknown
        }
        public static void ParseSetStoreDeepLink(InputMessage msg)
        {
            int currentlyFeaturedServiceType = msg.GetU8();
        }
        public static void ParseBlessings(InputMessage msg)
        {
            UInt16 blessings = msg.GetU16();
            //LocalPlayer.Current.SetBlessings(blessings);
        }
        public static void ParsePreset(InputMessage msg)
        {
            UInt32 preset = msg.GetU32();
        }
        public static void ParseRequestPurchaseData(InputMessage msg)
        {
            uint transactionId = msg.GetU32();
            int productType = msg.GetU8();
        }
        public static void ParseStore(InputMessage msg)
        {
            ParseCoinBalance(msg);

            // Parse all categories
            int count = msg.GetU16();
            for (int i = 0; i < count; i++)
            {
                string category = msg.GetString();
                string description = msg.GetString();

                int highlightState = 0;
                if (FeatureManager.GetFeature(GameFeature.GameIngameStoreHighlights))
                    highlightState = msg.GetU8();

                List<string> icons = new List<string>();
                int iconCount = msg.GetU8();
                for (int z = 0; z < iconCount; z++)
                {
                    string icon = msg.GetString();
                    icons.Add(icon);
                }

                // If this is a valid category name then
                // the category we just parsed is a child of that
                string parentCategory = msg.GetString();
            }
        }
        public static void ParseCoinBalance(InputMessage msg)
        {
            bool update = msg.GetU8() == 1;
            int coins = -1;
            int transferableCoins = -1;
            if (update)
            {
                // amount of coins that can be used to buy prodcuts
                // in the ingame store
                coins = (int)msg.GetU32();

                // amount of coins that can be sold in market
                // or be transfered to another player
                transferableCoins = (int)msg.GetU32();
            }
        }
        public static void ParseCoinBalanceUpdating(InputMessage msg)
        {
            // coin balance can be updating and might not be accurate
            bool isUpdating = msg.GetU8() == 1;
        }
        public static void ParseCompleteStorePurchase(InputMessage msg)
        {
            // not used
            msg.GetU8();

            string message = msg.GetString();
            uint coins = msg.GetU32();
            uint transferableCoins = msg.GetU32();

            Debug.Log(String.Format("Purchase Complete: {0}", message));
        }
        public static void ParseStoreTransactionHistory(InputMessage msg)
        {
            uint currentPage;
            if (Config.ClientVersion <= 1096)
            {
                currentPage = msg.GetU16();
                bool hasNextPage = msg.GetU8() == 1;
            }
            else
            {
                currentPage = msg.GetU32();
                uint pageCount = msg.GetU32();
            }

            int entries = msg.GetU8();
            for (int i = 0; i < entries; i++)
            {
                int time = msg.GetU16();
                int productType = msg.GetU8();
                uint coinChange = msg.GetU32();
                string productName = msg.GetString();
                Debug.LogError(String.Format("Time %i, type %i, change %i, product name {0}", time, productType, coinChange, productName));
            }
        }
        public static void ParseStoreOffers(InputMessage msg)
        {
            string categoryName = msg.GetString();

            int offers = msg.GetU16();
            for (int i = 0; i < offers; i++)
            {
                uint offerId = msg.GetU32();
                string offerName = msg.GetString();
                string offerDescription = msg.GetString();

                uint price = msg.GetU32();
                int highlightState = msg.GetU8();
                if (highlightState == 2 && FeatureManager.GetFeature(GameFeature.GameIngameStoreHighlights) && Config.ClientVersion >= 1097)
                {
                    uint saleValidUntilTimestamp = msg.GetU32();
                    uint basePrice = msg.GetU32();
                }

                int disabledState = msg.GetU8();
                string disabledReason = "";
                if (FeatureManager.GetFeature(GameFeature.GameIngameStoreHighlights) && disabledState == 1)
                {
                    disabledReason = msg.GetString();
                }

                int icons = msg.GetU8();
                for (int j = 0; j < icons; j++)
                {
                    string icon = msg.GetString();
                }

                int subOffers = msg.GetU16();
                for (int j = 0; j < subOffers; j++)
                {
                    string name = msg.GetString();
                    string description = msg.GetString();

                    int subIcons = msg.GetU8();
                    for (int k = 0; k < subIcons; k++)
                    {
                        string icon = msg.GetString();
                    }
                    string serviceType = msg.GetString();
                }
            }
        }
        public static void ParseStoreError(InputMessage msg)
        {
            int errorType = msg.GetU8();
            string message = msg.GetString();
            Debug.LogError(String.Format("Store Error: {0} [%i]", message, errorType));
        }
        public static void ParseUnjustifiedStats(InputMessage msg)
        {
            UnjustifiedPoints unjustifiedPoints = new UnjustifiedPoints();
            unjustifiedPoints.KillsDay = msg.GetU8();
            unjustifiedPoints.KillsDayRemaining = msg.GetU8();
            unjustifiedPoints.KillsWeek = msg.GetU8();
            unjustifiedPoints.KillsWeekRemaining = msg.GetU8();
            unjustifiedPoints.KillsMonth = msg.GetU8();
            unjustifiedPoints.KillsMonthRemaining = msg.GetU8();
            unjustifiedPoints.SkullTime = msg.GetU8();

            LocalPlayer.SetUnjustifiedPoints(unjustifiedPoints);
        }
        public static void ParsePvpSituations(InputMessage msg)
        {
            byte openPvpSituations = msg.GetU8();

            LocalPlayer.SetOpenPvpSituations(openPvpSituations);
        }
        public static void ParsePlayerHelpers(InputMessage msg)
        {
            uint id = msg.GetU32();
            int helpers = msg.GetU16();

            Creature creature = Map.Current.GetCreatureById(id);
            if (creature != null)
            {
                // ProcessPlayerHelpers
            }
            else
                throw new Exception(String.Format("could not get creature with id {0}", id));
        }
        public static void ParseGmActions(InputMessage msg)
        {
            List<byte> actions = new List<byte>();

            int numViolationReasons;

            if (Config.ClientVersion >= 850)
                numViolationReasons = 20;
            else if (Config.ClientVersion >= 840)
                numViolationReasons = 23;
            else
                numViolationReasons = 32;

            for (int i = 0; i < numViolationReasons; ++i)
                actions.Add(msg.GetU8());
            //GameState.ProcessGmActions(actions);
        }

        public static void ParseUpdateNeeded(InputMessage msg)
        {
            string signature = msg.GetString();
            //GameState.ProcessUpdateNeeded(signature);
        }

        public static void ParseLoginAdvice(InputMessage msg)
        {
            string message = msg.GetString();

            //GameState.ProcessLoginAdvice(message);
        }

        public static void ParseLoginWait(InputMessage msg)
        {
            string message = msg.GetString();
            int time = msg.GetU8();

            //GameState.ProcessLoginWait(message, time);
        }

        public static void ParseLoginToken(InputMessage msg)
        {
            bool unknown = (msg.GetU8() == 0);
            //GameState.ProcessLoginToken(unknown);
        }

        public static void ParsePing(InputMessage msg)
        {
            GameServer.Instance.SendPingBack();
        }

        public static void ParsePingBack(InputMessage msg)
        {
            PingSystem.PingBack();
        }

        public static void ParseChallenge(InputMessage msg)
        {
            uint timestamp = msg.GetU32();
            byte random = msg.GetU8();

            SendCharacterPacket(GameServer.Instance.CurrentCharacterName, timestamp, random);
            GameServer.Instance.CurrentCharacterName = null;
        }

        public static void ParseDeath(InputMessage msg)
        {
            int penality = 100;
            int deathType = (int)DeathType.DeathRegular;

            if (FeatureManager.GetFeature(GameFeature.GameDeathType))
                deathType = msg.GetU8();

            if (FeatureManager.GetFeature(GameFeature.GamePenalityOnDeath) &&
                deathType == (int)DeathType.DeathRegular)
                penality = msg.GetU8();

            LocalPlayer.ProcessDeath(deathType, penality);
        }

        public static void ParseMapDescription(InputMessage msg)
        {
                var pos = GetPosition(msg);

                if (!Map.Current.IsKnown)
                {
                    LocalPlayer.Current.Position = pos;
                }

                Map.Current.CentralPosition = pos;

                AwareRange range = Map.Current.AwareRange;
                MapRenderer.RemoveAll();


                SetMapDescription(msg, (int)pos.x - range.Left, (int)pos.y - range.Top, (int)pos.z, range.Horizontal,
                    range.Vertical, 0, false);

                if (!Map.Current.IsKnown)
                {
                    Map.Current.IsKnown = true;
                    LocalPlayer.Current.SetCamera();
                }

        }

        public static void ParseMapMoveNorth(InputMessage msg)
        {
            var pos = new Vector3();
            if (FeatureManager.GetFeature(GameFeature.GameMapMovePosition))
                pos = GetPosition(msg);
            else
                pos = (Vector3)Map.Current.CentralPosition;
            pos.y--;


            AwareRange range = Map.Current.AwareRange;

            SetMapDescription(msg, (int)pos.x - range.Left, (int)pos.y - range.Top - 0, (int)pos.z, range.Horizontal,
                1, range.Vertical - 1);

            Map.Current.CentralPosition = pos;

        }

        public static void ParseMapMoveEast(InputMessage msg)
        {
            var pos = new Vector3();
            if (FeatureManager.GetFeature(GameFeature.GameMapMovePosition))
                pos = GetPosition(msg);
            else
                pos = (Vector3)Map.Current.CentralPosition;
            pos.x++;

            AwareRange range = Map.Current.AwareRange;
            SetMapDescription(msg, (int)pos.x + range.Right + 0, (int)pos.y - range.Top, (int)pos.z, 1,
                range.Vertical);
            Map.Current.CentralPosition = pos;
        }

        public static void ParseMapMoveSouth(InputMessage msg)
        {
            var pos = new Vector3();
            if (FeatureManager.GetFeature(GameFeature.GameMapMovePosition))
                pos = GetPosition(msg);
            else
                pos = (Vector3)Map.Current.CentralPosition;
            pos.y++;

            AwareRange range = Map.Current.AwareRange;
            SetMapDescription(msg, (int)pos.x - range.Left, (int)pos.y + range.Bottom + 0, (int)pos.z,
                range.Horizontal, 1, range.Vertical - 1);
            Map.Current.CentralPosition = pos;
        }

        public static void ParseMapMoveWest(InputMessage msg)
        {
            var pos = new Vector3();
            if (FeatureManager.GetFeature(GameFeature.GameMapMovePosition))
                pos = GetPosition(msg);
            else
                pos = (Vector3)Map.Current.CentralPosition;
            pos.x--;

            AwareRange range = Map.Current.AwareRange;
            SetMapDescription(msg, (int)pos.x - range.Left - 0, (int)pos.y - range.Top, (int)pos.z, 1,
                range.Vertical);
            Map.Current.CentralPosition = pos;
        }

        public static void ParseUpdateTile(InputMessage msg)
        {
            var tilePos = GetPosition(msg);
            SetTileDescription(msg, tilePos, false);
        }

        public static void ParseTileAddThing(InputMessage msg)
        {
            ;
            var pos = GetPosition(msg);
            int stackPos = -1;

            if (Config.ClientVersion >= 841)
                stackPos = msg.GetU8();

            Thing thing = GetThing(msg);
            thing.Position = pos;
            Map.Current.AddThing(thing, pos, stackPos);

            var tile = Map.Current.GetTile(pos);
            tile.IsDrawed = false;
            tile.Draw(tile.RealPosition, tile.Order, Vector3.zero);
        }

        public static void ParseTileTransformThing(InputMessage msg)
        {
            Thing thing = GetMappedThing(msg);
            Thing newThing = GetThing(msg);

            if (thing == null)
            {
                throw new Exception("no thing");
            }

            var pos = (Vector3)thing.Position;
            var realPos = thing.RealPosition;
            int stackpos = thing.StackPos;

            var oldOrder = thing.Order;

            if (!Map.Current.RemoveThing(thing))
            {
                throw new Exception("unable to remove thing");
            }

            Map.Current.AddThing(newThing, pos, stackpos);
            var tile = Map.Current.GetTile(pos);
            tile.IsDrawed = false;
            tile.Draw(tile.RealPosition, tile.Order, Vector3.zero);
        }

        public static void ParseTileRemoveThing(InputMessage msg)
        {
            Thing thing = GetMappedThing(msg);
            if (thing == null)
            {
                throw new Exception("no thing");
            }

            if (!Map.Current.RemoveThing(thing))
                throw new Exception("unable to remove thing");

            thing.Sprite.Group.RemoveThing(thing);
        }

        public static void ParseMoveCreature(InputMessage msg)
        {
            Thing thing = GetMappedThing(msg);
            var newPos = GetPosition(msg);

            if (thing == null || !(thing is Creature))
            {
                throw new Exception("no creature found to move, newPos: " + newPos);
                return;
            }


            var fromTile = Map.Current.GetTile(thing.Position);

            if (!Map.Current.RemoveThing(thing, false))
            {
                throw new Exception("unable to remove creature");
                return;
            }

            Map.Current.AddThing(thing, newPos, -1);

            ((Creature)thing).Move(fromTile, Map.Current.GetTile(newPos));

        }

        public static void ParseOpenContainer(InputMessage msg)
        {
            int containerId = msg.GetU8();
            Item containerItem = GetItem(msg);
            string name = msg.GetString();
            int capacity = msg.GetU8();
            bool hasParent = (msg.GetU8() != 0);

            bool isUnlocked = true;
            bool hasPages = false;
            int containerSize = 0;
            int firstIndex = 0;

            if (FeatureManager.GetFeature(GameFeature.GameContainerPagination))
            {
                isUnlocked = (msg.GetU8() != 0); // drag and drop
                hasPages = (msg.GetU8() != 0); // pagination
                containerSize = msg.GetU16(); // container size
                firstIndex = msg.GetU16(); // first index
            }

            int itemCount = msg.GetU8();

            List<Item> items = new List<Item>(itemCount);
            for (int i = 0; i < itemCount; i++)
                items.Add(GetItem(msg));

            ContainerSystem.ProcessOpenContainer(containerId, containerItem, name, capacity, hasParent, items,
                isUnlocked, hasPages, containerSize, firstIndex);
        }

        public static void ParseCloseContainer(InputMessage msg)
        {
            int containerId = msg.GetU8();
            ContainerSystem.ProcessCloseContainer(containerId);
        }

        public static void ParseContainerAddItem(InputMessage msg)
        {
            int containerId = msg.GetU8();
            int slot = 0;
            if (FeatureManager.GetFeature(GameFeature.GameContainerPagination))
            {
                slot = msg.GetU16(); // slot
            }
            Item item = GetItem(msg);
            ContainerSystem.ProcessContainerAddItem(containerId, item, slot);
        }

        public static void ParseContainerUpdateItem(InputMessage msg)
        {
            int containerId = msg.GetU8();
            int slot;
            if (FeatureManager.GetFeature(GameFeature.GameContainerPagination))
            {
                slot = msg.GetU16();
            }
            else
            {
                slot = msg.GetU8();
            }
            Item item = GetItem(msg);
            ContainerSystem.ProcessContainerUpdateItem(containerId, slot, item);
        }
        public static void ParseContainerRemoveItem(InputMessage msg)
        {
            int containerId = msg.GetU8();
            int slot;
            Item lastItem = null;
            if (FeatureManager.GetFeature(GameFeature.GameContainerPagination))
            {
                slot = msg.GetU16();

                int itemId = msg.GetU16();
                if (itemId != 0)
                    lastItem = GetItem(msg, itemId);
            }
            else
            {
                slot = msg.GetU8();
            }
            ContainerSystem.ProcessContainerRemoveItem(containerId, slot, lastItem);
        }
        public static void ParseAddInventoryItem(InputMessage msg)
        {
            int slot = msg.GetU8();
            Item item = GetItem(msg);
            InventorySystem.ProcessInventoryChange((InventorySlot)slot, item);
        }

        public static void ParseRemoveInventoryItem(InputMessage msg)
        {
            int slot = msg.GetU8();
            InventorySystem.ProcessInventoryChange((InventorySlot)slot, null);
        }

        public static void ParseOpenNpcTrade(InputMessage msg)
        {
            List<Tuple<Item, string, int, int, int>> items = new List<Tuple<Item, string, int, int, int>>();
            string npcName;

            if (FeatureManager.GetFeature(GameFeature.GameNameOnNpcTrade))
                npcName = msg.GetString();

            int listCount;

            if (Config.ClientVersion >= 900)
                listCount = msg.GetU16();
            else
                listCount = msg.GetU8();

            for (int i = 0; i < listCount; ++i)
            {
                UInt16 itemId = msg.GetU16();
                byte count = msg.GetU8();

                Item item = Item.Create(itemId);
                item.SetCountOrSubType(count);

                string name = msg.GetString();
                int weight = (int)msg.GetU32();
                int buyPrice = (int)msg.GetU32();
                int sellPrice = (int)msg.GetU32();
                items.Add(new Tuple<Item, string, int, int, int>(item, name, weight, buyPrice, sellPrice));
            }

            TradeSystem.ProcessOpenNpcTrade(items);
        }
        public static void ParsePlayerGoods(InputMessage msg)
        {
            List<Tuple<Item, int>> goods = new List<Tuple<Item, int>>();

            UInt64 money;
            if (Config.ClientVersion >= 973)
                money = msg.GetU64();
            else
                money = msg.GetU32();

            int size = msg.GetU8();
            for (int i = 0; i < size; i++)
            {
                int itemId = msg.GetU16();
                int amount;

                if (FeatureManager.GetFeature(GameFeature.GameDoubleShopSellAmount))
                    amount = msg.GetU16();
                else
                    amount = msg.GetU8();

                goods.Add(new Tuple<Item, int>(Item.Create((ushort)itemId), amount));
            }

            TradeSystem.ProcessPlayerGoods(money, goods);
        }
        public static void ParseCloseNpcTrade(InputMessage UnnamedParameter1)
        {
            TradeSystem.ProcessCloseNpcTrade();
        }
        public static void ParseOwnTrade(InputMessage msg)
        {
            string name = Creature.FormatCreatureName(msg.GetString());
            int count = msg.GetU8();

            List<Item> items = new List<Item>(count);
            for (int i = 0; i < count; i++)
                items[i] = GetItem(msg);

            TradeSystem.ProcessOwnTrade(name, items);
        }
        public static void ParseCounterTrade(InputMessage msg)
        {
            string name = Creature.FormatCreatureName(msg.GetString());
            int count = msg.GetU8();

            List<Item> items = new List<Item>(count);
            for (int i = 0; i < count; i++)
                items[i] = GetItem(msg);

            TradeSystem.ProcessCounterTrade(name, items);
        }
        public static void ParseCloseTrade(InputMessage UnnamedParameter1)
        {
            TradeSystem.ProcessCloseTrade();
        }
        public static void ParseWorldLight(InputMessage msg)
        {
            var intensity = msg.GetU8();
            var color = msg.GetU8();

            var c = ByteColorConverter.GetColor(color);

            foreach (var item in GameObject.FindObjectsOfType<LightSurface>())
            {
                item.SetupAmbientLight(c, intensity);
            }

        }

        public static void ParseMagicEffect(InputMessage msg)
        {
            var pos = GetPosition(msg);
            int effectId = 0;
            if (FeatureManager.GetFeature(GameFeature.GameMagicEffectU16))
                effectId = msg.GetU16();
            else
                effectId = msg.GetU8();

            if (!ThingTypeManager.IsValidDatId((ushort)effectId, ThingCategory.ThingCategoryEffect))
            {
                throw new Exception(String.Format("invalid effect id {0}", effectId));
                return;
            }

            Effect effect = new Effect(effectId);

            Map.Current.AddThing(effect, pos, -1);

        }

        public static void ParseAnimatedText(InputMessage msg)
        {
            var position = GetPosition(msg);
            int color = msg.GetU8();
            string text = msg.GetString();

            AnimatedText animatedText = new AnimatedText
            {
                Color = color,
                RealPosition = position,
                Text = text
            };
            Map.Current.AddThing(animatedText, position, 0);
        }

        public static void ParseDistanceMissile(InputMessage msg)
        {
            var fromPos = GetPosition(msg);
            var toPos = GetPosition(msg);
            int shotId = msg.GetU8();

            if (!ThingTypeManager.IsValidDatId((ushort)shotId, ThingCategory.ThingCategoryMissile))
            {
                throw new Exception(String.Format("invalid missile id {0}", shotId));
            }

            Missile missile = new Missile(shotId);

            Map.Current.AddThing(missile, fromPos, -1);

        }

        public static void ParseCreatureMark(InputMessage msg)
        {
            uint id = msg.GetU32();
            int color = msg.GetU8();

            Creature creature = Map.Current.GetCreatureById(id);
            if (creature != null)
                creature.AddTimedSquare((byte)color);
            else
                throw new Exception("could not Get creature");
        }

        public static void ParseTrappers(InputMessage msg)
        {
            int numTrappers = msg.GetU8();

            if (numTrappers > 8)
                throw new Exception("too many trappers");

            for (int i = 0; i < numTrappers; ++i)
            {
                uint id = msg.GetU32();
                Creature creature = Map.Current.GetCreatureById(id);
                if (creature != null)
                {
                    //TODO: set creature as trapper
                }
                else
                    throw new Exception("could not get creature");
            }
        }

        public static void ParseCreatureHealth(InputMessage msg)
        {
            uint id = msg.GetU32();
            int healthPercent = msg.GetU8();

            Creature creature = Map.Current.GetCreatureById(id);
            if (creature != null)
                creature.HealthPercent = ((byte)healthPercent);
        }

        public static void ParseCreatureLight(InputMessage msg)
        {
            uint id = msg.GetU32();

            Game.DAO.Light t = new Game.DAO.Light();
            t.Intensity = msg.GetU8();
            t.Color = msg.GetU8();

            Creature creature = Map.Current.GetCreatureById(id);
            if (creature != null)
            {

            }
        }

        public static void ParseCreatureOutfit(InputMessage msg)
        {
            uint id = msg.GetU32();
            Outfit outfit = GetOutfit(msg);

            Creature creature = Map.Current.GetCreatureById(id);
            if (creature != null)
                creature.Outfit = (outfit);
            else
                throw new Exception("could not get creature");
        }

        public static void ParseCreatureSpeed(InputMessage msg)
        {
            uint id = msg.GetU32();

            int baseSpeed = -1;
            if (Config.ClientVersion >= 1059)
                baseSpeed = msg.GetU16();

            int speed = msg.GetU16();

            Creature creature = Map.Current.GetCreatureById(id);
            if (creature != null)
            {
                creature.Speed = ((ushort)speed);
                if (baseSpeed != -1)
                    creature.BaseSpeed = (baseSpeed);
            }

        }

        public static void ParseCreatureSkulls(InputMessage msg)
        {
            uint id = msg.GetU32();
            int skull = msg.GetU8();

            Creature creature = Map.Current.GetCreatureById(id);
            if (creature != null)
                creature.Skull = ((PlayerSkulls)skull);
            else
                throw new Exception("could not get creature");
        }

        public static void ParseCreatureShields(InputMessage msg)
        {
            uint id = msg.GetU32();
            int shield = msg.GetU8();

            Creature creature = Map.Current.GetCreatureById(id);
            if (creature != null)
                creature.Shield = ((PlayerShields)shield);
            else
                throw new Exception("could not get creature");
        }

        public static void ParseCreatureUnpass(InputMessage msg)
        {
            uint id = msg.GetU32();
            bool unpass = msg.GetU8() != 0;

            Creature creature = Map.Current.GetCreatureById(id);
            if (creature != null)
                creature.Passable = (!unpass);
            else
                throw new Exception("could not get creature");
        }

        public static void ParseEditText(InputMessage msg)
        {
            uint id = msg.GetU32();

            int itemId;
            if (Config.ClientVersion >= 1010)
            {

                Item item = GetItem(msg);
                itemId = item.DatId;
            }
            else
                itemId = msg.GetU16();

            int maxLength = msg.GetU16();
            string text = msg.GetString();

            string writer = msg.GetString();
            string date = "";
            if (FeatureManager.GetFeature(GameFeature.GameWritableDate))
                date = msg.GetString();

            TextMessageSystem.ProcessEditText(id, itemId, maxLength, text, writer, date);
        }
        public static void ParseEditList(InputMessage msg)
        {
            int doorId = msg.GetU8();
            uint id = msg.GetU32();
            string text = msg.GetString();

            TextMessageSystem.ProcessEditList(id, doorId, text);
        }
        public static void ParsePremiumTrigger(InputMessage msg)
        {
            int triggerCount = msg.GetU8();
            List<int> triggers = new List<int>();
            for (int i = 0; i < triggerCount; ++i)
            {
                triggers.Add(msg.GetU8());
            }

            if (Config.ClientVersion <= 1096)
            {
                bool something = msg.GetU8() == 1;
            }
        }
        public static void ParsePlayerInfo(InputMessage msg)
        {
            bool premium = msg.GetU8() != 0; // premium
            int vocation = msg.GetU8(); // vocation
            if (FeatureManager.GetFeature(GameFeature.GamePremiumExpiration))
            {
                uint premiumEx = msg.GetU32(); // premium expiration used for premium advertisement
            }

            int spellCount = msg.GetU16();
            List<int> spells = new List<int>();
            for (int i = 0; i < spellCount; ++i)
                spells.Add(msg.GetU8()); // spell id

            LocalPlayer.Current.Premium = premium;
            LocalPlayer.Current.Vocation = vocation;
            LocalPlayer.Current.Spells = spells;
        }

        public static void ParsePlayerStats(InputMessage msg)
        {
            double health;
            double maxHealth;

            if (FeatureManager.GetFeature(GameFeature.GameDoubleHealth))
            {
                health = msg.GetU32();
                maxHealth = msg.GetU32();
            }
            else
            {
                health = msg.GetU16();
                maxHealth = msg.GetU16();
            }

            double freeCapacity;
            if (FeatureManager.GetFeature(GameFeature.GameDoubleFreeCapacity))
                freeCapacity = msg.GetU32() / 100.0;
            else
                freeCapacity = msg.GetU16() / 100.0;

            double totalCapacity = 0;
            if (FeatureManager.GetFeature(GameFeature.GameTotalCapacity))
                totalCapacity = msg.GetU32() / 100.0;

            double experience;
            if (FeatureManager.GetFeature(GameFeature.GameDoubleExperience))
                experience = msg.GetU64();
            else
                experience = msg.GetU32();

            double level = msg.GetU16();
            double levelPercent = msg.GetU8();

            if (FeatureManager.GetFeature(GameFeature.GameExperienceBonus))
                if (Config.ClientVersion <= 1096)
                {
                    double experienceBonus = msg.GetDouble();
                }
                else
                {
                    int baseXpGain = msg.GetU16();
                    int voucherAddend = msg.GetU16();
                    int grindingAddend = msg.GetU16();
                    int storeBoostAddend = msg.GetU16();
                    int huntingBoostFactor = msg.GetU16();
                }

            double mana;
            double maxMana;

            if (FeatureManager.GetFeature(GameFeature.GameDoubleHealth))
            {
                mana = msg.GetU32();
                maxMana = msg.GetU32();
            }
            else
            {
                mana = msg.GetU16();
                maxMana = msg.GetU16();
            }

            double magicLevel = msg.GetU8();

            double baseMagicLevel;
            if (FeatureManager.GetFeature(GameFeature.GameSkillsBase))
                baseMagicLevel = msg.GetU8();
            else
                baseMagicLevel = magicLevel;

            double magicLevelPercent = msg.GetU8();
            double soul = msg.GetU8();
            double stamina = 0;
            if (FeatureManager.GetFeature(GameFeature.GamePlayerStamina))
                stamina = msg.GetU16();

            double baseSpeed = 0;
            if (FeatureManager.GetFeature(GameFeature.GameSkillsBase))
                baseSpeed = msg.GetU16();

            double regeneration = 0;
            if (FeatureManager.GetFeature(GameFeature.GamePlayerRegenerationTime))
                regeneration = msg.GetU16();

            double training = 0;
            if (FeatureManager.GetFeature(GameFeature.GameOfflineTrainingTime))
            {
                training = msg.GetU16();
                if (Config.ClientVersion >= 1097)
                {
                    int remainingStoreXpBoostSeconds = msg.GetU16();
                    bool canBuyMoreStoreXpBoosts = msg.GetU8() != 0;
                }
            }

            LocalPlayer.Current.Health = health;
            LocalPlayer.Current.MaxHealth = maxHealth;
            LocalPlayer.Current.FreeCapacity = freeCapacity;
            LocalPlayer.Current.TotalCapacity = totalCapacity;
            LocalPlayer.Current.Experience = experience;
            LocalPlayer.Current.Level = level;
            LocalPlayer.Current.LevelPercent = levelPercent;
            LocalPlayer.Current.Mana = mana;
            LocalPlayer.Current.MaxMana = maxMana;
            LocalPlayer.Current.MagicLevel = magicLevel;
            LocalPlayer.Current.MagicLevelPercent = magicLevelPercent;
            LocalPlayer.Current.BaseMagicLevel = baseMagicLevel;
            LocalPlayer.Current.Stamina = stamina;
            LocalPlayer.Current.Soul = soul;
            LocalPlayer.Current.BaseSpeed = baseSpeed;
            LocalPlayer.Current.RegenerationTime = regeneration;
            LocalPlayer.Current.OfflineTrainingTime = training;

            LocalPlayer.Current.InvokeStatsChanged();
        }

        public static void ParsePlayerSkills(InputMessage msg)
        {
            int lastSkill = (int)Skill.Fishing + 1;
            if (FeatureManager.GetFeature(GameFeature.GameAdditionalSkills))
                lastSkill = (int)Skill.LastSkill;

            for (int skill = 0; skill < lastSkill; skill++)
            {
                int level;

                if (FeatureManager.GetFeature(GameFeature.GameDoubleSkills))
                    level = msg.GetU16();
                else
                    level = msg.GetU8();

                int baseLevel;
                if (FeatureManager.GetFeature(GameFeature.GameSkillsBase))
                    if (FeatureManager.GetFeature(GameFeature.GameBaseSkillU16))
                        baseLevel = msg.GetU16();
                    else
                        baseLevel = msg.GetU8();
                else
                    baseLevel = level;

                int levelPercent = 0;
                // Critical, Life Leech and Mana Leech have no level percent
                if (skill <= (int)Skill.Fishing)
                    levelPercent = msg.GetU8();

                LocalPlayer.Current.SetSkill((Skill)skill, level, levelPercent);
                LocalPlayer.Current.SetBaseSkill((Skill)skill, baseLevel);
            }
        }

        public static void ParsePlayerState(InputMessage msg)
        {
            int states;
            if (FeatureManager.GetFeature(GameFeature.GamePlayerStateU16))
                states = msg.GetU16();
            else
                states = msg.GetU8();

            LocalPlayer.Current.States = (PlayerStates)states;
        }

        public static void ParsePlayerCancelAttack(InputMessage msg)
        {
            uint seq = 0;
            if (FeatureManager.GetFeature(GameFeature.GameAttackSeq))
                seq = msg.GetU32();

            LocalPlayer.ProcessAttackCancel(seq);
        }

        public static void ParsePlayerModes(InputMessage msg)
        {
            int fightMode = msg.GetU8();
            int chaseMode = msg.GetU8();
            bool safeMode = msg.GetU8() != 0;

            int pvpMode = 0;
            if (FeatureManager.GetFeature(GameFeature.GamePVPMode))
                pvpMode = msg.GetU8();

            LocalPlayer.ProcessPlayerModes((FightModes)fightMode, (ChaseModes)chaseMode, safeMode, (PVPModes)pvpMode);
        }

        public static void ParseSpellCooldown(InputMessage msg)
        {
            int spellId = msg.GetU8();
            uint delay = msg.GetU32();
        }

        public static void ParseSpellGroupCooldown(InputMessage msg)
        {
            int groupId = msg.GetU8();
            uint delay = msg.GetU32();
        }

        public static void ParseMultiUseCooldown(InputMessage msg)
        {
            uint delay = msg.GetU32();
        }

        public static void ParseTalk(InputMessage msg)
        {
            if (FeatureManager.GetFeature(GameFeature.GameMessageStatements))
                msg.GetU32(); // channel statement guid

            string name = Creature.FormatCreatureName(msg.GetString());

            int level = 0;
            if (FeatureManager.GetFeature(GameFeature.GameMessageLevel))
                level = msg.GetU16();

            MessageMode mode = MessageModeTranslator.translateMessageModeFromServer(msg.GetU8());
            int channelId = 0;
            var pos = new Vector3();

            switch (mode)
            {
                case MessageMode.Say:
                case MessageMode.Whisper:
                case MessageMode.Yell:
                case MessageMode.MonsterSay:
                case MessageMode.MonsterYell:
                case MessageMode.NpcTo:
                case MessageMode.BarkLow:
                case MessageMode.BarkLoud:
                case MessageMode.Spell:
                case MessageMode.NpcFromStartBlock:
                    pos = GetPosition(msg);
                    break;
                case MessageMode.Channel:
                case MessageMode.ChannelManagement:
                case MessageMode.ChannelHighlight:
                case MessageMode.GamemasterChannel:
                    channelId = msg.GetU16();
                    break;
                case MessageMode.NpcFrom:
                case MessageMode.PrivateFrom:
                case MessageMode.GamemasterBroadcast:
                case MessageMode.GamemasterPrivateFrom:
                case MessageMode.RVRAnswer:
                case MessageMode.RVRContinue:
                    break;
                case MessageMode.RVRChannel:
                    msg.GetU32();
                    break;
                default:
                    throw new Exception(String.Format("unknown message mode {0}", mode));
                    break;
            }

            string text = msg.GetString();

            ConsoleSystem.ProcessTalk(name, level, mode, text, channelId, pos);
        }

        public static void ParseChannelList(InputMessage msg)
        {
            int count = msg.GetU8();
            List<Tuple<int, string>> channelList = new List<Tuple<int, string>>();
            for (int i = 0; i < count; i++)
            {
                int id = msg.GetU16();
                string name = msg.GetString();
                channelList.Add(new Tuple<int, string>(id, name));
            }

            ConsoleSystem.ProcessChannelList(channelList);
        }

        public static void ParseOpenChannel(InputMessage msg)
        {
            int channelId = msg.GetU16();
            string name = msg.GetString();

            if (FeatureManager.GetFeature(GameFeature.GameChannelPlayerList))
            {
                int joinedPlayers = msg.GetU16();
                for (int i = 0; i < joinedPlayers; ++i)
                    Creature.FormatCreatureName(msg.GetString()); // player name
                int invitedPlayers = msg.GetU16();
                for (int i = 0; i < invitedPlayers; ++i)
                    Creature.FormatCreatureName(msg.GetString()); // player name
            }

            ConsoleSystem.ProcessOpenChannel(channelId, name);
        }

        public static void ParseOpenPrivateChannel(InputMessage msg)
        {
            string name = Creature.FormatCreatureName(msg.GetString());

            ConsoleSystem.ProcessOpenPrivateChannel(name);
        }

        public static void ParseOpenOwnPrivateChannel(InputMessage msg)
        {
            int channelId = msg.GetU16();
            string name = msg.GetString();

            TextMessageSystem.ProcessOpenOwnPrivateChannel(channelId, name);
        }

        public static void ParseCloseChannel(InputMessage msg)
        {
            int channelId = msg.GetU16();

            TextMessageSystem.ProcessCloseChannel(channelId);
        }

        public static void ParseRuleViolationChannel(InputMessage msg)
        {
            int channelId = msg.GetU16();

            //GameState.ProcessRuleViolationChannel(channelId);
        }

        public static void ParseRuleViolationRemove(InputMessage msg)
        {
            string name = msg.GetString();

            //GameState.ProcessRuleViolationRemove(name);
        }

        public static void ParseRuleViolationCancel(InputMessage msg)
        {
            string name = msg.GetString();

            //GameState.ProcessRuleViolationCancel(name);
        }

        public static void ParseRuleViolationLock(InputMessage msg)
        {
            //GameState.ProcessRuleViolationLock();
        }

        public static void ParseTextMessage(InputMessage msg)
        {
            int code = msg.GetU8();
            MessageMode mode = (MessageMode)MessageModeTranslator.translateMessageModeFromServer((byte)code);
            string text = string.Empty;
            switch (mode)
            {
                case MessageMode.ChannelManagement:
                    {
                        int channel = msg.GetU16();
                        text = msg.GetString();
                        break;
                    }
                case MessageMode.Guild:
                case MessageMode.PartyManagement:
                case MessageMode.Party:
                    {
                        int channel = msg.GetU16();
                        text = msg.GetString();
                        break;
                    }
                case MessageMode.DamageDealed:
                case MessageMode.DamageReceived:
                case MessageMode.DamageOthers:
                    {
                        var pos = GetPosition(msg);
                        uint[] value = new uint[2];
                        int[] color = new int[2];

                        // physical damage
                        value[0] = msg.GetU32();
                        color[0] = msg.GetU8();

                        // magic damage
                        value[1] = msg.GetU32();
                        color[1] = msg.GetU8();
                        text = msg.GetString();

                        for (int i = 0; i < 2; ++i)
                        {
                            if (value[i] == 0)
                                continue;
                            var animatedText = new AnimatedText();
                            animatedText.Color = (color[i]);
                            animatedText.Text = (value[i].ToString());
                            Map.Current.AddThing(animatedText, pos, 10000000);
                        }
                        break;
                    }
                case MessageMode.Heal:
                case MessageMode.Mana:
                case MessageMode.Exp:
                case MessageMode.HealOthers:
                case MessageMode.ExpOthers:
                    {
                        var pos = GetPosition(msg);
                        uint value = msg.GetU32();
                        int color = msg.GetU8();
                        text = msg.GetString();

                        AnimatedText animatedText = new AnimatedText();
                        animatedText.Color = (color);
                        animatedText.Text = (value.ToString());
                        Map.Current.AddThing(animatedText, pos, 10000000);
                        break;
                    }
                case MessageMode.Invalid:
                    throw new Exception(String.Format("unknown message mode {0}", mode));
                default:
                    text = msg.GetString();
                    break;
            }

            TextMessageSystem.ProcessTextMessage(mode, text);
        }

        public static void ParseCancelWalk(InputMessage msg)
        {
            Direction direction = (Direction)msg.GetU8();

            LocalPlayer.Current.CancelWalk(direction);
        }

        public static void ParseWalkWait(InputMessage msg)
        {
            int millis = msg.GetU16();
            LocalPlayer.Current.LockWalk(millis);
        }

        public static void ParseFloorChangeUp(InputMessage msg)
        {
            //var pos = new Vector3();
            //if (ProtocolManager.GetFeature(GameFeature.GameMapMovePosition))
            //    pos = GetPosition(msg);
            //else
            //    pos = (Vector3)GlobalMembersMap.GMap.CentralPosition;
            //AwareRange range = GlobalMembersMap.GMap.GetAwareRange();
            //pos.z--;

            //int skip = 0;
            //if (pos.z == (int)TileMaps.SEA_FLOOR)
            //    for (int i = (int)TileMaps.SEA_FLOOR - (int)TileMaps.AWARE_UNDEGROUND_FLOOR_RANGE;
            //        i >= 0;
            //        i--)
            //        skip = SetFloorDescription(msg, pos.x - range.Left, pos.y - range.Top, i, range.Horizontal,
            //            range.Vertical, 8 - i, 0, 0, 0);
            //else if (pos.z > (int)TileMaps.SEA_FLOOR)
            //    skip = SetFloorDescription(msg, pos.x - range.Left, pos.y - range.Top,
            //        pos.z - (int)TileMaps.AWARE_UNDEGROUND_FLOOR_RANGE, range.Horizontal, range.Vertical, 3,
            //        skip, 0, 0);

            //pos.x++;
            //pos.y++;
            //GlobalMembersMap.GMap.SetCentralPosition(pos);
        }

        public static void ParseFloorChangeDown(InputMessage msg)
        {
            //var pos = new Vector3();
            //if (ProtocolManager.GetFeature(GameFeature.GameMapMovePosition))
            //    pos = GetPosition(msg);
            //else
            //    pos = (Vector3)GlobalMembersMap.GMap.CentralPosition;
            //AwareRange range = GlobalMembersMap.GMap.GetAwareRange();
            //pos.z++;

            //int skip = 0;
            //if (pos.z == (int)TileMaps.UNDERGROUND_FLOOR)
            //{
            //    int j;
            //    int i;
            //    for (i = pos.z, j = -1; i <= pos.z + (int)TileMaps.AWARE_UNDEGROUND_FLOOR_RANGE; ++i, --j)
            //        skip = SetFloorDescription(msg, pos.x - range.Left, pos.y - range.Top, i, range.Horizontal,
            //            range.Vertical, j, skip, 0, 0);
            //}
            //else if (pos.z > (int)TileMaps.UNDERGROUND_FLOOR && pos.z < (int)TileMaps.MAX_Z - 1)
            //    skip = SetFloorDescription(msg, pos.x - range.Left, pos.y - range.Top,
            //        pos.z + (int)TileMaps.AWARE_UNDEGROUND_FLOOR_RANGE, range.Horizontal, range.Vertical,
            //        -3, skip, 0, 0);

            //pos.x--;
            //pos.y--;
            //GlobalMembersMap.GMap.SetCentralPosition(pos);
        }

        public static void ParseOpenOutfitWindow(InputMessage msg)
        {
            Outfit currentOutfit = GetOutfit(msg);
            List<Tuple<int, string, int>> outfitList = new List<Tuple<int, string, int>>();

            if (FeatureManager.GetFeature(GameFeature.GameNewOutfitProtocol))
            {
                int outfitCount = msg.GetU8();
                for (int i = 0; i < outfitCount; i++)
                {
                    int outfitId = msg.GetU16();
                    string outfitName = msg.GetString();
                    int outfitAddons = msg.GetU8();

                    outfitList.Add(new Tuple<int, string, int>(outfitId, outfitName, outfitAddons));
                }
            }
            else
            {
                int outfitStart;
                int outfitEnd;
                if (FeatureManager.GetFeature(GameFeature.GameLooktypeU16))
                {
                    outfitStart = msg.GetU16();
                    outfitEnd = msg.GetU16();
                }
                else
                {
                    outfitStart = msg.GetU8();
                    outfitEnd = msg.GetU8();
                }

                for (int i = outfitStart; i <= outfitEnd; i++)
                    outfitList.Add(new Tuple<int, string, int>(i, "", 0));
            }

            List<Tuple<int, string>> mountList = new List<Tuple<int, string>>();
            if (FeatureManager.GetFeature(GameFeature.GamePlayerMounts))
            {
                int mountCount = msg.GetU8();
                for (int i = 0; i < mountCount; ++i)
                {
                    int mountId = msg.GetU16(); // mount type
                    string mountName = msg.GetString(); // mount name

                    mountList.Add(new Tuple<int, string>(mountId, mountName));
                }
            }

            OutfitSystem.ProcessOpenOutfitWindow(currentOutfit, outfitList, mountList);
        }

        public static void ParseVipAdd(InputMessage msg)
        {
            uint id;
            uint iconId = 0;
            uint status;
            string name;
            string desc = "";
            bool notifyLogin = false;

            id = msg.GetU32();
            name = Creature.FormatCreatureName(msg.GetString());
            if (FeatureManager.GetFeature(GameFeature.GameAdditionalVipInfo))
            {
                desc = msg.GetString();
                iconId = msg.GetU32();
                notifyLogin = msg.GetU8() != 0;
            }
            status = msg.GetU8();

            VipSystem.ProcessVipAdd(id, name, status, desc, iconId, notifyLogin);
        }

        public static void ParseVipState(InputMessage msg)
        {
            uint id = msg.GetU32();
            if (FeatureManager.GetFeature(GameFeature.GameLoginPending))
            {
                uint status = msg.GetU8();
                VipSystem.ProcessVipStateChange(id, (int)status);
            }
            else
            {
                VipSystem.ProcessVipStateChange(id, 1);
            }
        }

        public static void ParseVipLogout(InputMessage msg)
        {
            uint id = msg.GetU32();
            VipSystem.ProcessVipStateChange(id, 0);
        }

        public static void ParseTutorialHint(InputMessage msg)
        {
            int id = msg.GetU8();
            //GameState.ProcessTutorialHint(id);
        }

        public static void ParseAutomapFlag(InputMessage msg)
        {
            var pos = GetPosition(msg);
            int icon = msg.GetU8();
            string description = msg.GetString();

            bool remove = false;
            if (FeatureManager.GetFeature(GameFeature.GameMinimapRemove))
                remove = msg.GetU8() != 0;

            if (!remove)
                Map.ProcessAddAutomapFlag(pos, icon, description);
            else
                Map.ProcessRemoveAutomapFlag(pos, icon, description);
        }

        public static void ParseQuestLog(InputMessage msg)
        {
            List<Tuple<int, string, bool>> questList = new List<Tuple<int, string, bool>>();
            int questsCount = msg.GetU16();
            for (int i = 0; i < questsCount; i++)
            {
                int id = msg.GetU16();
                string name = msg.GetString();
                bool completed = msg.GetU8() != 0;
                questList.Add(new Tuple<int, string, bool>(id, name, completed));
            }

            QuestSystem.ProcessQuestLog(questList);
        }

        public static void ParseQuestLine(InputMessage msg)
        {
            List<Tuple<string, string>> questMissions = new List<Tuple<string, string>>();
            int questId = msg.GetU16();
            int missionCount = msg.GetU8();
            for (int i = 0; i < missionCount; i++)
            {
                string missionName = msg.GetString();
                string missionDescrition = msg.GetString();
                questMissions.Add(new Tuple<string, string>(missionName, missionDescrition));
            }

            QuestSystem.ProcessQuestLine(questId, questMissions);
        }

        public static void ParseChannelEvent(InputMessage msg)
        {
            msg.GetU16(); // channel id
            Creature.FormatCreatureName(msg.GetString()); // player name
            msg.GetU8(); // event type
        }

        public static void ParseItemInfo(InputMessage msg)
        {
            Dictionary<Item, string> list = new Dictionary<Item, string>();
            int size = msg.GetU8();
            for (int i = 0; i < size; ++i)
            {
                Item item = Item.Create(msg.GetU16());
                item.SetCountOrSubType(msg.GetU8());

                string desc = msg.GetString();
                list.Add(item, desc);
            }
        }

        public static void ParsePlayerInventory(InputMessage msg)
        {
            int size = msg.GetU16();
            for (int i = 0; i < size; ++i)
            {
                msg.GetU16(); // id
                msg.GetU8(); // subtype
                msg.GetU16(); // count
            }
        }

        public static void ParseModalDialog(InputMessage msg)
        {
            UInt32 id = msg.GetU32();
            string title = msg.GetString();
            string message = msg.GetString();

            int sizeButtons = msg.GetU8();
            List<Tuple<int, string>> buttonList = new List<Tuple<int, string>>();
            for (int i = 0; i < sizeButtons; ++i)
            {
                string value = msg.GetString();
                int id_ = msg.GetU8();
                buttonList.Add(new Tuple<int, string>(id_, value));
            }

            int sizeChoices = msg.GetU8();
            List<Tuple<int, string>> choiceList = new List<Tuple<int, string>>();
            for (int i = 0; i < sizeChoices; ++i)
            {
                string value = msg.GetString();
                int id_ = msg.GetU8();
                choiceList.Add(new Tuple<int, string>(id_, value));
            }

            int enterButton;
            int escapeButton;
            if (Config.ClientVersion > 970)
            {
                escapeButton = msg.GetU8();
                enterButton = msg.GetU8();
            }
            else
            {
                enterButton = msg.GetU8();
                escapeButton = msg.GetU8();
            }

            bool priority = msg.GetU8() == 0x01;

            ModalDialogSystem.ProcessModalDialog(id, title, message, buttonList, enterButton, escapeButton, choiceList, priority);
        }

        public static void ParseExtendedOpcode(InputMessage msg)
        {
            int opcode = msg.GetU8();
            string buffer = msg.GetString();


            if (opcode == 0)
                EnableSendExtendedOpcode = true;
            else if (opcode == 2)
                ParsePingBack(msg);
        }

        public static void ParseChangeMapAwareRange(InputMessage msg)
        {
            int xrange = msg.GetU8();
            int yrange = msg.GetU8();

            AwareRange range = new AwareRange();
            range.Left = xrange / 2 - ((xrange + 1) % 2);
            range.Right = xrange / 2;
            range.Top = yrange / 2 - ((yrange + 1) % 2);
            range.Bottom = yrange / 2;

            Map.Current.AwareRange = (range);
        }

        public static void ParseCreaturesMark(InputMessage msg)
        {
            int len;
            if (Config.ClientVersion >= 1035)
            {
                len = 1;
            }
            else
            {
                len = msg.GetU8();
            }

            for (int i = 0; i < len; ++i)
            {
                UInt32 id = msg.GetU32();
                bool isPermanent = msg.GetU8() != 1;
                byte markType = msg.GetU8();

                Creature creature = Map.Current.GetCreatureById(id);
                if (creature != null)
                {
                    if (isPermanent)
                    {
                        if (markType == 0xff)
                            creature.HideStaticSquare();
                        else
                        {
                            creature.StaticSquareColor = ByteColorConverter.GetColor(markType);
                            creature.ShowStaticSquare();
                        }
                    }
                    else
                        creature.AddTimedSquare(markType);
                }
                else
                    throw new Exception("could not get creature");
            }
        }

        public static void ParseCreatureType(InputMessage msg)
        {
            UInt32 id = msg.GetU32();
            byte typename = msg.GetU8();
        }

        public static void SetMapDescription(InputMessage msg, int x, int y, int z, int width, int height,
            int specialOffset = 0, bool noAddThing = true)
        {
            int startz;
            int endz;
            int zstep;

            if (z > (int)TileMaps.SEA_FLOOR)
            {
                startz = z - (int)TileMaps.AWARE_UNDEGROUND_FLOOR_RANGE;
                endz = Math.Min(z + (int)TileMaps.AWARE_UNDEGROUND_FLOOR_RANGE,
                    (int)(int)TileMaps.MAX_Z);
                zstep = 1;
            }
            else
            {
                startz = (int)TileMaps.SEA_FLOOR;
                endz = 0;
                zstep = -1;
            }
            int skip = 0;
            for (int nz = startz; nz != endz + zstep; nz += zstep)
            {
                skip = SetFloorDescription(msg, x, y, nz, width, height, z - nz, skip, startz, specialOffset,
                    noAddThing);
            }

            MapRenderer.CameraCulling();

        }

        public static int SetFloorDescription(InputMessage msg, int x, int y, int z, int width, int height, int offset,
            int skip, int startz, int specialOffset, bool noAddThing = true)
        {
            for (int nx = 0; nx < width; nx++)
            {
                for (int ny = 0; ny < height; ny++)
                {
                    var tilePos = new Vector3(x + nx + offset, y + ny + offset, z);

                    if (skip == 0)
                    {
                        var tile = Map.Current.GetTile(tilePos);
                        skip = SetTileDescription(msg, tilePos, false);
                        tile = Map.Current.GetTile(tilePos);
                        if (tile != null)
                        {
                            tile.Draw(new Vector3(nx + x - startz, specialOffset + height - ny - y - startz, z), ((y * 100 + ny * 100) + (x * 100 + nx * 100)), new Vector3(x + nx, y + ny, z));
                        }
                    }
                    else
                    {
                        var tile = Map.Current.GetTile(tilePos);
                        if (tile != null)
                            Map.Current.CleanTile(tilePos);
                        skip--;
                    }
                }
            }

            return skip;
        }

        public static int SetTileDescription(InputMessage msg, Vector3 position, bool noAddThing = false)
        {
            if (!noAddThing)
                Map.Current.CleanTile(position);

            bool gotEffect = false;
            for (int stackPos = 0; stackPos < 256; stackPos++)
            {
                if (msg.PeekU16() >= 0xff00)
                    return msg.GetU16() & 0xff;

                if (FeatureManager.GetFeature(GameFeature.GameEnvironmentEffect) && !gotEffect)
                {
                    msg.GetU16(); // environment effect
                    gotEffect = true;
                    continue;
                }

                if (stackPos > 10)
                {
                    throw new Exception(String.Format("too many things, pos={0}, stackpos={1}", position.ToString(),
                        stackPos));
                    return 0;
                }

                Thing thing = GetThing(msg);
                if ((thing != null && (!noAddThing)))
                {
                    Map.Current.AddThing(thing, position, stackPos);
                }
            }

            return 0;
        }

        public static Outfit GetOutfit(InputMessage msg)
        {
            Outfit outfit = new Outfit();

            int lookType;
            if (FeatureManager.GetFeature(GameFeature.GameLooktypeU16))
                lookType = msg.GetU16();
            else
                lookType = msg.GetU8();

            if (lookType != 0)
            {
                outfit.Category = ThingCategory.ThingCategoryCreature;
                int head = msg.GetU8();
                int body = msg.GetU8();
                int legs = msg.GetU8();
                int feet = msg.GetU8();
                int addons = 0;
                if (FeatureManager.GetFeature(GameFeature.GamePlayerAddons))
                    addons = msg.GetU8();

                if (!ThingTypeManager.IsValidDatId((ushort)lookType, ThingCategory.ThingCategoryCreature))
                {
                    throw new Exception(String.Format("invalid outfit looktype {0}", lookType));
                    lookType = 0;
                }

                outfit.LookTypeId = (lookType);
                outfit.HeadColorByte = head;
                outfit.BodyColorByte = body;
                outfit.LegsColorByte = legs;
                outfit.FeetColorByte = feet;
                outfit.Addons = (addons);
            }
            else
            {
                int lookTypeEx = msg.GetU16();
                if (lookTypeEx == 0)
                {
                    outfit.Category = (ThingCategory.ThingCategoryEffect);
                    outfit.LookTypeId = (13); // invisible effect id
                }
                else
                {
                    if (!ThingTypeManager.IsValidDatId((ushort)lookTypeEx, ThingCategory.ThingCategoryItem))
                    {
                        lookTypeEx = 0;
                        throw new Exception(String.Format("invalid outfit looktypeex {0}", lookTypeEx));
                    }
                    outfit.Category = (ThingCategory.ThingCategoryItem);
                    outfit.LookTypeId = (lookTypeEx);
                }
            }

            if (FeatureManager.GetFeature(GameFeature.GamePlayerMounts))
            {
                int mount = msg.GetU16();
                outfit.Mount = (mount);
            }

            return outfit;
        }

        public static Thing GetThing(InputMessage msg)
        {
            Thing thing = null;

            int id = msg.GetU16();

            if (id == 0)
            {
                throw new Exception("invalid thing id");
                return null;
            }
            else if (id == (int)ItemOpcode.UnknownCreature || id == (int)ItemOpcode.OutdatedCreature ||
                     id == (int)ItemOpcode.Creature)
            {
                thing = GetCreature(msg, id);
                thing.DatId = (ushort)((Outfit)((Creature)thing).Outfit).LookTypeId;
            }
            else if (id == (int)ItemOpcode.StaticText) // otclient only
                thing = GetStaticText(msg, id);
            else
            {
                // item
                thing = GetItem(msg, id);
            }

            return thing;
        }

        public static Thing GetMappedThing(InputMessage msg)
        {
            Thing thing = new Thing();
            UInt16 x = msg.GetU16();

            if (x != 0xffff)
            {
                var pos = new Vector3();
                pos.x = x;
                pos.y = msg.GetU16();
                pos.z = msg.GetU8();
                byte stackpos = msg.GetU8();

                thing = Map.Current.GetThing(pos, stackpos);
                if (thing == null)
                {
                    throw new Exception(String.Format("no thing at pos:{0}, stackpos:{1}", pos.ToString(), stackpos));
                }
                if (thing != null)
                {
                    thing.Position = pos;
                }
            }
            else
            {
                UInt32 id = msg.GetU32();
                thing = Map.Current.GetCreatureById(id);
                if (thing == null)
                    throw new Exception(String.Format("no creature with id %u", id));
            }

            return thing;
        }

        public static Creature GetCreature(InputMessage msg, int typename)
        {
            if (typename == 0)
                typename = msg.GetU16();

            Creature creature = new Creature();
            bool known = (typename != (int)ItemOpcode.UnknownCreature);
            if (typename == (int)ItemOpcode.OutdatedCreature || typename == (int)ItemOpcode.UnknownCreature)
            {
                if (known)
                {
                    uint id = msg.GetU32();

                    creature = Map.Current.GetCreatureById(id);
                    if (creature == null)
                    {
                        throw new Exception("server said that a creature is known, but it's not");
                        return null;
                    }
                    creature.DatId = (ushort)typename;
                }
                else
                {
                    uint removeId = msg.GetU32();
                    Map.Current.RemoveCreatureById(removeId);

                    uint id = msg.GetU32();
                    creature.DatId = (ushort)typename;

                    int creatureType;
                    if (Config.ClientVersion >= 910)
                        creatureType = msg.GetU8();
                    else
                    {
                        if (id >= (int)CreaturesIdRange.PlayerStartId && id < (int)CreaturesIdRange.PlayerEndId)
                            creatureType = (int)CreatureType.CreatureTypePlayer;
                        else if (id >= (int)CreaturesIdRange.MonsterStartId &&
                                 id < (uint)CreaturesIdRange.MonsterEndId)
                            creatureType = (int)CreatureType.CreatureTypeMonster;
                        else
                            creatureType = (int)CreatureType.CreatureTypeNpc;
                    }

                    string name = Creature.FormatCreatureName(msg.GetString());

                    if (id == LocalPlayer.Current.Id)
                        creature = LocalPlayer.Current;
                    else if (creatureType == (int)CreatureType.CreatureTypePlayer)
                    {
                        // fixes a bug server side bug where GameInit is not sent and local player id is unknown
                        if (LocalPlayer.Current.Id == 0 &&
                            name == LocalPlayer.Current.Name)
                            creature = LocalPlayer.Current;
                        else
                            creature = new Player();
                        ;
                    }
                    else if (creatureType == (int)CreatureType.CreatureTypeMonster)
                        creature = new Monster();
                    else if (creatureType == (int)CreatureType.CreatureTypeNpc)
                        creature = new Npc();
                    else
                        throw new Exception("creature type is invalid");

                    if (creature != null)
                    {
                        creature.Id = (int)id;
                        creature.Name = name;

                        Map.Current.AddCreature(creature);
                    }
                }


                int healthPercent = msg.GetU8();
                Direction direction = (Direction)msg.GetU8();
                Outfit outfit = GetOutfit(msg);
                creature.Outfit = outfit;


                Game.DAO.Light light = new Game.DAO.Light();
                light.Intensity = msg.GetU8();
                light.Color = msg.GetU8();

                int speed = msg.GetU16();
                int skull = msg.GetU8();
                int shield = msg.GetU8();

                // emblem is sent only when the creature is not known
                int emblem = -1;
                int icon = -1;
                bool unpass = true;
                byte mark = new byte();

                if (FeatureManager.GetFeature(GameFeature.GameCreatureEmblems) && !known)
                    emblem = msg.GetU8();

                if (FeatureManager.GetFeature(GameFeature.GameThingMarks))
                {
                    msg.GetU8(); // creature type for summons
                }

                if (FeatureManager.GetFeature(GameFeature.GameCreatureIcons))
                {
                    icon = msg.GetU8();
                }

                if (FeatureManager.GetFeature(GameFeature.GameThingMarks))
                {
                    mark = msg.GetU8(); // mark
                    msg.GetU16(); // helpers

                    if (creature != null)
                    {
                        if (mark == 0xff)
                        {
                            creature.HideStaticSquare();
                        }
                        else
                        {
                            creature.StaticSquareColor = ByteColorConverter.from8bit(mark);
                            creature.ShowStaticSquare();
                        }
                    }
                }

                if (Config.ClientVersion >= 854)
                    unpass = msg.GetU8() != 0;

                if (creature != null)
                {
                    creature.HealthPercent = ((byte)healthPercent);
                    creature.Direction = direction;
                    creature.Outfit = (outfit);
                    creature.Speed = ((byte)speed);
                    creature.Skull = ((PlayerSkulls)skull);
                    creature.Shield = ((PlayerShields)shield);
                    creature.Passable = (!unpass);
                    //creature.SetLight(light);
                    if (emblem != -1)
                        creature.Emblem = ((PlayerEmblems)emblem);
                    if (icon != -1)
                        creature.Icon = ((CreatureIcons)icon);

                    if (creature == LocalPlayer.Current &&
                        !LocalPlayer.Current.IsKnown)
                        LocalPlayer.Current.IsKnown = (true);
                }
            }
            else if (typename == (int)ItemOpcode.Creature)
            {
                uint id = msg.GetU32();
                creature = Map.Current.GetCreatureById(id);

                if (creature == null)
                    throw new Exception("invalid creature");

                Direction direction = (Direction)msg.GetU8();
                if (creature != null)
                    creature.Turn(direction);
                //
                if (Config.ClientVersion >= 953)
                {
                    bool unpass = msg.GetU8() != 0;

                    if (creature != null)
                        creature.Passable = (!unpass);
                }
            }
            else
            {
                throw new Exception("invalid creature opcode");
            }

            return creature;
        }

        public static Item GetItem(InputMessage msg, int id = 0)
        {
            if (id == 0)
                id = msg.GetU16();

            Item item = Item.Create((ushort)id);
            if (item.DatId == 0)
            {
                throw new Exception(String.Format("unable to create item with invalid id {0}", id));
            }

            if (FeatureManager.GetFeature(GameFeature.GameThingMarks))
            {
                msg.GetU8(); // mark
            }

                
                if (item.ThingType.IsStackable || item.ThingType.IsFluidContainer || item.ThingType.IsSplash || item.ThingType.IsChargeable)
                    item.SetCountOrSubType(msg.GetU8());

            if (FeatureManager.GetFeature(GameFeature.GameItemAnimationPhase))
            {
                if (item.ThingType.AnimationPhases > 1)
                {
                    // 0x00 => automatic phase
                    // 0xFE => random phase
                    // 0xFF => async phase
                    msg.GetU8();
                }
            }


            return item.DatId == 0 ? null : item;
        }

        public static StaticText GetStaticText(InputMessage msg, int id)
        {
            int colorByte = msg.GetU8();
            Color color = ByteColorConverter.from8bit(colorByte);
            string fontName = msg.GetString();
            string text = msg.GetString();
            StaticText staticText = new StaticText();
            staticText.Text = (text);
            staticText.Color = (color);
            return staticText;
        }

        public static Vector3 GetPosition(InputMessage msg)
        {
            UInt16 x = msg.GetU16();
            UInt16 y = msg.GetU16();
            byte z = msg.GetU8();

            return new Vector3(x, y, z);
        }
    }
}

