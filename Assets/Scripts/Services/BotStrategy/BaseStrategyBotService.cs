using Assets.Scripts.Models.BotTurnSteps;

namespace Assets.Scripts.Services.BotStrategy
{
    public interface IStrtegyBotService
    {
        BaseBotCommand GetNextCommand(Unit unit);
    }
}
