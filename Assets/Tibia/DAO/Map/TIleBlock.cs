using Game.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public class TileBlock
    {
        public Tile[] Tiles = new Tile[(int)TileBlocksSize.BlockSize * (int)TileBlocksSize.BlockSize];

        public Tile Create(Vector3 pos)
        {
            var tile = Tiles[(int)GetTileIndex(pos)];
            tile = new Tile(pos);
            return tile;
        }

        public Tile GetOrCreate(Vector3 pos)
        {
            if (Tiles[(int)GetTileIndex(pos)] == null)
                Tiles[(int)GetTileIndex(pos)] = new Tile(pos);
            return Tiles[(int)GetTileIndex(pos)];
        }

        public Tile Get(Vector3 pos)
        {
            return Tiles[(int)GetTileIndex(pos)];
        }

        public void Remove(Vector3 pos)
        {
            if (Tiles[(int)GetTileIndex(pos)] != null)
            {
                foreach (var thing in Tiles[(int)GetTileIndex(pos)].Things.ToList())
                {
                    Tiles[(int)GetTileIndex(pos)].RemoveThing(thing);
                }
            }
            Tiles[(int)GetTileIndex(pos)] = null;
        }

        public uint GetTileIndex(Vector3 pos)
        {
            return (uint)(((pos.y % (float)TileBlocksSize.BlockSize) * (float)TileBlocksSize.BlockSize) +
                           (pos.x % (float)TileBlocksSize.BlockSize));
        }

    }
}
