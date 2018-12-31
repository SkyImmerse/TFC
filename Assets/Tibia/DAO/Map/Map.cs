using Assets.Tibia.DAO.Extensions;
using Game.DAO;
using Game.Graphics;
using GameClient.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public class Map
    {
        public static List<Map> Maps = new List<Map>()
        {
             new Map()
        };
        public static Map Current => Maps.FirstOrDefault();

        public bool IsKnown = false;
        private Vector3 _centralPosition;
        public Vector3 CentralPosition
        {
            set {
                _centralPosition = value;
                RemoveUnawareThings();
            }
            get => _centralPosition;
        }
        public AwareRange AwareRange;

        private Rect TilesRect = new Rect();

        public Dictionary<uint, TileBlock>[] TileBlocks =
          new Dictionary<uint, TileBlock>[(uint)TileMaps.MAX_Z + 1];

        public Dictionary<uint, Creature> KnownCreatures = new Dictionary<uint, Creature>();
        public Dictionary<int, List<Missile>> FloorMissiles = new Dictionary<int, List<Missile>>();
        public List<AnimatedText> AnimatedTexts = new List<AnimatedText>();
        public List<StaticText> StaticTexts = new List<StaticText>();


        public Map()
        {
            Clean();
            ResetAwareRange();
        }


        public void CleanDynamicThings()
        {
            foreach (var pair in KnownCreatures)
            {
                var creature = pair.Value;
                RemoveThing(creature);
            }
            KnownCreatures.Clear();

            FloorMissiles.Clear();
            for (int i = 0; i <= (int)TileMaps.MAX_Z; ++i)
                FloorMissiles.Add(i, new List<Missile>());

            CleanTexts();
        }
        public void CleanTexts()
        {
            AnimatedTexts.Clear();
            StaticTexts.Clear();
        }

        public void Clean()
        {
            CleanDynamicThings();
            for (var i = 0; i < TileBlocks.Length; i++)
            {
                TileBlocks[i] = new Dictionary<uint, TileBlock>();
            }
            TilesRect = new Rect(0, 0, 65534, 65534);
        }

        public void ResetAwareRange()
        {
            AwareRange = new AwareRange()
            {
                Left = (int)8,
                Top = (int)6,
                Bottom = (int)7,
                Right = (int)9,
            };
        }

        internal void AddThing(Thing thing, Vector3 pos, int stackPos)
        {
            if (thing == null)
                return;
            
            if (thing is Item || thing is Creature || thing is Effect)
            {
                var tile = GetOrCreateTile(pos);
                if (tile != null)
                    tile.AddThing(thing, stackPos);
            }
            else
            {
                if (thing is Missile)
                {
                    thing.Sprite = MapRenderer.AddThingToRender(thing, pos, 100);
                    FloorMissiles[(int)pos.z].Add((Missile)thing);
                }
                else if (thing is AnimatedText)
                {
                    // this code will stack animated texts of the same color
                    var animatedText = (AnimatedText)thing;
                    AnimatedText prevAnimatedText = null;
                    bool merged = false;
                    foreach(var other in AnimatedTexts)
                    {
                        if (other.Position == pos)
                        {
                            prevAnimatedText = other;
                            if (other.Merge(animatedText))
                            {
                                merged = true;
                                break;
                            }
                        }
                    }
                    if (!merged)
                    {
                        if (prevAnimatedText != null)
                        {
                            var offset = prevAnimatedText.Offset;
                            float t = prevAnimatedText.TicksElapsed;
                            if (t < (float)TileMaps.ANIMATED_TEXT_DURATION / 4f) // didnt move 12 pixels
                            {
                                int y = (int)(12 - 48 * t / (float)TileMaps.ANIMATED_TEXT_DURATION);
                                offset += new Vector2(0, y);
                            }
                            offset.y = Math.Min(offset.y, 12);
                            animatedText.Offset = offset;
                        }
                        AnimatedTexts.Add(animatedText);
                    }
                }
                else if (thing is StaticText)
                {
                    var staticText = (StaticText)thing;
                    bool mustAdd = true;
                    foreach(var other in StaticTexts)
                    {
                        // try to combine messages
                        if (other.Position == pos && other.AddMessage(staticText.Name, staticText.MessageMode, staticText.FirstMessage))
                        {
                            mustAdd = false;
                            break;
                        }
                    }

                    if (mustAdd)
                    {
                        StaticTexts.Add(staticText);
                    }
                    else
                        return;
                }

                thing.OnAppear();
            }
           
        }

        public Tile GetOrCreateTile(Vector3 pos)
        {
            ;
            if (!pos.IsMapPosition())
                return null;

            if (pos.x < TilesRect.xMin)
                TilesRect.xMin = (pos.x);
            if (pos.y < TilesRect.yMin)
                TilesRect.yMin = (pos.y);
            if (pos.x > TilesRect.xMax)
                TilesRect.xMax = (pos.x);
            if (pos.y > TilesRect.yMax)
                TilesRect.yMax = (pos.y);

            var index = GetBlockIndex(pos);
            if (!TileBlocks[(uint)pos.z].ContainsKey(index))
            {
                TileBlocks[(uint)pos.z].Add(index, new TileBlock());
            }
            var block = TileBlocks[(uint)pos.z][index];
            return block.GetOrCreate(pos);
        }

        public Tile GetTile(Vector3 pos)
        {
            if (!pos.IsMapPosition())
                return null;
            if (!TileBlocks[(int)pos.z].ContainsKey(GetBlockIndex(pos)))
                return null;
            var it = TileBlocks[(int)pos.z][GetBlockIndex(pos)];
            if (it != null)
                return it.Get(pos);
            return null;
        }

        internal bool RemoveThing(Thing thing)
        {
            return RemoveThing(thing, true);
        }

        internal bool RemoveThing(Thing thing, bool v)
        {
            if (thing == null)
                return false;

            var ret = false;
            if (thing is Missile)
            {
                var missile = (Missile)thing;
                int z = (int)missile.Position.z;
                if (FloorMissiles[z].Contains(missile))
                {
                    missile.Sprite?.Group?.RemoveThing(thing);
                    FloorMissiles[z].Remove(missile);
                    ret = true;
                }
            }
            else if (thing is AnimatedText)
            {
                var animatedText = (AnimatedText)thing;
                animatedText.Destroy();
                if (AnimatedTexts.Contains(animatedText))
                {
                    AnimatedTexts.Remove(animatedText);
                    ret = true;
                }
            }
            else if (thing is StaticText)
            {
                var staticText = (StaticText)thing;
                if (StaticTexts.Contains(staticText))
                {
                    StaticTexts.Remove(staticText);
                    ret = true;
                }
            }
            else if (thing.Tile != null)
                ret = thing.Tile.RemoveThing(thing, v);

            return ret;
        }

        private uint GetBlockIndex(Vector3 pos)
        {
            return (uint)(((pos.y / (float)TileBlocksSize.BlockSize) *
                            (65536 / (float)TileBlocksSize.BlockSize)) +
                           (pos.x / (float)TileBlocksSize.BlockSize));
        }


        internal bool IsLookPossible(object pos)
        {
            return false;
        }

        public bool CleanTile(Vector3 pos)
        {
            if (!pos.IsMapPosition())
                return false;
            if (!TileBlocks[(int)pos.z].ContainsKey(GetBlockIndex(pos)))
            {
                return false;
            }
            var it = TileBlocks[(int)pos.z][GetBlockIndex(pos)];
            if (it != null)
            {
                // remove static texts from tiles that we are not aware anymore
                foreach (var staticText in StaticTexts.ToList())
                {
                    if (staticText.MessageMode == MessageMode.None && staticText.Position == pos)
                        StaticTexts.Remove(staticText);
                }
                var block = it;
                var tile = block.Get(pos);
                if (tile != null)
                {
                    tile.Clean();
                    if (tile.CanErase)
                        block.Remove(pos);

                    return true;
                }

                return false;
            }


            return false;
        }

        internal Thing GetThing(Vector3 pos, byte stackpos)
        {
            if (GetTile(pos) != null)
                return GetTile(pos).GetThing(stackpos);
            return null;
        }
        public void AddCreature(Creature creature)
        {
            KnownCreatures.Add((uint)creature.Id, creature);
        }

        public Creature GetCreatureById(uint id)
        {
            var it = KnownCreatures.ContainsKey(id);
            if (!it)
                return null;
            return KnownCreatures[id];
        }

        public void RemoveCreatureById(uint id)
        {
            if (id == 0)
                return;

            if (KnownCreatures.ContainsKey(id))
                KnownCreatures.Remove(id);
        }

        public void RemoveUnawareThings()
        {
            //// remove creatures from tiles that we are not aware of anymore
            //foreach (var pair in KnownCreatures)
            //{
            //    var creature = pair.Value;
            //    if (!IsAwareOfPosition(creature.Position))
            //        RemoveThing(creature);
            //}

            //// remove static texts from tiles that we are not aware anymore
            //foreach (var staticText in StaticTexts.ToList())
            //{
            //    if (staticText.MessageMode == MessageMode.MessageNone && !IsAwareOfPosition(staticText.Position))
            //        StaticTexts.Remove(staticText);
            //}

            //if (!FeatureManager.GetFeature(GameFeature.GameKeepUnawareTiles))
            //{
            //    // remove tiles that we are not aware anymore
            //    for (var z = 0; z <= (int)TileMaps.MAX_Z; ++z)
            //    {
            //        var tileBlocks = TileBlocks[z];
            //        foreach (var it in tileBlocks.ToList())
            //        {
            //            var block = it.Value;
            //            var blockEmpty = true;
            //            foreach (var tile in block.Tiles)
            //            {
            //                if (tile == null)
            //                    continue;

            //                var pos = tile.Position;
            //                if (!IsAwareOfPosition(pos))
            //                {
            //                    block.Remove(pos);
            //                }
            //                else
            //                    blockEmpty = false;
            //            }

            //            if (blockEmpty)
            //                tileBlocks.Remove(it.Key);
            //            else
            //                continue;
            //        }
            //    }
            //}
        }

        class Node
        {
            public Node(Vector3 pos)
            {
                cost = 0;
                totalCost = 0;
                this.pos = pos;
                prev = null;
                dir = Direction.InvalidDirection;
            }
            public float cost;
            public float totalCost;
            public Vector3 pos;
            public Node prev;
            public Direction dir;
        };


        internal Tuple<List<Direction>, PathFindResult> FindPath(Vector3 startPos, Vector3 goalPos, int maxComplexity, PathFindFlags flags)
        {
            // pathfinding using A* search algorithm
            // as described in http://en.wikipedia.org/wiki/A*_search_algorithm




            List<Direction> dirs = new List<Direction>();
            PathFindResult result = new PathFindResult();

            result = PathFindResult.PathFindResultNoWay;

            if (startPos == goalPos)
            {
                result = PathFindResult.PathFindResultSamePosition;
                return new Tuple<List<Direction>, PathFindResult>(dirs, result);
            }

            if (startPos.z != goalPos.z)
            {
                result = PathFindResult.PathFindResultImpossible;
                return new Tuple<List<Direction>, PathFindResult>(dirs, result);
            }

            // check the goal pos is walkable
            if (IsAwareOfPosition(goalPos))
            {
                Tile goalTile = GetTile(goalPos);
                if (goalTile == null || !goalTile.IsWalkable)
                {
                    return new Tuple<List<Direction>, PathFindResult>(dirs, result);
                }
            }
            else
            {
                //const MinimapTile& goalTile = g_minimap.getTile(goalPos);
                //if(goalTile.HasFlag(MinimapTileNotWalkable)) {
                //    return new Tuple<List<Direction>, PathFindResult>(dirs, result);
                //}
            }

            Dictionary<Vector3, Node> nodes = new Dictionary<Vector3, Node>();
            Queue<KeyValuePair<Node, float>> searchList = new Queue<KeyValuePair<Node, float>>();
            Node currentNode = new Node(startPos);
            currentNode.pos = startPos;
            nodes[startPos] = currentNode;
            Node foundNode = null;
            while (currentNode != null)
            {
                if ((int)nodes.Count > maxComplexity)
                {
                    result = PathFindResult.PathFindResultTooFar;
                    break;
                }

                // path found
                if (currentNode.pos == goalPos && (foundNode == null || currentNode.cost < foundNode.cost))
                    foundNode = currentNode;

                // cost too high
                if (foundNode != null && currentNode.totalCost >= foundNode.cost)
                    break;

                for (int i = -1; i <= 1; ++i)
                {
                    for (int j = -1; j <= 1; ++j)
                    {
                        if (i == 0 && j == 0)
                            continue;

                        bool wasSeen = false;
                        bool hasCreature = false;
                        bool isNotWalkable = true;
                        bool isNotPathable = true;
                        int speed = 100;

                        Vector3 neighborPos = currentNode.pos.Translated(i, j);
                        if (IsAwareOfPosition(neighborPos))
                        {
                            wasSeen = true;
                            var tile = GetTile(neighborPos);
                            if (tile != null)
                            {
                                hasCreature = tile.HasCreature;
                                isNotWalkable = !tile.IsWalkable;
                                isNotPathable = !tile.IsPathable;
                                speed = tile.GroundSpeed;
                            }
                        }
                        else
                        {
                            //const MinimapTile& mtile = g_minimap.getTile(neighborPos);
                            //wasSeen = mtile.hasFlag(MinimapTileWasSeen);
                            //isNotWalkable = mtile.hasFlag(MinimapTileNotWalkable);
                            //isNotPathable = mtile.hasFlag(MinimapTileNotPathable);
                            //if(isNotWalkable || isNotPathable)
                            //    wasSeen = true;
                            //speed = mtile.getSpeed();
                        }

                        float walkFactor = 0;
                        if (neighborPos != goalPos)
                        {
                            if (!flags.HasFlag(PathFindFlags.PathFindAllowNotSeenTiles) && !wasSeen)
                                continue;
                            if (wasSeen)
                            {
                                if (!flags.HasFlag(PathFindFlags.PathFindAllowCreatures) && hasCreature)
                                    continue;
                                if (!flags.HasFlag(PathFindFlags.PathFindAllowNonPathable) && isNotPathable)
                                    continue;
                                if (!flags.HasFlag(PathFindFlags.PathFindAllowNonWalkable) && isNotWalkable)
                                    continue;
                            }
                        }
                        else
                        {
                            if (!flags.HasFlag(PathFindFlags.PathFindAllowNotSeenTiles) && !wasSeen)
                                continue;
                            if (wasSeen)
                            {
                                if (!flags.HasFlag(PathFindFlags.PathFindAllowNonWalkable) && isNotWalkable)
                                    continue;
                            }
                        }

                        Direction walkDir = currentNode.pos.GetDirectionFromPosition(neighborPos);
                        if (walkDir >= Direction.NorthEast)
                            walkFactor += 3.0f;
                        else
                            walkFactor += 1.0f;

                        float cost = currentNode.cost + (speed * walkFactor) / 100.0f;

                        Node neighborNode;
                        if (!nodes.ContainsKey(neighborPos))
                        {
                            neighborNode = new Node(neighborPos);
                            nodes.Add(neighborPos, neighborNode);
                        }
                        else
                        {
                            neighborNode = nodes[neighborPos];
                            if (neighborNode.cost <= cost)
                                continue;
                        }

                        neighborNode.prev = currentNode;
                        neighborNode.cost = cost;
                        neighborNode.totalCost = neighborNode.cost + Vector3.Distance(neighborPos, goalPos);
                        neighborNode.dir = walkDir;
                        searchList.Enqueue(new KeyValuePair<Node,float>(neighborNode, neighborNode.totalCost));
                    }
                }

                if (searchList.Count > 0)
                {
                    currentNode = searchList.Dequeue().Key;
                }
                else
                    currentNode = null;
            }

            if (foundNode != null)
            {
                currentNode = foundNode;
                while (currentNode != null)
                {
                    dirs.Add(currentNode.dir);
                    currentNode = currentNode.prev;
                }
                dirs.RemoveAt(dirs.Count - 1);
                dirs.Reverse();
                result = PathFindResult.PathFindResultOk;
            }

            foreach (var it in nodes)
                nodes[it.Key] = null;

            return new Tuple<List<Direction>, PathFindResult>(dirs, result);
        }

        public bool IsLookPossible(Vector3 pos)
        {
            var tile = GetTile(pos);
            return tile != null && tile.IsLookPossible;
        }

        public bool IsCovered(Vector3 tilePos, int firstFloor)
        {
            // check for tiles on top of the postion
            while (tilePos.CoveredUp() && (int)tilePos.z >= firstFloor)
            {
                var tile = GetTile(tilePos);
                // the below tile is covered when the above tile has a full ground
                if (tile != null && tile.IsFullGround)
                    return true;
            }
            return false;
        }

        public bool IsCompletelyCovered(Vector3 tilePos, int firstFloor)
        {
            var checkTile = GetTile(tilePos);

            while (tilePos.CoveredUp() && tilePos.z >= firstFloor)
            {
                var covered = true;
                var done = false;
                // check in 2x2 range tiles that has no transparent pixels
                for (var x = 0; x < 2 && !done; ++x)
                {
                    for (var y = 0; y < 2 && !done; ++y)
                    {
                        var tile = GetTile(tilePos.Translated(-x, -y));
                        if (tile == null || !tile.IsFullyOpaque)
                        {
                            covered = false;
                            done = true;
                        }
                        else if (x == 0 && y == 0 && (checkTile == null || checkTile.IsSingleDimension))
                        {
                            done = true;
                        }
                    }
                }
                if (covered)
                    return true;
            }
            return false;
        }

        public bool IsAwareOfPosition(Vector3 pos)
        {
            if (pos.z < GetFirstAwareFloor() || pos.z > GetLastAwareFloor())
                return false;

            var groundedPos = pos;
            while (groundedPos.z != CentralPosition.z)
            {
                if (groundedPos.z > CentralPosition.z)
                {
                    if (groundedPos.x == 65535 || groundedPos.y == 65535
                    ) // When pos == 65535,65535,15 we cant go up to 65536,65536,14
                        break;
                    groundedPos.CoveredUp();
                }
                else
                {
                    if (groundedPos.x == 0 || groundedPos.y == 0) // When pos == 0,0,0 we cant go down to -1,-1,1
                        break;
                    groundedPos.CoveredDown();
                }
            }
            return CentralPosition.IsInRange(groundedPos, AwareRange.Left, AwareRange.Right, AwareRange.Top,
                AwareRange.Bottom);
        }

        public int GetFirstAwareFloor()
        {
            if ((int)CentralPosition.z > (int) TileMaps.SEA_FLOOR)
                return (int)CentralPosition.z - (int)TileMaps.AWARE_UNDEGROUND_FLOOR_RANGE;
            else
                return 0;
        }

        public int GetLastAwareFloor()
        {
            if ((int)CentralPosition.z > (int)TileMaps.SEA_FLOOR)
                return Math.Min((int)CentralPosition.z + (int) TileMaps.AWARE_UNDEGROUND_FLOOR_RANGE,
                    (int)TileMaps.MAX_Z);
            else
                return (int)TileMaps.SEA_FLOOR;
        }

        internal static void ProcessAddAutomapFlag(Vector3 pos, int icon, string description)
        {
            throw new NotImplementedException();
        }

        internal static void ProcessRemoveAutomapFlag(Vector3 pos, int icon, string description)
        {
            throw new NotImplementedException();
        }
    }
}
