using Assets;
using Assets.Tibia.DAO.Extensions;
using Assets.Tibia.ThingTypeSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.DAO
{
    public static class ThingTypeManager
    {
        public static bool IsLoaded = false;

        public static event Action LoadingComplete;

        public static float DatLoadingProgress;

        public static void Init()
        {
            DatSignature = 0;
            ContentRevision = 0;
            OtbMinorVersion = 0;
            OtbMajorVersion = 0;
            DatLoaded = false;
            XmlLoaded = false;
            OtbLoaded = false;
            for (int i = 0; i < (int) ThingCategory.ThingLastCategory; ++i)
                ThingTypes.Add(i, new Dictionary<int, ThingType> { });

        }

        public static ItemType FindItemTypeByClientId(ushort id)
        {
            if (id == 0 || id >= ReverseItemTypes.Count)
                return null;

            if (ReverseItemTypes[id] != null)
                return ReverseItemTypes[id];
            else
                return null;
        }

        public static void Terminate()
        {
            for (int i = 0; i < (int) ThingCategory.ThingLastCategory; ++i)
                ThingTypes[i].Clear();
        }

        public static void LoadDat(BinaryReader fin)
        {
            DatLoaded = false;
            DatLoadingProgress = 0;
            DatSignature = 0;
            ContentRevision = 0;

            var lastCategory = 0;

            var lastId = 0;

            DatSignature = (int)fin.ReadUInt32();
            ContentRevision = (ushort)(DatSignature);
            var finalCount = 0;
            var current = 0;
            for (var category = 0; category < (int)ThingCategory.ThingLastCategory; ++category)
            {
                var count = fin.ReadUInt16() + 1;
                ThingTypes[category].Clear();
                for (var i = 1; i <= count; ++i)
                    ThingTypes[category].Add(i, null);

                finalCount += count;

            }
            
            for (var category = 0; category < (int)ThingCategory.ThingLastCategory; ++category)
            {
                lastCategory = category;
                ushort firstId = 1;
                if (category == (int)ThingCategory.ThingCategoryItem)
                    firstId = 100;
                for (var id = firstId; id < ThingTypes[category].Count; ++id)
                {
                    lastId = id;
                    var typename = new ThingType();
                    
                    typename.Unserialize(id, (ThingCategory)category, fin);
                    current++;
                    DatLoadingProgress = current / (float)finalCount;
                    ThingTypes[category][id] = typename;
                }
            }
            DatLoaded = true;
            DatLoadingProgress = 0;
            MicroTasks.Dispatch(() =>
            {
                LoadingComplete?.Invoke();
            });
        }


        public static void OpenThingsFile(FileStream fileStream)
        {
            var thread = new Thread(() => LoadDat(new BinaryReader(fileStream, System.Text.Encoding.UTF8)));
            thread.Start();
        }
        public static void AddItemType(ItemType itemType)
        {
            var id = itemType.ServerId;
            ItemTypes[id] = itemType;
        }

        public static ItemType FindItemTypeByName(string name)
        {
            foreach (var it in ItemTypes)
                if (it.Name == name)
                    return it;
            return null;
        }

        public static void LoadOtb(MemoryStream stream)
        {
            try
            {
                var fin = new BinaryReader(stream);

                uint signature = fin.ReadUInt32();

                if (signature != 0)
                    throw new Exception("invalid otb file");

                var root = fin.GetBinaryTree();
                root.BaseStream.Seek(1, SeekOrigin.Current); // otb first byte is always 0

                signature = root.ReadUInt32();
                if (signature != 0)
                    throw new Exception("invalid otb file");

                var rootAttr = root.ReadByte();
                if (rootAttr == 0x01)
                { // OTB_ROOT_ATTR_VERSION
                    var size = root.ReadUInt16();
                    if (size != 4 + 4 + 4 + 128)
                        throw new Exception("invalid otb root attr version size");

                    var otbMajorVersion = root.ReadUInt32();
                    var otbMinorVersion = root.ReadUInt32();
                    root.BaseStream.Seek(4, SeekOrigin.Current); // buildNumber
                    root.BaseStream.Seek(128, SeekOrigin.Current); // description
                }

                ItemTypes.Clear();
                ReverseItemTypes.Clear();

                foreach (var node in root.GetChildren())
                {
                    ItemType itemType = new ItemType();
                    itemType.Unserialize(node);
                    AddItemType(itemType);

                    var clientId = itemType.ClientId;
                    ReverseItemTypes.Add(clientId, itemType);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Failed to load '{0}' (OTB file): {1}", "", e));
            }
        }

        public static List<ItemType> FindItemTypesByName(string name)
        {
            var ret = new List<ItemType>();
            foreach (var it in ItemTypes)
                if (it.Name == name)
                    ret.Add(it);
            return ret;
        }

        public static ItemType GetItemType(ushort id)
        {
            if (id >= ItemTypes.Count || ItemTypes[id] == null)
            {
                return null;
            }
            return ItemTypes[id];
        }

        public static List<ItemType> FindItemTypesByString(string name)
        {
            return ItemTypes.Where((it) => it.Name.Contains(name)).ToList();
        }

        public static ThingType GetThingType(int id, ThingCategory category)
        {
            if (category >= ThingCategory.ThingLastCategory || id >= ThingTypes[(int) category].Count)
            {
                Debug.LogError("TTM: thing not foud " + id + " " + ThingTypes[(int)category].Count + " " + category);
                return null;
            }
            return ThingTypes[(int) category][id];
        }

        public static ThingType RawGetThingType(int id, ThingCategory category)
        {
            return ThingTypes[(int) category][id];
        }


        public static List<ThingType> FindThingTypeByAttr(ThingAttr attr, ThingCategory category)
        {
            List<ThingType> ret = new List<ThingType>();
            foreach (ThingType typename in ThingTypes[(int) category].Values)
                if (typename.HasAttr(attr))
                    ret.Add(typename);
            return ret;
        }


        public static Dictionary<int, ThingType> GetThingTypes(ThingCategory category)
        {
            List<ThingType> ret = new List<ThingType>();
            if (category >= ThingCategory.ThingLastCategory)
                throw new Exception(String.Format("invalid thing type category {0}", category));
            return ThingTypes[(int)category];
        }
        public static bool IsValidOtbId(ushort id)
        {
            return id >= 1 && id < ItemTypes.Count;
        }
        public static bool IsValidDatId(int id, ThingCategory category)
        {
            return id >= 1 && id < ThingTypes[(int) category].Count;
        }
        private static Dictionary<int, Dictionary<int, ThingType>> ThingTypes =
            new Dictionary<int, Dictionary<int, ThingType>>();
        private static Dictionary<int, ItemType> ReverseItemTypes = new Dictionary<int, ItemType>();
        private static List<ItemType> ItemTypes = new List<ItemType>();


        public static bool DatLoaded;
        public static bool XmlLoaded;
        public static bool OtbLoaded;

        public static int OtbMinorVersion;
        public static int OtbMajorVersion;
        public static int DatSignature;
        public static int ContentRevision;
    }

}