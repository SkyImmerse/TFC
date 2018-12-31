using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    internal class ContainerSystem
    {
        public static Dictionary<int, Container> Containers = new Dictionary<int, Container>();
        internal static Container GetContainer(int containerId)
        {
            throw new NotImplementedException();
        }

        internal static void ProcessContainerRemoveItem(int containerId, int slot, Item lastItem)
        {
            throw new NotImplementedException();
        }

        internal static void ProcessContainerUpdateItem(int containerId, int slot, Item item)
        {
            throw new NotImplementedException();
        }

        internal static void ProcessContainerAddItem(int containerId, Item item, int slot)
        {
            throw new NotImplementedException();
        }

        internal static void ProcessCloseContainer(int containerId)
        {
            throw new NotImplementedException();
        }

        internal static void ProcessOpenContainer(int containerId, Item containerItem, string name, int capacity, bool hasParent, List<Item> items, bool isUnlocked, bool hasPages, int containerSize, int firstIndex)
        {
            throw new NotImplementedException();
        }

        internal static void Open(Item useThing, Container parentContainer)
        {
            throw new NotImplementedException();
        }

        internal static void BrowseField(Vector3 position)
        {
            throw new NotImplementedException();
        }

        internal static void MoveToParentContainer(Thing lookThing, object p)
        {
            throw new NotImplementedException();
        }

        public static int FindEmptyContainerId()
        {
            var id = 0;

            foreach (var cont in Containers.Where((e) => e.Value != null))
            {
                id++;
            }
            return id;
        }
    }
}