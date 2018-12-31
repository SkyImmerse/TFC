using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tibia.ClassicNetwork.Account
{
    public class World
    {
        internal static List<World> Worlds = new List<World>();
        internal string WorldName;
        internal string WorldIp;
        internal ushort WorldPort;
        internal byte PreviewState;
    }
}
