using Game.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tibia.DAO
{
    public class Player : Creature
    {
        public event Action<Player> StatsChanged;
        public void InvokeStatsChanged()
        {
            StatsChanged?.Invoke(this);
        }
        internal bool Premium;
        internal List<int> Spells;
        internal int Vocation;
        internal double Health;
        internal double MaxHealth;
        internal double FreeCapacity;
        internal double TotalCapacity;
        internal double Experience;
        internal double Level;
        internal double LevelPercent;
        internal double Mana;
        internal double MaxMana;
        internal double MagicLevel;
        internal double MagicLevelPercent;
        internal double BaseMagicLevel;
        internal double Stamina;
        internal double Soul;
        internal double RegenerationTime;
        internal double OfflineTrainingTime;
        private PlayerStates states;
        public PlayerStates OldStates;
        internal PlayerStates States
        {
            set
            {
                OldStates = states;
                states = value;
            }
            get => states;
        }

       

        internal void SetSkill(Skill skill, int level, int levelPercent)
        {
            
        }

        internal void SetBaseSkill(Skill skill, int baseLevel)
        {
            
        }

    }
}
