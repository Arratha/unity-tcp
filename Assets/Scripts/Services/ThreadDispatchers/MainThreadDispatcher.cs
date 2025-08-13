using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Services.ThreadDispatchers
{
    public class MainThreadDispatcher : MonoBehaviour, IMainThreadDispatcher
    {
        private readonly Queue<Action> _executionQueue = new();

        private object _lock = new();

        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _executionQueue.Enqueue(action);
            }
        }

        private void Update()
        {
            lock (_lock)
            {
                while (_executionQueue.Any())
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }
    }
}