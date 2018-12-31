﻿using System;
 using System.Collections.Generic;
 using System.Linq;
using Assets.Tibia.DAO;
using SkyImmerseEngine.Loaders;
 using SkyImmerseEngine.Utils;
 using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SkyImmerseEngine.Graphics
{
    public static class ThingTypeRender
    {
        public static Mesh mesh;

        public static void Init()
        {
            var tempGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mesh = tempGo.GetComponent<MeshFilter>().mesh;
            GameObject.Destroy(tempGo);
        }

        public static void DrawThingType(Material t, int layer, Dictionary<Thing, Vector4>.ValueCollection TilePositions, Dictionary<string, Dictionary<Thing, Vector4>> additionalThingOffsets, Dictionary<string, Texture2D> additionalThingTextures, Dictionary<Thing, Matrix4x4>.ValueCollection matricies, List<Vector4> LightPositions)
        {
            var materialProperyBlock = new MaterialPropertyBlock();
            if (TilePositions.Count > 0)
                materialProperyBlock.SetVectorArray("_TilePosition", TilePositions.ToArray());
            foreach (var item in additionalThingOffsets)
            {
                if (item.Value.Count > 0)
                {
                    materialProperyBlock.SetVectorArray(item.Key, item.Value.Values.ToList());
                }
            }
            foreach (var item in additionalThingTextures)
            {
                materialProperyBlock.SetTexture(item.Key, item.Value);
            }
            UnityEngine.Graphics.DrawMeshInstanced(mesh, 0, t, matricies.ToArray(), matricies.Count, materialProperyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, layer);

        }

        public static void DrawThingType(Camera cam, Material t, int layer, List<Vector4> TilePositions, Dictionary<string, List<Vector4>> additionalThingOffsets, Dictionary<string, Texture2D> additionalThingTextures, List<Matrix4x4> matricies)
        {
            var materialProperyBlock = new MaterialPropertyBlock();
            if (TilePositions.Count > 0)
                materialProperyBlock.SetVectorArray("_TilePosition", TilePositions.ToArray());
            foreach (var item in additionalThingOffsets)
            {
                if (item.Value.Count > 0)
                    materialProperyBlock.SetVectorArray(item.Key, item.Value);
            }
            //foreach (var item in additionalThingTextures)
            //{
            //    materialProperyBlock.SetTexture(item.Key, item.Value);
            //}
            UnityEngine.Graphics.DrawMeshInstanced(mesh, 0, t, matricies.ToArray(), matricies.Count, materialProperyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, layer, cam);

        }

    }

   
}