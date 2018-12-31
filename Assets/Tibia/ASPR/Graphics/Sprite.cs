using Assets.Tibia.DAO;
using Assets.Tibia.DAO.Extensions;
using Game.DAO;
using Game.Graphics;
using SkyImmerseEngine.Graphics;
using SkyImmerseEngine.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Tibia.DAO.Animator;

namespace SkyImmerseEngine
{
    public class Sprite 
    {
        public ThingType ThingType;
        
        public Thing Thing;

        public RenderThingGroup Group;

        public Map Map;

        public TextureLocation Location;
        internal bool Selected;
        public float atlasWidth;

        public float atlasHeight;

        public int Layer;

        public int AnimationPhase;

        public Vector2 mainTextureOffset;
        public Dictionary<string, Texture2D> textures;
        public Dictionary<string, Vector4> offsetsScales;
        public Texture2D atlasTex;
        public Vector2 mainTextureScale;
        internal float Order;
        int xPattern = 0, yPattern = 0, zPattern = 0;
        public int FrameGroup = 0;

        public virtual void StartOffset()
        {
            offsetsScales = new Dictionary<string, Vector4>();
            textures = new Dictionary<string, Texture2D>();

            if (ThingType.Category != ThingCategory.ThingCategoryEffect)
            {
                Thing.CalculatePatterns(ref xPattern, ref yPattern, ref zPattern);

                RecalcOffsets();

                var o = GetVector2(0, 0, 0, 1, 1, 0);
                if (ThingType.Layers > 1)
                {
                    offsetsScales.Add("_TilePosition1", new Vector4(o.x, o.y, mainTextureScale.x, mainTextureScale.y));
                }
                else
                {
                    offsetsScales.Add("_TilePosition1", new Vector4(0, 0, 0, 0));
                }
                offsetsScales.Add("_SelectedColor", new Vector4(1, 1, 1, 10));

            } else
            {
                RecalcOffsets();
            }
            

            if (ThingType.Layers > 1)
            {
                textures.Add("_Layer1Tex", atlasTex);
            }
            else
            {
                textures.Add("_Layer1Tex", atlasTex);
            }

            UpdateAdditionalChannels();
        }

        public void EnableSelected()
        {
            if (Selected) return;
            Selected = true;

            offsetsScales["_SelectedColor"] = new Vector4(10, 100, 10, 10);

            UpdateAdditionalChannels();
        }

        public void DisableSelected()
        {
            if (!Selected) return;
            Selected = false;
            StartOffset();
        }

        internal virtual void UpdateAdditionalChannels()
        {
            offsetsScales.AddOrUpdate("_Orders", new Vector4(Order, Group.Layer, float.NegativeInfinity, float.NegativeInfinity));
            foreach (var item in offsetsScales)
            {
                if (!Group.AdditionalThingOffsets.ContainsKey(item.Key))
                    Group.AdditionalThingOffsets.Add(item.Key, new Dictionary<Thing, Vector4>());

                Group.AdditionalThingOffsets[item.Key].AddOrUpdate(Thing, item.Value);
            }


        }

        public float AnimationDuration;

        public AnimationDirection AnimationDirection;
        public int CurrentLoop;
        public bool AnimationEnd = false;

        public virtual void Update()
        {
           
            if (AnimationDuration < Time.realtimeSinceStartup && !AnimationEnd)
            {
                    if (ThingType.Animators[FrameGroup].LoopCount < 0)
                    {
                        int count = 1;
                        var nextPhase = AnimationPhase + count;
                        if (nextPhase < 0 || nextPhase >= ThingType.Animators[FrameGroup].AnimationPhases)
                        {
                            AnimationDirection = AnimationDirection == AnimationDirection.AnimDirForward ? AnimationDirection.AnimDirBackward : AnimationDirection.AnimDirForward;
                            count *= -1;
                        }
                        AnimationPhase += count;
                    }

                    if (ThingType.Animators[FrameGroup].LoopCount == 0)
                    {
                        AnimationPhase += 1;
                        if (AnimationPhase >= ThingType.Animators[FrameGroup].AnimationPhases)
                        {
                            AnimationPhase = 0;

                            if (ThingType.Category == ThingCategory.ThingCategoryEffect)
                            {
                                Thing.Tile?.RemoveEffect((Effect)Thing);
                            }
                        }
                    }

                    if (ThingType.Animators[FrameGroup].LoopCount > 0)
                    {
                        AnimationPhase += 1;

                        if (AnimationPhase >= ThingType.Animators[FrameGroup].AnimationPhases)
                        {
                            CurrentLoop++;
                            AnimationPhase = 0;
                            if (CurrentLoop >= (ThingType.Animators[FrameGroup].LoopCount))
                            {
                                CurrentLoop = 0;
                                AnimationPhase = 0;
                                AnimationEnd = true;

                                if (ThingType.Category == ThingCategory.ThingCategoryEffect)
                                {
                                    Thing.Tile?.RemoveEffect((Effect)Thing);
                                }
                            }
                        }

                        RecalcOffsets();
                    }
                if (ThingType.Category == ThingCategory.ThingCategoryEffect)
                {
                    AnimationDuration = Time.realtimeSinceStartup + 0.1f;
                } else
                {
                    AnimationDuration = Time.realtimeSinceStartup + 0.25f;
                }
                    
                
            }

            Group._thingOffsets[Thing] = new Vector4(mainTextureOffset.x, mainTextureOffset.y, mainTextureScale.x, mainTextureScale.y);
        }

        private void RecalcOffsets()
        {
            if (ThingType.Category != ThingCategory.ThingCategoryEffect)
            {
                mainTextureOffset =
                    GetVector2(xPattern, yPattern, 0, Layer, AnimationPhase, FrameGroup);
            }
            else
            {
                var offsetX = (int)(Thing.Position.x - Map.CentralPosition.x);
                var offsetY = (int)(Thing.Position.y - Map.CentralPosition.y);

                xPattern = offsetX % (int)ThingType.NumPattern.x;

                if (xPattern < 0)
                    xPattern += (int)ThingType.NumPattern.x;

                yPattern = offsetY % (int)ThingType.NumPattern.y;

                if (yPattern < 0)
                    yPattern += (int)ThingType.NumPattern.y;

                mainTextureOffset = GetVector2(xPattern, yPattern, 0, 0, AnimationPhase, FrameGroup);
            }
        }

        public Vector2 GetVector2(
           int patternX,
            int patternY,
            int patternZ,
            int layer, int frame, int group)
        {
            var spriteSize = Location.width / (ThingType.NumPattern.x * ThingType.AnimationPhases);
            var spriteHeight = Location.height / (ThingType.NumPattern.z * ThingType.NumPattern.y * ThingType.Layers);
            return new Vector2(
                // direction
                (Location.x + patternX * spriteSize
                 // animation phases
                 + frame * ThingType.NumPattern.x * spriteSize
                 + group * ThingType.Animators[(int)Math.Max(0, group - 1)].AnimationPhases * ThingType.NumPattern.x * spriteSize
                ) / atlasWidth,
                (atlasHeight - Location.y - spriteHeight
                 // mount
                 - (Location.height / 4f - spriteHeight) * (patternZ) * (ThingType.NumPattern.z - 1)
                 // addon
                 - (patternY) * spriteHeight
                // encoded blend layers
                  + layer * ThingType.NumPattern.y * (ThingType.NumPattern.z) * spriteHeight
                ) / atlasHeight);

        }


    }
}