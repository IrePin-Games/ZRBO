using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Zrbo.Scripts
{
    public class InputReader : MonoBehaviour, Controls.IGameplayActions
    {
        public Vector2 MovementValue { get; private set; }
    
        public event Action JumpEvent;
    
        private Controls _controls;
        private void Start()
        {
            _controls = new Controls();
            _controls.Gameplay.SetCallbacks(this);
            _controls.Gameplay.Enable();
        }

        private void OnDestroy()
        {
            _controls.Gameplay.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MovementValue = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if(!context.performed){return;}
            JumpEvent?.Invoke();
        }

        public void OnRun(InputAction.CallbackContext context)
        {
        
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
        
        }

        public void OnLook(InputAction.CallbackContext context)
        {
        
        }

        public void OnPause(InputAction.CallbackContext context)
        {
       
        }

        public void OnAim(InputAction.CallbackContext context)
        {
        
        }
    }
}
