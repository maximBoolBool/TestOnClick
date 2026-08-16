using Assets.Scripts.Services;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Assets.Scripts.Behaviours
{
    public class CameraInputController : IInitializable, IDisposable
    {
        private InputAction _mouseClickAction;
        private InputAction _mouseDeltaAction;
        private InputAction _mouseScrollAction;

        [Inject]
        private readonly ICameraService _cameraService;

        [Inject(Id = Constants.Camera)]
        private readonly Camera _mainCamera;

        private float2 _targetCameraPosition;
        private bool _isDragging;
        private float _targetZoom;
        private bool _isZooming;

        private const float DRAG_SPEED = 1f;
        private const float SMOOTH_SPEED = 15f;

        private const float ZOOM_SENSITIVITY = 0.2f; // Чувствительность колесика
        private const float ZOOM_SMOOTH_SPEED = 10f;  // Скорость плавности зума

        public void Initialize()
        {
            _mouseClickAction = new InputAction(binding: "<Mouse>/middleButton");
            _mouseDeltaAction = new InputAction(binding: "<Mouse>/delta");
            _mouseScrollAction = new InputAction(binding: "<Mouse>/scroll");

            _mouseClickAction.started += OnDragStarted;
            _mouseClickAction.canceled += OnDragCanceled;
            _mouseScrollAction.performed += OnScrollPerformed;

            _mouseClickAction.Enable();
            _mouseDeltaAction.Enable();
            _mouseScrollAction.Enable();
        }

        private void OnDragStarted(InputAction.CallbackContext context)
        {
            _isDragging = true;
            _targetCameraPosition = new float2(_mainCamera.transform.position.x, _mainCamera.transform.position.y);

            _ = DragLoopAsync();
        }

        private void OnDragCanceled(InputAction.CallbackContext context)
        {
            _isDragging = false;
        }

        private async UniTask DragLoopAsync()
        {
            while (_isDragging && _mainCamera != null)
            {
                Vector2 mouseDelta = _mouseDeltaAction.ReadValue<Vector2>();

                if (mouseDelta != Vector2.zero)
                {
                    _targetCameraPosition.x -= mouseDelta.x * DRAG_SPEED * Time.deltaTime;
                    _targetCameraPosition.y -= mouseDelta.y * DRAG_SPEED * Time.deltaTime;
                }

                float2 currentPos = new(_mainCamera.transform.position.x, _mainCamera.transform.position.y);
                float2 smoothedPos = math.lerp(currentPos, _targetCameraPosition, Time.deltaTime * SMOOTH_SPEED);

                _cameraService.SetCameraCordinates(smoothedPos);

                // Ждем один кадр
                await Task.Yield();
            }
        }

        private void OnScrollPerformed(InputAction.CallbackContext context)
        {
            Vector2 scrollValue = context.ReadValue<Vector2>();

            _targetZoom -= scrollValue.y * ZOOM_SENSITIVITY;

            if (!_isZooming)
            {
                _ = ZoomLoopAsync();
            }
        }

        private async Task ZoomLoopAsync()
        {
            _isZooming = true;

            while (_mainCamera != null)
            {
                float currentZoom = _mainCamera.orthographic ? _mainCamera.orthographicSize : _mainCamera.fieldOfView;

                if (math.abs(currentZoom - _targetZoom) < 0.01f)
                {
                    float finalDelta = currentZoom - _targetZoom;
                    _cameraService.ZoomCamera(finalDelta);
                    break;
                }

                float nextZoom = math.lerp(currentZoom, _targetZoom, Time.deltaTime * ZOOM_SMOOTH_SPEED);

                float deltaToApply = currentZoom - nextZoom;
                _cameraService.ZoomCamera(deltaToApply);

                await Task.Yield();
            }

            _isZooming = false;
        }

        public void Dispose()
        {
            _mouseClickAction.started -= OnDragStarted;
            _mouseClickAction.canceled -= OnDragCanceled;
            _mouseScrollAction.performed -= OnScrollPerformed;

            _mouseClickAction.Disable();
            _mouseDeltaAction.Disable();
            _mouseScrollAction.Disable();
        }
    }
}
