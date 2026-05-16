using Assets.Scripts.Models.Actions;
using Assets.Scripts.Models.Conditions;

namespace Assets.Scripts.Models
{
    public class BaseRuneCombinationEffect
    {
        public BaseAction[] Actions { get; set; }

        public BaseActionStep[] ActionSteps { get; set; }

        public BaseCondition[] Conditions { get; set; }
    }
}