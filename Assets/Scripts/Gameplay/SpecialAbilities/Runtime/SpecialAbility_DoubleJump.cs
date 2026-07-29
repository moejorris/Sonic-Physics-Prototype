using UnityEngine;

[System.Serializable]
public class SpecialAbility_DoubleJump : SpecialAbility
{
    public AudioClip sound;
    bool alreadyUsed = false;

    public override void UpdateInactive()
    {
        base.UpdateInactive();

        if(playerMovement.isGrounded() && alreadyUsed)
        {
            alreadyUsed = false;
        }
    }

    public override void ActivateAbility()
    {
        base.ActivateAbility();
        alreadyUsed = true;
        
        playerMovement.OverrideVelocity(Vector2.up * playerMovement.movementStats.jump_force);

        if(sound != null)
        {
            playerMovement.PlaySoundEffect(sound);
        }
        DeactivateAbility();
    }

    protected override bool SecondaryActivationCriteria()
    {
        return !alreadyUsed;
    }

    public SpecialAbility_DoubleJump(PlayerMovement playerMovement)
    {
        this.playerMovement = playerMovement;
    }
    
}
