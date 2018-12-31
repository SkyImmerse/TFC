using Assets.Tibia.DAO.Extensions;
using Assets.Tibia.UI.GameInterface;
using Game.DAO;
using Game.Graphics;
using SkyImmerseEngine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public class Tile
    {
        enum TileConst {
            MaxThings = 10
        }

        internal Vector3 RealPosition;
        internal int Order;

        internal List<Thing> Things = new List<Thing>();
        private List<Creature> WalkingCreatures = new List<Creature>();
        private List<Effect> Effects = new List<Effect>();

        internal bool IsDrawed;

        public Vector3 Position;

        public Vector3 LightPosition;

        private byte DrawElevation;
        private byte MinimapColor;

        public Tile(Vector3 pos)
        {
            this.Position = pos;
        }

        public void Draw(Vector3 dest, int order, Vector3 lightPos)
        {
            if (IsDrawed) return;
            IsDrawed = true;

            RealPosition = dest;
            LightPosition = lightPos;
            Order = order;

            DrawElevation = 0;

            var maxZ = 0;

            for (var i = 0; i < Things.Count; i++)
            {
                var thing = Things[i];

                if (!thing.ThingType.IsGround && !thing.ThingType.IsGroundBorder && !thing.ThingType.IsOnBottom)
                    continue;

                if (thing.ThingType.IsGround ||
                    thing.ThingType.IsGroundBorder ||
                    thing.ThingType.IsOnBottom)
                {
                    var pos = new Vector3(dest.x - (DrawElevation * 0.031f), dest.y + (DrawElevation * 0.031f), dest.z);

                    // FIRST: ground, border, onBottom
                    thing.Sprite = MapRenderer.AddThingToRender(thing, pos, order + i);
                    maxZ = Mathf.Max(maxZ, thing.Order);
                }

                DrawElevation += (byte)thing.ThingType.Elevation;

                if (DrawElevation > (byte)TileMaps.MAX_ELEVATION)
                    DrawElevation = (byte)TileMaps.MAX_ELEVATION;

            }

            var maxZ2 = maxZ;
            // common items in reversed order
            var reversed = Things.ToList();
            reversed.Reverse();
            for (var i = 0; i < reversed.Count; i++)
            {
                var thing = reversed[i];
                if (thing.ThingType.IsOnTop || thing.ThingType.IsOnBottom || thing.ThingType.IsGroundBorder || thing.ThingType.IsGround ||
                    thing is Creature)
                    continue;

                // SECOND: Common items
                var pos = new Vector3(dest.x - (DrawElevation * 0.031f), dest.y + (DrawElevation * 0.031f), dest.z);

                thing.Sprite = MapRenderer.AddThingToRender(thing, pos, maxZ + i);
                maxZ2 = Mathf.Max(maxZ2, thing.Order);

                DrawElevation += (byte)thing.ThingType.Elevation;
                if (DrawElevation > (byte)TileMaps.MAX_ELEVATION)
                    DrawElevation = (byte)TileMaps.MAX_ELEVATION;

            }


            // 2x2
            var maxZ3 = maxZ2;
            // creatures
            for (var i = 0; i < Things.Count; i++)
            {
                var thing = Things[i];
                if (!(thing is Creature))
                    continue;

                // ---4----: Creatures
                var pos = dest;

                thing.Sprite = MapRenderer.AddThingToRender(thing, pos, maxZ2 + i + 1);
                maxZ3 = Mathf.Max(maxZ3, thing.Order);
            }


            // top items;
            for (var i = 0; i < Things.Count; i++)
            {
                var thing = Things[i];
                if (thing.ThingType.IsOnTop)
                {
                    thing.Sprite = MapRenderer.AddThingToRender(thing, dest, maxZ + i + 1);
                }
            }

            foreach (var item in Things)
            {
                if(item is Creature && item.Sprite is SpriteCreature)
                {
                    if (((Creature)item).Information == null)
                    {
                        ((Creature)item).Information = UITextController.Instance.PoolCreatureInfo();
                        ((Creature)item).Information.SetName(((Creature)item).Name);
                        ((Creature)item).Information?.SetInfo(((Creature)item).InformationColor, ((Creature)item).HealthPercent, -1);
                        ((Creature)item).Information.enabled = true;
                        ((Creature)item).Move(item.Tile, item.Tile);
                    }
                   ((Creature)item).Information.Target = ((SpriteCreature)item.Sprite);
                }
            }
        }

        public void Clean()
        {

            foreach (var th in Things.ToList())
            {
                RemoveThing(th, true);
            }
        }

        public void AddWalkingCreature(Creature creature)
        {
            WalkingCreatures.Add(creature);
        }

        internal void RemoveEffect(Effect thing)
        {
            Effects.Remove((Effect)thing);
            thing?.Sprite?.Group?.RemoveThing((Thing)thing);
        }

        public void RemoveWalkingCreature(Creature creature)
        {
            WalkingCreatures.Remove(creature);
        }

        public void AddThing(Thing thing, int stackPos)
        {
            if (thing == null)
                return;

            if (thing is Effect)
            {
                var max = 0;
                if (Things.Count > 0)
                    max = Things.Max(e => e.Order);
                else
                    max = Order;
                if (thing.ThingType.IsTopEffect)
                {
                    Effects.Add((Effect)thing);
                    thing.Sprite = MapRenderer.AddThingToRender(thing, RealPosition, max + Effects.Count + 2);
                }
                else
                {
                    Effects.Insert(0, (Effect)thing);
                    int i = 0;
                    foreach (var item in Effects)
                    {
                        if(item.Sprite == null)
                        {
                            thing.Sprite = MapRenderer.AddThingToRender(thing, RealPosition, max + i + 2);
                        }
                        else item.Sprite.Group.ChangeThingOrder(item, max + i + 2);
                        i++;
                    }
                    
                }
            }
            else
            {
                // priority                                    854
                // 0 - ground,                        -->      -->
                // 1 - ground borders                 -->      -->
                // 2 - bottom (walls),                -->      -->
                // 3 - on top (doors)                 -->      -->
                // 4 - creatures, from top to bottom  <--      -->
                // 5 - items, from top to bottom      <--      <--
                if (stackPos < 0 || stackPos == 255)
                {
                    var priority = thing.StackPriority;

                    // -1 or 255 => auto detect position
                    // -2        => append

                    bool append;
                    if (stackPos == -2)
                        append = true;
                    else
                    {
                        append = (priority <= 3);

                        // newer protocols does not store creatures in reverse order
                        if (Config.ClientVersion >= 854 && priority == 4)
                            append = !append;
                    }


                    for (stackPos = 0; stackPos < Things.Count; ++stackPos)
                    {
                        var otherPriority = Things[stackPos].StackPriority;
                        if ((append && otherPriority > priority) || (!append && otherPriority >= priority))
                            break;
                    }
                }
                else if (stackPos > Things.Count)
                    stackPos = Things.Count;


                Things.Insert(stackPos, thing);


                if (Things.Count > (int)TileConst.MaxThings)
                    RemoveThing(Things[(int)TileConst.MaxThings]);

            }

            thing.Position = Position;
        }

        public bool RemoveThing(Thing thing, bool isRemoveFromRenderer = true)
        {
            if (thing == null)
                return false;

            if (isRemoveFromRenderer)
            {
                thing.Sprite?.Group?.RemoveThing(thing);
            }

            var stackPos = Things.IndexOf(thing);
            Things.Remove(thing);

            return true;
        }

        public Thing GetThing(int stackPos)
        {
            if (stackPos < Things.Count)
                return Things[stackPos];
            return null;
        }

        public int GetThingStackPos(Thing thing)
        {
            return Things.IndexOf(thing);
        }

        public Thing TopThing
        {
            get
            {
                if (IsEmpty)
                    return null;
                foreach (var thing in Things)
                    if (!thing.ThingType.IsGround && !thing.ThingType.IsGroundBorder && !thing.ThingType.IsOnBottom && !thing.ThingType.IsOnTop && !(thing is Creature))
                        return thing;
                return Things[Things.Count - 1];
            }
        }

        public Thing TopLookThing
        {
            get
            {
                if (IsEmpty)
                    return null;

                for (uint i = 0; i < Things.Count; ++i)
                {
                    var thing = Things[(int)i];
                    if (!thing.ThingType.IsIgnoreLook && (!thing.ThingType.IsGround && !thing.ThingType.IsGroundBorder && !thing.ThingType.IsOnBottom && !thing.ThingType.IsOnTop))
                        return thing;
                }

                return Things[0];
            }
        }
        public Thing TopUseThing
        {
            get
            {
                if (IsEmpty)
                    return null;

                for (uint i = 0; i < Things.Count; ++i)
                {
                    var thing = Things[(int)i];
                    if (thing.ThingType.IsForceUse || (!thing.ThingType.IsGround && !thing.ThingType.IsGroundBorder && !thing.ThingType.IsOnBottom && !thing.ThingType.IsOnTop && !(thing is Creature) && !thing.ThingType.IsSplash))
                        return thing;
                }

                for (uint i = 0; i < Things.Count; ++i)
                {
                    var thing = Things[(int)i];
                    if (!thing.ThingType.IsGround && !thing.ThingType.IsGroundBorder && !(thing is Creature) && !thing.ThingType.IsSplash)
                        return thing;
                }

                return Things[0];
            }
        }
        public Thing TopCreature
        {
            get
            {
                Creature creature = null;
                for (uint i = 0; i < Things.Count; ++i)
                {
                    var thing = Things[(int)i];
                    if (thing is LocalPlayer) // return local player if there is no other creature
                        creature = (Creature)thing;
                    else if (thing is Creature && !(thing is LocalPlayer))
                        return ((Creature)thing);
                }

                if (creature == null && WalkingCreatures.Count > 0)
                    creature = WalkingCreatures.First();

                // check for walking creatures in tiles around
                if (creature == null)
                {
                    for (int xi = -1; xi <= 1; ++xi)
                    {
                        for (int yi = -1; yi <= 1; ++yi)
                        {
                            Vector3 pos = Position.Translated(xi, yi);
                            if (pos == Position)
                                continue;

                            Tile tile = Map.Current.GetTile(pos);
                            if (tile != null)
                            {
                                foreach (var c in tile.Creatures)
                                {
                                    if (c.TargetPosition == pos && c.WalkProgress > 0f)
                                    {
                                        creature = c;
                                    }
                                }
                            }
                        }
                    }
                }
                return creature;
            }
        }
        public Thing TopMoveThing
        {
            get
            {
                if (IsEmpty)
                    return null;

                for (var i = 0; i < Things.Count; ++i)
                {
                    var thing = Things[i];
                    if (!thing.ThingType.IsGround && !thing.ThingType.IsGroundBorder && !thing.ThingType.IsOnBottom && !thing.ThingType.IsOnTop && !(thing is Creature))
                    {
                        if (i > 0 && thing.ThingType.IsNotMoveable)
                            return Things[i - 1];
                        return thing;
                    }
                }

                foreach (var thing in Things)
                {
                    if (thing is Creature)
                        return thing;
                }

                return Things[0];
            }
        }
        public Thing TopMultiUseThing
        {
            get
            {
                if (IsEmpty)
                    return null;

                var topCreature = TopCreature;
                if (topCreature != null) return topCreature;

                for (int i = 0; i < Things.Count; ++i)
                {
                    var thing = Things[i];
                    if (thing.ThingType.IsForceUse)
                        return thing;
                }

                for (int i = 0; i < Things.Count; ++i)
                {
                    var thing = Things[i];
                    if (!thing.ThingType.IsGround && !thing.ThingType.IsGroundBorder && !thing.ThingType.IsOnBottom && !thing.ThingType.IsOnTop)
                    {
                        if (i > 0 && thing.ThingType.IsSplash)
                            return Things[i - 1];
                        return thing;
                    }
                }

                for (int i = 0; i < Things.Count; ++i)
                {
                    var thing = Things[i];
                    if (!thing.ThingType.IsGround && !thing.ThingType.IsOnTop)
                        return thing;
                }

                return Things[0];
            }
        }


        public bool IsPathable
        {
            get
            {
                foreach (var thing in Things)
                    if (thing.ThingType.IsNotPathable)
                        return false;
                return true;
            }
        }

        public List<Item> Items
        {
            get {
                List<Item> items = new List<Item>();
                foreach(Thing thing in Things)
        		{
                    if (!(thing is Item))
                        continue;
                    items.Add((Item)thing);
                }
                return items;
            }
        }
        public List<Creature> Creatures
        {
            get
            {
                var creatures = new List<Creature>();
                foreach (var thing in Things)
                {
                    if (thing is Creature)
                        creatures.Add((Creature)thing);
                }
                return creatures;
            }
        }

        public Item Ground
        {
            get
            {
                var firstObject = GetThing(0);
                if (firstObject == null)
                    return null;
                if (firstObject.ThingType.IsGround && firstObject is Item)
                    return (Item)firstObject;
                return null;
            }
        }

        public int GroundSpeed
        {
            get
            {
                var groundSpeed = 100;
                var ground = Ground;
                if (ground != null)
                    groundSpeed = ground.ThingType.GroundSpeed;
                return groundSpeed;
            }
        }

        public byte GetMinimapColorByte()
        {
            byte color = 255; // alpha
            if (MinimapColor != 0)
                return MinimapColor;

            foreach (var thing in Things)
            {
                if (!thing.ThingType.IsGround && !thing.ThingType.IsGroundBorder && !thing.ThingType.IsOnBottom && !thing.ThingType.IsOnTop)
                    break;
                var c = (byte)thing.ThingType.MinimapColor;
                if (c != 0)
                    color = c;
            }
            return color;
        }
        public int ThingCount => Things.Count;

        public bool IsWalkable => isWalkable(false);

        private bool isWalkable(bool ignoreCreatures)
        {
            if (Ground == null)
                return false;

            if (Config.ClientVersion <= 740 && HasElevation(2))
                return false;

            foreach (var thing in Things.ToArray())
            {
                if (thing == null)
                {
                    continue;
                }
                if (thing.ThingType.IsNotWalkable)
                    return false;

                if (!ignoreCreatures)
                {
                    if (thing is Creature)
                    {
                        var creature = (Creature)thing;
                        if (!creature.Passable && creature.CanBeSeen)
                            return false;
                    }
                }
            }
            return true;
        }

        public bool IsFullGround
        {
            get
            {
                var ground = Ground;
                if (ground != null && ground.ThingType.IsFullGround)
                    return true;
                return false;
            }
        }

        public bool IsFullyOpaque
        {
            get
            {
                var firstObject = GetThing(0);
                return firstObject != null && firstObject.ThingType.IsFullGround;
            }
        }

        public bool IsSingleDimension
        {
            get
            {
                if (WalkingCreatures.Count != 0)
                    return false;
                foreach (var thing in Things)
                    if (thing.ThingType.Size.height != 1 || thing.ThingType.Size.width != 1)
                        return false;
                return true;
            }
        }

        public bool IsLookPossible
        {
            get
            {
                foreach (var thing in Things)
                    if (thing.ThingType.IsBlockProjectile)
                        return false;
                return true;
            }
        }

        public bool IsClickable
        {
            get
            {
                var hasGround = false;
                var hasOnBottom = false;
                var hasIgnoreLook = false;
                foreach (var thing in Things)
                {
                    if (thing.ThingType.IsGround)
                        hasGround = true;
                    if (thing.ThingType.IsOnBottom)
                        hasOnBottom = true;
                    if ((hasGround || hasOnBottom) && !hasIgnoreLook)
                        return true;
                }
                return false;
            }
        }

        public bool IsEmpty => Things.Count == 0;

        public bool IsDrawable => Things.Count > 0 || WalkingCreatures.Count > 0;


        public bool MustHookSouth
        {
            get
            {
                foreach (var thing in Things)
                    if (thing.ThingType.IsHookSouth)
                        return true;
                return false;
            }
        }

        public bool MustHookEast
        {
            get
            {
                foreach (var thing in Things)
                    if (thing.ThingType.IsHookEast)
                        return true;
                return false;
            }
        }

        public bool HasCreature
        {
            get
            {
                foreach (var thing in Things)
                    if (thing is Creature)
                        return true;
                return false;
            }
        }
        public bool LimitsFloorsVie() { return LimitsFloorsView(false); }

        public bool LimitsFloorsView(bool isFreeView)
        {
            // ground and walls limits the view
            var firstThing = GetThing(0);

            if (isFreeView)
            {
                if (firstThing != null && !firstThing.ThingType.IsDontHide &&
                    (firstThing.ThingType.IsGround || firstThing.ThingType.IsOnBottom))
                    return true;
            }
            else if (firstThing != null && !firstThing.ThingType.IsDontHide &&
                     (firstThing.ThingType.IsGround || (firstThing.ThingType.IsOnBottom && firstThing.ThingType.IsBlockProjectile)))
                return true;
            return false;
        }

        public bool CanErase => WalkingCreatures.Count == 0 && Things.Count == 0 &&
                   MinimapColor == 0;

        public int Elevation
        {
            get
            {
                int elevation = 0;
                foreach (var thing in Things)
                    if (thing.ThingType.Elevation > 0)
                        elevation++;
                return elevation;
            }
        }

    public bool HasElevation(int elevation = 1)
        {
            var count = 0;
            foreach (var thing in Things)
                if (thing.ThingType.Elevation > 0)
                    count++;
            return count >= elevation;
        }
    }
}
