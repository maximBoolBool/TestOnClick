using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IUIAnimationService
    {
        void ShakeCamera();
            
        void MoveVeils();

        UniTaskVoid SwitchPanelUnitIconAsync(string unitName);    
    }

    class UIAnimationService : IUIAnimationService
    {
        [Inject(Id = Constants.Camera)]
        private readonly Camera _camera;

        [Inject(Id = Constants.TopVeilCloudPart)]
        private readonly GameObject _topCloudVeil;

        [Inject(Id = Constants.BottomVeilCloudPart)]
        private readonly GameObject _bottomCloudVeil;

        [Inject(Id = Constants.LeftVeilCloudPart)]
        private readonly GameObject _leftCloudVeil;

        [Inject(Id = Constants.RightVeilCloudPart)]
        private readonly GameObject _rightCloudVeil;

        [Inject(Id = Constants.UnitInformationPanelIcon)]
        private readonly GameObject _unitInformationPanelIcon;

        private const float TARGET_ORTHO_SIZE = 4f;
        private const float VEILS_VECTOR_MOVE = 1200f;
        private const float ANIMATION_DURATION = 10f;
        private const float FADE_MIN_VALUE = 0;
        private const float FADE_MAX_VALUE = 1;
        private const float FADE_DURATION = 1f;

        public void MoveVeils()
        {
            Sequence sceneOpenSequence = DOTween.Sequence();

            sceneOpenSequence.Append(_camera.DOOrthoSize(TARGET_ORTHO_SIZE, ANIMATION_DURATION).SetEase(Ease.OutCubic));

            var leftCloudTarget = _leftCloudVeil.transform.position + new Vector3(-VEILS_VECTOR_MOVE, 0f, 0f);
            var rightCloudTarget = _rightCloudVeil.transform.position + new Vector3(VEILS_VECTOR_MOVE, 0f, 0f);
            var topCloudTarget = _topCloudVeil.transform.position + new Vector3(0f, VEILS_VECTOR_MOVE, 0f);
            var bottomCloudTarget = _bottomCloudVeil.transform.position + new Vector3(0f, -VEILS_VECTOR_MOVE, 0f);

            sceneOpenSequence.Join(_leftCloudVeil.transform.DOMove(leftCloudTarget, ANIMATION_DURATION).SetEase(Ease.OutCubic));
            sceneOpenSequence.Join(_rightCloudVeil.transform.DOMove(rightCloudTarget, ANIMATION_DURATION).SetEase(Ease.OutCubic));
            sceneOpenSequence.Join(_topCloudVeil.transform.DOMove(topCloudTarget, ANIMATION_DURATION).SetEase(Ease.OutCubic));
            sceneOpenSequence.Join(_bottomCloudVeil.transform.DOMove(bottomCloudTarget, ANIMATION_DURATION).SetEase(Ease.OutCubic));

            sceneOpenSequence.OnComplete(() =>
            {
                Debug.Log("Open scene complete");
            });
        }

        public void ShakeCamera()
        {
            return;
            _camera.DOShakePosition(0.5f, 0.5f, 10, 90, false);
        }

        public async UniTaskVoid SwitchPanelUnitIconAsync(string unitName)
        {
            var iconImage = _unitInformationPanelIcon.GetComponent<Image>();

            var sequence = DOTween.Sequence();

            sequence.Append(iconImage.DOFade(FADE_MIN_VALUE, FADE_DURATION));

            sequence.AppendCallback(async () => 
            { 
                var sprite = await Addressables.LoadAssetAsync<Sprite>(unitName);
                iconImage.sprite = sprite;
            });

            sequence.Append(iconImage.DOFade(FADE_MAX_VALUE, FADE_DURATION));
        }
    }
}
