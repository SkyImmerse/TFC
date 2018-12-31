using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.DAO;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public class StaticText : Thing
    {
        internal string Text;
        internal Color Color;
        internal MessageMode MessageMode;
        internal string FirstMessage;

        public object Name { get; internal set; }

        internal bool AddMessage(object p1, object p2, object p3)
        {
            return true;
        }
    }
}
