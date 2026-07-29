using System;
using UnityEngine;
using System.Collections.Generic;
using static PlayerSensors;
using static UnitConversion;

public class PlayerMovement : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] bool useSubStepsPastSpeedLimit = false;
    public static PlayerMovement Player;
    public Vector2 Position {get{return position;}}
    public Vector2 Velocity {get{return velocity;}}
    public float GroundSpeed {get{return groundSpeed;}}
    public float Height {get{return curHeight;}}
    public float Width {get{return curWidth;}}
    Vector2 DEBUG_resetPos;
    public CharacterData characterData;
    [HideInInspector] public PlayerMovementStats movementStats;
    [HideInInspector] public PlayerSensors sensors;
    public bool isBall = false;
    public bool isPushing = false;
    public bool isJumping = false;
    public bool facingRight = true;
    bool wantsJump = false;
    public bool disableSlopeFactor;

    [Range(-1, 240)] public int maxFps = 144;
    [Range(0, 120)] public int timeScale = 60;

    public enum PlayerState
    {
        Grounded,
        Airborne,
        Rolling,
        DEBUG
    };

    [SerializeField] PlayerState curPlayerState = PlayerState.Airborne;

    public enum DescriptivePlayerState
    {
        Standing,
        Walking,
        Jumping,
        Falling,
        SpecialAbility,
        Crouching = -1,
        Rolling = -2,
        LookingUp = -3
    };
    [SerializeField] public DescriptivePlayerState descState = DescriptivePlayerState.Standing;

    //speed vars
    [SerializeField] float groundSpeed = 0;
    [SerializeField] int controlLock = 0;
    Vector2 moveInput;
    [SerializeField] Vector2 velocity;
    Vector2 previousPosition;
    Vector2 position;
    float curHeight = 0f;
    float curWidth = 0f;
    public Bounds bounds;

    CompetingSensors groundSensors = new();
    public SensorHit PrimaryGroundSensor {get {return groundSensors.primarySensor;}}
    public SensorHit SecondaryGroundSensor {get {return groundSensors.secondarySensor;}}
    public SensorHit GroundSensor {get{return groundSensors.primarySensor;}}

    //Abilities
    [SerializeField]List<SpecialAbility> equippedAbilities = new();
    public SpecialAbility activeAbility;

    public bool isGrounded()
    {
        return curPlayerState == PlayerState.Grounded || curPlayerState == PlayerState.Rolling;
    }

    public bool isRolling()
    {
        return curPlayerState == PlayerState.Rolling;
    }
    
    void Awake()
    {
        Player = this;
        sensors = new(this);

        if(characterData.playerMovementStats != null)
        {
            movementStats = characterData.playerMovementStats;
        }
        else movementStats = ScriptableObject.CreateInstance<PlayerMovementStats>();

        groundSensors = sensors.CastGroundSensors();
        // groundSensors = new CompetingSensors(new SensorHit(), new SensorHit());

        curHeight = movementStats.standingHeight;
        curWidth = movementStats.standingWidth;

        Time.fixedDeltaTime = 1f/60f;        
        DEBUG_resetPos = transform.position;
        position = transform.position;
    }

    void Start()
    {
        equippedAbilities.Clear();
        if(characterData.specialAbilities.Count > 0)
        {
            foreach(ScriptableObject ability in characterData.specialAbilities)
            {
                if(ability is IAbility)
                {
                    equippedAbilities.Add((ability as IAbility).GetAbility(this));
                }
            }
        }
    }

    void Update()
    {
        HandleInput();

        Time.timeScale = timeScale != 0 ? timeScale/60f : 0;
        Application.targetFrameRate = maxFps;
        QualitySettings.vSyncCount = Application.targetFrameRate == 0 ? 1 : 0;

        MovePlayerObject(position);
    }

    void HandleInput()
    {
        if(InputManager.Instance.jumpButtonDown)
        {
            wantsJump = true;
        }

        if(InputManager.Instance.debugButtonDown)
        {
            curPlayerState = curPlayerState == PlayerState.DEBUG ? PlayerState.Airborne : PlayerState.DEBUG;
            velocity = Vector2.zero;
        }

        if(InputManager.Instance.sloModeButtonDown)
        {
            timeScale = 1;
        }
        else if(InputManager.Instance.slowModeButtonUp)
        {
            timeScale = 60;
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            velocity = Vector2.zero;
            groundSpeed = 0;
            curPlayerState = PlayerState.Airborne;

            position = DEBUG_resetPos;
        }

        if(equippedAbilities.Count > 0)
        {
            foreach(SpecialAbility ability in equippedAbilities)
            {
                ability.UpdateInput();
            }
        }

        moveInput = InputManager.Instance.moveInputDir;
    }

    void FixedUpdate()
    {
        previousPosition = position;
        switch(curPlayerState)
        {
            case PlayerState.Rolling:
            case PlayerState.Grounded:
                GroundedMovement();
            break;

            case PlayerState.Airborne:
                AirborneMovement();
            break;

            case PlayerState.DEBUG:
                DebugMovement();
            break;
        }

        wantsJump = false; //clears the jumping input flag after the movement code has executed. Done this way so inputs aren't missed between fixed updates.

        UpdateHitbox();
        UpdateFacingDirection();
        ClampSpeeds();
        
        if(curPlayerState != PlayerState.DEBUG)
        {
            // sensors.CollisionPopOut();
        }
    }

    void ClampSpeeds()
    {
        velocity.x = Mathf.Clamp(velocity.x, -movementStats.max_speed_x, movementStats.max_speed_x);
        velocity.y = Mathf.Clamp(velocity.y, -movementStats.max_speed_y, movementStats.max_speed_y);
        groundSpeed = Mathf.Clamp(groundSpeed, -movementStats.max_speed_ground, movementStats.max_speed_ground);
    }

    void UpdateFacingDirection()
    {
        float xInput = moveInput.x;

        if(isGrounded())
        {
            if(xInput != 0 && Mathf.Sign(xInput) == Mathf.Sign(groundSpeed) && groundSpeed != 0f)
            {
                facingRight = xInput >= 0;
            }
        }
        else
        {
            if(xInput != 0)
            {
                facingRight = xInput >= 0;
            }
        }
    }

    void UpdateHitbox()
    {
        curHeight = isBall ? movementStats.ballHeight : movementStats.standingHeight;
        curWidth = isBall ? movementStats.ballWidth : movementStats.standingWidth;

        Vector2 size = new Vector2(curWidth, curHeight);

        bool isHorizontal = PrimaryGroundSensor.castDirection.x != 0f;

        if(isHorizontal)
        {
            size = size.FlipAxes();
        }

        bounds = new Bounds(position, size);
    }

    void DebugMovement()
    {
        AdjustSpeedDebugX();
        AdjustSpeedDebugY();
        Vector2 input = moveInput.normalized;
        velocity.x *= input.x != 0f ? 1f : 0f;
        velocity.y *= input.y != 0f ? 1f : 0f;
        position += velocity;
    }

    void UpdateSpecialAbilities()
    {
        foreach(SpecialAbility ability in equippedAbilities)
        {
            if(ability.readyToActivate && activeAbility == null)
            {
                ability.ActivateAbility();
                activeAbility = ability;
            }

            if(ability == activeAbility && ability.isActive)
            {
                switch (ability.inputAxesLockType)
                {
                    case SpecialAbility.Ability_InputAxesLockType.BothAxes:
                    moveInput = Vector2.zero;
                    break;

                    case SpecialAbility.Ability_InputAxesLockType.YAxis:
                    moveInput *= Vector2.right;
                    break;

                    case SpecialAbility.Ability_InputAxesLockType.XAxis:
                    moveInput *= Vector2.up;
                    break;
                }

                switch(ability.inputButtonLockType)
                {
                    case SpecialAbility.Ability_InputButtonLockType.Jump:
                    wantsJump = false;
                    break;

                    //Add functionality to disable action button?
                }

                descState = DescriptivePlayerState.SpecialAbility;
                ability.UpdateActive();
            }
            
            if(!ability.isActive)
            {
                ability.UpdateInactive();
            }

            if(activeAbility != null && !activeAbility.isActive)
            {
                activeAbility = null;
                DisableSpecialAbilityState();
            }

            ability.ClearInput();
        }
    }

    void GroundedMovement()
    {
        //clear jump flag
        isJumping = false;

        if(curPlayerState != PlayerState.Rolling)
        {
            //update ball mode. Should only ever be a ball on the ground in a special state or if rolling
            ExitBallState();

            // -1 - check for special state
            GroundedUpdateDescriptiveState();


        }
        // 0 - update special abilities
        UpdateSpecialAbilities();

        // 1 - apply slope factor
        groundSpeed -= GetSlopeFactor();

        // 2 - check for jump
        if(StartJump())
        {
            return;
        }

        // 3 - apply input movement (acce/deccel/fric)
        ApplyInputToGroundSpeed();

        // 4 - check for starting crouch (Original handled it here, mine handles it at the top)

        // 5 - check for rolling
        bool startRolling = Mathf.Abs(groundSpeed) >= movementStats.roll_activation_speed && moveInput.y < 0f && curPlayerState != PlayerState.Rolling && Mathf.Abs(moveInput.y) > Mathf.Abs(moveInput.x);
        if(startRolling)
        {
            StartRolling();
        }

        if(curPlayerState == PlayerState.Rolling && groundSpeed == 0f)
        {
            curPlayerState = PlayerState.Grounded;
            ExitBallState();
            UpdateHitbox();
        }

        // 6 - calc velocity
        velocity = GroundSpeedToVelocity(true, true);

        // 7 - push sensor collision
        GroundedPushCollision();
        
        // 8 - apply velocity to position
            position += velocity;

            // 9 - ground sensor collision check
            if(GroundCollision())
            {
                //10 - snap to floor on collision
                SnapToFloor(PrimaryGroundSensor);
            }
        
        // 11 - check for slipping/falling when ground speed is too low on walls and ceilings
        if(controlLock == 0 && activeAbility == null)
        {
            if(Mathf.Abs(groundSpeed) < 2.5f/PIXELS_PER_UNIT && (PrimaryGroundSensor.angle >= 35f && PrimaryGroundSensor.angle <= 326f) && false)
            {

                controlLock = 30;

                //S3 method- only detach from ground if slope is steep enough, otherwise add/subtract 0.5 from groundspeed to make player fall down the slope
                if(PrimaryGroundSensor.angle >= 69f && PrimaryGroundSensor.angle <= 293f)
                {
                    curPlayerState = PlayerState.Airborne;                
                }
                else
                {
                    groundSpeed += (PrimaryGroundSensor.angle > 180f ? 0.5f : -0.5f) / PIXELS_PER_UNIT;
                }
            }
            else if(Mathf.Abs(groundSpeed) < 2.5f/PIXELS_PER_UNIT && (PrimaryGroundSensor.angle >= 46f && PrimaryGroundSensor.angle <= 315f))
            {
                controlLock = 30;
                curPlayerState = PlayerState.Airborne;
                groundSpeed = 0f;
            }
        }
        else //important to note that the control lock is only ticked down while on the ground- it pauses when airborne and resumes upon re entering the
        {
            controlLock--;
            if(controlLock <= 0)
            {
                controlLock = 0;
            }
        }
    }

    void GroundedSubStep(float amountToMove)
    {
        //recalculate the velocity direction every step, so we always move around the slope.
        velocity = GroundSpeedToVelocity(true, true);
        position += velocity.normalized * amountToMove;

        if(GroundCollision())
        {
            SnapToFloor(PrimaryGroundSensor);
        }
    }
    
    void AirborneMovement()
    {
        // 0 - update special abilities
        UpdateSpecialAbilities();
        
        // 1 - Check for jump button release (variable jump velocity)
        CheckForVariableJumpHeight();

        // 1.5 - update descriptive state. TODO: Make this a function that accounts for special ability state
        descState = isJumping ? DescriptivePlayerState.Jumping : DescriptivePlayerState.Falling;

        // 2 - check for super transformation (not implemented)


        // 3 - update x speed based on dir input
        ApplyAirAcceleration();

        // 4 - apply air drag
        ApplyAirDrag();

        // 5 - move the player object
            //update position based on velocity
        position += velocity;

        // 6 - apply gravity and clamp
        //occurs after new position is set, important for ensuring jump height is correct
        velocity.y = Mathf.Max(velocity.y - movementStats.gravity_force, -1f);

        // 7 - reduce underwater gravity

        // 8 - rotate ground angle back to 0. Note: the angle is calculated from the ground normal when called, that's why we only change the normal. This is largely unnecessary because it's overwritten by the ground check.
        PrimaryGroundSensor.normal = Vector2.up;

        // 9 - check collisions

        switch(GetAirCollisionMode())
        {
            case AirCollisionMode.MostlyX:
            if(GroundCollision()) SnapToFloor(PrimaryGroundSensor);
            CeilingCollision();
            AirbornePushCollision();
            break;

            case AirCollisionMode.MostlyUp:
            AirbornePushCollision();
            CeilingCollision();
            break;

            case AirCollisionMode.MostlyDown:
            if(GroundCollision()) SnapToFloor(PrimaryGroundSensor);
            AirbornePushCollision();
            break;
        }

        if(isGrounded())
        {
            groundSpeed = AirSpeedToGroundSpeed();
        }
    }


    bool StartJump(bool useGroundAngle = true)
    {
        // bool canJump = descState >= 0 || descState == DescriptivePlayerState.Rolling;
        bool canJump = true;
        if(wantsJump && canJump)
        {
            SensorHit ceilingSensor = sensors.CastCeilingSensors().primarySensor;
            if(ceilingSensor.distance < 6f/PIXELS_PER_UNIT && ceilingSensor.hit) return false; //check if the player is too close to the ceiling to jump.

            // update state flag
            isJumping = true;

            //adjust position by the difference in the current height and the height when in ball form. This snaps us to the floor when going from standing -> jumping
            //this does nothing when going from rolling -> jumping because we're already snapped to the floor as a ball.
            EnterBallState();

            //Calculate direction to apply jump force in based on whether we're using the ground normal or the ground angle.
            Vector2 jumpDirection = PrimaryGroundSensor.normal;
            if(useGroundAngle)
            {
                jumpDirection.y = Mathf.Cos(PrimaryGroundSensor.angle * Mathf.Deg2Rad);
                jumpDirection.x = -Mathf.Sin(PrimaryGroundSensor.angle * Mathf.Deg2Rad);

                //I don't know that the original games do this- I imagine not. leaving it in because normalizing it makes sense to in terms of force magnitude accuracy.
                jumpDirection.Normalize();
            }

            //apply the jump force
            velocity += movementStats.jump_force * jumpDirection;

            //update current player state
            curPlayerState = PlayerState.Airborne;
            // descState = DescriptivePlayerState.Jumping;
            
            //remove ground hit/reset to uninitialized (angle 0, normal up, hit false)
            // groundSensorHit = new SensorHit();

            //clear jump input flag since we don't need it anymore.
            wantsJump = false;

            //return true so we know that we jumped successfully.
            return true;
        }
        else return false;
    }

    void CheckForVariableJumpHeight()
    {
        if(isJumping && !InputManager.Instance.jumpButton)
        {
            float minJumpForce = 4f/PIXELS_PER_UNIT;

            if(velocity.y > minJumpForce && velocity.y < movementStats.jump_force)
            {
                velocity.y = minJumpForce;            
            }
        }
    }

    float GetSlopeFactor(bool useGroundAngle = true)
    {
        if(disableSlopeFactor) return 0f;
        
        float angleFactor = useGroundAngle ? Mathf.Sin(PrimaryGroundSensor.angle * Mathf.Deg2Rad) : PrimaryGroundSensor.normal.x;

        if(curPlayerState == PlayerState.Grounded && sensors.curGroundMode != GroundSensorMode.Ceiling)
        {
            float slopeFactor = movementStats.slope_factor_normal * angleFactor;
            bool tooSteepToStand = Mathf.Abs(slopeFactor) >= movementStats.slope_factor_standing_threshold && movementStats.slope_factor_standing_threshold != 0f;

            if(groundSpeed != 0 || tooSteepToStand)
            {
                return slopeFactor;
            }
            else return 0f;
        }
        else if(curPlayerState == PlayerState.Rolling)
        {
            bool rollingUp = Mathf.Sign(groundSpeed) != Mathf.Sign(PrimaryGroundSensor.normal.x);
            float slopeFactor = rollingUp ? movementStats.slope_factor_rollup : movementStats.slope_factor_rolldown;
            return slopeFactor * angleFactor;
        }
        return 0f;
    }

    void MovePlayerObject(Vector2 newPosition, bool useLerp = true)
    {
        if(useLerp)
        {
            float alpha = Mathf.Min((Time.time - Time.fixedTime) / Time.fixedDeltaTime, 1f);
            transform.position = Vector3.Lerp(previousPosition, newPosition, alpha);
        }
        else
        {
            previousPosition = newPosition;
            transform.position = newPosition;
        }
    }

    void ApplyInputToGroundSpeed()
    {
        //lot of bools here, but makes the execution much more readable. 
        float xInput = Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y) ? moveInput.x : 0f;

        bool isRolling = curPlayerState == PlayerState.Rolling;
        bool isInputting = xInput != 0f;
        bool isMoving = Mathf.Abs(groundSpeed) != 0f;
        bool inputInMoveDir = Mathf.Sign(xInput) == Mathf.Sign(groundSpeed);

        float friction_speed = isRolling ?  movementStats.roll_friction_speed : movementStats.friction_speed;
        float decceleration_speed = isRolling ? movementStats.roll_deceleration_speed : movementStats.decceleration_speed;
        float acceleration_speed = isRolling ? 0f : movementStats.acceleration_speed;

        bool applyFriction = (!isInputting || isRolling) && isMoving; //quirk of the original games- friction is always applied while rolling, even if the player is deccelerating.
        bool applyDecceleration = isInputting && isMoving && !inputInMoveDir;
        bool applyAcceleration = isInputting && !isRolling && (inputInMoveDir || !isMoving) && Mathf.Abs(groundSpeed) < movementStats.top_speed;

        if(applyFriction)
        {
            groundSpeed -= Mathf.Min(Mathf.Abs(groundSpeed), friction_speed) * Mathf.Sign(groundSpeed); //player is deccelerating via friction
        }
        //no else here, because friction AND deccel can be applied while rolling

        if(applyDecceleration && controlLock == 0)
        {
            groundSpeed -= Mathf.Sign(groundSpeed) * decceleration_speed;
        }
        else if(applyAcceleration && controlLock == 0)
        {
            groundSpeed += acceleration_speed * Mathf.Sign(xInput);
            if(Mathf.Abs(groundSpeed) > movementStats.top_speed)
            {
                groundSpeed = movementStats.top_speed * Mathf.Sign(groundSpeed);
            }
        }
    }
    void ApplyAirAcceleration()
    {
        float xInput = moveInput.x;
        bool isInputting = xInput != 0f;
        bool inputInMoveDir = Mathf.Sign(xInput) == Mathf.Sign(velocity.x);
        bool applyAcceleration = (Mathf.Abs(velocity.x) < movementStats.top_speed && inputInMoveDir) || !inputInMoveDir;

        if(isInputting && applyAcceleration)
        {
            velocity.x += movementStats.air_acceleration_speed * Mathf.Sign(moveInput.x);

            if(Mathf.Abs(velocity.x) > movementStats.top_speed)
            {
                velocity.x = movementStats.top_speed * Mathf.Sign(velocity.x);
            }
        }
    }
    
    void ApplyAirDrag()
    {
        float maxAirDragSpeed = 4f/PIXELS_PER_UNIT;
        if(velocity.y > 0f && velocity.y < maxAirDragSpeed)
        {
            int roundedVelX = (int) (velocity.x / 0.125f);
            velocity.x -= roundedVelX / 256f;
        }
    }

    void DisableSpecialAbilityState()
    {
        if(isGrounded())
        {
            if(curPlayerState == PlayerState.Rolling)
            {
                descState = DescriptivePlayerState.Rolling;
            }
            else GroundedUpdateDescriptiveState();
        }
        else
        {
            descState = isJumping ? DescriptivePlayerState.Jumping : DescriptivePlayerState.Falling;
        }
    }
    void GroundedUpdateDescriptiveState()
    {
        isBall = false;

        if(activeAbility != null)
        {
            descState = DescriptivePlayerState.SpecialAbility;
            return;
        }

        if(groundSpeed == 0)
        {
            float input = moveInput.y;
            if(input == 0)
            {
                descState = DescriptivePlayerState.Standing;                
            }
            else
            {
                descState = input > 0 ? DescriptivePlayerState.LookingUp : DescriptivePlayerState.Crouching;                
            }
        }
        else if(groundSpeed != 0 && descState != DescriptivePlayerState.Rolling)
        {
            descState = DescriptivePlayerState.Walking;
        }
        else
        {
            descState = DescriptivePlayerState.Standing;
        }
    }
    void GroundedPushCollision()
    {
        isPushing = false;

        bool outsideAngleRange = PrimaryGroundSensor.angle > 90 && PrimaryGroundSensor.angle < 270f;
        bool angleIsMultipleOf90 = PrimaryGroundSensor.angle % 90f == 0f;

        bool isAngleValid = !outsideAngleRange || angleIsMultipleOf90;

        bool intoFloor = sensors.PushDirection() == Vector2.down;

        if(!isAngleValid) return;
        if(intoFloor) return;

        SensorHit pushSensor = sensors.CastPushSensor(true);

        Debug.DrawRay(pushSensor.point + Vector2.up * 2f, pushSensor.normal * pushSensor.distance, pushSensor.distance < 0f ? Color.green : Color.red);


        if(pushSensor.distance < 0f && pushSensor.hit)
        {
            groundSpeed = 0f;
            Debug.Log($"velocity before: {velocity}");
            Debug.Log($"distance: {pushSensor.distance}");
            Debug.Log($"amount added: {pushSensor.distance * pushSensor.castDirection}");
            velocity += pushSensor.distance * pushSensor.castDirection;
            
            Debug.Log($"velocity after: {velocity}");

            if(!isBall && curPlayerState != PlayerState.Rolling)
            {
                isPushing = true;
            }
        } 
    }
    void AirbornePushCollision()
    {
        SensorHit pushSensor = sensors.CastPushSensor();

        if(pushSensor.distance < 0f && pushSensor.hit)
        {
            velocity.x = 0f;

            position += pushSensor.distance * pushSensor.castDirection;

            if(!isBall && curPlayerState != PlayerState.Rolling)
            {
                isPushing = true;
            }
        } 
    }

    void CeilingCollision()
    {
        CompetingSensors ceilingSensors = sensors.CastCeilingSensors();
        SensorHit ceilingSensorHit = ceilingSensors.primarySensor;

        if(ceilingSensorHit.distance < 0f && ceilingSensorHit.hit)
        {
            bool angleTooFlat = ceilingSensorHit.angle >= 136f && ceilingSensorHit.angle <= 225f;

            if(!angleTooFlat)
            {
                groundSensors.primarySensor = ceilingSensorHit;
                curPlayerState = PlayerState.Grounded;
                groundSpeed = velocity.y * -Mathf.Sign(Mathf.Sin(PrimaryGroundSensor.angle * Mathf.Deg2Rad));
                Debug.DrawRay(position, sensors.CeilingDirection(), Color.green);
            }
            else
            {
                Debug.DrawRay(position, sensors.CeilingDirection(), Color.red);
                if(velocity.y > 0f)
                {
                    velocity.y = 0f;                
                }

                position.y += ceilingSensorHit.distance;
            }
        }
    }

    // Known limitation:
    // Two-point sensor collision can occasionally miss extremely shallow airborne slope landings.
    // This is a consequence of matching the original games' sensor-based collision approach.
    // Future refinement could use additional sampling or predictive collision checks.

    bool GroundCollision(CompetingSensors? overrideSensors = null)
    {
        bool prevGrounded = isGrounded();
        bool newGrounded = false;
        
        
        if(overrideSensors == null) //used by the ceiling sensors when landing on the ceiling
        {
            groundSensors = sensors.CastGroundSensors();
        }
        else groundSensors = (CompetingSensors) overrideSensors;

        if(PrimaryGroundSensor.hit)
        {
            if(prevGrounded)
            {
                float snapLimit = 14f/PIXELS_PER_UNIT;
                newGrounded = Mathf.Abs(PrimaryGroundSensor.distance) <= snapLimit;
            }
            else
            {
                if(PrimaryGroundSensor.distance < 0f)
                {
                    AirCollisionMode airCollisionMode= GetAirCollisionMode();

                    if(airCollisionMode == AirCollisionMode.MostlyDown) //moving mostly down
                    {
                        float threshold = velocity.y - 8f/PIXELS_PER_UNIT;
                        if(groundSensors.primarySensor.distance >= threshold || groundSensors.secondarySensor.distance >= threshold)
                        {
                            newGrounded = true;
                        }
                    }
                    else if(airCollisionMode == AirCollisionMode.MostlyX) //moving mostly left or right
                    {
                        if(velocity.y < 0f)
                        {
                            newGrounded = true;
                        }
                    }
                }
            }
        }

        if(newGrounded != prevGrounded)
        {
            if(newGrounded)
            {
                curPlayerState = PlayerState.Grounded;
            }
            else
            {
                curPlayerState = PlayerState.Airborne;
            }
        }

        return newGrounded;
    }

    void SnapToFloor(SensorHit groundSensor)
    {
        if(groundSensor.hit == false) return;

        bool isVertical = groundSensor.castDirection.y != 0f;

        if(isVertical)
        {
            position.y -= groundSensor.distance * Mathf.Sign(PrimaryGroundSensor.normal.y);
        }
        else
        {
            position.x -= groundSensor.distance * Mathf.Sign(PrimaryGroundSensor.normal.x);
        }
    }

    void AdjustSpeedDebugX()
    {
        float friction_speed = movementStats.friction_speed;
        float decceleration_speed = curPlayerState == PlayerState.Grounded ? movementStats.decceleration_speed : movementStats.roll_deceleration_speed; 

        bool applyFriction = moveInput.x == 0 && groundSpeed != 0; //quirk of the originals- friction is always applied while rolling, even if the player is deccelerating.
        bool applyDecceleration = Mathf.Sign(velocity.x) != Mathf.Sign(moveInput.x) && Mathf.Abs(velocity.x) > 0;
        bool applyAcceleration = Mathf.Abs(velocity.x) < movementStats.top_speed && (Mathf.Sign(moveInput.x) == Mathf.Sign(velocity.x) || velocity.x == 0) && moveInput.x != 0;


        if(applyFriction)
        {
            velocity.x -= Mathf.Min(Mathf.Abs(velocity.x), friction_speed) * Mathf.Sign(velocity.x); //player is deccelerating via friction
        }
        else if(applyDecceleration) //player is decelerating via force
        {
            velocity.x -= Mathf.Sign(velocity.x) * decceleration_speed;
        }
        else if(applyAcceleration) //player is accelerating
        {
            velocity.x += movementStats.acceleration_speed * Mathf.Sign(moveInput.x);
            if(Mathf.Abs(velocity.x) > movementStats.top_speed)
            {
                velocity.x = movementStats.top_speed * Mathf.Sign(velocity.x);
            }
        }
    }    
    
    void AdjustSpeedDebugY()
    {
        float friction_speed = movementStats.friction_speed;
        float decceleration_speed = curPlayerState == PlayerState.Grounded ? movementStats.decceleration_speed : movementStats.roll_deceleration_speed; 

        bool applyFriction = moveInput.y == 0 && groundSpeed != 0; //quirk of the originals- friction is always applied while rolling, even if the player is deccelerating.
        bool applyDecceleration = Mathf.Sign(velocity.y) != Mathf.Sign(moveInput.y) && Mathf.Abs(velocity.y) > 0;
        bool applyAcceleration = Mathf.Abs(velocity.y) < movementStats.top_speed && (Mathf.Sign(moveInput.y) == Mathf.Sign(velocity.y) || velocity.y == 0) && moveInput.y != 0;


        if(applyFriction)
        {
            velocity.y -= Mathf.Min(Mathf.Abs(velocity.y), friction_speed) * Mathf.Sign(velocity.y); //player is deccelerating via friction
        }
        else if(applyDecceleration) //player is decelerating via force
        {
            velocity.y -= Mathf.Sign(velocity.y) * decceleration_speed;
        }
        else if(applyAcceleration) //player is accelerating
        {
            velocity.y += movementStats.acceleration_speed * Mathf.Sign(moveInput.y);
            if(Mathf.Abs(velocity.y) > movementStats.top_speed)
            {
                velocity.y = movementStats.top_speed * Mathf.Sign(velocity.y);
            }
        }
    }

    enum AirCollisionMode
    {
        MostlyX,
        MostlyUp,
        MostlyDown,
    }

    AirCollisionMode GetAirCollisionMode()
    {
        if(Mathf.Abs(velocity.y) > Mathf.Abs(velocity.x))
        {
            return velocity.y > 0f ? AirCollisionMode.MostlyUp : AirCollisionMode.MostlyDown;
        }
        else
        {
            return AirCollisionMode.MostlyX;   
        }
    }

    float AirSpeedToGroundSpeed(bool useGroundAngle = true)
    {
        if(!useGroundAngle)
        {
            return Vector2.Dot(velocity, PrimaryGroundSensor.normal.Tangent());   
        }

        float symmetricalAngle = PrimaryGroundSensor.angle;

        //Done to keep angle on the 0-180 side to keep angle ranges symmetrical without repeating logic (if a > x && a < y) -> if(a < y)
        if(symmetricalAngle > 180f)
        {
            symmetricalAngle = Mathf.Abs(symmetricalAngle - 360f);
        }

        bool movingMostlyRight = Mathf.Abs(velocity.x) >= Mathf.Abs(velocity.y);
        float ySpeedToGround = velocity.y * Mathf.Sign(Mathf.Sin(PrimaryGroundSensor.angle * Mathf.Deg2Rad));

        if(symmetricalAngle <= 23f)
        {
            return velocity.x;
        }
        else if(symmetricalAngle <= 45f)
        {
            return movingMostlyRight ? velocity.x : ySpeedToGround * 0.5f;
        }
        else
        {
           return movingMostlyRight ? velocity.x : ySpeedToGround;
        }
    }

    public Vector2 GroundSpeedToVelocity(bool useGroundAngle = true, bool applySpeedCaps = false)
    {
        Vector2 newVelocity = Vector2.zero;

        float max_gsp = movementStats.max_speed_ground;
        float max_x = movementStats.max_speed_x;
        float max_y = movementStats.max_speed_y;

        if(applySpeedCaps)
        {
            groundSpeed = Mathf.Clamp(groundSpeed, -max_gsp, max_gsp);        
        }

        if(useGroundAngle)
        {
            float angle = PrimaryGroundSensor.angle * Mathf.Deg2Rad;

            newVelocity.x = groundSpeed * Mathf.Cos(angle);
            newVelocity.y = groundSpeed * Mathf.Sin(angle);
        }
        else
        {
            newVelocity.x = groundSpeed * PrimaryGroundSensor.normal.y;
            newVelocity.y = groundSpeed * -PrimaryGroundSensor.normal.x;
        }

        if(applySpeedCaps)
        {
            newVelocity.x = Mathf.Clamp(newVelocity.x, -max_x, max_x);    
            newVelocity.y = Mathf.Clamp(newVelocity.y, -max_y, max_y);
        }

        return newVelocity;
    }

    public void StartRolling()
    {
        curPlayerState = PlayerState.Rolling;
        EnterBallState();

        //Little quirk of the originals- if the player somehow has zero GSP when starting a roll, his GSP is set to 2 (pixels). 
        // This behavior is primarily observed in S Tunnels, which force a Roll regardless of current speed.

        if(groundSpeed == 0f)
        {
            groundSpeed = 2f/PIXELS_PER_UNIT;
        }
    }

    public void EnterBallState()
    {
        if(isBall) return;
        //Note: we don't change the playerState to PlayerState.Rolling because we don't always need the player to be rolling while in ball form (jumping, special abilities, etc.) so we handle that explicitly

        //moves the player towards the floor, so when they change size the bottom of the player is correctly aligned to the floor. 
        //This does nothing if the player is already at ball height, to prevent accidental position shifts.
        position += sensors.GroundDirection() * (curHeight - movementStats.ballHeight) / 2f;
        MovePlayerObject(position, false);

        isBall = true;

        //update bounds/hitbox to correctly match the height and width while in ball form.
        UpdateHitbox();
    }

    public void ExitBallState()
    {
        if(!isBall) return;

        //moves the player away from the floor, so when they change size the bottom of the player is correctly aligned to the floor. 
        //This does nothing if the player is already at standing height, to prevent accidental position shifts.
        position += sensors.GroundDirection() * (curHeight - movementStats.standingHeight) / 2f;
        MovePlayerObject(position, false);

        isBall = false;

        //update bounds/hitbox to correctly match the height and width while in standing form.
        UpdateHitbox();
    }

    public void ToggleBallState() //This just automatically picks whether or not to enter/exit the ball state based on if the player is already in the ball mode. Will likely remain unused.
    {
        if(isBall)
        {
            ExitBallState();
        }
        else
        {
            EnterBallState();
        }
    }

    /// <summary>
    ///     Adds a force to the player's velocity.
    /// </summary>
    public void AddForce(Vector2 force)
    {
        ForceStateChange(PlayerState.Airborne);
        velocity += force;
        groundSpeed += force.x;
    }

    public void SpringPlayer(Vector2 springForce)
    {
        OverrideVelocity(springForce);
        
        isJumping = false;

        if(springForce.y != 0f)
        {
            ExitBallState();
        }

        //TODO: Add logic to track player's spring state so the animation can be played.
    }

    /// <summary>
    ///     Sets the velocity to the new value, but only on the axes that are non-zero in the newVelocity vector.
    /// </summary>
    public void OverrideVelocity(Vector2 newVelocity)
    {
        ForceStateChange(PlayerState.Airborne);
        if(newVelocity.x != 0f)
        {
            velocity.x = newVelocity.x;   
            groundSpeed = newVelocity.x;     
        }
        if(newVelocity.y != 0f)
        {
            velocity.y = newVelocity.y;        
        }
    }

    /// <summary>
    ///     Sets the player's velocity to the new value.
    /// </summary>
    public void SetVelocity(Vector2 newVelocity)
    {
        ForceStateChange(PlayerState.Airborne);
        velocity = newVelocity;
        groundSpeed = newVelocity.x;
    }

    public void SetPosition(Vector2 newPosition, bool resetVelocity = true)
    {
        if(resetVelocity)
        {
            position = newPosition;
            velocity = Vector2.zero;
            groundSpeed = 0f;
        }

        ForceStateChange(PlayerState.Airborne);
    }

    public void SetGroundSpeed(float speed)
    {
        groundSpeed = speed;
    }

    public void ForceStateChange(PlayerState newState)
    {
        if(curPlayerState == newState || curPlayerState == PlayerState.DEBUG) return;

        curPlayerState = newState;
    
        if(newState == PlayerState.Rolling)
        {
            StartRolling();
        }
    }

    public void PlaySoundEffect(AudioClip clip, float pitch = 1f)
    {
        if(clip == null) return;

        if(audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip);
    }
}