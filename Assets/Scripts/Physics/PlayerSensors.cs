using Unity.VisualScripting;
using UnityEngine;

public class PlayerSensors
{
    PlayerMovement playerMovement;
    
    public PlayerSensors(PlayerMovement playerMovement)
    {
        this.playerMovement = playerMovement;
    }

    public enum GroundSensorMode
    {
        Floor,
        Ceiling,
        Wall_R,
        Wall_L
    };

    public GroundSensorMode curGroundMode = GroundSensorMode.Floor;    
    public Vector2 PushDirection()
    {
        float angle = playerMovement.PrimaryGroundSensor.angle;

        if(!playerMovement.isGrounded()) angle = 0f;


        if(angle >= 316f && angle <= 360f) //floor lhalf
        {
            return Vector2.right;
        }
        else if(angle >= 225f && angle <= 315f) //left wall
        {
            return Vector2.down;
        }
        else if(angle >= 136f && angle <= 224f) //ceil
        {
            return Vector2.left;
        }
        else if(angle >= 45f && angle <= 135f) //right wall
        {
            return Vector2.up;
        }
        else //floor rhalf
        {
            return Vector2.right;
        }
    }
    public Vector2 GroundDirection()
    {
        float angle = playerMovement.PrimaryGroundSensor != null ? playerMovement.PrimaryGroundSensor.angle : 0f;

        if(!playerMovement.isGrounded()) angle = 0f;


        if(!playerMovement.isGrounded())
        {
            curGroundMode = GroundSensorMode.Floor;
            return Vector2.down;
        }

        if(angle >= 315f && angle <= 360f) //floor left half
        {
            curGroundMode = GroundSensorMode.Floor;
            return Vector2.down;
        }
        else if(angle >= 226f && angle <= 314f) //left wall
        {
            curGroundMode = GroundSensorMode.Wall_L;
            return Vector2.left;
        }
        else if(angle >= 135f && angle <= 225f) //ceil
        {
            curGroundMode = GroundSensorMode.Ceiling;
            return Vector2.up;
        }
        else if(angle >= 46f && angle <= 134f) //right wall
        {
            curGroundMode = GroundSensorMode.Wall_R;
            return Vector2.right;
        }
        else //floor right half
        {
            curGroundMode = GroundSensorMode.Floor;
            return Vector2.down;
        }
    }

    public Vector2 CeilingDirection()
    {
        return -GroundDirection();
    }

    public CompetingSensors CastGroundSensors()
    {                
        return CastCompetingVerticalSensors(GroundDirection(), Color.green, Color.cyan);
    }

    public CompetingSensors CastCeilingSensors()
    {
        return CastCompetingVerticalSensors(CeilingDirection(), Color.blue, Color.yellow);
    }

    public struct CompetingSensors
    {
        public SensorHit primarySensor;
        public SensorHit secondarySensor;
        public CompetingSensors(SensorHit primarySensor, SensorHit secondarySensor)
        {
            this.primarySensor = primarySensor;
            this.secondarySensor = secondarySensor;
        }
    }

    CompetingSensors CastCompetingVerticalSensors(Vector2 direction, Color? sensorLColor = null, Color? sensorRColor = null)
    {
        float heightRadius = playerMovement.Height /2f;
        float widthRadius = playerMovement.Width /2f;
        var layerMask = playerMovement.movementStats.groundLayerMask;

        Vector2 xOffset = widthRadius * direction.Perpendicular2();
        Vector2 yOffset = heightRadius * direction;

        Vector2 anchorL = playerMovement.Position + yOffset - xOffset;
        Vector2 anchorR = playerMovement.Position + yOffset + xOffset;

        Color colorL = sensorLColor != null ? (Color) sensorLColor : Color.white;
        Color colorR = sensorRColor != null ? (Color) sensorRColor : Color.white;

        SensorHit sensorL = Sensor.SensorCast_WorldSpace(anchorL, direction, heightRadius, layerMask, true, colorL);
        SensorHit sensorR = Sensor.SensorCast_WorldSpace(anchorR, direction, heightRadius, layerMask, true, colorR);

        SensorHit primary;
        SensorHit secondary;
        
        if(sensorL.hit && sensorR.hit)
        {
            primary = sensorL.distance <= sensorR.distance ? sensorL : sensorR;
            secondary = primary == sensorL ? sensorR : sensorL;
        }
        else if(sensorL.hit)
        {
            primary = sensorL;
            secondary = sensorR;
        }
        else if(sensorR.hit)
        {
            primary = sensorR;
            secondary = sensorL;
        }
        else
        {
            primary = new SensorHit();
            secondary = new SensorHit();
        }

        return new CompetingSensors(primary, secondary);
    }


    public SensorHit CastPushSensor(bool beforePositionChange = false)
    {
        float speed = playerMovement.isGrounded() ? playerMovement.GroundSpeed : playerMovement.Velocity.x;

        if(speed == 0f) return new SensorHit();

        float pushRadius = PlayerMovement.Player.movementStats.pushRadius;
        float sign = Mathf.Sign(speed);
        
        Vector2 pushDir = PushDirection() * sign;
        Vector2 origin = playerMovement.Position + (pushDir * pushRadius);
        if(playerMovement.isGrounded())
        {
            if(playerMovement.PrimaryGroundSensor.angle == 0f && !playerMovement.isRolling())
            {
                origin -= Vector2.up * 8f/16f; //the original system moves the push sensor down on flat ground to detect steps.
            }
        }

        if(beforePositionChange)
        {
            origin += playerMovement.Velocity; //while on the ground, push sensors trigger before movement happens, so we start where the player's position will be after being moved.
        }

        int layerMask = PlayerMovement.Player.movementStats.groundLayerMask;

        Color sensorEColor = Color.magenta * 0.75f + Color.white * 0.25f;
        Color sensorFColor = Color.red * 0.75f + Color.white * 0.25f;

        SensorHit sensor = Sensor.SensorCast_WorldSpace(origin, pushDir, pushRadius, layerMask, true, sign > 0f ? sensorFColor : sensorEColor, false);
        return sensor;

    }
}
