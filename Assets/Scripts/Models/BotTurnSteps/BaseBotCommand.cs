using Assets.Scripts.Enums;
using Assets.Scripts.Models.Actions;
using UnityEngine;

namespace Assets.Scripts.Models.BotTurnSteps
{
    public abstract class BaseBotCommand
    {
        public virtual BotCommandType Type { get; }
    }

    public class MoveBotCommand : BaseBotCommand
    {
        public override BotCommandType Type => BotCommandType.Move;

        public Vector3Int[] Path { get; set; }
    }

    public class ExecuteActionBotCommand : BaseBotCommand
    {
        public override BotCommandType Type => BotCommandType.ExecuteAction;
        public BaseAction Action { get; set; }
        public Vector3Int TargetCordinate { get; set; }
    }

    public class SkipBotCommand : BaseBotCommand
    {
        public override BotCommandType Type => BotCommandType.Skip;
    }
}