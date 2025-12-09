using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Action <Vector2> OnMoveInput;
    public Action <bool> SprintInput;
    public Action OnjumpInput;
    public Action OnClimbInput;
    public Action OnCancellClimb;
    public Action OnChangePOV;
    public Action OnCrouching;
    public Action OnGliding;
    public Action OnCancellGlide;
    public Action OnPunchInput;
    public Action OnMainMenuInput;
   
    void Update()
    {
        CheckInputMovement();  
        CheckSPrintInput();
        CheckJumpInput();
        CheckCrouchInput();
        CheckChangePOVInput();
        CheckClimbInput();
        CheckGlideInput();
        CheckCancelInput();
        CheckPunchInput();
        CheckMainMenuInput();
    }

    private void CheckInputMovement ()
    {
        float VerticalInput = Input.GetAxisRaw("Vertical");
        float HorizontalInput = Input.GetAxisRaw("Horizontal");
        Vector2 AxisInput = new Vector2(HorizontalInput, VerticalInput);
        if (OnMoveInput != null)
        {
            OnMoveInput(AxisInput);
        }
    }

    private void CheckSPrintInput ()
    {
        bool IsholdInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (IsholdInput)
        {
            SprintInput (true);
        }
        else
        {
            SprintInput (false);
        }
    }

    private void CheckJumpInput ()
    {
        bool IsJumpInput = Input.GetKeyDown(KeyCode.Space);
        if (IsJumpInput)
        {
            OnjumpInput();
        }
    }

    private void CheckCrouchInput()
    {
        bool isPressCrouchInput = Input.GetKeyDown(KeyCode.LeftControl) ||
                                    Input.GetKeyDown(KeyCode.RightControl);
        if (isPressCrouchInput)
        {
            OnCrouching();
        }
    }

    private void CheckChangePOVInput()
    {
        bool isPressChangePOVInput = Input.GetKeyDown(KeyCode.Q);

        if (isPressChangePOVInput)
        {
            if (OnChangePOV != null)
            {
                OnChangePOV();
            }
        }
    }

    private void CheckClimbInput()
    {
        bool isPressClimbInput = Input.GetKeyDown(KeyCode.E);

        if (isPressClimbInput)
        {
            OnClimbInput();
        }
    }

    private void CheckGlideInput()
    {
        bool isPressGlideInput = Input.GetKeyDown(KeyCode.G);

        if (isPressGlideInput)
        {
            if (OnGliding != null)
            {
                OnGliding();
            }
        }
    }

    private void CheckCancelInput()
    {
        bool isPressCancelInput = Input.GetKeyDown(KeyCode.C);

        if (isPressCancelInput)
        {
            if (OnCancellClimb != null)
            {
                OnCancellClimb();
                OnCancellGlide();
            }
        }
    }

    private void CheckPunchInput()
    {
        bool isPressPunchInput = Input.GetKeyDown(KeyCode.Mouse0);

        if (isPressPunchInput)
        {
            OnPunchInput();
        }
    }

    private void CheckMainMenuInput()
    {
        bool isPressMainMenuInput = Input.GetKeyDown(KeyCode.Escape);

        if (isPressMainMenuInput)
        {
            if (OnMainMenuInput != null)
            {
                OnMainMenuInput();
            }
        }
    }
}
