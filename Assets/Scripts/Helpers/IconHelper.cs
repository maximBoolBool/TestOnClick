using System;

public static class IconHelper
{
    public static string GetIconFullPath(object obj)
    {
        return obj switch
        {
            BaseEquipment equipment => $"Icons/Equipment/{equipment.IconName}",
            Rune rune => $"Icons/Equipment/{RuneHelper.GetRuneIconName(rune.Type)}",
            _ => throw new NotImplementedException($"Тип {obj.GetType()} не поддерживается"),
        };
    }
}