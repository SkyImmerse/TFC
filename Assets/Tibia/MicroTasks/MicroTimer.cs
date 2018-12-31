using UnityEngine;

namespace Assets
{
    public class MicroTimer {

        public long CurrentTicks => (long)(Time.realtimeSinceStartup * 1000);

        public long StartTicks = 0;
        public long TicksElapsed => (long)(CurrentTicks - StartTicks);

        public MicroTimer()
        {
            StartTicks = CurrentTicks;
        }
        public void Restart()
        {
            StartTicks = CurrentTicks;
    }
    }
}
