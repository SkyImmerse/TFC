using Game.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tibia.ClassicNetwork.Account
{
    public class Account
    {
        internal static Account CurrectAccount;
        internal ushort PremDays;
        internal AccountStatus Status;
        internal SubscriptionStatus SubStatus;
    }
}
