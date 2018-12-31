using Assets.Tibia.DAO.Extensions;
using Assets.Tibia.Game_Systems;
using Assets.Tibia.UI.GameInterface;
using Game.DAO;
using GameClient.Network;
using SkyImmerseEngine.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SpriteMask = Game.DAO.SpriteMask;

namespace Assets.Tibia.DAO
{
    public class Creature : Thing
    {

        public UICreatureInformation Information;

        public enum CreatureConst
        {
            ShieldBlinkTicks = 500,
            VolatileSquareDuration = 1000
        }


        public int Id;
        public string Name;
        private byte healthPercent;
        public byte HealthPercent
        {
            set
            {
                if (value > 92)
                    InformationColor = new Color(0x00 / 255f, 0xBC / 255f, 0x00 / 255f);
                else if (value > 60)
                    InformationColor = new Color(0x50 / 255f, 0xA1 / 255f, 0x50 / 255f);
                else if (value > 30)
                    InformationColor = new Color(0xA1 / 255f, 0xA1 / 255f, 0x00 / 255f);
                else if (value > 8)
                    InformationColor = new Color(0xBF / 255f, 0x0A / 255f, 0x0A / 255f);
                else if (value > 3)
                    InformationColor = new Color(0x91 / 255f, 0x0F / 255f, 0x0F / 255f);
                else
                    InformationColor = new Color(0x85 / 255f, 0x0C / 255f, 0x0C / 255f);

                healthPercent = value;
                Information?.SetInfo(InformationColor, HealthPercent/100f, -1);
                if (value <= 0)
                    OnDeath();
            }
            get => healthPercent;
        }
        public Direction Direction
        {
            get => Sprite ==null ? Direction.InvalidDirection : ((SpriteCreature)Sprite).Direction;
            set
            {
                if (Sprite != null && Sprite is SpriteCreature)
                    ((SpriteCreature)Sprite).Direction = value;
            }

        }
        private Outfit outfit;
        public Outfit Outfit
        {
            get => outfit;
            set
            {
                if (value.Category != ThingCategory.ThingCategoryCreature)
                {
                    if (!ThingTypeManager.IsValidDatId((ushort)value.LookTypeId, value.Category))
                        return;
                    outfit = value;
                }
                else
                {
                    if (value.LookTypeId > 0 && !ThingTypeManager.IsValidDatId((ushort)value.LookTypeId, ThingCategory.ThingCategoryCreature))
                    {
                        Debug.LogError("Error outfit " + outfit.LookTypeId);
                        return;
                    }
                    outfit = value;
                }
                WalkAnimationPhase = 0; // might happen when player is walking and outfit is changed.
                DatId = (ushort)value.LookTypeId;

                if (Sprite != null)
                {
                    var position = Sprite.Group.GetThingPosition(this);
                    Sprite.Group.RemoveThing(this);
                    Sprite.Group.AddThing(Map.Current, this, position, StackPos);
                }
            }

        }
        protected bool WalkingRequested;
        public int Speed { get; set; }
        public double BaseSpeed;
        public PlayerSkulls Skull = PlayerSkulls.SkullNone;
        public PlayerShields Shield = PlayerShields.ShieldNone;
        public PlayerEmblems Emblem = PlayerEmblems.EmblemNone;
        public CreatureIcons Icon = CreatureIcons.NpcIconNone;
        public Sprite SkullTexture;
        public Sprite ShieldTexture;
        public Sprite EmblemTexture;
        public Sprite IconTexture;
        public bool ShowShieldTexture = true;
        public bool ShieldBlink = false;
        public bool Passable = false;
        public Color TimedSquareColor;
        public Color StaticSquareColor;
        public Color InformationColor;
        public Color OutfitColor = Color.white;

        protected Dictionary<SpeedFormula, double> SpeedFormula = new Dictionary<SpeedFormula, double>()
        {
            {Game.DAO.SpeedFormula.SpeedFormulaA, -1},
            {Game.DAO.SpeedFormula.SpeedFormulaB, -1},
            {Game.DAO.SpeedFormula.SpeedFormulaC, -1},
        };

        int FrameGroup
        {
            get => Sprite.FrameGroup;
            set => Sprite.FrameGroup = value;
        }
        public int WalkAnimationPhase
        {
            get => Sprite.AnimationPhase;
            set
            {
                if (ThingType == null) return;

                if (Sprite!=null)
                {
                    Sprite.AnimationPhase = value;
                    if (Sprite.AnimationPhase >= ThingType.Animators[FrameGroup].AnimationPhases)
                        Sprite.AnimationPhase = 0;
                    if (Sprite.AnimationPhase <= 0) Sprite.AnimationPhase = ThingType.Animators[FrameGroup].StartPhase;
                }
            }
        }

        // walk related
        protected Coroutine WalkingCoroutine;
        public float WalkProgress;
        public Vector3 TargetPosition;
        public Vector3 OldPosition;

        // jump related
        protected float JumpHeight;
        protected float JumpDuration;
        protected Vector3 JumpOffset;

        public void SetSpeedFormula(double speedA, double speedB, double speedC)
        {
            SpeedFormula[Game.DAO.SpeedFormula.SpeedFormulaA] = speedA;
            SpeedFormula[Game.DAO.SpeedFormula.SpeedFormulaB] = speedB;
            SpeedFormula[Game.DAO.SpeedFormula.SpeedFormulaC] = speedC;
        }

        public float GetSpeed(Tile tile = null)
        {
            if (Speed < 1)
                return 0;

            if(tile == null)
            {
                tile = Tile;
            }
            int groundSpeed = tile.GroundSpeed;
            if (groundSpeed == 0)
                groundSpeed = 150;

            float finalSpeed = groundSpeed * 1000f;

            if (FeatureManager.GetFeature(GameFeature.GameNewSpeedLaw) && HasSpeedFormula)
            {
                int formulatedSpeed = 1;
                if (Speed > -SpeedFormula[Game.DAO.SpeedFormula.SpeedFormulaB])
                {
                    formulatedSpeed = Mathf.Max(1, (int)Mathf.Floor(((float)SpeedFormula[Game.DAO.SpeedFormula.SpeedFormulaA] * Mathf.Log((Speed / 2f)
                         + (float)SpeedFormula[Game.DAO.SpeedFormula.SpeedFormulaB]) + (float)SpeedFormula[Game.DAO.SpeedFormula.SpeedFormulaC]) + 0.5f));
                }
                finalSpeed = finalSpeed / (float)formulatedSpeed;
            }
            else
                finalSpeed /= Speed;

            return 255 - (finalSpeed/1000f);
        }

        public bool HasSpeedFormula => (double)SpeedFormula[Game.DAO.SpeedFormula.SpeedFormulaA] != -1 && (double)SpeedFormula[Game.DAO.SpeedFormula.SpeedFormulaB] != -1 && (double)SpeedFormula[Game.DAO.SpeedFormula.SpeedFormulaC] != -1;


        internal static string FormatCreatureName(string name)
        {
            char[] formatedName = name.ToCharArray();
            if (FeatureManager.GetFeature(GameFeature.GameFormatCreatureName) && name.Length > 0)
            {
                bool upnext = true;
                for (uint i = 0; i < formatedName.Length; ++i)
                {
                    char ch = formatedName[i];
                    if (upnext)
                    {
                        formatedName[i] = char.ToUpper(ch);
                        upnext = false;
                    }
                    if (ch == ' ')
                        upnext = true;
                }
            }
            return new string(formatedName);
        }

        // walk related
        public void Turn(Direction direction)
        {
            Direction = direction;
        }

        public bool IsInvisible => Outfit.Category == ThingCategory.ThingCategoryEffect && Outfit.LookTypeId == 13;

        public bool IsDead => HealthPercent <= 0;

        public bool CanBeSeen => !IsInvisible || (this is Player);

        public override ThingType ThingType => ThingTypeManager.GetThingType((ushort)Outfit.LookTypeId, Outfit.Category);

        public virtual void OnDeath()
        {
            Information.Remove();
        }

        internal override void OnAppear()
        {

        }
        internal override void OnDisappear()
        {
        }

        internal void HideStaticSquare()
        {
            if (Information != null) Information.Frame.gameObject.SetActive(false);
            if (Information != null) Information.Frame.color = StaticSquareColor;
        }

        internal void AddTimedSquare(byte markType)
        {
            if (Information != null) Information.Frame.gameObject.SetActive(false);
            if (Information != null) Information.Frame.color = StaticSquareColor;
        }

        public virtual void Move(Tile fromTile, Tile toTile)
        {
            if (fromTile == null || toTile == null) return;
            if (WalkProgress > 0) return;
            WalkingRequested = false;

            FrameGroup = 1;

            OldPosition = Sprite.Group.GetThingPosition(this);
            // get direction

            var dir = fromTile.Position.GetDirectionFromPosition(toTile.Position);

            Direction = dir;

            var offset = new Vector3();
            if(ThingType.Size.width == 2 && ThingType.Size.height == 2)
                offset = new Vector3(-1.25f, 2.25f, 0);
            if (ThingType.Size.width == 1 && ThingType.Size.height == 1)
                offset = new Vector3(-0.5f, 1.5f, 0);
            if (ThingType.Size.width == 2 && ThingType.Size.height == 1)
                offset = new Vector3(-1.25f, 1.5f, 0);
            if (ThingType.Size.width == 1 && ThingType.Size.height == 2)
                offset = new Vector3(-0.5f, 2.25f, 0);

            var fromPosition = fromTile.RealPosition + offset;
            var toPosition = toTile.RealPosition + offset;

            fromPosition = new Vector3(fromPosition.x, fromPosition.y, 0);
            toPosition = new Vector3(toPosition.x, toPosition.y, 0);

            // get direction vector from `fromTile` to `toTile`
            var direction = (toPosition - fromPosition).normalized;

            // set animation phase to 1 (first step)
            WalkAnimationPhase = 0;

            // change phase every step
            var changePhaseStep = (Sprite.Group.GetThingPosition(this) - toPosition).magnitude / (ThingType.Animators[FrameGroup].AnimationPhases);

            MicroTasks.DropCoroutine(WalkingCoroutine);
            WalkingCoroutine = MicroTasks.CreateCoroutine(InternalMove(changePhaseStep*2, direction, toPosition, CalcWalkOrder(fromTile, toTile)));
        }
        
        private IEnumerator InternalMove(float changePhaseStep, Vector3 direction, Vector3 end, Tuple<int, int> orders)
        {
            float nowChangePhaseStep = 0;

            var orderFirstHalfWalk = orders.Item1;
            var orderSecondHalfWalk = orders.Item2;

            WalkAnimationPhase = ThingType.Animators[FrameGroup].StartPhase;

            WalkProgress = 0f;
            var maxProgress = Vector3.Distance(Sprite.Group.GetThingPosition(this), end);

            // get speed
            var _speed = (GetSpeed()/255f)*3f;

            // move from `fromTile` to `toTile`
            while ((Sprite.Group.GetThingPosition(this) - end).magnitude > 0.01f)
            {
                // shift every iteration
                var shift = direction * (_speed) * (Time.deltaTime);

                nowChangePhaseStep += ((Sprite.Group.GetThingPosition(this) + shift) - Sprite.Group.GetThingPosition(this)).magnitude;
                if (nowChangePhaseStep >= changePhaseStep)
                {
                    WalkAnimationPhase += Mathf.RoundToInt(nowChangePhaseStep / changePhaseStep);
                    nowChangePhaseStep = 0;
                }

                var topos = Vector3.MoveTowards(Sprite.Group.GetThingPosition(this), end, (_speed) * (Time.deltaTime));

                Sprite.Group.ChangeThingPosition(this, topos);

                WalkProgress = 1.1f - Vector3.Distance(Sprite.Group.GetThingPosition(this), end) / maxProgress;

                if (WalkProgress < 0.75f)
                {
                    Order = Sprite.Group.ChangeThingOrder(this, orderFirstHalfWalk);
                }
                else
                {
                    Order = Sprite.Group.ChangeThingOrder(this, orderSecondHalfWalk);
                }

                // if thing is LocalPlayer -> set camera position
                if (this is LocalPlayer)
                {
                    ((LocalPlayer)this).SetCamera(Sprite.Group.GetThingPosition(this));
                }

                yield return new WaitForEndOfFrame();
            }
            if (this is LocalPlayer)
                {
                    ((LocalPlayer)this).SetCamera(Sprite.Group.GetThingPosition(this));
                }
            Order = Sprite.Group.ChangeThingOrder(this, orderSecondHalfWalk);
            WalkAnimationPhase = ThingType.Animators[FrameGroup].StartPhase;
            FrameGroup = 0;
            WalkProgress = 0;
        }

        public Tuple<int, int> CalcWalkOrder(Tile fromTile, Tile toTile)
        {

            var orderFirstHalfWalk = 0;
            var orderSecondHalfWalk = 0;

            var topOrderFromTile = fromTile
                .Things.Where(e => !e.ThingType.IsOnTop && e != this).OrderBy(e => e.StackPos).Last().Order;

            var thingBelowCreature = 0;
            for (var x = 0; x < ThingType.Size.width+1; x++)
            {
                for (var y = 0; y < ThingType.Size.height+1; y++)
                {
                    var xyTile = Map.Current.GetTile(toTile.Position + new Vector3(-x, -y, 0));

                    if (xyTile == null) continue;

                    var list = xyTile.Things.ToList();
                    var woTop = list.Where(e => !e.ThingType.IsOnTop && e != this);
                    var thingMax = 0;
                    if (woTop.Count() > 0)
                    {
                        var ordered = woTop.OrderBy(e => e.StackPos);
                        if(ordered.Count() > 0)
                        {
                           thingMax = ordered.Last().Order;
                        }
                    }
                    
                    thingBelowCreature = Mathf.Max(thingBelowCreature, thingMax);
                }
            }


            var topOrderToTile = toTile
                .Things.Where(e => !e.ThingType.IsOnTop && e != this).OrderBy(e => e.StackPos).Last().Order;

            orderFirstHalfWalk = Mathf.Max(thingBelowCreature, topOrderFromTile) + (int)ThingType.Size.width * (int)ThingType.Size.height;

            orderSecondHalfWalk = topOrderToTile + (int)ThingType.Size.width * (int)ThingType.Size.height;

            return new Tuple<int, int>(orderFirstHalfWalk, orderSecondHalfWalk);
        }

        internal void ShowStaticSquare()
        {
            if (Information != null) Information.Frame.gameObject.SetActive(true);
            if (Information!=null) Information.Frame.color = StaticSquareColor;
        }
    }
}
