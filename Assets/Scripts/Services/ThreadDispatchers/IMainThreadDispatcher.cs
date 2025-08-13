using System;

namespace Services.ThreadDispatchers
{
    public interface IMainThreadDispatcher
    {
        public void Enqueue(Action action);
    }
}