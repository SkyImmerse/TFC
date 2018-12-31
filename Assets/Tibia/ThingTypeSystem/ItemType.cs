using Assets.Tibia.DAO;
using Game.DAO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tibia.ThingTypeSystem
{
    enum ItemCategory
    {
        Invalid = 0,
        Ground = 1,
        Container = 2,
        Weapon = 3,
        Ammunition = 4,
        Armor = 5,
        Charges = 6,
        Teleport = 7,
        MagicField = 8,
        Writable = 9,
        Key = 10,
        Splash = 11,
        Fluid = 12,
        Door = 13,
        Deprecated = 14,
        Last = 15
    }

    enum ItemTypeAttr
    {
        ServerId = 16,
        ClientId = 17,
        Name = 18,   // deprecated
        Desc = 19,   // deprecated
        Speed = 20,
        Slot = 21,   // deprecated
        MaxItems = 22,   // deprecated
        Weight = 23,   // deprecated
        Weapon = 24,   // deprecated
        Ammunition = 25,   // deprecated
        Armor = 26,   // deprecated
        MagicLevel = 27,   // deprecated
        MagicField = 28,   // deprecated
        Writable = 29,   // deprecated
        RotateTo = 30,   // deprecated
        Decay = 31,   // deprecated
        SpriteHash = 32,
        MinimapColor = 33,
        Attr07 = 34,
        Attr08 = 35,
        Light = 36,
        Decay2 = 37,   // deprecated
        Weapon2 = 38,   // deprecated
        Ammunition2 = 39,   // deprecated
        Armor2 = 40,   // deprecated
        Writable2 = 41,   // deprecated
        Light2 = 42,
        TopOrder = 43,
        Wrtiable3 = 44,   // deprecated
        WareId = 45,
        Last = 46
    }

    enum ClientVersion
    {
        ClientVersion750 = 1,
        ClientVersion755 = 2,
        ClientVersion760 = 3,
        ClientVersion770 = 3,
        ClientVersion780 = 4,
        ClientVersion790 = 5,
        ClientVersion792 = 6,
        ClientVersion800 = 7,
        ClientVersion810 = 8,
        ClientVersion811 = 9,
        ClientVersion820 = 10,
        ClientVersion830 = 11,
        ClientVersion840 = 12,
        ClientVersion841 = 13,
        ClientVersion842 = 14,
        ClientVersion850 = 15,
        ClientVersion854_OLD = 16,
        ClientVersion854 = 17,
        ClientVersion855 = 18,
        ClientVersion860_OLD = 19,
        ClientVersion860 = 20,
        ClientVersion861 = 21,
        ClientVersion862 = 22,
        ClientVersion870 = 23,
        ClientVersion871 = 24,
        ClientVersion872 = 25,
        ClientVersion873 = 26,
        ClientVersion900 = 27,
        ClientVersion910 = 28,
        ClientVersion920 = 29,
        ClientVersion940 = 30,
        ClientVersion944_V1 = 31,
        ClientVersion944_V2 = 32,
        ClientVersion944_V3 = 33,
        ClientVersion944_V4 = 34,
        ClientVersion946 = 35,
        ClientVersion950 = 36,
        ClientVersion952 = 37,
        ClientVersion953 = 38,
        ClientVersion954 = 39,
        ClientVersion960 = 40,
        ClientVersion961 = 41
    };

    public class ItemType
    {
        ItemCategory Category;

        Dictionary<ItemTypeAttr, object> Attribs = new Dictionary<ItemTypeAttr, object>();
        public ItemType()
        {
            Category = ItemCategory.Invalid;
            for (int i = 0; i < (int)ItemTypeAttr.Last; i++)
            {
                Attribs.Add((ItemTypeAttr)i, null);
            }
        }

        public ushort ServerId
        {
            get => (ushort)Attribs[ItemTypeAttr.ServerId];
            set => Attribs[ItemTypeAttr.ServerId] = value;
        }
        public ushort ClientId
        {
            get => (ushort)Attribs[ItemTypeAttr.ClientId];
            set => Attribs[ItemTypeAttr.ClientId] = value;
        }
        public string Name
        {
            get => (string)Attribs[ItemTypeAttr.Name];
            set => Attribs[ItemTypeAttr.Name] = value;
        }

        public void Unserialize(BinaryReader node)
        {
            Category = (ItemCategory)node.ReadByte();

            node.ReadUInt32(); // flags

            UInt16 lastId = 99;
            while (node.BaseStream.CanRead)
            {
                var attr = (ItemTypeAttr)node.ReadByte();
                if (attr == 0 || (int)attr == 0xFF)
                    break;

                UInt16 len = node.ReadUInt16();
                switch (attr)
                {
                    case ItemTypeAttr.ServerId:
                        {
                            UInt16 serverId = node.ReadUInt16();
                            if (Config.ClientVersion < 960)
                            {
                                if (serverId > 20000 && serverId < 20100)
                                {
                                    serverId -= 20000;
                                }
                                else if (lastId > 99 && lastId != serverId - 1)
                                {
                                    while (lastId != serverId - 1)
                                    {
                                        ItemType tmp = new ItemType();
                                        tmp.ServerId = (lastId++);
                                        ThingTypeManager.AddItemType(tmp);
                                    }
                                }
                            }
                            else
                            {
                                if (serverId > 30000 && serverId < 30100)
                                {
                                    serverId -= 30000;
                                }
                                else if (lastId > 99 && lastId != serverId - 1)
                                {
                                    while (lastId != serverId - 1)
                                    {
                                        ItemType tmp = new ItemType();
                                        tmp.ServerId = (lastId++);
                                        ThingTypeManager.AddItemType(tmp);
                                    }
                                }
                            }
                            ServerId = (serverId);
                            lastId = serverId;
                            break;
                        }
                    case ItemTypeAttr.ClientId:
                        ClientId = node.ReadUInt16();
                        break;
                    case ItemTypeAttr.Name:
                        Name = new string(node.ReadChars(node.ReadUInt16()));
                        break;
                    case ItemTypeAttr.Writable:
                        Attribs[ItemTypeAttr.Writable] = true;
                        break;
                    default:
                        node.BaseStream.Seek(len, SeekOrigin.Current); // skip attribute
                        break;
                }
            }
        }
    }
}
