using Game.DAO;
using GameClient.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public enum ItemAttr
    {
        AttrEnd = 0,
        //ATTR_DESCRIPTION = 1,
        //ATTR_EXT_FILE = 2,
        AttrTileFlags = 3,
        AttrActionId = 4,
        AttrUniqueId = 5,
        AttrText = 6,
        AttrDesc = 7,
        AttrTeleDest = 8,
        AttrItem = 9,
        AttrDepotId = 10,
        //ATTR_EXT_SPAWN_FILE = 11,
        AttrRuneCharges = 12,
        //ATTR_EXT_HOUSE_FILE = 13,
        AttrHousedoorid = 14,
        AttrCount = 15,
        AttrDuration = 16,
        AttrDecayingState = 17,
        AttrWrittendate = 18,
        AttrWrittenby = 19,
        AttrSleeperguid = 20,
        AttrSleepstart = 21,
        AttrCharges = 22,
        AttrContainerItems = 23,
        AttrName = 30,
        AttrPluralname = 31,
        AttrAttack = 33,
        AttrExtraattack = 34,
        AttrDefense = 35,
        AttrExtradefense = 36,
        AttrArmor = 37,
        AttrAttackspeed = 38,
        AttrHitchance = 39,
        AttrShootrange = 40,
        AttrArticle = 41,
        AttrScriptprotected = 42,
        AttrDualwield = 43,
        AttrAttributeMap = 128
    }

    public class Item : Thing
    {
        private ushort ClientId;
        private ushort ServerId;
        private byte CountOrSubType;
        private Dictionary<ItemAttr, object> Attribs = new Dictionary<ItemAttr, object>();
        private List<Item> ContainerItems = new List<Item>();
        private Color Color;
        private bool Async;

        private byte Phase;
        private long LastPhase;

        public Item()
        {
            ClientId = 0;
            ServerId = 0;
            CountOrSubType = 1;
            Color = new Color(0, 0, 0, 1.0f);
            Async = true;
            Phase = 0;
            LastPhase = 0;
        }

        public static Item Create(int id)
        {
            var item = new Item();
            item.SetId((ushort)id);
            return item;
        }

        public void SetId(ushort id)
        {
            if (!ThingTypeManager.IsValidDatId(id, ThingCategory.ThingCategoryItem))
                id = 0;
            //ServerId = ThingTypeManager.FindItemTypeByClientId(id).ServerId;
            ClientId = id;
            DatId = id;
        }

        public void SetOtbId(ushort id)
        {
            if (!ThingTypeManager.IsValidOtbId(id))
                id = 0;
            var itemType = ThingTypeManager.GetItemType(id);
            ServerId = id;

            id = itemType.ClientId;
            if (!ThingTypeManager.IsValidDatId(id, ThingCategory.ThingCategoryItem))
                id = 0;
            ClientId = id;
        }
        public void SetCountOrSubType(int value)
        {
            CountOrSubType = (byte)value;
        }

        public void SetCount(int count)
        {
            CountOrSubType = (byte)count;
        }
        public void SetSubType(int subType)
        {
            CountOrSubType = (byte)subType;
        }

        public int GetSubType()
        {
            if (ThingType.IsSplash || ThingType.IsFluidContainer)
                return CountOrSubType;
            if (Config.ClientVersion > 862)
                return 0;
            return 1;
        }
        public override int Count
        {
            get
            {
                if (ThingType.IsStackable)
                    return CountOrSubType;
                return 1;
            }
        }

        public string GetName()
        {
            return ThingTypeManager.FindItemTypeByClientId(ClientId).Name;
        }
        public bool IsValid()
        {
            return ThingTypeManager.IsValidDatId(ClientId, ThingCategory.ThingCategoryItem);
        }


        public void UnserializeItem(BinaryReader fin)
        {
            try
            {
                while (fin.BaseStream.CanRead)
                {
                    var attrib = (ItemAttr)fin.ReadByte();
                    if (attrib == 0)
                        break;

                    switch (attrib)
                    {
                        case ItemAttr.AttrCount:
                        case ItemAttr.AttrRuneCharges:
                            SetCount(fin.ReadByte());
                            break;
                        case ItemAttr.AttrCharges:
                            SetCount(fin.ReadUInt16());
                            break;
                        case ItemAttr.AttrHousedoorid:
                        case ItemAttr.AttrScriptprotected:
                        case ItemAttr.AttrDualwield:
                        case ItemAttr.AttrDecayingState:
                            Attribs[attrib] = fin.ReadByte();
                            break;
                        case ItemAttr.AttrActionId:
                        case ItemAttr.AttrUniqueId:
                        case ItemAttr.AttrDepotId:
                            Attribs[attrib] = fin.ReadUInt16();
                            break;
                        case ItemAttr.AttrContainerItems:
                        case ItemAttr.AttrAttack:
                        case ItemAttr.AttrExtraattack:
                        case ItemAttr.AttrDefense:
                        case ItemAttr.AttrExtradefense:
                        case ItemAttr.AttrArmor:
                        case ItemAttr.AttrAttackspeed:
                        case ItemAttr.AttrHitchance:
                        case ItemAttr.AttrDuration:
                        case ItemAttr.AttrWrittendate:
                        case ItemAttr.AttrSleeperguid:
                        case ItemAttr.AttrSleepstart:
                        case ItemAttr.AttrAttributeMap:
                            Attribs[attrib]= fin.ReadUInt32();
                            break;
                        case ItemAttr.AttrTeleDest:
                            {
                                Vector3 pos = new Vector3();
                                pos.x = fin.ReadUInt16();
                                pos.y = fin.ReadUInt16();
                                pos.z = fin.ReadByte();
                                Attribs[attrib] = pos;
                                break;
                            }
                        case ItemAttr.AttrName:
                        case ItemAttr.AttrText:
                        case ItemAttr.AttrDesc:
                        case ItemAttr.AttrArticle:
                        case ItemAttr.AttrWrittenby:
                            var length = fin.ReadUInt16();
                            Attribs[attrib] = new string(fin.ReadChars(length));
                            break;
                        default:
                            throw new Exception(string.Format("invalid item attribute {0}", attrib));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Failed to unserialize OTBM item: {0}", e));
            }
        }
        public void SetDepotId(ushort depotId)
        {
            Attribs[ItemAttr.AttrDepotId] = depotId;
        }
        public ushort DepotId => (ushort)Attribs[ItemAttr.AttrDepotId];

        public void SetDoorId(byte doorId)
        {
            Attribs[ItemAttr.AttrHousedoorid] = doorId;
        }
        public byte DoorId => (byte)Attribs[ItemAttr.AttrHousedoorid];

        public ushort UniqueId => (ushort)Attribs[ItemAttr.AttrActionId];

        public ushort ActionId => (ushort)Attribs[ItemAttr.AttrUniqueId];

        public void SetActionId(ushort actionId)
        {
            Attribs[ItemAttr.AttrActionId] = actionId;
        }
        public void SetUniqueId(ushort uniqueId)
        {
            Attribs[ItemAttr.AttrUniqueId] = uniqueId;
        }

        public string Text => (string)Attribs[ItemAttr.AttrText];

        public string Description => (string)Attribs[ItemAttr.AttrDesc];

        public void SetDescription(string desc)
        {
            Attribs[ItemAttr.AttrDesc] = desc;
        }
        public void SetText(string txt)
        {
            Attribs[ItemAttr.AttrText] = txt;
        }

        public Vector3 TeleportDestination =>  (Vector3)Attribs[ItemAttr.AttrTeleDest];

        public void SetTeleportDestination(Vector3 pos)
        {
            Attribs.Add(ItemAttr.AttrTeleDest, pos);
        }
        public Item GetContainerItem(int slot)
        {
            return ContainerItems[slot];
        }
        public void AddContainerItemIndexed(Item i, int slot)
        {
            ContainerItems[slot] = i;
        }
        public void AddContainerItem(Item i)
        {
            ContainerItems.Add(i);
        }
        public void RemoveContainerItem(int slot)
        {
            ContainerItems[slot] = null;
        }
        public void ClearContainerItems()
        {
            ContainerItems.Clear();
        }


        public override void CalculatePatterns(ref int xPattern, ref int yPattern, ref int zPattern)
        {
            // Avoid crashes with invalid items
            if (!IsValid())
                return;

            if (ThingType.IsStackable && ThingType.NumPattern.x == 4 && ThingType.NumPattern.y == 2)
            {
                if (CountOrSubType <= 0)
                {
                    xPattern = 0;
                    yPattern = 0;
                }
                else if (CountOrSubType < 5)
                {
                    xPattern = CountOrSubType - 1;
                    yPattern = 0;
                }
                else if (CountOrSubType < 10)
                {
                    xPattern = 0;
                    yPattern = 1;
                }
                else if (CountOrSubType < 25)
                {
                    xPattern = 1;
                    yPattern = 1;
                }
                else if (CountOrSubType < 50)
                {
                    xPattern = 2;
                    yPattern = 1;
                }
                else
                {
                    xPattern = 3;
                    yPattern = 1;
                }
            }
            else if (ThingType.IsHangable)
            {
                var tile = (Tile)Tile;
                if (tile != null)
                {
                    if (tile.MustHookSouth)
                        xPattern = ThingType.NumPattern.x >= 2 ? 1 : 0;
                    else if (tile.MustHookEast)
                        xPattern = ThingType.NumPattern.x >= 3 ? 2 : 0;
                }
            }
            else if (ThingType.IsSplash || ThingType.IsFluidContainer)
            {
                var color = (int)FluidsColor.FluidTransparent;
                if (FeatureManager.GetFeature(GameFeature.GameNewFluids))
                {
                    switch (CountOrSubType)
                    {
                        case (int)FluidsType.FluidNone:
                            color = (int)FluidsColor.FluidTransparent;
                            break;
                        case (int)FluidsType.FluidWater:
                            color = (int)FluidsColor.FluidBlue;
                            break;
                        case (int)FluidsType.FluidMana:
                            color = (int)FluidsColor.FluidPurple;
                            break;
                        case (int)FluidsType.FluidBeer:
                            color = (int)FluidsColor.FluidBrown;
                            break;
                        case (int)FluidsType.FluidOil:
                            color = (int)FluidsColor.FluidBrown;
                            break;
                        case (int)FluidsType.FluidBlood:
                            color = (int)FluidsColor.FluidRed;
                            break;
                        case (int)FluidsType.FluidSlime:
                            color = (int)FluidsColor.FluidGreen;
                            break;
                        case (int)FluidsType.FluidMud:
                            color = (int)FluidsColor.FluidBrown;
                            break;
                        case (int)FluidsType.FluidLemonade:
                            color = (int)FluidsColor.FluidYellow;
                            break;
                        case (int)FluidsType.FluidMilk:
                            color = (int)FluidsColor.FluidWhite;
                            break;
                        case (int)FluidsType.FluidWine:
                            color = (int)FluidsColor.FluidPurple;
                            break;
                        case (int)FluidsType.FluidHealth:
                            color = (int)FluidsColor.FluidRed;
                            break;
                        case (int)FluidsType.FluidUrine:
                            color = (int)FluidsColor.FluidYellow;
                            break;
                        case (int)FluidsType.FluidRum:
                            color = (int)FluidsColor.FluidBrown;
                            break;
                        case (int)FluidsType.FluidFruidJuice:
                            color = (int)FluidsColor.FluidYellow;
                            break;
                        case (int)FluidsType.FluidCoconutMilk:
                            color = (int)FluidsColor.FluidWhite;
                            break;
                        case (int)FluidsType.FluidTea:
                            color = (int)FluidsColor.FluidBrown;
                            break;
                        case (int)FluidsType.FluidMead:
                            color = (int)FluidsColor.FluidBrown;
                            break;
                        default:
                            color = (int)FluidsColor.FluidTransparent;
                            break;
                    }
                }
                else
                    color = CountOrSubType;

                xPattern = (color % 4) % (int)ThingType.NumPattern.x;
                yPattern = (color / 4) % (int)ThingType.NumPattern.y;
            }
            else
            {
                xPattern = (int)(Position.x) % (int)ThingType.NumPattern.x;
                yPattern = (int)(Position.y) % (int)ThingType.NumPattern.y;
                zPattern = (int)Position.z % (int)ThingType.NumPattern.z;
            }
        }
 
    }
}
