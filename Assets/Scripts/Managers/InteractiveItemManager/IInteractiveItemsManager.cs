using Assets.Scripts.Factory;
using Assets.Scripts.Models;
using System.Collections.Generic;

namespace Assets.Scripts.Managers.InteractiveItemManager
{
    public interface IInteractiveItemsManager
    {
    }

    public class InteractiveItemsManager : IInteractiveItemsManager
    {
        #region Injections

        private readonly InteractiveItemFactory _interactiveItemFactory;

        #endregion

        #region States

        private readonly List<InteractiveItem> _interactiveItems = new();

        #endregion

        #region Public Methods

        public void GenerateInteractiveItems()
        {
        }

        #endregion

        #region Private Methods


        #endregion
    }
}
