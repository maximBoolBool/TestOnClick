using Assets.Scripts.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Behaviours
{
    public class TurnBehaviour : MonoBehaviour
    {
        [Inject]
        private readonly ITurnManager _turnManager;

        public async UniTaskVoid Start()
        {
            await _turnManager.SceneStart();
        }

        public void TurnSkipHandler()
        {
             _turnManager.SkipTurnAsync().Forget();
        }
    }
}