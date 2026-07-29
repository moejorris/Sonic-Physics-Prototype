using System.Collections;
using UnityEngine;

public class GenesisSpriteAnimator : MonoBehaviour
{
    public static GenesisSpriteAnimator instance;
    GenesisSpriteAnimations animations;
    public CharacterSprite[] currentAnimation;
    PlayerMovement playerMovement;
    PlayerMovementStats movementStats;
    [SerializeField] SpriteRenderer spriteRenderer;
    float airRotationSpeed = 2.8125f; //how fast in degrees sonic will rotate back to upright after leaving the ground. This is cosmetic ONLY, and taken straight from classic sonic's code.
    float brakeSpeedThreshold = 4f/16f;
    [SerializeField] float floorOffset = 0f;
    [SerializeField] int frame = 0;
    bool isRunning = false;
    float zRot = 0f;

    [SerializeField] float smoothRotSpeed = 10f;

    public Sprite GetCurrentPlayerSprite()
    {
        return spriteRenderer.sprite;
    }
    public Material GetPlayerSpriteMaterial()
    {
        return spriteRenderer.material;
    }

    public SpriteRenderer GetPlayerSpriteRenderer()
    {
        return spriteRenderer;
    }

    void AssignReferences()
    {
        instance = this;
        
        playerMovement = PlayerMovement.Player;
        if(playerMovement == null)
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }
        
        movementStats = playerMovement.movementStats;
        animations = playerMovement.characterData.spriteAnimations;

        if(animations == null || playerMovement == null)
        {
            enabled = false;
            return;    
        }
        
        if(spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if(spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

                if(spriteRenderer == null)
                {
                    spriteRenderer = gameObject.AddComponent<SpriteRenderer>();                
                }
            }        
        }
    }

    void Start()
    {
        AssignReferences();

        if(animations.spindash == null || animations.spindash.Length == 0)
        {
            animations.spindash = animations.ball;
            animations.spindash[0].shouldLoop = true;
            animations.spindash[0].subImage_duration = 2;
        }
    }

    void FixedUpdate()
    {
        UpdateSpriteFlip();
        UpdateAnimation();
        UpdateSpriteRotation();
        UpdateSpritePosition();
    }

    void UpdateSpriteRotation()
    {
        //we still want to update zRot, even though we don't want to use it while in ball form.
        
        if(!playerMovement.isGrounded())
        {
            zRot = Mathf.MoveTowardsAngle(zRot, 0f, airRotationSpeed * Time.deltaTime * 60f);
        }
        else
        {
            zRot = playerMovement.PrimaryGroundSensor.angle;
        }

        float displayRot = transform.eulerAngles.z;

        switch(playerMovement.characterData.groundRotationStyle)
        {
            case CharacterData.GroundRotationStyle.Genesis:
                displayRot = SnappedAngle(zRot);
                break;

            case CharacterData.GroundRotationStyle.Mania:
                float targetAngle = SnappedAngle(zRot);

                
                if(Mathf.Abs(targetAngle) >= 45f)
                {
                    targetAngle = zRot;
                }
                else targetAngle = 0f;

                displayRot = Mathf.MoveTowardsAngle(displayRot, targetAngle, Mathf.Max(smoothRotSpeed, smoothRotSpeed * Mathf.Abs(playerMovement.GroundSpeed)) * 60f * Time.deltaTime);

                break;
            
            // case SonicCharacter.GroundRotationStyle.Modern:
            // displayRot = zRot;
            // break;
        }


        if(currentAnimation == animations.ball)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else transform.eulerAngles = Vector3.forward * displayRot;
    }

    float SnappedAngle(float angle)
    {
        if(angle > 180f)
        {
            angle -= 360f;
        }

        return Mathf.Abs(angle) > 45f ? Mathf.RoundToInt(Mathf.Abs(angle)/45f) * 45 * Mathf.Sign(angle) : 0f;
    }

    void UpdateSpritePosition()
    {
        spriteRenderer.transform.position = playerMovement.transform.position - ((floorOffset + playerMovement.Height/2f) * transform.up);
    }

    void UpdateSpriteFlip()
    {
        spriteRenderer.flipX = !playerMovement.facingRight;
        // float dirInput = InputManager.Instance.dirInput.x;

        // if(playerMovement.isGrounded())
        // {
        //     if(dirInput != 0 && Mathf.Sign(dirInput) == Mathf.Sign(playerMovement.groundSpeed))
        //     {
        //         spriteRenderer.flipX = dirInput < 0 ? true : false;
        //     }
        // }
        // else
        // {
        //     if(dirInput != 0)
        //     {
        //         spriteRenderer.flipX = dirInput < 0 ? true : false;
        //     }
        // }

    }

    IEnumerator PlayAnimation()
    {
        isRunning = true;
        CharacterSprite prevAnimation = null;

        while(isRunning)
        {
            for(int i = 0; i < currentAnimation.Length; i++)
            {
                if(currentAnimation[i] != prevAnimation && !currentAnimation[i].shouldLoop) //prevent looping when an animation is only one set and not set to loop
                {
                    frame = 0;                
                }

                int timesLooped = 0;
                int max = currentAnimation[i].sprites.Length;
                while(frame < max)
                {
                    GetNextSprite();

                    if(frame > max - 1)
                    {
                        frame = max -1;
                    }

                    int circleInterval = playerMovement.GroundSpeed >= playerMovement.movementStats.top_speed ? 2 : 4;
                    bool beCircle = currentAnimation == animations.ball && frame % circleInterval == 0 && spriteRenderer.sprite != animations.ball[i].ballSprite && animations.ball[i].ballSprite != null;

                    max = currentAnimation[i].sprites.Length;

                    if(beCircle)
                    {
                        spriteRenderer.sprite = animations.ball[i].ballSprite;
                    }
                    else
                    {
                        spriteRenderer.sprite = currentAnimation[i].sprites[frame];
                    }
                    
                    float duration = currentAnimation[i].subImage_duration;

                    if(currentAnimation[i].variableByGroundSpeed)
                    {
                        duration = Mathf.Max(0, duration-Mathf.Abs(playerMovement.GroundSpeed * spriteRenderer.sprite.pixelsPerUnit)); //multiply by 16 because og code 
                    }

                    duration = Mathf.CeilToInt(Mathf.Max(1f, duration));

                    yield return new WaitForSeconds(1f/60f * duration);

                    if(!beCircle)
                    {
                        frame++;
                    }

                    if(currentAnimation[i].shouldLoop)
                    {                        
                        if(currentAnimation[i].loopTimes != 0)
                        {
                            timesLooped++;
                        }

                        if(frame >= max && (timesLooped < currentAnimation[i].loopTimes || currentAnimation[i].loopTimes == 0))
                        {
                            frame = 0;                        
                        }
                    }
                }

                prevAnimation = currentAnimation[i];
            }
            yield return new WaitForSeconds(1f/60f);
        }
    }

    public void ResetCurrentAnimation()
    {
        frame = 0;
    }

    Sprite GetNextSprite()
    {
        //if ball sprite

        //else normal sprite
        return null;
    }

    void UpdateAnimation()
    {
        CharacterSprite[] prevAnimation = currentAnimation;

        //Idle/Walking/Running
        float absGSP = Mathf.Abs(playerMovement.GroundSpeed);

        //Check if the player has an active special ability, if so, check for animation override
        SpecialAbility ability = playerMovement.activeAbility;
        bool abilityAnimationValid = playerMovement.descState == PlayerMovement.DescriptivePlayerState.SpecialAbility && 
        ability != null && 
        ability.isActive &&
        (ability.animationMode == SpecialAbility.Ability_AnimationOverride.Ball ||
        (ability.animationMode == SpecialAbility.Ability_AnimationOverride.Custom && ability.animationFrames.Length > 0 && ability.animationFrames[0].sprites.Length > 0));
        
        if(abilityAnimationValid)
        {
            switch(ability.animationMode)
            {
                case SpecialAbility.Ability_AnimationOverride.Ball:
                currentAnimation = animations.ball;
                break;

                case SpecialAbility.Ability_AnimationOverride.Custom:
                currentAnimation = playerMovement.activeAbility.animationFrames;
                break;
            }
        }
        else if(playerMovement.isBall)
        {
            currentAnimation = animations.ball;   
        }
        else if(ShouldBrake(absGSP) && playerMovement.isGrounded())
        {
            currentAnimation = animations.braking;
            spriteRenderer.flipX = Mathf.Sign(playerMovement.GroundSpeed) == -1f;
        }
        else if(absGSP >= movementStats.dash_speed && animations.dashing.Length > 0) //dash
        {
            currentAnimation = animations.dashing;
        }
        else if(absGSP >= movementStats.top_speed) //run
        {
            currentAnimation = animations.running;
        }
        else if(playerMovement.isPushing && playerMovement.isGrounded())
        {
            currentAnimation = animations.pushing;
        }
        else if(absGSP != 0f || !playerMovement.isGrounded()) //walk/is in the air (player should never be able to play idle anim while airborne)
        {
            currentAnimation = animations.walking;
        }
        else if(absGSP == 0f) //not moving
        {
            float lookDir = InputManager.Instance.moveInputDir.y;
            if(lookDir < 0)
            {
                currentAnimation = animations.crouch;
            }
            else if(lookDir > 0)
            {
                currentAnimation = animations.lookUp;
            }
            else if(playerMovement.PrimaryGroundSensor.hit && !playerMovement.SecondaryGroundSensor.hit)
            {
                RaycastHit2D balanceSensor = Physics2D.Raycast(playerMovement.Position, Vector2.down, movementStats.standingHeight / 2f, movementStats.groundLayerMask);

                if(balanceSensor.collider == null)
                {
                    currentAnimation = animations.balancing;                
                }
                else currentAnimation = animations.idle;
            }
            else
            {
                currentAnimation = animations.idle;
            }
        }

        bool animChanged = prevAnimation != currentAnimation;

        if(animChanged)
        {
            frame = 0;
            isRunning = false;
            StopCoroutine("PlayAnimation");
            StartCoroutine("PlayAnimation");
        }
    }

    bool ShouldBrake(float absGSP)
    {
        bool wantsToBrake = InputManager.Instance.moveInputDir.x != 0 && Mathf.Sign(InputManager.Instance.moveInputDir.x) != Mathf.Sign(playerMovement.GroundSpeed);
        bool fastEnoughToBrake = absGSP > brakeSpeedThreshold;
        bool alreadyBraking = currentAnimation == animations.braking;
        bool finishUntilStopped = alreadyBraking && absGSP > 0 && (Mathf.Sign(InputManager.Instance.moveInputDir.x) != Mathf.Sign(playerMovement.GroundSpeed) || InputManager.Instance.moveInputDir.x == 0f) && animations.brakingStyle == GenesisSpriteAnimations.BrakeStyle.Sonic1_CD;
        
        return (wantsToBrake || finishUntilStopped) && (fastEnoughToBrake || alreadyBraking) && playerMovement.isGrounded();
    }
}
