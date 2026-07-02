namespace Assets.Scripts.Services
{
    public interface IActionCostService
{
    public bool IsActionAvaliable(int points, int pointCost);
}

    public class ActionCostService : IActionCostService
    {
        public bool IsActionAvaliable(int points, int pointCost)
        {
            // НИ одно действие не должно быть бесплатным!!!!
            if (pointCost == 0)
            {
                return false;
            }
            return points >= pointCost;
        }
    }
}
     