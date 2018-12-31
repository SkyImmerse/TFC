using Assets.Tibia.ClassicNetwork;
using Game.DAO;
using GameClient.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tibia.Game_Systems
{
    public static class PingSystem
    {
        private static long ping = -1;
        private static uint PingSent;
        private static uint PingReceived;
        private static int PingDelay = 1;
        private static MicroTask token;
        internal static int ServerBeat;

        public static void Start()
        {

            if (FeatureManager.GetFeature(GameFeature.GameClientPing) ||
                FeatureManager.GetFeature(GameFeature.GameExtendedClientPing))
            {
                Ping();
            }
        }

        public static void Ping()
        {
            if (PingReceived != PingSent)
                return;

            GameServer.Instance.SendPing();
            PingSent++;

            if(token!=null) token.Running = false;
            token = MicroTasks.DelayedTask((e) => Ping(), PingDelay);
        }

        internal static void PingBack()
        {
            PingReceived++;

            if (PingReceived == PingSent)
            {
                if (token != null)
                    ping = (int)token.Timer.TicksElapsed;
            }
            else
                UnityEngine.Debug.LogError("got an invalid ping from server " + PingReceived + " " + PingSent);

            MicroTasks.DelayedTask((e) => Ping(), PingDelay);
        }
    }
}
