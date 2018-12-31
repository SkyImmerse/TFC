using Assets.Tibia.DAO;
using Assets.Tibia.DAO.Extensions;
using Game.DAO;
using SkyImmerseEngine;
using SkyImmerseEngine.Graphics;
using SkyImmerseEngine.Loaders;
using SkyImmerseEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Graphics
{
    public class RenderThingGroup
    {
        public float ScaleFactor = 1;
        private Dictionary<Vector3, Thing> _posToThings = new Dictionary<Vector3, Thing>();

        private Dictionary<Thing, Vector3> _thingPos = new Dictionary<Thing, Vector3>();
        private Dictionary<Thing, Vector3> _thingScale = new Dictionary<Thing, Vector3>();

        public Dictionary<Thing, Vector4> _thingOffsets = new Dictionary<Thing, Vector4>();
        public Dictionary<Thing, Vector4>.ValueCollection ThingOffsets
        {
            get
            {
                return _thingOffsets.Values;
            }
        }
        public Dictionary<string, Dictionary<Thing, Vector4>> AdditionalThingOffsets { get; } = new Dictionary<string, Dictionary<Thing, Vector4>>();
        public Dictionary<string, Texture2D> AdditionalThingTextures { get; } = new Dictionary<string, Texture2D>();

        public Dictionary<Thing, Matrix4x4> _thingsPositions = new Dictionary<Thing, Matrix4x4>();
        public Dictionary<Thing, Matrix4x4>.ValueCollection ThingPositions
        {
            get
            {
                return _thingsPositions.Values;
            }
        }

        public int Layer;
        private int _layer;
        public Dictionary<Thing, SkyImmerseEngine.Sprite> Things = new Dictionary<Thing, SkyImmerseEngine.Sprite>();
        private Dictionary<Thing, Vector4> _lightPositions = new Dictionary<Thing, Vector4>();
        public List<Vector4> LightPositions
        {
            get
            {
                return _lightPositions.Values.ToList();
            }
        }

        public SkyImmerseEngine.Sprite AddThing(Map map, Thing t, Vector3 position, int stackPos)
        {
            t.RealPosition = position;
            // thing type
            var thingType = t.ThingType;

            // Z Position
            var zPosition = stackPos + (thingType.Size.width +thingType.Size.height) / 2f;

            var tuple = AtlasSpriteManager.GetThingType((t.DatId), (int)thingType.Category);

            if (tuple == null) return null;

            TextureLocation thingLocation = tuple.Item2;

            var mainTex = tuple.Item1.mainTexture;

            var mainTextureScale = new Vector2(
                (thingLocation.width/(thingType.NumPattern.x*thingType.AnimationPhases) / mainTex.width),
                (thingLocation.height /(thingType.NumPattern.y * thingType.NumPattern.z* thingType.Layers)) / mainTex.height);

            SkyImmerseEngine.Sprite behaviorComponent = null;
            switch (thingType.Category)
            {
                case ThingCategory.ThingCategoryItem:
                    behaviorComponent = new SkyImmerseEngine.Sprite();
                    break;
                case ThingCategory.ThingCategoryCreature:
                    behaviorComponent = new SpriteCreature();
                    ((SpriteCreature)behaviorComponent).AnimationPhase = 0;
                    ((SpriteCreature)behaviorComponent).Addon = ((Creature)t).Outfit.Addons;
                    ((SpriteCreature)behaviorComponent).Mount = ((Creature)t).Outfit.Mount > 0;

                    break;
                case ThingCategory.ThingCategoryEffect:
                    behaviorComponent = new SkyImmerseEngine.Sprite();
                    break;
                case ThingCategory.ThingCategoryMissile:
                    behaviorComponent = new SkyImmerseEngine.Graphics.SpriteMissile();
                    break;
            }
            behaviorComponent.atlasTex = (Texture2D)mainTex;
            behaviorComponent.ThingType = thingType;
            behaviorComponent.Group = this;
            behaviorComponent.mainTextureScale = mainTextureScale;
            behaviorComponent.Map = map;
            behaviorComponent.Thing = t;
            behaviorComponent.Location = thingLocation;
            behaviorComponent.atlasWidth = mainTex.width;
            behaviorComponent.atlasHeight = mainTex.height;
            behaviorComponent.Layer = 0;
            behaviorComponent.AnimationPhase = thingType.Animators[0].StartPhase;
            behaviorComponent.AnimationDirection = thingType.Animators[0].CurrentDirection;
            behaviorComponent.CurrentLoop = 0;
            behaviorComponent.AnimationDuration = 0;

            var order = Mathf.RoundToInt(zPosition);
            behaviorComponent.Order = order;


            
            var localScale = new Vector3((thingType.Size.width + 0.001f)* ScaleFactor, (thingType.Size.height + 0.001f)* ScaleFactor, 1);
            var localPosition =
                new Vector3(position.x, position.y+1, 0)
                + new Vector3(-thingType.Size.width / 2f, thingType.Size.height / 2f, 0)
                + new Vector3(-thingType.Displacement.x * 0.031f, thingType.Displacement.y * 0.031f, 0);


            

            Layer = LayerMask.NameToLayer(position.z.ToString());
            _layer = (int)position.z;

            RemoveThing(t);
            Things.AddOrUpdate(t, behaviorComponent);

            var matrix = Matrix4x4.TRS(localPosition, Quaternion.identity, localScale);
            _thingOffsets.AddOrUpdate(t, new Vector4(0, 0, mainTextureScale.x, mainTextureScale.y));
            _thingsPositions.AddOrUpdate(t, matrix);


            behaviorComponent.StartOffset();

            foreach (var item in behaviorComponent.textures)
            {
                if (!AdditionalThingTextures.ContainsKey(item.Key))
                    AdditionalThingTextures.AddOrUpdate(item.Key, (Texture2D)mainTex);

                AdditionalThingTextures[item.Key] = item.Value;
            }

            _thingScale.AddOrUpdate(t, localScale);
            _thingPos.AddOrUpdate(t, localPosition);

            if (thingType.HasLight)
            {
                var color = ByteColorConverter.from8bit(thingType.Light.Color);
                var offsetX = (localPosition.x- 1f);
                var offsetY = (localPosition.y+1.25f);
                var lightSurface = GameObject.FindObjectsOfType<LightSurface>().FirstOrDefault(e => e.gameObject.layer == Layer);
                if (lightSurface != null)
                {
                    lightSurface.Colors.AddOrUpdate(t, new Vector4(color.r / 255, color.g / 255, color.b / 255, 0.1f));
                    lightSurface.Positions.AddOrUpdate(t, new Vector4(offsetX, (offsetY), 0, 0.1f));
                }
            }

            t.Order = order;
            return behaviorComponent;
        }

        public void RemoveThing(Thing t)
        {
            t.OnDisappear();
            if (Things.ContainsKey(t))
            {
                Things.Remove(t);
                foreach (var item in AdditionalThingOffsets)
                {
                    item.Value.Remove(t);
                }
                _thingsPositions.Remove(t);
                _thingOffsets.Remove(t);
                _thingPos.Remove(t);
                _thingScale.Remove(t);
                var lightSurface = GameObject.FindObjectsOfType<LightSurface>().FirstOrDefault(e => e.gameObject.layer == Layer);
                if (lightSurface != null)
                {
                    lightSurface.Colors.Remove(t);
                    lightSurface.Positions.Remove(t);
                }
            }
        }

        public int ChangeThingOrder(Thing t, int newOrder)
        {
            if(_thingsPositions.ContainsKey(t))
            {
                t.Sprite.Order = newOrder;
                t.Sprite.UpdateAdditionalChannels();
                return newOrder;
            }
            return -99999999;
        }

        public Vector3 ChangeThingPosition(Thing t, Vector3 newPos)
        {
            if (_thingsPositions.ContainsKey(t))
            {
                _thingPos[t] = new Vector3(newPos.x, newPos.y, _thingPos[t].z);
                _thingsPositions[t] = Matrix4x4.TRS(_thingPos[t], Quaternion.identity, _thingScale[t]);
                return newPos;
            }
            return Vector3.zero;
        }
        public Vector3 GetThingPosition(Thing t)
        {
            return _thingPos.ContainsKey(t) ? new Vector3(_thingPos[t].x, _thingPos[t].y, 0) : Vector3.zero;
        }

        public void Update()
        {
            foreach (var item in Things.ToList())
            {
                item.Value.Update();

            }
        }

    }
}
