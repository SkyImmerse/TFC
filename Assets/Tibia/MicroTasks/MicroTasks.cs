using Assets.Tibia.ClassicNetwork;
using Assets.Tibia.DAO;
using Game.DAO;
using Game.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets
{
    public class MicroTasks : MonoBehaviour
    {
        public static float DequeueDelay = 0.4f;
        private static Queue<Action> Tasks = new Queue<Action>();
        private static Queue<MicroTask> Queue = new Queue<MicroTask>();
        private static MicroTasks staticMain;
        public static event Action QueueEmpty;

        void FixedUpdate()
        {
            MapRenderer.Update();
        }
        private void Update()
        {
            MapRenderer.Draw();
            while (Tasks.Count > 0)
            {
                Tasks.Dequeue()?.Invoke();
            }

            if(Input.GetKey(KeyCode.UpArrow))
            {
                LocalPlayer.Walk(Direction.North);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                LocalPlayer.Walk(Direction.West);
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                LocalPlayer.Walk(Direction.East);
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                LocalPlayer.Walk(Direction.South);
            }

        }

        private void Awake()
        {
            staticMain = this;
            var worker = new Thread(TaskQueue);
            worker.Start();
        }

        public IEnumerator PeriodicTask(MicroTask task)
        {
            while (task.Running)
            {
                yield return new WaitForSeconds(task.Seconds);
                task.CallCount++;
                // task stoped
                if(task.MaxCallCount < 0)
                {
                    task.Running = false;
                    yield break;
                }
                task.Action?.Invoke(task);
                if (task.MaxCallCount > 0 && task.CallCount >= task.MaxCallCount)
                {
                    task.Running = false;
                    yield break;
                }
            }
        }

        public void TaskQueue()
        {
            int lastCount = Queue.Count;
            while (true)
            {
                System.Threading.Thread.Sleep((int)(DequeueDelay*1000));

                if (Queue.Count > 0)
                {
                    var task = Queue.Dequeue();
                    task.CallCount++;
                    MicroTasks.Dispatch(() => task.Action?.Invoke(task));
                }
                if(lastCount > 0 && Queue.Count == 0)
                {
                    MicroTasks.Dispatch(() =>
                    {
                        QueueEmpty?.Invoke();
                    });
                }
                lastCount = Queue.Count;
            }
        }

        public static void Dispatch(Action action)
        {
            Tasks.Enqueue(action);
        }

        public static Coroutine CreateCoroutine(IEnumerator enumerator)
        {
            return staticMain.StartCoroutine(enumerator);
        }

        public static void DropCoroutine(Coroutine enumerator)
        {
            if (enumerator != null)
                staticMain.StopCoroutine(enumerator);
        }

        public static MicroTask PeriodicTask(Action<MicroTask> action, float periodic, bool initTimer = true)
        {
            var task = new MicroTask() { Action = action, Seconds = periodic, Running = true };
            if (initTimer)
            {
                task.Timer = new MicroTimer();
            }
            staticMain.StartCoroutine(staticMain.PeriodicTask(task));

            return task;
        }

        public static MicroTask DelayedTask(Action<MicroTask> action, float delay = 0, bool initTimer = true)
        {
            var task = new MicroTask() { Action = action, Seconds = delay, Running = true, MaxCallCount = 1 };
            if(initTimer)
            {
                task.Timer = new MicroTimer();
            }
            staticMain.StartCoroutine(staticMain.PeriodicTask(task));

            return task;
        }

        public static MicroTask QueueTask(Action<MicroTask> action)
        {
            var task = new MicroTask() { Action = action, Seconds = 0, Running = true, MaxCallCount = 1 };

            Queue.Enqueue(task);

            return task;
        }
    }
}
