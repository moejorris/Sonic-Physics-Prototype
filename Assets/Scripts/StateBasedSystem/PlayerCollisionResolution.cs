using Unity.VisualScripting;
using UnityEngine;

public class PlayerCollisionResolution
{
    PlayerObject player;
    float width_radius => player.isBall ? player.stats.roll_width_radius : player.stats.width_radius;
    float height_radius => player.isBall ? player.stats.roll_height_radius : player.stats.height_radius;
    float sensorAngle => player.isGrounded ? player.groundAngle : 0f; //sensors act as if angle is 0 when not grounded.
    public bool bothGroundSensorsHit = false;

    SensorHit winningGroundSensor;

    public void OnDrawGizmos()
    {
        Color color = Color.blue;
        color.a = 0.5f;
        Gizmos.color = color;

        bool vertical = GroundDirection().y != 0f;

        Vector3 size = new Vector3(vertical ? width_radius : height_radius, vertical ? height_radius : width_radius, 0.1f);

        Gizmos.DrawCube(player.position, UnitConversion.ToWorldSpace(size));
    }

    public PlayerCollisionResolution(PlayerObject player)
    {
        this.player = player;
    }

    public Vector2 GroundDirection()
    {
        float angle = sensorAngle;

        if(angle >= 315f)
        {
            return Vector2.down;
        }
        else if(angle >= 226f && angle <= 314f)
        {
            return Vector2.left;
        }
        else if(angle >= 135f && angle <= 225f)
        {
            return Vector2.up;
        }
        else if(angle >= 46f && angle <= 134f)
        {
            return Vector2.right;
        }
        else
        {
            return Vector2.down;
        }
    }

    public Vector2 PushDirection()
    {
        float angle = sensorAngle;

        if(angle >= 316f)
        {
            return Vector2.right;
        }
        else if(angle >= 225f && angle <= 315f)
        {
            return Vector2.down;
        }
        else if(angle >= 136f && angle <= 224f)
        {
            return Vector2.left;
        }
        else if(angle >= 45f && angle <= 135f)
        {
            return Vector2.up;
        }
        else
        {
            return Vector2.right;
        }
    }

    void Internal_ResolvePushCollision(Vector2 position, bool invert = false)
    {
        float sign = Mathf.Sign(player.groundSpeed);

        if(invert) sign = -sign;

        Vector2 direction = PushDirection() * sign;
        if(direction == GroundDirection()) return;

        float checkDist = player.stats.push_radius;

        Vector2 widthOffset = direction * checkDist;
        Vector2 anchorPoint = position + widthOffset;

        Color rightPushColor = Color.red;
        Color leftPushColor = Color.magenta;

        if(player.isGrounded)
        {
            //on the ground, the push sensor is checked before velocity is applied to the position, so we add it to the anchor position.
            anchorPoint += player.velocity;

            //in the original games, the push sensors y position is shifted down by 8 pixels on flat ground to detect tall steps.
            if(player.groundAngle == 0f)
            {
                anchorPoint.y -= 8f;
            }
        }

        SensorHit pushHit = Sensor.SensorCast_PixelSpace(anchorPoint, direction, checkDist, player.stats.collisionLayerMask, true, sign > 0f ? rightPushColor : leftPushColor);
    
        if(pushHit.hit && pushHit.distance < 0f)
        {
            if(player.isGrounded)
            {
                player.groundSpeed = 0f;
                player.velocity.x += pushHit.distance * sign;
            }
            else
            {
                player.velocity.x = 0f;
                player.position.x += pushHit.distance * sign;
            }
        }
    }

    public void ResolvePushCollisions()
    {

        if(player.isGrounded)
        {
            bool validAngle = player.groundAngle > 270f || player.groundAngle < 90f || player.groundAngle % 90f == 0f;
            if(!validAngle || player.groundSpeed == 0f) return;
        }
        Vector2 playerPos = player.position;

        Internal_ResolvePushCollision(playerPos);

        //both the left and right push sensors are cast when the player is airborne and moving mostly up and down.
        bool movingMostlyVertically = Mathf.Abs(player.velocity.x) < Mathf.Abs(player.velocity.y);
        if(!player.isGrounded && movingMostlyVertically)
        {
            Internal_ResolvePushCollision(playerPos, true);
        }
    }

    public void ResolveGroundCollisions()
    {
        CompetingSensors groundSensors = CastCompetingVerticalSensors(GroundDirection(), Color.green, Color.cyan);

        if(player.isGrounded)
        {
            bothGroundSensorsHit = groundSensors.primarySensor.hit && groundSensors.secondarySensor.hit;
            GroundedGroundCollision(groundSensors.primarySensor);
        }
        else
        {
            AirborneGroundCollision(groundSensors);
        }
    }

    void GroundedGroundCollision(SensorHit winningSensor)
    {
        winningGroundSensor = winningSensor;

        bool newGrounded = false;

        if(winningSensor.hit && Mathf.Abs(winningSensor.distance) <= PhysicsConsts.MAX_GROUNDED_SNAP_DISTANCE)
        {
            float positive_snap_limit = Mathf.Min(Mathf.Abs(player.velocity.x) + 4, PhysicsConsts.MAX_GROUNDED_SNAP_DISTANCE);
            float testedDist = winningSensor.distance;
            
            if(testedDist <= positive_snap_limit)
            {
                newGrounded = true;
            }
        }

        if(newGrounded)
        {
            player.BecomeGrounded();
            SnapPlayerToFloor(winningSensor.distance);

            player.groundAngle = GetTerrainAngle(winningSensor);
        }
        else player.BecomeAirborne();
    }

    public void SnapPlayerToFloor(float distanceToFloor = 999)
    {

        if(distanceToFloor == 999)
        {
            distanceToFloor = winningGroundSensor.distance;
        }

        Vector2 direction = GroundDirection();

        bool isVertical = Mathf.Abs(direction.y) >= Mathf.Abs(direction.x);

        if(isVertical)
        {
            player.position.y += distanceToFloor * Mathf.Sign(direction.y);
        }
        else
        {
            player.position.x += distanceToFloor * Mathf.Sign(direction.x);
        }

        // player.position += direction * distanceToFloor;
    }
    void AirborneGroundCollision(CompetingSensors groundSensors)
    {
        bool newGrounded = false;
        bool mostlyHorizontal = Mathf.Abs(player.velocity.x) >= Mathf.Abs(player.velocity.y);

        if(groundSensors.primarySensor.distance < 0f)
        {
            if(mostlyHorizontal)
            {
                newGrounded = true;
            }
            else
            {
                float distanceThreshold = player.velocity.y - 8f;
                bool eitherSensorCloseEnough = groundSensors.primarySensor.distance >= distanceThreshold || groundSensors.secondarySensor.distance >= distanceThreshold;
                
                if(eitherSensorCloseEnough)
                {
                    newGrounded = true;
                }
            }
        }

        if(newGrounded)
        {
            player.groundAngle = groundSensors.primarySensor.angle;
            (player.stateManager.currentState as AirborneState)?.VelocityToGroundSpeed();
            
            player.BecomeGrounded();
            SnapPlayerToFloor(groundSensors.primarySensor.distance);
        }
        else player.BecomeAirborne();
    }

    public void ResolveCeilingCollisions()
    {
        
    }

    float GetTerrainAngle(SensorHit sensor)
    {
        //Angle flagging system:
        //The original games had a system where tiles with an angle of 360 would be treated as "flagged".
        //The player would instead snap their angle to the nearest 90 degree increment, rather than using the angle of 360. 
        //This was also used for longer stretches of flat ground, so the grounds angle would not need to be manually entered for devs.
        //Since we're deriving our angles from edge normals rather than manually assigning them,
        //we're going to instead manually flag tiles instead of manually setting angles like the originals.
        //This allows us to preserve certain gameplay features that rely on the angle not matching the actual angle of the geometry.
        //Most notably this allows us to create ramps that guarantee an upward launch without needing to edit the spline to be perfectly flat going upwards.
        //The nice thing here, is that we have the ability to round the slope angle instead of snapping the player angle.
        //For accuracy to original behavior, I will snap/round the player's angle, but will leave the logic in for optionally snapping the slopes angle.

        bool isAngleFlagged = false;

        if(isAngleFlagged)
        {
            bool snapPlayerAngle = true;

            float angleToSnap = snapPlayerAngle ? player.groundAngle : sensor.angle;

            return Mathf.Floor(angleToSnap / 90f) * 90;
        }
        else
        {
            return sensor.angle;
        }
    }

    struct CompetingSensors
    {
        public SensorHit primarySensor;
        public  SensorHit secondarySensor;

        public CompetingSensors(SensorHit primarySensor, SensorHit secondarySensor)
        {
            this.primarySensor = primarySensor;
            this.secondarySensor = secondarySensor;
        }
    }

    CompetingSensors CastCompetingVerticalSensors(Vector2 direction, Color? primaryColor, Color? secondaryColor) //Casts competing sensors vertically relative to the player's ground angle.
    {
        Vector2 origin = player.position;
        Vector2 offset_width = direction.Perpendicular2() * width_radius;
        Vector2 offset_height = direction * height_radius;

        SensorHit sensorL = Sensor.SensorCast_PixelSpace(origin + offset_height - offset_width, direction, height_radius, player.stats.collisionLayerMask, true, primaryColor);
        SensorHit sensorR = Sensor.SensorCast_PixelSpace(origin + offset_height + offset_width, direction, height_radius, player.stats.collisionLayerMask, true, secondaryColor);

        SensorHit primary;
        SensorHit secondary;

        if(sensorL.hit || sensorR.hit)
        {
            if(sensorL.hit && sensorR.hit)
            {
                primary = sensorR.distance < sensorL.distance ? sensorR : sensorL;
            }
            else if(sensorL.hit)
            {
                primary = sensorL;
            }
            else
            {
                primary = sensorR;
            }
            secondary = primary == sensorL ? sensorR : sensorL;
        }
        else
        {
            primary = new SensorHit().InPixelSpace();
            secondary = new SensorHit().InPixelSpace();
        }

        return new CompetingSensors(primary, secondary);        
    }

    void CastGroundSensors()
    {
        
    }

    void CastCeilingSensors()
    {
        
    }

    void CastPushSensor()
    {
        
    }
}
