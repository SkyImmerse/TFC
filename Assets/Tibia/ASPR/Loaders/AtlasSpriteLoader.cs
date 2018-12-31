using System.Collections;
using System.Collections.Generic;
using System.IO;
using SkyImmerseEngine.Loaders;
using SkyImmerseEngine.Utils;
using UnityEngine;
using System;
using System.Threading;
using Assets;
using Assets.Tibia.DAO;
using System.Text;

namespace SkyImmerseEngine
{
    public class AtlasSpriteManager
    {
        public static bool IsReady = false;
        public static float SpriteSize = 32;

        public static event Action LoadingComplete;

        private static readonly Dictionary<int, Material> _atlasesMaterials = new Dictionary<int, Material>();
        
        
        private static readonly Dictionary<int, TextureLocation> _thingItems = new Dictionary<int, TextureLocation>();

        private static readonly Dictionary<int, TextureLocation> _thingCreatures = new Dictionary<int, TextureLocation>();

        private static readonly Dictionary<int, TextureLocation> _thingEffects = new Dictionary<int, TextureLocation>();

        internal static void OpenFile(Stream fileStream, string shaderCreature, string shaderCommon)
        {
            ParseFile(fileStream, shaderCreature, shaderCommon);
        }

        private static readonly Dictionary<int, TextureLocation> _thingMissiles = new Dictionary<int, TextureLocation>();


        public static void ParseFile(Stream fileStream, string shaderCreature, string shaderCommon)
        {
            IsReady = false;
           
            Debug.Log(fileStream.CanRead);
            using (var bw = new BinaryReader(fileStream, Encoding.UTF8, true))
            {
                Config.ResoltionASPR = bw.ReadInt32();
                AtlasSpriteManager.SpriteSize = Config.ResoltionASPR * 32;

                // atlases
                var countAtlases = bw.ReadInt32();

                for (int v = 0; v < countAtlases; v++)
                {
                    var atlasCategory = bw.ReadInt32();

                    var atlasLength = bw.ReadInt32();

                    var atlas = bw.ReadBytes(atlasLength);
                    Debug.Log(atlasLength);
                    var atlasId = v;

                    if (atlasCategory == 99 && atlasLength == 1) continue;

                    MicroTasks.QueueTask((t) =>
                    {
                        var texture2D = new Texture2D(1, 1, TextureFormat.DXT5, false);
                        texture2D.LoadImage(atlas);
                        Debug.Log(texture2D.width);
                        texture2D.filterMode = FilterMode.Point;
                        texture2D.Apply();
                        var shader = Shader.Find(atlasCategory == 1 /* creature category */ ? shaderCreature : shaderCommon);
                        var material = new Material(shader);
                        material.enableInstancing = true;

                        material.mainTexture = texture2D;

                        material.SetTexture("_Layer1Tex", material.mainTexture);
                        material.SetTexture("_MaskTex", material.mainTexture);

                        material.SetTexture("_Addon1Tex", material.mainTexture);
                        material.SetTexture("_Addon2Tex", material.mainTexture);

                        material.SetTexture("_AddonMask1Tex", material.mainTexture);
                        material.SetTexture("_AddonMask2Tex", material.mainTexture);


                        _atlasesMaterials.Add(atlasId, material);

                    });

                }

                // read thing type locations in atlases
                var countLocations = bw.ReadInt32();

                for (int j = 0; j < countLocations; j++)
                {
                    var category = bw.ReadInt32();

                    var id = bw.ReadInt32();
                    var atlasId = bw.ReadInt32();

                    var x = bw.ReadSingle();
                    var y = bw.ReadSingle();

                    var width = bw.ReadSingle();
                    var height = bw.ReadSingle();

                    var tl = new TextureLocation()
                    {
                        id = id,
                        atlasId = atlasId,
                        x = x,
                        y = y,
                        width = width,
                        height = height
                    };

                    if (category == 0) // items
                    {
                        _thingItems.Add(id, tl);
                    }
                    if (category == 1) // creatures
                    {
                        _thingCreatures.Add(id, tl);
                    }
                    if (category == 2) // effects
                    {
                        _thingEffects.Add(id, tl);
                    }
                    if (category == 3) // missiles
                    {
                        _thingMissiles.Add(id, tl);
                    }
                }
                fileStream.Flush();
                fileStream.Close();

                IsReady = true;
                Action loadingComplete = null;
                loadingComplete = () =>
                {
                    LoadingComplete?.Invoke();
                    MicroTasks.QueueEmpty -= loadingComplete;
                };
                MicroTasks.QueueEmpty += loadingComplete;
            }
        }


        
        /// <summary>
        /// Get thing type atlas material and thing location
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category"></param>
        public static Tuple<Material, TextureLocation> GetThingType(int id, int category)
        {
            if (category == 0) // items
            {
                return new Tuple<Material, TextureLocation>(_atlasesMaterials[_thingItems[id].atlasId], _thingItems[id]);
            }
            if (category == 1) // creatures
            {
                return new Tuple<Material, TextureLocation>(_atlasesMaterials[_thingCreatures[id].atlasId], _thingCreatures[id]);
            }
            if (category == 2) // effects
            {
                return new Tuple<Material, TextureLocation>(_atlasesMaterials[_thingEffects[id].atlasId], _thingEffects[id]);
            }
            if (category == 3) // missiles
            {
                return new Tuple<Material, TextureLocation>(_atlasesMaterials[_thingMissiles[id].atlasId], _thingMissiles[id]);
            }

            return null;
        }
    }
    
    
}