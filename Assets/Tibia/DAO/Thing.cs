using Game.DAO;
using SkyImmerseEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public class Thing
    {
        public virtual void OnPositionChange() { }

        public Vector3 Position;
        private int __datId = -1;
        internal int DatId
        {
            get
            {
                if(__datId < 0)
                {
                    throw new Exception("FATAL ERROR - ACCESS TO THING WITHOUT DATID");
                }
                return __datId;
            }
            set
            {
                __datId = value;
            }
        }
        /// <summary>
        /// Server shared tibia stack pos
        /// </summary>
        internal int StackPos
        {
            get
            {
                if (Position.x == 65535 && this is Item) // is inside a container
                    return (int)Position.z;
                else
                {
                    if (Tile == null)
                    {
                        return 0;
                    }
                    return Tile.GetThingStackPos(this);
                }
            }
        }
        /// <summary>
        /// Tibia Unity Engine CLIENT order
        /// </summary>
        internal int Order;
        /// <summary>
        /// Current Tile
        /// </summary>
        internal Tile Tile =>  Map.Current.GetTile(Position);


        public Container ParentContainer
        {
            get
            {
                if (Position.x == 0xffff /*&& Position.Y & 0x40*/)
                {
                    int containerId = (int)Math.Pow(Position.y, 0x40);
                    return ContainerSystem.GetContainer(containerId);
                }
                return null;
            }
        }

        public int StackPriority
        {
            get
            {
                if (ThingType.IsGround)
                    return 0;
                else if (ThingType.IsGroundBorder)
                    return 1;
                else if (ThingType.IsOnBottom)
                    return 2;
                else if (ThingType.IsOnTop)
                    return 3;
                else if (this is Creature)
                    return 4;
                else // common items
                    return 5;
            }
        }


        internal virtual void OnAppear()
        {
            
        }

        internal virtual void OnDisappear()
        {
            
        }

        public virtual int Count => 0;

        public virtual void CalculatePatterns(ref int xPattern, ref int yPattern, ref int zPattern)
        {

        }
        private ThingType _cacheThingType;
        public virtual ThingType ThingType
        {
            get
            {
                if(_cacheThingType == null)
                    _cacheThingType = ThingTypeManager.GetThingType((ushort)DatId, (this is Item) ? ThingCategory.ThingCategoryItem : (this is Creature) ? ThingCategory.ThingCategoryCreature : (this is Missile) ? ThingCategory.ThingCategoryMissile : (this is Effect) ? ThingCategory.ThingCategoryEffect : ThingCategory.ThingInvalidCategory);

                return _cacheThingType;
            }
        }
        internal SkyImmerseEngine.Sprite Sprite;
        internal Vector3 RealPosition;
    }
}
