using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.DAO;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Tibia.UI.GameInterface
{
    public class UIInventory : MonoBehaviour
    {
        public RawImage SlotHead;
        public RawImage SlotNecklace;
        public RawImage SlotBackpack;
        public RawImage SlotArmor;
        public RawImage SlotRight;
        public RawImage SlotLeft;
        public RawImage SlotLegs;
        public RawImage SlotFeet;
        public RawImage SlotRing;
        public RawImage SlotAmmo;
        public RawImage SlotPurse;
        public void Toggle(InventorySlot slot, Texture2D texture)
        {
            var emptyColor = new Color(0, 0, 0, 0);
            var fillColor = new Color(1, 1, 1, 1);
            if (slot == InventorySlot.SlotHead)
            {
                SlotHead.color = texture != null ? fillColor : emptyColor;
                SlotHead.texture = texture;
            }
            else if (slot == InventorySlot.SlotNecklace)
            {
                SlotNecklace.color = texture != null ? fillColor : emptyColor;
                SlotNecklace.texture = texture;
            }
            else if (slot == InventorySlot.SlotBackpack)
            {
                SlotBackpack.color = texture != null ? fillColor : emptyColor;
                SlotBackpack.texture = texture;
            }
            else if (slot == InventorySlot.SlotArmor)
            {
                SlotArmor.color = texture != null ? fillColor : emptyColor;
                SlotArmor.texture = texture;
            }
            else if (slot == InventorySlot.SlotRight)
            {
                SlotRight.color = texture != null ? fillColor : emptyColor;
                SlotRight.texture = texture;
            }
            else if (slot == InventorySlot.SlotLeft)
            {
                SlotLeft.color = texture != null ? fillColor : emptyColor;
                SlotLeft.texture = texture;
            }
            else if (slot == InventorySlot.SlotLegs)
            {
                SlotLegs.color = texture != null ? fillColor : emptyColor;
                SlotLegs.texture = texture;
            }
            else if (slot == InventorySlot.SlotFeet)
            {
                SlotFeet.color = texture != null ? fillColor : emptyColor;
                SlotFeet.texture = texture;
            }
            else if (slot == InventorySlot.SlotRing)
            {
                SlotRing.color = texture != null ? fillColor : emptyColor;
                SlotRing.texture = texture;
            }
            else if (slot == InventorySlot.SlotAmmo)
            {
                SlotAmmo.color = texture != null ? fillColor : emptyColor;
                SlotAmmo.texture = texture;
            }
            else if (slot == InventorySlot.SlotPurse)
            {
                SlotPurse.color = texture != null ? fillColor : emptyColor;
                SlotPurse.texture = texture;            }


        }
    }
}
