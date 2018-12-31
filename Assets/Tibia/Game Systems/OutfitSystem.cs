using Assets.Tibia.ClassicNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tibia.DAO
{
    class OutfitSystem
    {
        internal static void ProcessOpenOutfitWindow(Outfit currentOutfit, List<Tuple<int, string, int>> outfitList, List<Tuple<int, string>> mountList)
        {
            
        }

        internal static void RequestOutfit()
        {
            GameServer.Instance.SendRequestOutfit();
        }
    }
}
