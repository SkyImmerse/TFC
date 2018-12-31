using Assets.Tibia.DAO;
using Assets.Tibia.UI.Shared;
using Game.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Tibia.UI.GameInterface
{
    public class UIHealthInfo : MonoBehaviour
    {
        public UIProgress Health;
        public UIProgress Mana;
        public UIProgress Exp;
        public Transform Conditions;
        public GameObject ConditionPrefab;

        public TMP_Text Soul;
        public TMP_Text Cap;

        const string healthTooltip = "Your character health is {0} out of {1}";
        const string manaTooltip = "Your character mana is {0} out of {1}";

        public void Start()
        {
            ResetController();
        }

        internal void ResetController()
        {
            OnHealthChange(100, 100);
            OnManaChange(100, 100);
            OnLevelChange(1, 100);
            OnSoulChange(0);
            OnFreeCapacityChange(0);
            foreach (Transform item in Conditions)
            {
                GameObject.Destroy(item.gameObject);
            }
        }

        const string experienceTooltip = "You have {0} to advance to level {1}";

        public void Subscribe(LocalPlayer p)
        {
            p.StatsChanged += Current_StatsChanged;
        }

        private void Current_StatsChanged(Player p)
        {
            OnHealthChange(p.Health, p.MaxHealth);
            OnManaChange(p.Mana, p.MaxMana);
            OnLevelChange(p.Level, p.LevelPercent/100f);
            OnSoulChange(p.Soul);
            OnFreeCapacityChange(p.FreeCapacity);
            OnStatsChanged(p.States, p.OldStates);
        }

        public void OnHealthChange(double health, double maxHealth)
        {
            Health.SetProgress(health / (double)maxHealth, health + " / " + maxHealth, string.Format(healthTooltip, health, maxHealth));
        }

        public void OnManaChange(double mana, double maxMana)
        {
            Mana.SetProgress(mana / (double)maxMana, mana + " / " + maxMana, string.Format(manaTooltip, mana, maxMana));
        }

        public void OnLevelChange(double value, double percent)
        {
            Exp.SetProgress(percent, Math.Round(percent*100) + "%", string.Format(experienceTooltip, Math.Round(percent * 100), value+1));
        }

        public void OnSoulChange(double soul)
        {
            Soul.text = "Soul: " + soul;
        }

        public void OnFreeCapacityChange(double value)
        {
            Cap.text = "Cap: " + value;
        }

        public void OnStatsChanged(PlayerStates now, PlayerStates old)
        {
            var bitsChanged = now ^ old;
            for (int i = 1; i < 32; i++)
            {
                var pow = Math.Pow(2, i - 1);
                if (pow > (double)bitsChanged) break;
                var bitChanged = (PlayerStates)bitsChanged & (PlayerStates)pow;
                if(bitChanged != 0)
                {
                    ToggleIcon(bitChanged);
                }
            }
        }

        private void ToggleIcon(PlayerStates bitChanged)
        {
            foreach (Transform item in Conditions)
            {
                if(item.GetComponent<UIConditionIcon>().State == bitChanged)
                {
                    GameObject.Destroy(item.gameObject);
                } else
                {
                    var icon = GameObject.Instantiate(ConditionPrefab, Conditions).GetComponent<UIConditionIcon>();
                    icon.State = bitChanged;
                    icon.SetState();
                }
            }
        }
    }
}
