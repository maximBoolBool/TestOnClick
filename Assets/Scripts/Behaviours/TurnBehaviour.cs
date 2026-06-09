using Assets.Scripts.Managers;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Behaviours
{
    public class TurnBehaviour : MonoBehaviour
    {
        [Inject]
        private readonly ITurnManager _turnManager;

        void Start()
        {
            _turnManager.SceneStart();
        }

        public void TurnSkipHandler()
        {
            _turnManager.SkipTurn();
        }
    }
}