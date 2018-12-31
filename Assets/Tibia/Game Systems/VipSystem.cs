using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tibia.DAO
{
    class VipSystem
    {
        public static bool PartyShareExperience { get; internal set; }

        internal static void ProcessVipStateChange(uint id, int v)
        {
            
        }

        internal static void ProcessVipAdd(uint id, string name, uint status, string desc, uint iconId, bool notifyLogin)
        {
            
        }

        internal static void PartyLeave()
        {
            throw new NotImplementedException();
        }

        internal static void AddVip(string creatureName)
        {
            throw new NotImplementedException();
        }

        internal static void PartyJoin(int id)
        {
            throw new NotImplementedException();
        }

        internal static void PartyInvite(int id)
        {
            throw new NotImplementedException();
        }

        internal static void PartyRevokeInvitation(int id)
        {
            throw new NotImplementedException();
        }

        internal static void PartyPassLeadership(int id)
        {
            throw new NotImplementedException();
        }
    }
}
