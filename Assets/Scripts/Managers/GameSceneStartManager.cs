using Zenject;

namespace Assets.Scripts.Managers
{
    public interface IGameSceneStartManager
    {
        void InitScene();
    }

    class GameSceneStartManager : IGameSceneStartManager
    {
        [Inject]
        private readonly IGridLayersManager _gridLayersManager;

        public void InitScene()
        {

        }

        public void InitRoomLayers() 
        {
        
        }
    }
}
