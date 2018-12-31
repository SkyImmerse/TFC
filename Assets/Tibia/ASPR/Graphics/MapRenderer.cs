using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Assets;
using Assets.Tibia.DAO;
using Assets.Tibia.DAO.Extensions;
using Cinemachine;
using Game.DAO;
using SkyImmerseEngine;
using SkyImmerseEngine.Graphics;
using SkyImmerseEngine.Loaders;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Game.Graphics
{
    public static class MapRenderer
    {

        public static Dictionary<int, List<Dictionary<Material, RenderThingGroup>>> RenderList = new Dictionary<int, List<Dictionary<Material, RenderThingGroup>>>();

        static Dictionary<Vector3, Dictionary<int, SkyImmerseEngine.Sprite>> ByPosition = new Dictionary<Vector3, Dictionary<int, SkyImmerseEngine.Sprite>>();
        private static Camera uiCamera;
        
        public static void InitCameras()
        {
            for (int i = 0; i < (int)TileMaps.MAX_Z; i++)
            {
                var camera = new GameObject("Layer Camera " + i);
                camera.transform.SetParent(Camera.main.transform);

                var cam = camera.AddComponent<Cinemachine.CinemachineVirtualCamera>();
                camera.layer = LayerMask.NameToLayer(i.ToString());
            }
            uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
        }


        public static Texture2D ThingToTexture(Map map, Thing t)
        {
            if (t == null)
            {
                return null;
            }
            if (t.DatId == 0) return null;
            var virtualRenderGroup = new RenderThingGroup();
            virtualRenderGroup.ScaleFactor = 2f;
            var sprite = virtualRenderGroup.AddThing(map, t, Vector3.zero, 0);
            if (sprite == null) return null;
            t.Sprite = sprite;

            if (t is Creature)
            {
                ((SpriteCreature)sprite).Direction = ((Creature)t).Direction;
                ((SpriteCreature)sprite).Addon = ((Creature)t).Outfit.Addons;
                ((SpriteCreature)sprite).Mount = ((Creature)t).Outfit.Mount > 0;
            }

            virtualRenderGroup.Update();

            Texture2D tex = new Texture2D(256, 256);
            MicroTasks.QueueTask(e=> MicroTasks.CreateCoroutine(Render(Vector3.zero, tex, virtualRenderGroup, t)));


            return tex;
        }

        static IEnumerator Render(Vector3 pos, Texture2D tex, RenderThingGroup virtualRenderGroup, Thing t)
        {
            yield return new WaitForEndOfFrame();

            var photoCamera = GameObject.Find("PhotoCamera").GetComponent<Camera>();

            RenderTexture renderTexture = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            photoCamera.targetTexture = renderTexture;

            photoCamera.transform.position = new Vector3(pos.x - 0.5f, pos.y + 3, Camera.main.transform.position.z/8);

            var tuple = AtlasSpriteManager.GetThingType((t.DatId), (int)t.ThingType.Category);

            ThingTypeRender.DrawThingType(tuple.Item1, LayerMask.NameToLayer("PhotoLayer"), virtualRenderGroup.ThingOffsets, virtualRenderGroup.AdditionalThingOffsets, virtualRenderGroup.AdditionalThingTextures, virtualRenderGroup.ThingPositions, virtualRenderGroup.LightPositions);

            photoCamera.Render();


            RenderTexture.active = renderTexture;

            tex.ReadPixels(new UnityEngine.Rect(0, 0, 256, 256), 0, 0, false);
            tex.Apply();

            for (int y = 0; y < tex.height; ++y)
            {
                // each row
                for (int x = 0; x < tex.width; ++x)
                {
                    // each column
                    var c = tex.GetPixel(x, y);
                    if (c.r == 0 && c.g == 0 && c.b == 0)
                    {
                        c = Color.clear;
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();

            RenderTexture.active = null;

        }

        public static void Draw()
        {
            foreach (var layer in RenderList)
            {
                foreach (var list1023 in layer.Value)
                {
                    foreach (var item in list1023)
                    {

                        if (item.Value.ThingOffsets.Count > 0 && item.Value.ThingPositions.Count > 0 && layer.Key < 32 && layer.Key >= 0)
                        {
                            ThingTypeRender.DrawThingType(item.Key, layer.Key, item.Value.ThingOffsets, item.Value.AdditionalThingOffsets, item.Value.AdditionalThingTextures, item.Value.ThingPositions, item.Value.LightPositions);
                        }
                    }
                }
            }
        }

        public static void Update()
        {
            foreach (var layer in RenderList)
            {
                foreach (var list1023 in layer.Value)
                {
                    foreach (var item in list1023)
                    {

                        if (item.Value.ThingOffsets.Count > 0 && item.Value.ThingPositions.Count > 0)
                        {
                            item.Value.Update();
                        }
                    }
                }
            }
        }


        public static SkyImmerseEngine.Sprite AddThingToRender(Map map, Thing t, Vector3 position, int stackPos)
        {
            var layer = LayerMask.NameToLayer(position.z.ToString());
            var thing = t.ThingType;
            var tuple = AtlasSpriteManager.GetThingType((t.DatId), (int)thing.Category);

            if (!RenderList.ContainsKey(layer)) RenderList.Add(layer, new List<Dictionary<Material, RenderThingGroup>>());
            if (RenderList[layer].Count == 0) RenderList[layer].Add(new Dictionary<Material, RenderThingGroup>());
            if (!RenderList[layer].Last().ContainsKey(tuple.Item1)) RenderList[layer].Last().Add(tuple.Item1, new RenderThingGroup());

            if (RenderList[layer].Last()[tuple.Item1].ThingPositions.Count >= 1022)
            {
                RenderList[layer].Add(new Dictionary<Material, RenderThingGroup>());
                if (!RenderList[layer].Last().ContainsKey(tuple.Item1)) RenderList[layer].Last().Add(tuple.Item1, new RenderThingGroup());
            }
            var sprite = RenderList[layer].Last()[tuple.Item1].AddThing(map, t, position, stackPos);
            if (t.Tile != null)
            {
                var realPos = t.Tile.RealPosition;
                realPos.z = position.z;
                if (ByPosition.ContainsKey(realPos))
                {
                    if (!ByPosition[realPos].ContainsKey(stackPos))
                        ByPosition[realPos].Add(stackPos, sprite);
                    else
                        ByPosition[realPos][stackPos] = sprite;
                }
                else
                {
                    ByPosition.Add(realPos, new Dictionary<int, SkyImmerseEngine.Sprite>());
                    ByPosition[realPos].Add(stackPos, sprite);
                }
            }
            return sprite;
        }

        public static Dictionary<int, SkyImmerseEngine.Sprite> GetSpriteByPos(Map map, Vector3 position, int stackPos, float distance)
        {
            var centralPos = map.CentralPosition;

            var vec = position;
            if (ByPosition.ContainsKey(position))
                return ByPosition[position];
            return null;
        }


        internal static void RemoveAll()
        {
            ByPosition.Clear();
            RenderList.Clear();
        }

        internal static SkyImmerseEngine.Sprite AddThingToRender(Thing thing, Vector3 p1, int p2)
        {
            return AddThingToRender(Map.Current, thing, p1, p2);
        }

        internal static void CameraCulling()
        {
            int lz = 7;
            var cameraPosition = LocalPlayer.Current.Position;

            // if nothing is limiting the view, the first visible floor is 0
            int firstFloor = 0;

            // limits to underground floors while under sea level
            if (cameraPosition.z > (int)TileMaps.SEA_FLOOR)
                firstFloor = Math.Max((int)cameraPosition.z - (int)TileMaps.AWARE_UNDEGROUND_FLOOR_RANGE,
                    (int)TileMaps.UNDERGROUND_FLOOR);

            // loop in 3x3 tiles around the camera
            for (int ix = -1; ix <= 1 && firstFloor < cameraPosition.z; ++ix)
            {
                for (int iy = -1; iy <= 1 && firstFloor < cameraPosition.z; ++iy)
                {
                    var pos = cameraPosition.Translated(ix, iy);


                    // process tiles that we can look through, e.g. windows, doors
                    if ((ix == 0 && iy == 0) || ((Math.Abs(ix) != Math.Abs(iy)) &&
                                                 Map.Current.IsLookPossible(pos)))
                    {
                        var upperPos = new Vector3(pos.x, pos.y, pos.z);
                        var coveredPos = new Vector3(pos.x, pos.y, pos.z);
                        var isCoveredUp = false;
                        var isUp = false;
                        {
                            int nx = (int)coveredPos.x + 1;
                            int ny = (int)coveredPos.y + 1;
                            int nz = (int)coveredPos.z - 1;
                            if (nx >= 0 && nx <= 65535 && ny >= 0 && ny <= 65535 && nz >= 0 && nz <= (float)TileMaps.MAX_Z)
                            {
                                coveredPos.x = nx;
                                coveredPos.y = ny;
                                coveredPos.z = nz;
                                isCoveredUp = true;
                            }
                            else
                                isCoveredUp = false;
                        }
                        {
                            int nz = (int)upperPos.z - 1;
                            if (nz >= 0 && nz <= (float)TileMaps.MAX_Z)
                            {
                                upperPos.z = nz;
                                isUp = true;
                            }
                            else isUp = false;
                        }

                        do
                        {
                            // check tiles physically above
                            Tile tile = Map.Current.GetTile(upperPos);
                            if (tile != null && tile.LimitsFloorsView(!Map.Current.IsLookPossible(pos)))
                            {
                                firstFloor = (int)upperPos.z + 1;
                                break;
                            }

                            // check tiles geometrically above
                            tile = Map.Current.GetTile(coveredPos);
                            if (tile != null && tile.LimitsFloorsView(Map.Current.IsLookPossible(pos)))
                            {
                                firstFloor = (int)coveredPos.z + 1;
                                break;
                            }
                            {
                                int nx = (int)coveredPos.x + 1;
                                int ny = (int)coveredPos.y + 1;
                                int nz = (int)coveredPos.z - 1;
                                if (nx >= 0 && nx <= 65535 && ny >= 0 && ny <= 65535 && nz >= 0 && nz <= (float)TileMaps.MAX_Z)
                                {
                                    coveredPos.x = nx;
                                    coveredPos.y = ny;
                                    coveredPos.z = nz;
                                    isCoveredUp = true;
                                }
                                else
                                    isCoveredUp = false;
                            }
                            {
                                int nz = (int)upperPos.z - 1;
                                if (nz >= 0 && nz <= (float)TileMaps.MAX_Z)
                                {
                                    upperPos.z = nz;
                                    isUp = true;
                                }
                                else isUp = false;
                            }
                        } while (isCoveredUp && isUp && upperPos.z >= firstFloor);
                    }
                }
            }
            lz = firstFloor;

            lz = Mathf.Clamp(lz, 0, (int)TileMaps.MAX_Z);

            List<string> mask = new List<string>();

            for (int i = lz; i < (int)TileMaps.MAX_Z; i++)
            {
                mask.Add(i.ToString());
            }

            Camera.main.cullingMask = LayerMask.GetMask(mask.ToArray());


            foreach (var item in GameObject.FindObjectsOfType<LightSurface>())
            {
                if (item != null && item.gameObject != null && item.gameObject.GetComponent<MeshRenderer>() != null)
                    item.gameObject.GetComponent<MeshRenderer>().enabled = item.gameObject.layer == LayerMask.NameToLayer((cameraPosition.z).ToString());
            }

        }
    }
}