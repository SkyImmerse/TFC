using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public class Animator
    {
        public enum AnimationPhase
        {
            AnimPhaseAutomatic = -1,
            AnimPhaseRandom = 254,
            AnimPhaseAsync = 255,
        }

        public enum AnimationDirection
        {
            AnimDirForward = 0,
            AnimDirBackward = 1
        }

        public int AnimationPhases = 1;
        private int startPhase;
        public int LoopCount;
        private bool Async;
        public readonly List<Tuple> PhaseDurations = new List<Tuple>();

        private int CurrentDuration;
        public AnimationDirection CurrentDirection;
        private int CurrentLoop;

        private long LastPhaseTicks;
        private bool IsComplete;

        private int Phase;

        public Animator()
        {
            AnimationPhases = 1;
            startPhase = 0;
            LoopCount = 0;
            Async = false;
            CurrentDuration = 0;
            CurrentDirection = AnimationDirection.AnimDirForward;
            CurrentLoop = 0;
            LastPhaseTicks = 0;
            IsComplete = false;
            Phase = 0;
        }

        public void ResetPhaseDurarations()
        {
            for (var i = 0; i < AnimationPhases; ++i)
            {
                var minimum = 250;
                var maximum = 250;
                PhaseDurations.Add(new Tuple(minimum, maximum));
            }
        }
        public void Unserialize(int animationPhases, BinaryReader fin)
        {
            AnimationPhases = animationPhases;
            Async = fin.ReadByte() == 0;
            LoopCount = fin.ReadInt32();
            StartPhase = fin.ReadSByte();

            for (var i = 0; i < AnimationPhases; ++i)
            {
                var minimum = (int)fin.ReadUInt32();
                var maximum = (int)fin.ReadUInt32();
                PhaseDurations.Add(new Tuple(minimum, maximum));
            }

            Phase = StartPhase;

            Debug.Assert(AnimationPhases == PhaseDurations.Count);
            Debug.Assert(StartPhase >= -1 && StartPhase < AnimationPhases);
        }

        public void SetPhase(int phase)
        {
            if (Phase == phase)
                return;

            if (Async)
            {
                if (phase == (int)AnimationPhase.AnimPhaseAsync)
                    Phase = 0;
                else if (phase == (int)AnimationPhase.AnimPhaseRandom)
                    Phase = UnityEngine.Random.Range(0, AnimationPhases);
                else if (phase >= 0 && phase < AnimationPhases)
                    Phase = phase;
                else
                    Phase = StartPhase;

                IsComplete = false;
                LastPhaseTicks = (long)Time.realtimeSinceStartup * 1000;
                CurrentDuration = GetPhaseDuration(phase);
                CurrentLoop = 0;
            }
            else
                CalculateSynchronous();
        }

        public int GetPhase()
        {
            long ticks = (long)Time.realtimeSinceStartup * 1000;
            if (ticks != LastPhaseTicks && !IsComplete)
            {
                var elapsedTicks = (int)(ticks - LastPhaseTicks);
                if (elapsedTicks >= CurrentDuration)
                {
                    var phase = 0;
                    if (LoopCount < 0)
                        phase = PingPongPhase;
                    else
                        phase = LoopPhase;

                    if (Phase != phase)
                    {
                        var duration = GetPhaseDuration(phase) - (elapsedTicks - CurrentDuration);
                        if (duration < 0 && !Async)
                        {
                            CalculateSynchronous();
                        }
                        else
                        {
                            Phase = phase;
                            CurrentDuration = Math.Max(0, duration);
                        }
                    }
                    else
                        IsComplete = true;
                }
                else
                    CurrentDuration -= elapsedTicks;

                LastPhaseTicks = ticks;
            }
            return Phase;
        }

        public int StartPhase {
            set => startPhase = value;
            get
            {
                if (startPhase > -1)
                    return startPhase;
                return rand.Next(0, AnimationPhases);
            }
        }

        System.Random rand = new System.Random();
        

        public void ResetAnimation()
        {
            IsComplete = false;
            CurrentDirection = AnimationDirection.AnimDirForward;
            CurrentLoop = 0;
            SetPhase((int)AnimationPhase.AnimPhaseAutomatic);
        }

        private int PingPongPhase
        {
            get
            {
                int count = CurrentDirection == AnimationDirection.AnimDirForward ? 1 : -1;
                var nextPhase = Phase + count;
                if (nextPhase < 0 || nextPhase >= AnimationPhases)
                {
                    CurrentDirection = CurrentDirection == AnimationDirection.AnimDirForward ? AnimationDirection.AnimDirBackward : AnimationDirection.AnimDirForward;
                    count *= -1;
                }
                return Phase + count;
            }
        }

        private int LoopPhase 
        {
            get
            {
                var nextPhase = Phase + 1;
                if (nextPhase < AnimationPhases)
                    return nextPhase;

                if (LoopCount == 0)
                    return 0;

                if (CurrentLoop < (LoopCount - 1))
                {
                    CurrentLoop++;
                    return 0;
                }

                return Phase;
            }
        }

        public int GetPhaseDuration(int phase)
        {
            Tuple data = PhaseDurations[phase];
            int min = data.Minimum;
            int max = data.Maximum;
            if (min == max) return min;
            return (int)rand.Next((int)min, (int)max);
        }

        private void CalculateSynchronous()
        {
            var totalDuration = 0;
            for (var i = 0; i < AnimationPhases; i++)
                totalDuration += GetPhaseDuration(i);

            long ticks = (long)Time.realtimeSinceStartup * 1000;
            var elapsedTicks = (int)(ticks % totalDuration);
            var totalTime = 0;
            for (var i = 0; i < AnimationPhases; i++)
            {
                var duration = GetPhaseDuration(i);
                if (elapsedTicks >= totalTime && elapsedTicks < totalTime + duration)
                {
                    Phase = i;
                    CurrentDuration = duration - (elapsedTicks - totalTime);
                    break;
                }
                totalTime += duration;
            }
            LastPhaseTicks = ticks;
        }

        public class Tuple
        {
            public int Minimum;
            public int Maximum;

            public Tuple(int minimum, int maximum)
            {
                this.Minimum = minimum;
                this.Maximum = maximum;
            }
        }
    }

}
