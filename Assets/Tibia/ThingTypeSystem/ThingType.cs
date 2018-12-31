using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Tibia.DAO;
using GameClient.Network;
using UnityEngine;
using Animator = Assets.Tibia.DAO.Animator;
namespace Game.DAO
{
    #region Enums 
    public enum FrameGroupType : short
    {
        FrameGroupDefault = 0,
        FrameGroupIdle = FrameGroupDefault,
        FrameGroupMoving
    }

    public enum ThingCategory : short
    {
        ThingCategoryItem = 0,
        ThingCategoryCreature,
        ThingCategoryEffect,
        ThingCategoryMissile,
        ThingInvalidCategory,
        ThingLastCategory = ThingInvalidCategory
    }

    public enum ThingAttr : short
    {
        ThingAttrGround = 0,
        ThingAttrGroundBorder = 1,
        ThingAttrOnBottom = 2,
        ThingAttrOnTop = 3,
        ThingAttrContainer = 4,
        ThingAttrStackable = 5,
        ThingAttrForceUse = 6,
        ThingAttrMultiUse = 7,
        ThingAttrWritable = 8,
        ThingAttrWritableOnce = 9,
        ThingAttrFluidContainer = 10,
        ThingAttrSplash = 11,
        ThingAttrNotWalkable = 12,
        ThingAttrNotMoveable = 13,
        ThingAttrBlockProjectile = 14,
        ThingAttrNotPathable = 15,
        ThingAttrPickupable = 16,
        ThingAttrHangable = 17,
        ThingAttrHookSouth = 18,
        ThingAttrHookEast = 19,
        ThingAttrRotateable = 20,
        ThingAttrLight = 21,
        ThingAttrDontHide = 22,
        ThingAttrTranslucent = 23,
        ThingAttrDisplacement = 24,
        ThingAttrElevation = 25,
        ThingAttrLyingCorpse = 26,
        ThingAttrAnimateAlways = 27,
        ThingAttrMinimapColor = 28,
        ThingAttrLensHelp = 29,
        ThingAttrFullGround = 30,
        ThingAttrLook = 31,
        ThingAttrCloth = 32,
        ThingAttrMarket = 33,
        ThingAttrUsable = 34,
        ThingAttrWrapable = 35,
        ThingAttrUnwrapable = 36,
        ThingAttrTopEffect = 37,

        // additional
        ThingAttrOpacity = 100,
        ThingAttrNotPreWalkable = 101,

        ThingAttrFloorChange = 252,
        ThingAttrNoMoveAnimation = 253, // 10.10: real value is 16, but we need to do this for backwards compatibility
        ThingAttrChargeable = 254, // deprecated
        ThingLastAttr = 255
    }

    public enum SpriteMask
    {
        SpriteMaskRed = 1,
        SpriteMaskGreen,
        SpriteMaskBlue,
        SpriteMaskYellow
    }

    #endregion

    #region Supoort Structs
    public class MarketData
    {
        public string Name;
        public int Category;
        public UInt16 RequiredLevel;
        public UInt16 RestrictVocation;
        public UInt16 ShowAs;
        public UInt16 TradeAs;

        public override string ToString()
        {
            return "[Market Data \n" +
                   "Name: " + Name + "\n" +
                   "Category: " + Category + "\n" +
                   "RequiredLevel: " + RequiredLevel + "\n" +
                   "RestrictVocation: " + RestrictVocation + "\n" +
                   "ShowAs: " + ShowAs + "\n" +
                   "TradeAs: " + TradeAs + "\n";
        }
    }

    public class Light
    {
        public Light()
        {
            Intensity = 0;
            Color = 215;
        }
        public byte Intensity;
        public byte Color;
    }

    #endregion

    public class ThingType
    {
        #region Variables
        public ThingCategory Category;
        public UInt16 Id = new UInt16();
        public Dictionary<ThingAttr, object> Attribs = new Dictionary<ThingAttr, object>();

        public Rect Size = new Rect();
        public Vector3 Displacement;
        public List<Animator> Animators = new List<Animator>();
        public byte GroupCount;
        public int AnimationPhases;
        public int ExactSize;
        public int RealSize;
        public Vector3 NumPattern;
        public int Layers;
        public int Elevation;
        public float Opacity;
        public string CustomImage;

        public List<int> SpritesIndex = new List<int>();
        public List<List<Rect>> TexturesFramesRects = new List<List<Rect>>();
        public List<List<Rect>> TexturesFramesOriginRects = new List<List<Rect>>();
        public List<List<Vector3>> TexturesFramesOffsets = new List<List<Vector3>>();

        public int AnimationPhase;

        #endregion

        public ThingType()
        {
            Category = ThingCategory.ThingInvalidCategory;
            Id = 0;
            ExactSize = 0;
            RealSize = 0;
            AnimationPhases = 0;
            Layers = 0;
            Elevation = 0;
            Opacity = 1.0f;
        }

        public void Unserialize(ushort clientId, ThingCategory category, BinaryReader fin)
        {
            Id = clientId;
            Category = category;

            int count = 0;
            int attr = -1;
            bool done = false;
            for (int i = 0; i < (int)ThingAttr.ThingLastAttr; ++i)
            {
                count++;
                attr = fin.ReadByte();
                if (attr == (int)ThingAttr.ThingLastAttr)
                {
                    done = true;
                    break;
                }

                //                if (attr == 254)
                //                {
                //                    continue;
                //                }


                if (Config.ClientVersion >= 1000)
                {
                    //             In 10.10+ all attributes from 16 and up were
                    //             * incremented by 1 to make space for 16 as
                    //             * "No Movement Animation" flag.
                    //
                    if (attr == 16)
                        attr = (int)ThingAttr.ThingAttrNoMoveAnimation;
                    else if (attr > 16)
                        attr -= 1;
                }
                else if (Config.ClientVersion >= 860)
                {
                    //             Default attribute values follow
                    //             * the format of 8.6-9.86.
                    //             * Therefore no changes here.
                    //
                }
                else if (Config.ClientVersion >= 780)
                {
                    //             In 7.80-8.54 all attributes from 8 and higher were
                    //             * incremented by 1 to make space for 8 as
                    //             * "Item Charges" flag.
                    //
                    if (attr == 8)
                    {
                        Attribs[ThingAttr.ThingAttrChargeable] = true;
                        continue;
                    }
                    else if (attr > 8)
                        attr -= 1;
                }
                else if (Config.ClientVersion >= 755)
                {
                    // In 7.55-7.72 attributes 23 is "Floor Change".
                    if (attr == 23)
                        attr = (int)ThingAttr.ThingAttrFloorChange;
                }
                else if (Config.ClientVersion >= 740)
                {
                    //             In 7.4-7.5 attribute "Ground Border" did not exist
                    //             * attributes 1-15 have to be adjusted.
                    //             * Several other changes in the format.
                    //
                    if (attr > 0 && attr <= 15)
                        attr += 1;
                    else if (attr == 16)
                        attr = (int)ThingAttr.ThingAttrLight;
                    else if (attr == 17)
                        attr = (int)ThingAttr.ThingAttrFloorChange;
                    else if (attr == 18)
                        attr = (int)ThingAttr.ThingAttrFullGround;
                    else if (attr == 19)
                        attr = (int)ThingAttr.ThingAttrElevation;
                    else if (attr == 20)
                        attr = (int)ThingAttr.ThingAttrDisplacement;
                    else if (attr == 22)
                        attr = (int)ThingAttr.ThingAttrMinimapColor;
                    else if (attr == 23)
                        attr = (int)ThingAttr.ThingAttrRotateable;
                    else if (attr == 24)
                        attr = (int)ThingAttr.ThingAttrLyingCorpse;
                    else if (attr == 25)
                        attr = (int)ThingAttr.ThingAttrHangable;
                    else if (attr == 26)
                        attr = (int)ThingAttr.ThingAttrHookSouth;
                    else if (attr == 27)
                        attr = (int)ThingAttr.ThingAttrHookEast;
                    else if (attr == 28)
                        attr = (int)ThingAttr.ThingAttrAnimateAlways;

                    // "Multi Use" and "Force Use" are swapped
                    if (attr == (int)ThingAttr.ThingAttrMultiUse)
                        attr = (int)ThingAttr.ThingAttrForceUse;
                    else if (attr == (int)ThingAttr.ThingAttrForceUse)
                        attr = (int)ThingAttr.ThingAttrMultiUse;
                }

                switch (attr)
                {
                    case (int)ThingAttr.ThingAttrDisplacement:
                        {
                            if (Config.ClientVersion >= 755)
                            {
                                Displacement.x = fin.ReadUInt16();
                                Displacement.y = fin.ReadUInt16();
                            }
                            else
                            {
                                Displacement.x = 8;
                                Displacement.y = 8;
                            }
                            Attribs[(ThingAttr)attr] = true;
                            break;
                        }
                    case (int)ThingAttr.ThingAttrLight:
                        {
                            Light light = new Light();
                            light.Intensity = (byte)fin.ReadUInt16();
                            light.Color = (byte)fin.ReadUInt16();
                            Attribs[(ThingAttr)attr] = light;
                            break;
                        }
                    case (int)ThingAttr.ThingAttrMarket:
                        {
                            MarketData market = new MarketData();
                            market.Category = fin.ReadUInt16();
                            market.TradeAs = fin.ReadUInt16();
                            market.ShowAs = fin.ReadUInt16();
                            var nameLength = fin.ReadUInt16();
                            var chars = fin.ReadChars(nameLength);
                            market.Name = new string(chars);
                            market.RestrictVocation = fin.ReadUInt16();
                            market.RequiredLevel = fin.ReadUInt16();
                            Attribs[(ThingAttr)attr] = market;
                            break;
                        }
                    case (int)ThingAttr.ThingAttrElevation:
                        {
                            Elevation = fin.ReadUInt16();
                            Attribs[(ThingAttr)attr] = Elevation;
                            break;
                        }
                    case (int)ThingAttr.ThingAttrUsable:
                    case (int)ThingAttr.ThingAttrGround:
                    case (int)ThingAttr.ThingAttrWritable:
                    case (int)ThingAttr.ThingAttrWritableOnce:
                    case (int)ThingAttr.ThingAttrMinimapColor:
                    case (int)ThingAttr.ThingAttrCloth:
                    case (int)ThingAttr.ThingAttrLensHelp:
                        Attribs[(ThingAttr)attr] = fin.ReadUInt16();
                        ;
                        break;
                    default:
                        Attribs[(ThingAttr)attr] = true;
                        break;
                }

            }

            if (!done)
            {
                return;
            }

            bool hasFrameGroups = (category == ThingCategory.ThingCategoryCreature && FeatureManager.GetFeature(GameFeature.GameIdleAnimations));
            GroupCount = hasFrameGroups ? fin.ReadByte() : (byte)1;

            AnimationPhases = 0;

            if(GroupCount == 0)
            {
                Animators.Add(new Animator());
            }

            for (int i = 0; i < GroupCount; ++i)
            {
                uint frameGroupType = (uint)FrameGroupType.FrameGroupDefault;
                if (hasFrameGroups)
                    frameGroupType = fin.ReadByte();

                uint width = fin.ReadByte();
                uint height = fin.ReadByte();
                Size = new Rect(0, 0, (int)width, (int)height);
                if (width > 1 || height > 1)
                {
                    RealSize = fin.ReadByte();
                    ExactSize = Math.Min(RealSize, Math.Max((int)width * (int)32, (int)height * (int)32));
                }
                else
                    ExactSize = 32;

                Layers = fin.ReadByte();
                NumPattern.x = fin.ReadByte();
                NumPattern.y = fin.ReadByte();
                if (Config.ClientVersion >= 755)
                    NumPattern.z = fin.ReadByte();
                else
                    NumPattern.z = 1;

                var groupAnimationsPhases = fin.ReadByte();
                AnimationPhases += groupAnimationsPhases;

                if (groupAnimationsPhases > 1 && FeatureManager.GetFeature(GameFeature.GameEnhancedAnimations))
                {
                    Animators.Add(new Animator());
                    Animators[i].Unserialize(groupAnimationsPhases, fin);
                } else {
                    Animators.Add(new Animator());
                    Animators[i].AnimationPhases = groupAnimationsPhases;
                    Animators[i].StartPhase = 0;
                    Animators[i].ResetPhaseDurarations();
                }

                int totalSprites = (int)Size.width * (int)Size.height * Layers * (int)NumPattern.x *
                                   (int)NumPattern.y *
                                   (int)NumPattern.z * groupAnimationsPhases;

                if (totalSprites > 4096)
                {
                    return;
                }

                for (int x = 0; x < totalSprites; x++)
                {
                    SpritesIndex.Add(FeatureManager.GetFeature(GameFeature.GameSpritesU32) ? (int)fin.ReadUInt32() : fin.ReadUInt16());
                }
            }

        }

        public bool HasAttr(ThingAttr attr)
        {
            return Attribs.ContainsKey(attr);
        }


        #region Attributes

        public int GroundSpeed => (UInt16)Attribs[ThingAttr.ThingAttrGround];

        public int MaxTextLength => Attribs.ContainsKey(ThingAttr.ThingAttrWritableOnce)
                ? (UInt16)Attribs[ThingAttr.ThingAttrWritableOnce]
                : (UInt16)Attribs[ThingAttr.ThingAttrWritable];

        public Light Light => (Light)Attribs[ThingAttr.ThingAttrLight];

        public int MinimapColor => (UInt16)Attribs[ThingAttr.ThingAttrMinimapColor];

        public int LensHelp => (UInt16)Attribs[ThingAttr.ThingAttrLensHelp];

        public int ClothSlot => (UInt16)Attribs[ThingAttr.ThingAttrCloth];

        public MarketData MarketData => (MarketData)Attribs[ThingAttr.ThingAttrMarket];

        public bool IsGround => Attribs.ContainsKey(ThingAttr.ThingAttrGround);

        public bool IsGroundBorder => Attribs.ContainsKey(ThingAttr.ThingAttrGroundBorder);

        public bool IsOnBottom => Attribs.ContainsKey(ThingAttr.ThingAttrOnBottom);

        public bool IsOnTop => Attribs.ContainsKey(ThingAttr.ThingAttrOnTop);

        public bool IsContainer => Attribs.ContainsKey(ThingAttr.ThingAttrContainer);

        public bool IsStackable => Attribs.ContainsKey(ThingAttr.ThingAttrStackable);

        public bool IsForceUse => Attribs.ContainsKey(ThingAttr.ThingAttrForceUse);

        public bool IsMultiUse => Attribs.ContainsKey(ThingAttr.ThingAttrMultiUse);

        public bool IsWritable => Attribs.ContainsKey(ThingAttr.ThingAttrWritable);

        public bool IsChargeable => Attribs.ContainsKey(ThingAttr.ThingAttrChargeable);

        public bool IsWritableOnce => Attribs.ContainsKey(ThingAttr.ThingAttrWritableOnce);

        public bool IsFluidContainer => Attribs.ContainsKey(ThingAttr.ThingAttrFluidContainer);

        public bool IsSplash => Attribs.ContainsKey(ThingAttr.ThingAttrSplash);

        public bool IsNotWalkable => Attribs.ContainsKey(ThingAttr.ThingAttrNotWalkable);

        public bool IsNotMoveable => Attribs.ContainsKey(ThingAttr.ThingAttrNotMoveable);

        public bool IsBlockProjectile => Attribs.ContainsKey(ThingAttr.ThingAttrBlockProjectile);

        public bool IsNotPathable => Attribs.ContainsKey(ThingAttr.ThingAttrNotPathable);

        public bool IsPickupable => Attribs.ContainsKey(ThingAttr.ThingAttrPickupable);

        public bool IsHangable => Attribs.ContainsKey(ThingAttr.ThingAttrHangable);

        public bool IsHookSouth => Attribs.ContainsKey(ThingAttr.ThingAttrHookSouth);

        public bool IsHookEast => Attribs.ContainsKey(ThingAttr.ThingAttrHookEast);

        public bool IsRotateable => Attribs.ContainsKey(ThingAttr.ThingAttrRotateable);

        public bool HasLight => Attribs.ContainsKey(ThingAttr.ThingAttrLight);

        public bool IsDontHide => Attribs.ContainsKey(ThingAttr.ThingAttrDontHide);

        public bool IsTranslucent => Attribs.ContainsKey(ThingAttr.ThingAttrTranslucent);

        public bool HasDisplacement => Attribs.ContainsKey(ThingAttr.ThingAttrDisplacement);

        public bool HasElevation => Attribs.ContainsKey(ThingAttr.ThingAttrElevation);

        public bool IsLyingCorpse => Attribs.ContainsKey(ThingAttr.ThingAttrLyingCorpse);

        public bool IsAnimateAlways => Attribs.ContainsKey(ThingAttr.ThingAttrAnimateAlways);

        public bool HasMiniMapColor => Attribs.ContainsKey(ThingAttr.ThingAttrMinimapColor);

        public bool HasLensHelp => Attribs.ContainsKey(ThingAttr.ThingAttrLensHelp);

        public bool IsFullGround => Attribs.ContainsKey(ThingAttr.ThingAttrFullGround);

        public bool IsIgnoreLook => Attribs.ContainsKey(ThingAttr.ThingAttrLook);

        public bool IsCloth => Attribs.ContainsKey(ThingAttr.ThingAttrCloth);

        public bool IsMarketable => Attribs.ContainsKey(ThingAttr.ThingAttrMarket);

        public bool IsUsable => Attribs.ContainsKey(ThingAttr.ThingAttrUsable);

        public bool IsWrapable => Attribs.ContainsKey(ThingAttr.ThingAttrWrapable);

        public bool IsUnwrapable => Attribs.ContainsKey(ThingAttr.ThingAttrUnwrapable);

        public bool IsTopEffect => Attribs.ContainsKey(ThingAttr.ThingAttrTopEffect);

        public bool IsNotPreWalkable => Attribs.ContainsKey(ThingAttr.ThingAttrNotPreWalkable);

        public int TotalSprites =>
            (int)(this.Size.width *
                   this.Size.height *
                   this.NumPattern.x *
                   this.NumPattern.y *
                   this.NumPattern.z *
                   this.AnimationPhases *
                   this.Layers);

        public int TotalTextures => (int)(this.NumPattern.x *
                   this.NumPattern.y *
                   this.NumPattern.z *
                   this.AnimationPhases *
                   this.Layers);

        public int GetSpriteIndex(int width,
            int height,
            int layer,
            int patternX,
            int patternY,
            int patternZ,
            int frame)
        {
            return ((((((frame % this.AnimationPhases) *
                        (int)this.NumPattern.z + patternZ) *
                       (int)this.NumPattern.y + patternY) *
                      (int)this.NumPattern.x + patternX) *
                     this.Layers + layer) *
                    (int)this.Size.height + height) *
                   (int)this.Size.width + width;
        }

        public int GetTextureIndex2(int layer,
            int patternX,
            int patternY,
            int patternZ,
            int frame)
        {
            return Convert.ToInt32((((frame % this.AnimationPhases *
                                      this.NumPattern.z + patternZ) *
                                     this.NumPattern.y + patternY) *
                                    this.NumPattern.x + patternX) *
                                   this.Layers + layer);
        }

        public int GetTextureIndex(int l, int x, int y, int z)
        {
            return ((l * (int)NumPattern.z + z)
                    * (int)NumPattern.y + y)
                   * (int)NumPattern.x + x;
        }


        public override string ToString()
        {
            return
"[IsGround: " + IsGround.ToString() + "]\n" +
"[IsGroundBorder: " + IsGroundBorder.ToString() + "]\n" +
"[IsOnBottom: " + IsOnBottom.ToString() + "]\n" +
"[IsOnTop: " + IsOnTop.ToString() + "]\n" +
"[IsContainer: " + IsContainer.ToString() + "]\n" +
"[IsStackable: " + IsStackable.ToString() + "]\n" +
"[IsForceUse: " + IsForceUse.ToString() + "]\n" +
"[IsMultiUse: " + IsMultiUse.ToString() + "]\n" +
"[IsWritable: " + IsWritable.ToString() + "]\n" +
"[IsChargeable: " + IsChargeable.ToString() + "]\n" +
"[IsWritableOnce: " + IsWritableOnce.ToString() + "]\n" +
"[IsFluidContainer: " + IsFluidContainer.ToString() + "]\n" +
"[IsSplash: " + IsSplash.ToString() + "]\n" +
"[IsNotWalkable: " + IsNotWalkable.ToString() + "]\n" +
"[IsNotMoveable: " + IsNotMoveable.ToString() + "]\n" +
"[IsBlockProjectile: " + IsBlockProjectile.ToString() + "]\n" +
"[IsNotPathable: " + IsNotPathable.ToString() + "]\n" +
"[IsPickupable: " + IsPickupable.ToString() + "]\n" +
"[IsHangable: " + IsHangable.ToString() + "]\n" +
"[IsHookSouth: " + IsHookSouth.ToString() + "]\n" +
"[IsHookEast: " + IsHookEast.ToString() + "]\n" +
"[IsRotateable: " + IsRotateable.ToString() + "]\n" +
"[HasLight: " + HasLight.ToString() + "]\n" +
"[IsDontHide: " + IsDontHide.ToString() + "]\n" +
"[IsTranslucent: " + IsTranslucent.ToString() + "]\n" +
"[HasDisplacement: " + HasDisplacement.ToString() + "]\n" +
"[HasElevation: " + HasElevation.ToString() + "]\n" +
"[IsLyingCorpse: " + IsLyingCorpse.ToString() + "]\n" +
"[IsAnimateAlways: " + IsAnimateAlways.ToString() + "]\n" +
"[HasMiniMapColor: " + HasMiniMapColor.ToString() + "]\n" +
"[HasLensHelp: " + HasLensHelp.ToString() + "]\n" +
"[IsFullGround: " + IsFullGround.ToString() + "]\n" +
"[IsIgnoreLook: " + IsIgnoreLook.ToString() + "]\n" +
"[IsCloth: " + IsCloth.ToString() + "]\n" +
"[IsMarketable: " + IsMarketable.ToString() + "]\n" +
"[IsUsable: " + IsUsable.ToString() + "]\n" +
"[IsWrapable: " + IsWrapable.ToString() + "]\n" +
"[IsUnwrapable: " + IsUnwrapable.ToString() + "]\n" +
"[IsTopEffect: " + IsTopEffect.ToString() + "]\n" +
"[IsNotPreWalkable: " + IsNotPreWalkable.ToString() + "]\n" +
(IsGround ? "[GroundSpeed: " + GroundSpeed.ToString() + "]\n" : "");
        }
        #endregion
    }
}