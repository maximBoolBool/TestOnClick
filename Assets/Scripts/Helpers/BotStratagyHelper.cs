using Assets.Scripts.Models.BotTurnSteps;

namespace Assets.Scripts.Helpers
{
    public static class BotStratagyHelper
    {
        public static BaseBotCommand GetSharedFinishStep()
        {
            return new SkipBotCommand();
        }
    }
}
