using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tibia.DAO
{
    public class TradeSystem
    {
        internal static void ProcessCloseTrade()
        {
            throw new NotImplementedException();
        }

        internal static void ProcessCounterTrade(string name, List<Item> items)
        {
            throw new NotImplementedException();
        }

        internal static void ProcessOwnTrade(string name, List<Item> items)
        {
            throw new NotImplementedException();
        }

        internal static void ProcessCloseNpcTrade()
        {
            throw new NotImplementedException();
        }

        internal static void ProcessOpenNpcTrade(List<Tuple<Item, string, int, int, int>> items)
        {
            throw new NotImplementedException();
        }

        internal static void ProcessPlayerGoods(ulong money, List<Tuple<Item, int>> goods)
        {
            throw new NotImplementedException();
        }
    }
}
