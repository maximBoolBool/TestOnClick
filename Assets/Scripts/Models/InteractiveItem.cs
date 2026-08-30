using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class InteractiveItem : MonoBehaviour {}

    public enum InteractiveItemType
    {
        #region Bushes

        Bush_1 = 10,
        Bush_2 = 11,
        Bush_3 = 12,
        Bush_4 = 13,

        #endregion

        #region Trees

        Tree_1 = 20,
        Tree_2 = 21,
        Tree_3 = 22,
        Tree_4 = 23,

        #endregion

        #region GoldStones
        
        GoldStone_1 = 30,
        GoldStone_2 = 31,
        GoldStone_3 = 32,
        GoldStone_4 = 33,

        #endregion

        #region Stumps

        Stump_1 = 40,
        Stump_2 = 41,
        Stump_3 = 42,
        Stump_4 = 43

        #endregion
    }

    public static class InteractiveItemTypeExtensions
    {
        public static string GetAnimatorOverrideControllerName(this InteractiveItemType itemType)
        {
            return itemType switch
            {
                InteractiveItemType.Bush_1 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.BUSH_1,
                InteractiveItemType.Bush_2 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.BUSH_2,
                InteractiveItemType.Bush_3 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.BUSH_3,
                InteractiveItemType.Bush_4 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.BUSH_4,
                InteractiveItemType.Tree_1 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.TREE_1,
                InteractiveItemType.Tree_2 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.TREE_2,
                InteractiveItemType.Tree_3 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.TREE_3,
                InteractiveItemType.Tree_4 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.TREE_4,
                InteractiveItemType.GoldStone_1 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.GOLDEN_STONE_1,
                InteractiveItemType.GoldStone_2 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.GOLDEN_STONE_2,
                InteractiveItemType.GoldStone_3 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.GOLDEN_STONE_3,
                InteractiveItemType.GoldStone_4 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.GOLDEN_STONE_4,
                InteractiveItemType.Stump_1 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.STUMP_1,
                InteractiveItemType.Stump_2 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.STUMP_2,
                InteractiveItemType.Stump_3 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.STUMP_3,
                InteractiveItemType.Stump_4 => InteractiveItemsAnimatorOverrideControllerAdressableResourceNames.STUMP_4,
                _ => throw new System.ArgumentOutOfRangeException(nameof(itemType), itemType, null)
            };
        }
    }
}
