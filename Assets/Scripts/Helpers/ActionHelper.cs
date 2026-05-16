public static class ActionHelper
{
    public static BaseAction[] GetUserUnitTestActions()
    {
        return new BaseAction[]
        {
            new UnitTargetAction()
            {
                PointCost = 2,
                Name = "Hit",
                Description = "Hit enemy. Fust!!",
                Range = 1,
                Steps = new BaseActionStep[]
                {
                    new ActionDamageStep()
                    {
                        Order = 1,
                        MaxDamageValue = 1,
                        MinDamageValue = 1
                    },
                }
            }
        };
    }
}