using UnityEngine;
using Zenject;

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