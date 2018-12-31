using Assets.Tibia.UI.GameInterface;
using Game.DAO;
using Game.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public class InventorySystem
    {
        internal static void ProcessInventoryChange(InventorySlot slot, Item i)
        {
            var texture = MapRenderer.ThingToTexture(Map.Current, i);
            GameObject.FindObjectOfType<UIInventory>().Toggle(slot, texture);
        }
    }
}
