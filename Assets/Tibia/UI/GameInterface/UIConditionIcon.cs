using Assets.Tibia.UI.Shared;
using Game.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Tibia.UI.GameInterface
{
    [RequireComponent(typeof(UITooltipTrigger))]
    public class UIConditionIcon : MonoBehaviour
    {
        public PlayerStates State;

        public Sprite Poison;

        public Sprite Burn;

        public Sprite Energy;

        public Sprite Drunk;

        public Sprite ManaShield;

        public Sprite Paralyze;

        public Sprite Haste;

        public Sprite Swords;

        public Sprite Drowning;

        public Sprite Freezing;

        public Sprite Dazzled;

        public Sprite Cursed;

        public Sprite PartyBuff;

        public Sprite PzBlock;

        public Sprite Pz;

        public Sprite Bleeding;

        public Sprite Hungry;

        public void SetState()
        {
            if (State == PlayerStates.Poison)
                Set("You are poisoned", Poison);
            if (State == PlayerStates.Burn)
                Set("You are burning", Burn);
            if (State == PlayerStates.Energy)
                Set("You are electrified", Energy);
            if (State == PlayerStates.Drunk)
                Set("You are drunk", Drunk);
            if (State == PlayerStates.ManaShield)
                Set("You are protected by a magic shield", ManaShield);
            if (State == PlayerStates.Paralyze)
                Set("You are paralysed", Paralyze);
            if (State == PlayerStates.Haste)
                Set("You are hasted", Haste);
            if (State == PlayerStates.Swords)
                Set("You may not logout during a fight", Swords);
            if (State == PlayerStates.Drowning)
                Set("You are drowning", Drowning);
            if (State == PlayerStates.Freezing)
                Set("You are freezing", Freezing);
            if (State == PlayerStates.Dazzled)
                Set("You are dazzled", Dazzled);
            if (State == PlayerStates.Cursed)
                Set("You are cursed", Cursed);
            if (State == PlayerStates.PartyBuff)
                Set("You are strengthened", PartyBuff);
            if (State == PlayerStates.PzBlock)
                Set("You may not logout or enter a protection zone", PzBlock);
            if (State == PlayerStates.Pz)
                Set("You are within a protection zone", Pz);
            if (State == PlayerStates.Bleeding)
                Set("You are bleeding", Bleeding);
            if (State == PlayerStates.Hungry)
                Set("You are hungry", Hungry);


        }

        private void Set(string tooltip, Sprite sprite)
        {
            GetComponent<Image>().sprite = sprite;
            GetComponent<UITooltipTrigger>().text = tooltip;
        }
    }
}
