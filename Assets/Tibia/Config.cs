using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tibia.DAO
{
    public static class Config
    {
        public static int ClientVersion = 1098;
        public static int ProtocolVersion => ClientVersion;

        public static int Port = 7171;
        public static string Host = "127.0.0.1";

        public static float LoginCheckTimeout = 1;
        public static int LoginCheckAttemptCount = 5;
        public static int ResoltionASPR = 2;

        public static void Load()
        {

        }

        public static void Save()
        {

        }
    }
}
