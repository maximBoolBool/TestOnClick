public interface IGameGlobalStateManager
{
    Unit? SelectedUnit { get; set; }

    void SwitchGameStatus(GameStatus gameStatus);

    GameStatus GameStatus { get; }
}

public class GameGlobalStateManager : IGameGlobalStateManager
{
    private GameStatus _gameStatus = GameStatus.Non;

    public Unit SelectedUnit { get; set; }

    public GameStatus GameStatus => _gameStatus;

    public void SwitchGameStatus(GameStatus gameStatus)
    {
        _gameStatus = gameStatus;
    }
}

public enum GameStatus
{
    ActiveTurn = 0,
    ReactiveTurn = 1,
    Loading = 2,
    Looting = 3,
    Deployment = 4,
    Non = 5
}