using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using GameClient.Network;
using GameClient.Network.Protocol;
using System.Threading;
using UnityEngine;
using Game.DAO;
using Assets.Tibia.DAO;
using Assets.Tibia.ClassicNetwork.Account;
using Assets;

namespace GameClient
{
    public class LoginServer
    {
        public static readonly LoginServer Instance = new LoginServer();

        private const string RsaN =
       "109120132967399429278860960508995541528237502902798129123468757937266291492576446330739696001110603907230888610072655818825358503429057592827629436413108566029093628212635953836686562675849720620786279431090218017681061521755056710823876476444260558147179707119674283982419152118103759076030616683978566631413";

        
        public TCPClient Connection;
        public TCPCmdHandler Protocol;
        public string SessionKey;
        public string CurrentLogin;
        public string CurrentPassword;


        #region Events
        public delegate void CharacterListDelegate(List<Character> characters, Account account);
        public static event CharacterListDelegate CharacterList;

        public static event Action<string> MOTD;
        public static event Action<string> LoginError;
        public static event Action ConnectionSuccussfull;
        #endregion


        public LoginServer()
        {

            Rsa.N = new BigInteger(RsaN, 10);
            Connection = new TCPClient();
            Connection.OnConnect += OnConnect;
            Protocol = new TCPCmdHandler();
            Connection.SocketEvent += Protocol.ProcessCmd;

            Init();
        }

        public void Init()
        {
            Protocol.IsSkipWorker += (opcode, msg) =>
            {
                Debug.Log((LoginServerOpts)opcode);
                return false;
            };
            Protocol.RegisterCmd((int)LoginServerOpts.LoginServerError, ParseLoginError);
            Protocol.RegisterCmd((int)LoginServerOpts.LoginServerErrorOld, ParseLoginError);
            Protocol.RegisterCmd((int)LoginServerOpts.LoginServerMotd, ParseMotd);
            Protocol.RegisterCmd((int)LoginServerOpts.LoginServerTokenSuccess, ParseTokenSuccess);
            Protocol.RegisterCmd((int)LoginServerOpts.LoginServerTokenError, ParseTokenError);
            Protocol.RegisterCmd((int)LoginServerOpts.LoginServerUpdate, ParseUpdate);
            Protocol.RegisterCmd((int)LoginServerOpts.LoginServerSessionKey, ParseSessionKey);
            Protocol.RegisterCmd((int)LoginServerOpts.LoginServerCharacterList, ParseCharacterList);
            Protocol.RegisterCmd((int)LoginServerOpts.LoginServerExtendedCharacterList, ParseCharacterList);
        }

        private void ParseSessionKey(TCPClient client, InputMessage msg)
        {
            SessionKey = msg.GetString();
        }

        private void ParseCharacterList(TCPClient client, InputMessage msg)
        {
            World.Worlds.Clear();
            Character.Characters.Clear();
            if (Config.ClientVersion > 1010)
            {
                var worldsCount = msg.GetU8();
                for (int i = 0; i < worldsCount; i++)
                {
                    var world = new World();
                    var worldId = msg.GetU8();
                    world.WorldName = msg.GetString();
                    world.WorldIp = msg.GetString();
                    world.WorldPort = msg.GetU16();
                    world.PreviewState = msg.GetU8();
                    World.Worlds.Add(world);
                }

                var charactersCount = msg.GetU8();
                for (int i = 0; i < charactersCount; i++)
                {
                    var character = new Character();
                    var worldId = msg.GetU8();
                    character.Name = msg.GetString();
                    character.WorldName = World.Worlds[worldId].WorldName;
                    character.WorldIp = World.Worlds[worldId].WorldIp;
                    character.WorldPort = World.Worlds[worldId].WorldPort;
                    character.PreviewState = World.Worlds[worldId].PreviewState;
                    Character.Characters.Add(character);

                }
            }
            else
            {
                var charactersCount = msg.GetU8();
                for (int i = 0; i < charactersCount; i++)
                {
                    var character = new Character();
                    character.Name = msg.GetString();
                    character.WorldName = msg.GetString();
                    character.WorldIp = msg.GetU32().ToString();
                    character.WorldPort = msg.GetU16();

                    if (FeatureManager.GetFeature(GameFeature.GamePreviewState))
                    {
                        character.PreviewState = msg.GetU8();
                    }

                    Character.Characters.Add(character);
                }
            }

            var account = new Account();
            if (Config.ClientVersion > 1077) {
                account.Status = (AccountStatus)msg.GetU8();
                account.SubStatus = (SubscriptionStatus)msg.GetU8();

                account.PremDays = (ushort)msg.GetU32();
                if (account.PremDays != 0 && account.PremDays != 65535)
                    account.PremDays = (ushort)Math.Floor((double)(account.PremDays - DateTimeOffset.UtcNow.ToUnixTimeSeconds()) / 86400);
            }
            else {
                account.Status = AccountStatus.Ok;
                account.PremDays = msg.GetU16();
                account.SubStatus = account.PremDays > 0 ? SubscriptionStatus.Premium : SubscriptionStatus.Free;
            }

            Account.CurrectAccount = account;


            CharacterList?.Invoke(Character.Characters, account);
        }

        private void ParseUpdate(TCPClient client, InputMessage msg)
        {
            
        }

        private void ParseTokenError(TCPClient client, InputMessage msg)
        {
            
        }

        private void ParseTokenSuccess(TCPClient client, InputMessage msg)
        {
            
        }

        public bool Connect(string host, int port)
        {
            Connection.IsRun = true;

            return Connection.Connect(host, port);
        }

        void OnConnect()
        {
            Debug.Log("Connected");

            ConnectionSuccussfull?.Invoke();
        }

        public void Login(string username, string password)
        {
            CurrentLogin = username;
            CurrentPassword = password;
            SendLoginPacket(username, password, 0, 0);
        }

        private void SendLoginPacket(string username, string password, uint challengeTimestamp, byte challengeRandom)
        {
            var msg = new OutputMessage(Connection);

            msg.AddU8((byte)ClientOpcodes.ClientEnterAccount);
            msg.AddU16((ushort)10);
            msg.AddU16((ushort)Config.ClientVersion);

            if (FeatureManager.GetFeature(GameFeature.GameClientVersion))
            {
                msg.AddU32((uint)Config.ClientVersion);
            }

            if (FeatureManager.GetFeature(GameFeature.GameContentRevision))
            {
                msg.AddU16(0);
                msg.AddU16(0);
            }
            else
            {
                msg.AddU32(0);
            }
            msg.AddU32(0);

            uint picSignature = 0x56C5DDE7;
            msg.AddU32(picSignature);

            if (FeatureManager.GetFeature(GameFeature.GamePreviewState))
            {
                msg.AddU8(0);
            }

            var offset = msg.GetMessageSize();
            ;
            if (FeatureManager.GetFeature(GameFeature.GameLoginPacketEncryption))
            {
                // first RSA byte must be 0
                msg.AddU8(0);
                // xtea key
                msg.AddU32((uint)LoginServer.Instance.Connection.XteaKey[0]);
                msg.AddU32((uint)LoginServer.Instance.Connection.XteaKey[1]);
                msg.AddU32((uint)LoginServer.Instance.Connection.XteaKey[2]);
                msg.AddU32((uint)LoginServer.Instance.Connection.XteaKey[3]);
            }

            if (FeatureManager.GetFeature(GameFeature.GameAccountNames))
            {
                msg.AddString(username);//login
            }
            else
            {
                //	        msg.AddU32(tonumber(accountName));
            }

            msg.AddString(password); //pwd

            var extended = "";
            if (!string.IsNullOrEmpty(extended))
                msg.AddString(extended);

            var paddingBytes = Rsa.RsaGetSize() - (msg.GetMessageSize() - offset);

            msg.AddPaddingBytes(paddingBytes);

            if (FeatureManager.GetFeature(GameFeature.GameLoginPacketEncryption))
            {
                msg.EncryptRsa();
            }

            if (FeatureManager.GetFeature(GameFeature.GameOGLInformation))
            {
                msg.AddU8(1); // unknown
                msg.AddU8(1); // unknown


                if (Config.ClientVersion >= 1072)
                {
                    msg.AddString(string.Format("{0} {1}", "nVidia", 1));
                }
                else
                {
                    msg.AddString("GeForce");
                }
                msg.AddString("2.0");
            }

            // add RSA encrypted auth token
            if (FeatureManager.GetFeature(GameFeature.GameAuthenticator))
            {
                offset = msg.GetMessageSize();

                // first RSA byte must be 0
                msg.AddU8(0);
                msg.AddString("");//_mAuthenticatorToken

                if (FeatureManager.GetFeature(GameFeature.GameSessionKey))
                {
                    msg.AddU8(false ? (byte)1 : (byte)0);//_mStayLogged
                }

                paddingBytes = Rsa.RsaGetSize() - (msg.GetMessageSize() - offset);
                msg.AddPaddingBytes(paddingBytes, 0xff);

                msg.EncryptRsa();
            }

            if (FeatureManager.GetFeature(GameFeature.GameProtocolChecksum))
                Connection.ChecksumEnabled = true;

            Connection.Send(msg);

            if (FeatureManager.GetFeature(GameFeature.GameLoginPacketEncryption))
                Connection.XteaEncryptionEnabled = true;
        }



        private static void ParseLoginError(TCPClient client, InputMessage msg)
        {
            var error = msg.GetString();
            Debug.Log(error);
            LoginError?.Invoke(error);
        }
        private static void ParseMotd(TCPClient client, InputMessage msg)
        {
            var motd = msg.GetString();
            Debug.Log(motd);
            MOTD?.Invoke(motd);
        }


    }
}
