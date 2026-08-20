using Assets.Scripts.Helpers;
using Assets.Scripts.Managers.UnitManager;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IUnitQueueUiService
    {
        UniTask SetUniticonsAsync();

        UniTask MoveQueueUiAsync();
    }

    public class UnitQueueUiManager : IUnitQueueUiService
    {
        private const int ITEMS_Y_CORDINATES = 0;

        private static readonly int[] ITEMS_X_CORDINATES = { -100, -50, 0, 50, 100 };

        [Inject]
        private readonly IUnitManager _unitManager;

        [Inject(Id = Constants.UnitQueuePanel)]
        private readonly GameObject queue;

        private Dictionary<string, Sprite> _unitIconSprites = new();

        private List<GameObject> _queueItems = new();

        public async UniTask SetUniticonsAsync()
        {
            ClearQueueItems();

            var livingUnits = _unitManager.Units
                .Where(x => !x.IsDead)
                .ToArray();

            if (livingUnits.Length == 0) return;

            var queueItemPrefab = await Addressables.LoadAssetAsync<GameObject>(Constants.UnitQueueItemPrefab);

            var iconNames = livingUnits
                .Select(x => UnitLoadIconHelper.GetUnitIconAddressableName(x.Name, x.Characteristic.Side))
                .Distinct()
                .ToArray();

            var sprites = await Addressables.LoadAssetsAsync<Sprite>(
                iconNames,
                callback: null,
                Addressables.MergeMode.Union
            );
            _unitIconSprites = sprites.ToDictionary(sprite => sprite.texture.name, sprite => sprite);

            var items = await GameObject.InstantiateAsync<GameObject>(
                original: queueItemPrefab,
                count: livingUnits.Length,
                parent: queue.transform
            );

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var unit = livingUnits[i];

                _queueItems.Add(item);

                item.transform.localScale = Vector3.one;
                item.transform.localRotation = Quaternion.identity;

               item.transform.localPosition = new Vector3(ITEMS_X_CORDINATES[i], ITEMS_Y_CORDINATES, 0);

                var iconName = UnitLoadIconHelper.GetUnitIconAddressableName(unit.Name, unit.Characteristic.Side);
                if (_unitIconSprites.TryGetValue(iconName, out var sprite))
                {
                    item.transform.Find("Image").GetComponent<Image>().sprite = sprite;
                }
            }
        }

        public async UniTask MoveQueueUiAsync()
        {

        }

        private void ClearQueueItems()
        {
            foreach (var item in _queueItems)
            {
                Object.Destroy(item);
            }
            _queueItems.Clear();
        }
    }
}
