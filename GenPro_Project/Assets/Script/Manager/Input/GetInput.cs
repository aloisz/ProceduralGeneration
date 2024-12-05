using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class GetInput : GenericSingletonClass<GetInput>
{
    private bool isPressingMovement;
    private Vector2 movementVector;
    
    private bool isFire;

    private Vector2 accelerating;

    public void OnMovement(InputAction.CallbackContext ctx)
    {
        isPressingMovement = ctx.ReadValue<Vector2>() != Vector2.zero;
        movementVector = ctx.ReadValue<Vector2>();
    }

    public bool GetIsMovement()
    {
        return isPressingMovement;
    }
    public Vector2 GetMovementVector()
    {
        return movementVector;
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        isFire = ctx.ReadValueAsButton();
    }
    public bool GetIsFiring()
    {
        return isFire;
    }


    public void OnAccelerating(InputAction.CallbackContext ctx)
    {
        accelerating = ctx.ReadValue<Vector2>();
    }

    public float GetAccelerating()
    {
        return accelerating.y;
    }
}


