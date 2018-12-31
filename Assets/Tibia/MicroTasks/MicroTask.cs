using System;
using UnityEngine;

namespace Assets
{
    public class MicroTask
    {
        public float Seconds;
        public Action<MicroTask> Action;
        public bool Running;
        public int CallCount;

        public int MaxCallCount;
        public MicroTimer Timer;

        public void Cancel()
        {
            MaxCallCount = -1;
        }
    }
}
