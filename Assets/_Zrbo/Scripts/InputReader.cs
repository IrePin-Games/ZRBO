using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, Controls.IGameplayActions
{
    public event Action JumpEvent;
    
    private Controls controls;
    private void Start()
    {
        controls = new Controls();
        controls.Gameplay.SetCallbacks(this);
        controls.Gameplay.Enable();
    }

    private void OnDestroy()
    {
        controls.Gameplay.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        
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
