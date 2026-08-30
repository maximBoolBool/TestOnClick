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
}
