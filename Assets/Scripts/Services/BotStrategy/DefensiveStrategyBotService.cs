using Assets.Scripts.Helpers;
using Assets.Scripts.Models.BotTurnSteps;

namespace Assets.Scripts.Services.BotStrategy
{
    public interface IDefensiveStrategyBotService : IStrtegyBotService { }

    public class DefensiveStrategyBotService : IDefensiveStrategyBotService
    {
        public BaseBotCommand GetNextCommand(Unit unit)
        {
            var moveCommand = GetMoveCommand(unit);

            if (moveCommand != null)
            {
                return moveCommand;
            }

            var actionCommand = GetActionCommand(unit);

            if (actionCommand != null)
            {
                return actionCommand;
            }

            return BotStratagyHelper.GetSharedFinishStep();
        }

        private BaseBotCommand? GetActionCommand(Unit unit)
        {
            return null;
        }

        private BaseBotCommand? GetMoveCommand(Unit unit)
        {
            return null;
        }
    }
}
