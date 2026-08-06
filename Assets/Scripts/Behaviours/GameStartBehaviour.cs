using Assets.Scripts.Managers;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Behaviours
{
    class GameStartBehaviour : MonoBehaviour
    {

        [Inject]
        private readonly IGameSceneStartManager _gameSceneStartManager;

        public void Start()
        {
            _gameSceneStartManager.InitScene();
        }
    }
}