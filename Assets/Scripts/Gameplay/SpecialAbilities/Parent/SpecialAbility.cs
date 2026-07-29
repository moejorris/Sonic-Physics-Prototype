using UnityEngine.InputSystem;

[System.Serializable]
public abstract class SpecialAbility
{
    protected PlayerMovement playerMovement;
    public enum Ability_AnimationOverride
    {
        None,
        Ball,
        Custom,

    }
    public Ability_AnimationOverride animationMode = Ability_AnimationOverride.None;
    public SonicSprite[] animationFrames;
    public enum Ability_ActivationCriteria
    {
        LookUp,
        Crouch,
        Jumping,
        Airborne,
        Rolling,
        Walking,
        Grounded,
        None,
    };
    public Ability_ActivationCriteria criteria = Ability_ActivationCriteria.None;
    public InputActionReference button;
    protected bool buttonPressed = false;
    public enum Ability_ButtonPressType
    {
        None,
        Press,
        Hold,
    }
    public Ability_ButtonPressType buttonPressType = Ability_ButtonPressType.None;

    public enum Ability_InputAxesLockType
    {
        None,
        BothAxes,
        XAxis,
        YAxis,
    }
    public enum Ability_InputButtonLockType
    {
        None,
        Jump,
        Action,
        BothButtons,
    }
    public Ability_InputAxesLockType inputAxesLockType;
    public Ability_InputButtonLockType inputButtonLockType;
    public virtual void ActivateAbility()
    {
        isActive = true;
    }

    public virtual void UpdateInactive()
    {

    }

    public virtual void UpdateActive()
    {
        
    }

    public virtual void DeactivateAbility()
    {
        isActive = false;
    }

    public bool readyToActivate => ButtonCriteriaMet() && ActivationCriteriaMet() && SecondaryActivationCriteria();
    public bool isActive = false;
    public void ClearInput()
    {
        buttonPressed = false;
    }
    public void UpdateInput()
    {
        if(buttonPressType == Ability_ButtonPressType.None)
        {
            buttonPressed = true;
            return;
        }
        if(!buttonPressed)
        {
            buttonPressed = buttonPressType == Ability_ButtonPressType.Hold ? button.action.IsPressed() : button.action.WasPressedThisFrame();        
        }
    }
    bool ButtonCriteriaMet()
    {
        if(button == null)
        {
            if(buttonPressType != Ability_ButtonPressType.None)
            {
                buttonPressType = Ability_ButtonPressType.None;
            }
            return true;
        }
        
        return buttonPressed;
    }
    protected bool ActivationCriteriaMet()
    {
        PlayerMovement.DescriptivePlayerState playerState = PlayerMovement.Player.descState;

        if(playerState == PlayerMovement.DescriptivePlayerState.SpecialAbility)
        {
            return false;
        }

        switch(criteria)
        {
            case Ability_ActivationCriteria.Crouch:
            return playerState == PlayerMovement.DescriptivePlayerState.Crouching;

            case Ability_ActivationCriteria.LookUp:
            return playerState == PlayerMovement.DescriptivePlayerState.LookingUp;

            case Ability_ActivationCriteria.Jumping:
            return playerMovement.isJumping;

            case Ability_ActivationCriteria.Airborne:
            return !playerMovement.isGrounded();
            //return playerState == PlayerMovement.DescriptivePlayerState.Jumping || playerState == PlayerMovement.DescriptivePlayerState.Falling || !playerMovement.isGrounded();

            case Ability_ActivationCriteria.Walking:
            return playerState == PlayerMovement.DescriptivePlayerState.Walking;

            case Ability_ActivationCriteria.Rolling:
            return playerState == PlayerMovement.DescriptivePlayerState.Rolling;

            case Ability_ActivationCriteria.Grounded:
            return playerMovement.isGrounded();

            case Ability_ActivationCriteria.None:
            return true;

            default: 
            return false;
        }
    }

    protected virtual bool SecondaryActivationCriteria() //Can be overwritten by child abilities that require more specific things to happen (eg. for a homing attack, a target must be in range)
    {
        return true;
    }

}
