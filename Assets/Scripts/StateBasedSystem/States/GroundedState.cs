using UnityEngine;

public class GroundedState : PhysicsState
{
    enum SubState
    {
        Normal,
        Rolling
    }

    SubState curSubState = SubState.Normal;

    void RollCheck()
    {
        //start rolling?
        if(curSubState != SubState.Rolling && absGroundSpeed >= stats.roll_activation_speed && input.moveInputDir.y < 0f)
        {
            StartRoll();
        }
        //stop rolling?
        else if(curSubState == SubState.Rolling && absGroundSpeed < stats.roll_deactivation_speed)
        {
            StopRoll();
        }
    }

    public void StartRoll()
    {
        if(groundSpeed == 0f)
        {
            groundSpeed = stats.roll_min_ground_speed;
        }

        curSubState = SubState.Rolling;
        player.EnterBallForm();
    }

    public void StopRoll()
    {
        curSubState = SubState.Normal;
        player.ExitBallForm();
    }

    public override void Enter()
    {
        player.ExitBallForm();
        player.isJumping = false;
    }

    public override void Exit()
    {
        curSubState = SubState.Normal;
    }

    public override void Update()
    {
        if(curSubState == SubState.Normal)
        {
            //check for special animations (eg balancing) that prevent control.

            //check for various special abilities
        }

        //Adjust ground speed based on current ground angle (slope factor)
        groundSpeed -=  GetSlopeFactor();

        //jump check
        if(StartJump())
        {
            return; //Quirk of the originals- exits the rest of the cycle if the player jumps.
        }

        //update groundspeed based on input and apply friction
        ApplyInputToGroundSpeed();

        //check for starting crouching/looking up/balancing
        CheckForCrouchingOrLookingUp();

        //push sensor collision
            //only if ground speed is non zero and correct angle (within -90 to 90 or a multiple of 90)
            //occurs before new position is set, current X and Y speeds (velocity) are used to offset the sensor anchor pos
        collision.ResolvePushCollisions();

        //Rolling check
        RollCheck();

        //Move the player object
            //calculate X and Y speed from ground speed and ground angle
            SetVelocityToGroundSpeed();
            //update x and y position based on x and y speed
            position += velocity;

        //Grounded Ground Sensor Collision occurs
            //update ground angle
            //snap object to surface of the floor or become airborne if none found
        collision.ResolveGroundCollisions();

        //check for slipping/falling when ground speed is too low on walls/ceilings
        UpdateControlLock();
    }

    float GetSlopeFactor()
    {
        float sine = Mathf.Sin(Mathf.Deg2Rad * groundAngle);

        if(curSubState == SubState.Rolling)
        {
            float hillSpeed = Mathf.Sign(sine) == Mathf.Sign(groundSpeed) ? stats.roll_slope_factor_up : stats.roll_slope_factor_down;
            return  hillSpeed * sine; 
        }
        else
        {
            return stats.slope_factor * sine;
        }
    }

    void ApplyInputToGroundSpeed()
    {
        bool lockControls = controlLock != 0;
        bool wantsInput = input.moveInputDir.x != 0f && (Mathf.Abs(input.moveInputDir.x) >= Mathf.Abs(input.moveInputDir.y));

        float curSign = Mathf.Sign(groundSpeed);
        float inputSign = Mathf.Sign(input.moveInputDir.x);

        bool isInputting = wantsInput && !lockControls;

        if(isInputting)
        {
            bool applyAcceleration = (inputSign == curSign || groundSpeed == 0f) && curSubState != SubState.Rolling; //Player can't accelerate while rolling.

            if(applyAcceleration)
            {
                //Only accelerates if we are already below the top speed, and only cap speed if we are accelerating.
                //Allows the player to go faster than their top speed by going down a slope but not faster than their top speed just by acceleration.

                if(absGroundSpeed < stats.top_speed)
                {
                    groundSpeed += stats.acceleration_speed * inputSign;

                    if(absGroundSpeed > stats.top_speed)
                    {
                        groundSpeed = stats.top_speed * inputSign;
                    }
                }
            }
            else //if we're inputting but not accelerating, then we must be deccelerating.
            {
                bool isRolling = curSubState == SubState.Rolling;

                float cur_decceleration_speed = isRolling ? stats.roll_deceleration_speed : stats.decceleration_speed;

                groundSpeed -= cur_decceleration_speed * curSign;

                //if the player turns around while deccelerating, we set their speed to a small value in the new direction.
                //The turn around threshold is slightly more lenient for rolling. Normal only cares if the sign of ground speed changed.
                bool turnedAround = false;
                float inputDir = input.moveInputDir.x != 0f ? inputSign : 0f;
                if(inputDir > 0f)
                {
                    turnedAround = groundSpeed <= 0f;
                }
                else if(inputDir != 0f) turnedAround = groundSpeed >= 0f;


                bool normalTurnedAround = Mathf.Sign(groundSpeed) != curSign && turnedAround;
                bool rollTurnedAround = isRolling && absGroundSpeed < stats.roll_turn_around_threshold;

                if(normalTurnedAround || rollTurnedAround)
                {
                    groundSpeed = stats.turn_around_speed * -curSign;
                }
            }
        }
        else if(!wantsInput) //Quirk of the originals- holding input always prevents friction, regardless of control lock.
        {
            groundSpeed -= Mathf.Min(absGroundSpeed, stats.friction_speed) * Mathf.Sign(groundSpeed);
        }
    }

    void CheckForCrouchingOrLookingUp()
    {
        if(curSubState == SubState.Rolling) return;

        //check if the player is balancing on a ledge. This is primarily for animation purposes, but balancing prevents crouching and looking up.
        //What makes this more important is the player needs to be crouching in order to initiate a spindash, so balancing prevents spindashing as well.
        if(!collision.bothGroundSensorsHit && groundSpeed == 0f)
        {
            SensorHit balanceCheck = Sensor.SensorCast_PixelSpace(position + Vector2.down * stats.height_radius, Vector2.down, 16f, stats.collisionLayerMask, true, Color.black);
            player.isBalancing = !balanceCheck.hit || balanceCheck.distance >= 0f;
        }

        //check for crouching/looking up
        //if the player is pressing down or up and their ground angle is steep enough in the correct direction, they will enter a crouching or looking up state respectively that prevents them from moving and changes their hitbox.
        //Side note: the player can not look up or crouch while balancing on a ledge.

        if(absGroundSpeed < stats.max_crouch_speed && input.moveInputDir.y != 0f && !player.isBalancing)
        {
            if(input.moveInputDir.y > 0f)
            {
                //look up
            }
            else
            {
                //crouch
            }
        }
    }

    void UpdateControlLock()
    {
        if(controlLock != 0)
        {
            controlLock = Mathf.Max(controlLock - 1, 0); //as opposed to just doing controlLock--, this prevents it from ever going negative and getting stuck there :)
        }
        else
        {
            CheckForSlipping();
        }
    }

    void CheckForSlipping(bool s3Style = false)
    {
        if(absGroundSpeed >= stats.slipping_speed || controlLock != 0) return;

        //define normal (pre s3) slipping angle range
        bool inSlippingRange = groundAngle >= 46f && groundAngle <= 315f;

        if(s3Style)
        {
            inSlippingRange = groundAngle >= 35f && groundAngle <= 326f;     
            bool inFallingRange = groundAngle >= 69f && groundAngle <= 293f;

            if(inSlippingRange || inFallingRange)
            {
                player.SetControlLock(PlayerObject.ControlLockType.Slipping);

                if(inFallingRange)
                {
                    player.BecomeAirborne();
                }
                else
                {
                    groundSpeed += groundAngle < 180f ? -stats.slipping_s3_repel_speed : stats.slipping_s3_repel_speed;
                }
            }
        }
        else if(inSlippingRange)
        {
            player.SetControlLock(PlayerObject.ControlLockType.Slipping);
            groundSpeed = 0f;
            player.BecomeAirborne();
        }
    }

    bool StartJump()
    {
        if(input.jumpButton)
        {
            y_speed = stats.max_jump_force;
            player.BecomeAirborne();
            player.EnterBallForm();
            return true;
        }
        else return false;
    }

    void SetVelocityToGroundSpeed()
    {
        float angleRad = Mathf.Deg2Rad * groundAngle;
        x_speed = Mathf.Cos(angleRad) * groundSpeed;
        y_speed = Mathf.Sin(angleRad) * groundSpeed;
    }
}
