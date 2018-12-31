using System;
using Game.DAO;
using SkyImmerseEngine.Utils;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public class Outfit
    {
        internal Vector4 LegsColor => ByteColorConverter.GetColor(LegsColorByte);
        internal Vector4 FeetColor => ByteColorConverter.GetColor(FeetColorByte);
        internal Vector4 BodyColor => ByteColorConverter.GetColor(BodyColorByte);
        internal Vector4 HeadColor => ByteColorConverter.GetColor(HeadColorByte);

        internal int LegsColorByte;
        internal int FeetColorByte;
        internal int BodyColorByte;
        internal int HeadColorByte;
        internal int Addons;
        internal int Mount;
        internal int LookTypeId;
        internal ThingCategory Category;
    }
}