using System.Collections;
using UnityEngine;

namespace Game.Graphics
{
    public class TaskRunner : MonoBehaviour
    {
        private static TaskRunner Instance;

        void Awake()
        {
            Instance = this;
            
        }

        public static void Init()
        {
            GameObject go = new GameObject("TaskRunner");
            go.AddComponent<TaskRunner>();
        }

        public void StartCoroutineI(IEnumerator enumerator)
        {
            StartCoroutine(enumerator);
        }

        public static new void StartCoroutine(IEnumerator enumerator)
        {
            Instance.StartCoroutineI(enumerator);
        }
    }
}