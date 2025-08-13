using System;

namespace Utils.DI
{
    public interface IServiceCollection
    {
        public void RegisterSingleton<TService>(TService service) where TService : class;

        public void RegisterSingleton<TService>(Func<TService> constructor) where TService : class;
        
        public void RegisterTransient<TService>(Func<TService> constructor) where TService : class;

        public TService Resolve<TService>() where TService : class;
    }
}