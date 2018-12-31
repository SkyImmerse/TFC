using Game.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.DAO.Extensions
{
    public static class Vector3Ext
    {
        public static void Translate(this Vector3 vec, int dx, int dy)
        {
            Translate(vec, dx, dy, 0);
        }

        public static void Translate(this Vector3 vec, int dx, int dy, short dz)
        {
            vec.x += dx;
            vec.y += dy;
            vec.z += dz;
        }

        public static Vector3 Translated(this Vector3 vec, int dx, int dy)
        {
            return Translated(vec, dx, dy, 0);
        }

        public static Vector3 Translated(this Vector3 vec, int dx, int dy, short dz)
        {
            Vector3 pos = vec;
            pos.x += dx;
            pos.y += dy;
            pos.z += dz;
            return pos;
        }

         public static bool Up(this Vector3 vec)
        {
            return Up(vec, 1);
        }

        public static bool Up(this Vector3 vec, int n)
        {
            int nz = (int)vec.z - n;
            if (nz >= 0 && nz <= (float) TileMaps.MAX_Z)
            {
                vec.z = (short) nz;
                return true;
            }
            return false;
        }

        public static bool Down(this Vector3 vec)
        {
            return Down(vec, 1);
        }

        public static bool Down(this Vector3 vec, int n)
        {
            int nz = (int)vec.z + n;
            if (nz >= 0 && nz <= (float) TileMaps.MAX_Z)
            {
                vec.z = (short) nz;
                return true;
            }
            return false;
        }
            
        public static bool CoveredUp(this Vector3 vec)
        {
            return CoveredUp(vec, 1);
        }

        public static bool CoveredUp(this Vector3 vec, int n)
        {
            int nx = (int)vec.x + n;
            int ny = (int)vec.y + n;
            int nz = (int)vec.z - n;
            if (nx >= 0 && nx <= 65535 && ny >= 0 && ny <= 65535 && nz >= 0 && nz <= (float) TileMaps.MAX_Z)
            {
                vec.x = nx;
                vec.y = ny;
                vec.z = (short) nz;
                return true;
            }
            return false;
        }

        public static bool CoveredDown(this Vector3 vec)
        {
            return CoveredDown(vec, 1);
        }

        public static bool CoveredDown(this Vector3 vec, int n)
        {
            int nx = (int)vec.x - n;
            int ny = (int)vec.y - n;
            int nz = (int)vec.z + n;
            if (nx >= 0 && nx <= 65535 && ny >= 0 && ny <= 65535 && nz >= 0 && nz <= (float) TileMaps.MAX_Z)
            {
                vec.x = nx;
                vec.y = ny;
                vec.z = (short) nz;
                return true;
            }
            return false;
        }

        public static float GetAngleFromPositions(Vector3 fromPos, Vector3 toPos)
        {
            // Returns angle in radians from 0 to 2Pi. -1 means positions are equal.
            int dx = (int)toPos.x - (int)fromPos.x;
            int dy = (int)toPos.y - (int)fromPos.y;
            if (dx == 0 && dy == 0)
                return -1;

            float angle = Mathf.Atan2(dy * -1, dx);
            if (angle < 0)
                angle += 2 * Mathf.PI;

            return angle;
        }

        public static double GetAngleFromPosition(this Vector3 vec, Vector3 position)
        {
            return GetAngleFromPositions(vec, position);
        }

        public static List<Vector3> TranslatedToDirections(this Vector3 lastPos, List<Direction> dirs)
        {
            List<Vector3> positions = new List<Vector3>();

            if (!lastPos.IsValid())
                return positions;

            positions.Add(lastPos);

            foreach (var dir in dirs)
            {
                lastPos = lastPos.TranslatedToDirection(dir);
                if (!lastPos.IsValid())
                    break;
                positions.Add(lastPos);
            }

            return positions;
        }

public static Direction GetDirectionFromPositions(Vector3 fromPos, Vector3 toPos)
        {
            float angle = GetAngleFromPositions(fromPos, toPos) * Mathf.Rad2Deg;

            if (angle >= 360 - 22.5 || angle < 0 + 22.5)
                return Direction.East;
            else if (angle >= 45 - 22.5 && angle < 45 + 22.5)
                return Direction.NorthEast;
            else if (angle >= 90 - 22.5 && angle < 90 + 22.5)
                return Direction.North;
            else if (angle >= 135 - 22.5 && angle < 135 + 22.5)
                return Direction.NorthWest;
            else if (angle >= 180 - 22.5 && angle < 180 + 22.5)
                return Direction.West;
            else if (angle >= 225 - 22.5 && angle < 225 + 22.5)
                return Direction.SouthWest;
            else if (angle >= 270 - 22.5 && angle < 270 + 22.5)
                return Direction.South;
            else if (angle >= 315 - 22.5 && angle < 315 + 22.5)
                return Direction.SouthEast;
            else
                return Direction.InvalidDirection;
        }

         public static Direction GetDirectionFromPosition(this Vector3 v, Vector3 position)
        {
            return GetDirectionFromPositions(v, position);
        }


        public static bool IsMapPosition(this Vector3 v)
        {
            return (v.x >= 0 && v.y >= 0 && v.z >= 0 && v.x < 65535 && v.y < 65535 && v.z <= (float)TileMaps.MAX_Z);
        }

        public static bool IsInRange(this Vector3 v, Vector3 pos, int xRange, int yRange)
        {
            return Mathf.Abs(v.x - pos.x) <= xRange && Mathf.Abs(v.y - pos.y) <= yRange && v.z == pos.z;
        }

        public static bool IsInRange(this Vector3 v, Vector3 pos, int minXRange, int maxXRange, int minYRange, int maxYRange)
        {
            return (pos.x >= v.x - minXRange && pos.x <= v.x + maxXRange && pos.y >= v.y - minYRange &&
                    pos.y <= v.y + maxYRange && pos.z == v.z);
        }

        public static Vector3 TranslatedToDirection(this Vector3 pos, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    pos.y--;
                    break;
                case Direction.East:
                    pos.x++;
                    break;
                case Direction.South:
                    pos.y++;
                    break;
                case Direction.West:
                    pos.x--;
                    break;
                case Direction.NorthEast:
                    pos.x++;
                    pos.y--;
                    break;
                case Direction.SouthEast:
                    pos.x++;
                    pos.y++;
                    break;
                case Direction.SouthWest:
                    pos.x--;
                    pos.x++;
                    break;
                case Direction.NorthWest:
                    pos.x--;
                    pos.y--;
                    break;
            }
            return pos;
        }

        public static bool IsValid(this Vector3 v)
        {
            return !(v.x == 65535 && v.y == 65535 && v.z == 255);
        }

        public static Vector3 TranslatedToReverseDirection(this Vector3 pos, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    pos.y++;
                    break;
                case Direction.East:
                    pos.x--;
                    break;
                case Direction.South:
                    pos.y--;
                    break;
                case Direction.West:
                    pos.x++;
                    break;
                case Direction.NorthEast:
                    pos.x--;
                    pos.y++;
                    break;
                case Direction.SouthEast:
                    pos.x--;
                    pos.y--;
                    break;
                case Direction.SouthWest:
                    pos.x++;
                    pos.y--;
                    break;
                case Direction.NorthWest:
                    pos.x++;
                    pos.y++;
                    break;
            }
            return pos;
        }

    }
}
