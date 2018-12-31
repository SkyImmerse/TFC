using Game.DAO;
using UnityEngine;

namespace SkyImmerseEngine.Graphics
{
    public class SpriteMissile : Sprite
    {
        public Direction Direction = Direction.North;

        private Direction _prevDirection = Direction.InvalidDirection;
        private int _prevAnimationPhase = -1;

        public SpriteMissile()
        {
            AnimationPhase = 0;
        }

        public override void StartOffset()
        {
            offsetsScales = new System.Collections.Generic.Dictionary<string, Vector4>();
            textures = new System.Collections.Generic.Dictionary<string, Texture2D>();
            SetFrame(AnimationPhase, Direction, 0, 0);
        }

        public void SetFrame(int animationPhase, Direction direction, int mount, int addon)
        {
            int xPattern = 0, yPattern = 0;

            if (direction == Direction.NorthWest)
            {

                xPattern = 0;

                yPattern = 0;

            }
            else if (direction == Direction.North)
            {

                xPattern = 1;

                yPattern = 0;

            }
            else if (direction == Direction.NorthEast)
            {

                xPattern = 2;

                yPattern = 0;

            }
            else if (direction == Direction.East)
            {

                xPattern = 2;

                yPattern = 1;

            }
            else if (direction == Direction.SouthEast)
            {

                xPattern = 2;

                yPattern = 2;

            }
            else if (direction == Direction.South)
            {

                xPattern = 1;

                yPattern = 2;

            }
            else if (direction == Direction.SouthWest)
            {

                xPattern = 0;

                yPattern = 2;

            }
            else if (direction == Direction.West)
            {

                xPattern = 0;

                yPattern = 1;

            }
            else
            {

                xPattern = 1;

                yPattern = 1;

            }
            mainTextureOffset = GetVector2(xPattern, yPattern, 0, animationPhase, 0);

        }

        public new Vector2 GetVector2(
            int patternX,
            int patternY,
            int patternZ,
            int frame, int layer = 0)
        {

            var spriteSize = Location.width / (ThingType.NumPattern.x * ThingType.AnimationPhases * ThingType.Layers);
            var spriteHeight = Location.height / (ThingType.NumPattern.z * ThingType.NumPattern.y);
            return new Vector2(
                // direction
                (Location.x + patternX * spriteSize
                 // animation phases
                 + frame * ThingType.NumPattern.x * spriteSize
                 + layer * ThingType.AnimationPhases * ThingType.NumPattern.x * spriteSize
                ) / atlasWidth,
                (atlasHeight - Location.y - spriteHeight
                 // mount
                 - (Location.height / 2f) * (patternZ + 1) * (ThingType.NumPattern.z - 1)
                 // addon
                 - patternY * spriteHeight
                ) / atlasHeight);
        }

        public override void Update()
        {
            if (Direction != _prevDirection ||
                AnimationPhase != _prevAnimationPhase)
            {
                SetFrame(AnimationPhase, Direction, 0, 0);

                _prevDirection = Direction;
                _prevAnimationPhase = AnimationPhase;
            }
        }
    }
}