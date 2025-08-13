using Runtime.Registrars;
using Runtime.Stages;
using UnityEngine;
using Utils.DI;

namespace Runtime
{
    [DefaultExecutionOrder(-105)]
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Registrars")] [SerializeField]
        private UtilsRegistrar utilsRegistrar;

        [SerializeField] private NetworkRegistrar networkRegistrar;

        [Space, Header("Stages")] [SerializeField]
        private ServerStage serverStage;

        private void Awake()
        {
            Application.runInBackground = true;
            
            var serviceCollection = new SimpleServiceCollection();

            Register(serviceCollection);
            InitializeStages(serviceCollection);
        }

        private void Start()
        {
            serverStage.StartServer();
        }

        private void OnDestroy()
        {
            serverStage.StopServer();
        }
        
        private void Register(IServiceCollection serviceCollection)
        {
            utilsRegistrar.Register(serviceCollection);
            networkRegistrar.Register(serviceCollection);
        }

        private void InitializeStages(IServiceCollection serviceCollection)
        {
            serverStage.Initialize(serviceCollection);
        }
    }
}