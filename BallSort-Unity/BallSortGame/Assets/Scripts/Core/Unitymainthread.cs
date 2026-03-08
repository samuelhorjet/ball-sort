using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace BallSort
{
    /// <summary>
    /// Dispatches actions back to the Unity main thread from async/background threads.
    /// Attach this MonoBehaviour to the Bootstrap GameObject.
    /// </summary>
    public class UnityMainThread : MonoBehaviour
    {
        private static UnityMainThread _instance;
        private static readonly ConcurrentQueue<Action> _queue = new();

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            while (_queue.TryDequeue(out var action))
            {
                try { action(); }
                catch (Exception e) { Debug.LogError($"[MainThread] {e}"); }
            }
        }

        public static void Enqueue(Action action) => _queue.Enqueue(action);
    }
}