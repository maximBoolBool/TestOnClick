using Assets.Scripts.Factory;
using Assets.Scripts.Models;
using Assets.Scripts.Services;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Managers.InteractiveItemManager
{
    public interface IInteractiveItemsManager
    {
        void GenerateInteractiveItems();
    }

    public class InteractiveItemsManager : IInteractiveItemsManager
    {
        #region Injections

        [Inject(Id = Constants.INTERACTIVE_ITEMS_GO_NAME)]
        private readonly GameObject _interactiveItemsGO;

        [Inject]
        private readonly InteractiveItemFactory _interactiveItemFactory;

        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly IAddresableResourceManager _addresableResourceManager;

        #endregion

        #region States

        private readonly List<InteractiveItem> _interactiveItems = new();

        #endregion

        #region Public Methods

        public void GenerateInteractiveItems()
        {
            var cordinates = GetItemGridCordinates();

            foreach (var cordinate in cordinates)
            {
                var item = _interactiveItemFactory.Create();

                item.transform.position = _gridService.FromGridCordinates(cordinate);

                item.transform.SetParent(_interactiveItemsGO.transform);
                _interactiveItems.Add(item);
            }
        }

        #endregion

        #region Private Methods

        private Vector3Int[] GetItemGridCordinates()
        {
            return new Vector3Int[] 
            {
                new(-2,-2,0),
                new(-8,-8,0)
            };
        }

        #endregion
    }
}
