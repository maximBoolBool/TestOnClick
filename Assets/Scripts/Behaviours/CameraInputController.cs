using Assets.Scripts.Services;
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

        [Inject]
        private readonly ICameraService _cameraService;

        [Inject(Id = Constants.Camera)]
        private readonly Camera _mainCamera;

        private float2 _targetCameraPosition;
        private bool _isDragging;

        private const float DRAG_SPEED = 1f;
        private const float SMOOTH_SPEED = 15f;

        public void Initialize()
        {
            _mouseClickAction = new InputAction(binding: "<Mouse>/middleButton");
            _mouseDeltaAction = new InputAction(binding: "<Mouse>/delta");

            _mouseClickAction.started += OnDragStarted;
            _mouseClickAction.canceled += OnDragCanceled;

            _mouseClickAction.Enable();
            _mouseDeltaAction.Enable();
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

        private async Task DragLoopAsync()
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

        public void Dispose()
        {
            _mouseClickAction.started -= OnDragStarted;
            _mouseClickAction.canceled -= OnDragCanceled;

            _mouseClickAction.Disable();
            _mouseDeltaAction.Disable();
        }
    }
}
