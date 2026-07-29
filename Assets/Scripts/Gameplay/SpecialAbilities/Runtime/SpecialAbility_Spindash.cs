using UnityEngine;

[System.Serializable]
public class SpecialAbility_Spindash : SpecialAbility
{   
    public float spinRev = 0f;
    AudioClip chargeSound;
    AudioClip releaseSound;
    float revTime;
    float revInterval = 0.1f;
    AudioSource spinAudioSource;
    public GameObject dustPrefab;
    ParticleSystem dustPS;

    float maxRev = 8f;

    float minPitch = 1f;
    float maxPitch = 1.5f;

    bool holdingReachedMaxPitch = false;

    bool cancelledRoll = false;

    public override void UpdateInactive()
    {
        base.UpdateInactive();
        if(cancelledRoll && !buttonPressed) cancelledRoll = false;

    }

    public override void UpdateActive()
    {
        bool isAdventure = criteria != Ability_ActivationCriteria.Crouch;
        
        bool holdingCanCharge = isAdventure ? Time.time - revTime > revInterval && spinRev < maxRev: true;
        bool continueRevving = isAdventure ? !holdingReachedMaxPitch : true;
        if(isAdventure)
        {
            buttonPressed = button.action.IsPressed();
        }

        if(buttonPressed && holdingCanCharge)
        {
            revTime = Time.time;

            spinRev += 2f;
            if(spinRev > maxRev)
            {
                spinRev = maxRev;
                holdingReachedMaxPitch = true;
            }

            if(GenesisSonicAnimator.instance != null)
            {
                GenesisSonicAnimator.instance.ResetCurrentAnimation();
            }


            if(continueRevving)
            {
                spinAudioSource.Stop();
                float pitchAlpha = spinRev/8f;
                float pitch = Mathf.Lerp(minPitch, maxPitch, pitchAlpha);
                spinAudioSource.pitch = pitch;

                spinAudioSource.PlayOneShot(chargeSound);
            }
        }

        if(playerMovement.GroundSpeed != 0f)
        {
            playerMovement.SetGroundSpeed(0f);
        }
        if(playerMovement.Velocity != Vector2.zero)
        {
            playerMovement.SetVelocity(Vector2.zero);
        }
        playerMovement.ForceStateChange(PlayerMovement.PlayerState.Grounded);

        //This happens every ability update because the player will actively fight the ball state while on the ground and not rolling
        //Nothing happens if the player is already in ball form though!
        playerMovement.EnterBallState();

        if(continueRevving) //stops spindash decay when holding the button after reaching max charge
        {
            spinRev -= ((int)(spinRev / 0.125f)) / 256f;        
        }
        else spinRev = maxRev;

        if(spinRev < 0f)
        {
            spinRev = 0f;
        }

        bool noLongerCrouching = InputManager.Instance.moveInputDir.y != -1f && criteria == Ability_ActivationCriteria.Crouch;
        bool noLongerHoldingButton = !buttonPressed && isAdventure;

        if(noLongerCrouching || noLongerHoldingButton)
        {
            DeactivateAbility();
        }
    }

    public override void ActivateAbility()
    {
        base.ActivateAbility();

        if(cancelledRoll) //Makes the player let go of the button before they can start a spindash after cancelling the roll.
        {
            base.DeactivateAbility();
            return;
        }

        if(playerMovement.isBall && criteria == Ability_ActivationCriteria.Grounded) //if rolling when we start, then cancel the roll and return like SA1. Using base.Deactivate because we don't want to launch, just end.
        {
            cancelledRoll = true;
            playerMovement.ExitBallState();
            playerMovement.ForceStateChange(PlayerMovement.PlayerState.Grounded);
            base.DeactivateAbility();
            return;
        }

        spinRev = 0f;
        holdingReachedMaxPitch = false;
        playerMovement.EnterBallState();

        if(dustPS)
        {
            dustPS.Play();

            // dustPS.transform.localScale = (playerMovement.facingRight ? 0 : -2) * Vector2.right + Vector2.one;
            Vector2 upDir = GenesisSonicAnimator.instance.transform.up;
            dustPS.transform.localPosition = -upDir * (playerMovement.movementStats.ballHeight /2f) ;
            dustPS.transform.up = upDir;
            if(!playerMovement.facingRight)
            {
                dustPS.transform.eulerAngles *= -1f;
                dustPS.transform.eulerAngles -= Vector3.up * 180;
            }
        }
    }

    public override void DeactivateAbility()
    {
        base.DeactivateAbility();
        float sign = playerMovement.facingRight ? 1f : -1f;
        playerMovement.SetGroundSpeed((spinRev + 8f)/16f * sign);
        playerMovement.ForceStateChange(PlayerMovement.PlayerState.Rolling);
        
        spinAudioSource.Stop();
        spinAudioSource.pitch = minPitch;
        spinAudioSource.PlayOneShot(releaseSound);

        if(dustPS)
        {
            dustPS.Stop();
            dustPS.Clear();
        }
    }

    public void ApplySoundEffects(AudioClip chargeSound, AudioClip releaseSound)
    {
        if(!chargeSound || !releaseSound) return;

        GameObject soundObject = new GameObject("SpinDashSFX");
        spinAudioSource = soundObject.AddComponent<AudioSource>();
        soundObject.transform.parent = playerMovement.transform;
        spinAudioSource.clip = chargeSound;
        spinAudioSource.volume = 0.75f;

        this.chargeSound = chargeSound;
        this.releaseSound = releaseSound;
    }

    public void AssignDustParticle(GameObject prefab)
    {
        if(!prefab) return;

        GameObject vfxObject = Object.Instantiate(prefab, playerMovement.transform);
        vfxObject.name = "SpinDashVFX";
        dustPS = vfxObject.GetComponent<ParticleSystem>();
        dustPS.Stop();

    }

    public SpecialAbility_Spindash(PlayerMovement playerMovement)
    {
        this.playerMovement = playerMovement;
    }
    
}
