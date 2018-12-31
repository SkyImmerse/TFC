using Assets.Tibia.DAO;
using Assets.Tibia.DAO.Extensions;
using Game.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkyImmerseEngine.Graphics
{
    public class SpriteCreature : Sprite
    {
        public Direction Direction = Direction.North;
        public int Addon = 0;
        public bool Mount = false;

        private Direction _prevDirection = Direction.InvalidDirection;
        private int _prevAnimationPhase = -1;
        private int _prevAddon = -1;
        private bool _prevMount = true;
        private int _prevFrameGroup;

        
        public SpriteCreature()
        {
            AnimationPhase = 0;
            ;
        }


        public override void StartOffset()
        {
            NormalizeDirection();
            SetFrame(FrameGroup, atlasTex, AnimationPhase, (int)Direction, Mount, Addon);
        }

        public void SetFrame(int frameGroup, Texture2D mainTexture, int animationPhase, int direction, bool mount, int addon)
        {
            Dictionary<string, Vector2> offsets = new Dictionary<string, Vector2>();
            Dictionary<string, Vector2> scales = new Dictionary<string, Vector2>();
            
            textures = new Dictionary<string, Texture2D>();

            mainTextureOffset = GetVector2(direction, 0, mount ? 1 : 0, 0, animationPhase, FrameGroup);
            textures.Add("_MaskTex", mainTexture);
            offsets.Add("_MaskTex_Positions", GetVector2(direction, 0, mount ? 1 : 0, 1, animationPhase, FrameGroup));

            offsets.Add("_AddonMask1Tex_Positions", GetVector2(direction, addon > 0 ? 1 : 0, mount ? 1 : 0, 1, animationPhase, FrameGroup));
            offsets.Add("_AddonMask2Tex_Positions", GetVector2(direction, addon > 1 ? 2 : 0, mount ? 1 : 0, 1, animationPhase, FrameGroup));


            if (addon > 0)
            {
                textures.Add("_Addon1Tex", mainTexture);
                textures.Add("_AddonMask1Tex", mainTexture);
            }
            else
            {
                textures.Add("_Addon1Tex", mainTexture);
                textures.Add("_AddonMask1Tex", mainTexture);
                scales.Add("_MaskTex_Positions", new Vector2());
            }

            var offset = GetVector2(direction, addon > 0 ? 1 : 0, mount ? 1 : 0, 0, animationPhase, FrameGroup);
            offsets.Add("_Addon1Tex_Positions", offset);

            if (addon > 1)
            {
                textures.Add("_Addon2Tex", mainTexture);
                textures.Add("_AddonMask2Tex", mainTexture);
            }
            else
            {
                textures.Add("_Addon2Tex", mainTexture);
                textures.Add("_AddonMask2Tex", mainTexture);
                if (scales.ContainsKey("_MaskTex_Positions")) scales["_MaskTex_Positions"] = new Vector2();
                else scales.Add("_MaskTex_Positions", new Vector2());
            }

            offset = GetVector2(direction, addon > 1 ? 2 : 0, mount ? 1 : 0, 0, animationPhase, FrameGroup);
            offsets.Add("_Addon2Tex_Positions", offset);

            offsetsScales = new Dictionary<string, Vector4>();
            foreach (var o in offsets)
            {
                var vec = new Vector4(o.Value.x, o.Value.y, scales.ContainsKey(o.Key) ? scales[o.Key].x : mainTextureScale.x, scales.ContainsKey(o.Key) ? scales[o.Key].y : mainTextureScale.y);
                offsetsScales.Add(o.Key, vec);
            }
            if (addon > 0)
                UpdateAdditionalChannels();

            Group._thingOffsets[Thing] = new Vector4(mainTextureOffset.x, mainTextureOffset.y, mainTextureScale.x, mainTextureScale.y);

        }
        public override void Update()
        {

            if (FrameGroup == 0)
            {
                if (AnimationDuration < Time.realtimeSinceStartup && !AnimationEnd)
                {

                    if (ThingType.Animators[FrameGroup].LoopCount < 0)
                    {
                        int count = 1;
                        var nextPhase = AnimationPhase + count;
                        if (nextPhase < 0 || nextPhase >= ThingType.Animators[FrameGroup].AnimationPhases)
                        {
                            AnimationDirection = AnimationDirection == Assets.Tibia.DAO.Animator.AnimationDirection.AnimDirForward ? Assets.Tibia.DAO.Animator.AnimationDirection.AnimDirBackward : Assets.Tibia.DAO.Animator.AnimationDirection.AnimDirForward;
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
                            }
                        }

                    }
                    AnimationDuration = Time.realtimeSinceStartup + ThingType.Animators[FrameGroup].GetPhaseDuration(AnimationPhase) / 1000f;
                }
            }
            else
            {
                AnimationDuration = 0;
                AnimationEnd = false;
                CurrentLoop = 0;
            }

            if (Direction != _prevDirection ||
                    AnimationPhase != _prevAnimationPhase ||
                    FrameGroup != _prevFrameGroup ||
                    Addon != _prevAddon ||
                    Mount != _prevMount)
            {

                NormalizeDirection();
                SetFrame(FrameGroup, atlasTex, AnimationPhase, (int)Direction, Mount, Addon);

                _prevDirection = Direction;
                _prevAnimationPhase = AnimationPhase;
                _prevAddon = Addon;
                _prevMount = Mount;
                _prevFrameGroup = FrameGroup;
            }
        }

        internal override void UpdateAdditionalChannels()
        {
            var mOutfit = ((Creature)Thing).Outfit;
            offsetsScales.AddOrUpdate("_HeadColor", (mOutfit.HeadColor / 255) * 3);
            offsetsScales.AddOrUpdate("_BodyColor", (mOutfit.BodyColor / 255) * 3);
            offsetsScales.AddOrUpdate("_LegsColor", (mOutfit.LegsColor / 255) * 3);
            offsetsScales.AddOrUpdate("_FeetColor", (mOutfit.FeetColor / 255) * 3);

            base.UpdateAdditionalChannels();

            
        }

        public void NormalizeDirection()
        {
            if (Direction == Direction.NorthEast || Direction == Direction.NorthWest)
                Direction = Direction.North;
            if (Direction == Direction.SouthEast || Direction == Direction.SouthWest)
                Direction = Direction.South;
        }
    }
}