using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tibia.ClassicNetwork.Account
{
    public class Character
    {
        internal static List<Character> Characters = new List<Character>();
        internal string Name;
        internal string WorldName;
        internal string WorldIp;
        internal ushort WorldPort;
        internal byte PreviewState;
    }
}
