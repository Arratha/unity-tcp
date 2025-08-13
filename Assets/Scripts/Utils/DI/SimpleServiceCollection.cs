using System;
using System.Collections.Generic;

namespace Utils.DI
{
    public class SimpleServiceCollection : IServiceCollection
    {
        private readonly Dictionary<Type, Func<object>> _services = new();
        private readonly object _lock = new();

        public void RegisterSingleton<TService>(TService service) where TService : class
        {
            lock (_lock)
            {
                _services.Add(typeof(TService), () => service);
            }
        }

        public void RegisterSingleton<TService>(Func<TService> constructor) where TService : class
        {
            lock (_lock)
            {
                var service = new Lazy<TService>(constructor);
                _services.Add(typeof(TService), () => service.Value);
            }
        }

        public void RegisterTransient<TService>(Func<TService> constructor) where TService : class
        {
            lock (_lock)
            {
                _services.Add(typeof(TService), constructor);
            }
        }

        public TService Resolve<TService>() where TService : class
        {
            lock (_lock)
            {
                if (!_services.TryGetValue(typeof(TService), out var service))
                {
                    throw new InvalidOperationException($"Service of type {typeof(TService)} is not registered.");
                }

                return (TService)service();
            }
        }
    }
}