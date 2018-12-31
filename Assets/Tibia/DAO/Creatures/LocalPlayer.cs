using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Tibia.ClassicNetwork;
using Assets.Tibia.DAO.Extensions;
using Assets.Tibia.UI.GameInterface;
using Game.DAO;
using GameClient.Network;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public class LocalPlayer : Player
    {
        private static LocalPlayer _current;
        public static LocalPlayer Current
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
                if (_current == null)
                    GameObject.FindObjectOfType<UIHealthInfo>().ResetController();
                else
                    GameObject.FindObjectOfType<UIHealthInfo>().Subscribe(value);
            }
        }
        public bool IsKnown;
        internal bool IsMounted;
        internal bool IsPartyMember;
        internal bool IsPartyLeader;
        internal bool IsPartySharedExperienceActive;
        internal static Creature AttackingCreature;
        internal static Creature FollowingCreature;
        private Direction LastWalkDir;
        private MicroTask WalkTask = new MicroTask();
        private static bool IsAutoWalking;
        private static bool IsServerWalking;
        private static bool IsFollowing;
        private static int WalkLockExpiration;
        private static MicroTimer LockWalkTimer = new MicroTimer();
        private bool PreWalking;
        private bool LastPrewalkDone;
        private Vector3 LastPrewalkDestination;
        private Vector3 AutoWalkDestination;
        private Vector3 LastAutoWalkPosition;
        private MicroTask AutoWalkContinueTask = new MicroTask();
        private static bool IsAttacking;
        public static int seq;
        private bool KnownCompletePath;
        private bool IsPreMove;


        internal static void ProcessPlayerModes(FightModes fightMode, ChaseModes chaseMode, bool safeMode, PVPModes pvpMode)
        {
            
        }

        internal static void ProcessAttackCancel(uint seq)
        {
            if (IsAttacking && (seq == 0 || LocalPlayer.seq == seq))
                CancelAttack();
        }

        internal static void ProcessDeath(int deathType, int penality)
        {

        }

        internal static void SetOpenPvpSituations(byte openPvpSituations)
        {
            throw new NotImplementedException();
        }

        internal static void SetUnjustifiedPoints(UnjustifiedPoints unjustifiedPoints)
        {
            //throw new NotImplementedException();
        }

        public static void Walk(Direction direction)
        {
            if (!LocalPlayer.Current.CanWalk(direction)) return;
            
            switch (direction)
            {
                case Direction.North:
                    GameServer.Instance.SendWalkNorth();
                    break;
                case Direction.East:
                    GameServer.Instance.SendWalkEast();
                    break;
                case Direction.South:
                    GameServer.Instance.SendWalkSouth();
                    break;
                case Direction.West:
                    GameServer.Instance.SendWalkWest();
                    break;
                case Direction.NorthEast:
                    GameServer.Instance.SendWalkNorthEast();
                    break;
                case Direction.SouthEast:
                    GameServer.Instance.SendWalkSouthEast();
                    break;
                case Direction.SouthWest:
                    GameServer.Instance.SendWalkSouthWest();
                    break;
                case Direction.NorthWest:
                    GameServer.Instance.SendWalkNorthWest();
                    break;
                case Direction.InvalidDirection:
                    break;
                default:
                    break;
            }
            LocalPlayer.Current.WalkingRequested = true;
        }

        public void LockWalk(int millis = 250)
        {
            WalkLockExpiration = Math.Max(WalkLockExpiration, (int)LockWalkTimer.CurrentTicks + millis);
        }

        private bool CanWalk(Direction direction)
        {
            if (LockWalkTimer.CurrentTicks < WalkLockExpiration)
                return false;

            // paralyzed
            if (Speed == 0) return false;

            var tile = Map.Current.GetTile(Position.TranslatedToDirection(direction));
            if (tile == null || !tile.IsWalkable) return false;

            if (WalkProgress > 0) return false;

            if (WalkingRequested) return false;

            return true;
        }

        internal void SetCamera()
        {
            Camera.main.transform.position = new Vector3(Sprite.Group.GetThingPosition(this).x+1.25f, Sprite.Group.GetThingPosition(this).y - 1.25f, Camera.main.transform.position.z);
        }

        internal void SetCamera(Vector3 v)
        {
            Camera.main.transform.position = new Vector3(v.x+1.25f, v.y - 1.25f, Camera.main.transform.position.z);
        }

        internal static void Look(Thing lookThing)
        {
            GameServer.Instance.SendLook(lookThing.Position, lookThing.DatId, lookThing.StackPos);
        }

        internal static void Use(Thing useThing)
        {
            var pos = (useThing.Position);
            if (!pos.IsValid()) // virtual item
                pos = new Vector3(0xFFFF, 0, 0); // inventory item

            // some items, e.g. parcel, are not set as containers but they are.
            // always try to use these items in free container slots.
            GameServer.Instance.SendUseItem(pos, useThing.DatId, useThing.StackPos, ContainerSystem.FindEmptyContainerId());
        }


       
        internal static void Attack(Creature attackCreature)
        {
            if (Config.ClientVersion >= 963)
            {
                if (attackCreature != null)
                    seq = attackCreature.Id;
            }
            else
                seq++;
            GameServer.Instance.SendAttack((uint)attackCreature.Id, (uint)seq);
        }
        
        public static void CancelFollow()
        {
            Follow(null);
        }

        internal static void Rotate(Thing thing)
        {
            GameServer.Instance.SendRotateItem(thing.Position, thing.DatId, thing.StackPos);
        }

        internal void Mount()
        {
            throw new NotImplementedException();
        }

        internal void Dismount()
        {
            throw new NotImplementedException();
        }

        internal static void CancelAttack()
        {
            Attack(null);
        }

        internal static void Follow(Creature creature)
        {
            if (creature == LocalPlayer.Current)
                return;

            // cancel when following again
            if (creature != null && creature == FollowingCreature)
                creature = null;

            if (creature != null && IsAttacking)
                CancelAttack();

            FollowingCreature = creature;

            if (Config.ClientVersion >= 963)
            {
                if (creature != null)
                    seq = creature.Id;
            }
            else
                seq++;

            GameServer.Instance.SendFollow(creature!=null ? creature.Id : 0, seq);
        }
        bool HasSight(Vector3 pos)
        {
            return Position.IsInRange(pos, Map.Current.AwareRange.Left - 1, Map.Current.AwareRange.Top - 1);
        }

        internal void CancelWalk(Direction direction)
        {
            LockWalk();

            // turn to the cancel direction
            if (direction != Direction.InvalidDirection)
                Direction = (direction);
        }

        private void Stop()
        {
            if (IsFollowing)
                CancelFollow();

            GameServer.Instance.SendStop();
        }

        internal bool HasVip(string creatureName)
        {
            return false;
        }
    }
}
