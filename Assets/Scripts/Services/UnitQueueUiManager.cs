using Assets.Scripts.Helpers;
using Assets.Scripts.Managers.UnitManager;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        private const int FADE_MIN_VALUE = 0;
        private const float ANIMATION_DURATION = 2f;
        private const int ITEMS_Y_CORDINATES = 0;
        private const int FIRST_ITEM_DELETE_RADIUS = 50;
        private const int ICON_GAP_ITEMS = 50;
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
            var actualUnitIcon = _queueItems[0];
            _queueItems.RemoveAt(0);

            var sequence = DOTween.Sequence();

            var actualUnitIconTargetposition = actualUnitIcon.transform.position + new Vector3Int(-FIRST_ITEM_DELETE_RADIUS, 0, 0);
            var actualIconImage = actualUnitIcon.transform.Find("Image").GetComponent<Image>();

            sequence.Join(actualUnitIcon.transform.DOMove(actualUnitIconTargetposition, ANIMATION_DURATION).SetEase(Ease.InOutCubic));
            sequence.Join(actualIconImage.DOFade(FADE_MIN_VALUE, ANIMATION_DURATION));

            foreach (var item in _queueItems)
            {
                var targetPosition = item.transform.position + new Vector3Int(-ICON_GAP_ITEMS, 0, 0);
                sequence.Join(item.transform.DOMove(targetPosition, ANIMATION_DURATION).SetEase(Ease.InOutCubic));
            }

            var zz = _unitManager
                .Units
                .Where(x => !x.IsDead)
                .Where(x => x.ActualActionPoints == x.Characteristic.ActiveActionPoints)
                .Count() != _queueItems.Count;

            if (zz)
            {

            }

            await sequence.AsyncWaitForCompletion();

            sequence.onComplete += () =>
            {
                Object.Destroy(actualUnitIcon);
            };
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
