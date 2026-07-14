using UnityEngine;
using Zenject;
using DG.Tweening;

namespace Assets.Scripts.Services
{
    public interface IUIAnimationService
    {
        void ShakeCamera();

        void MoveVeils();
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

        private float targetOrthoSize = 4f;
        private float veilsVectorMove = 700f;
        private float duration = 7f;

        public void MoveVeils()
        {
            Sequence sceneOpenSequence = DOTween.Sequence();

            sceneOpenSequence.Append(_camera.DOOrthoSize(targetOrthoSize, duration).SetEase(Ease.OutCubic));

            var leftCloudTarget = _leftCloudVeil.transform.position + new Vector3(-veilsVectorMove, 0f, 0f);
            var rightCloudTarget = _rightCloudVeil.transform.position + new Vector3(veilsVectorMove, 0f, 0f);
            var topCloudTarget = _topCloudVeil.transform.position + new Vector3(0f, veilsVectorMove, 0f);
            var bottomCloudTarget = _bottomCloudVeil.transform.position + new Vector3(0f, -veilsVectorMove, 0f);

            sceneOpenSequence.Join(_leftCloudVeil.transform.DOMove(leftCloudTarget, duration).SetEase(Ease.OutCubic));
            sceneOpenSequence.Join(_rightCloudVeil.transform.DOMove(rightCloudTarget, duration).SetEase(Ease.OutCubic));
            sceneOpenSequence.Join(_topCloudVeil.transform.DOMove(topCloudTarget, duration).SetEase(Ease.OutCubic));
            sceneOpenSequence.Join(_bottomCloudVeil.transform.DOMove(bottomCloudTarget, duration).SetEase(Ease.OutCubic));

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
    }
}
