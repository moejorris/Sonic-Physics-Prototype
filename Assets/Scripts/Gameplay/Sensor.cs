using UnityEngine;
using UnityEngine.Rendering;
using static UnitConversion;

public static class Sensor
{
    //in world units. In the originals, the max a sensor can reach is 32 pixels, which translates to 2 world units. 
    // Sensors cast in world units, and only convert to pixel space so we just store this in world units.
    internal static float maxDistance = 1f; 
    internal static Vector2 Floor(Vector2 vector)
    {
        return new Vector2(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
    }
    public static SensorHit SensorCast_PixelSpace(Vector2 anchorPoint, Vector2 checkDirection, float checkDistance, int layerMask = ~0, bool drawDebugLines = false, Color? debugLineColor = null, bool wholePixelOnly = true)
    {
        anchorPoint = ToWorldSpace(Floor(anchorPoint));
        checkDistance = ToWorldSpace(Mathf.FloorToInt(checkDistance));

        SensorHit returnHit = SensorCast_WorldSpace(anchorPoint, checkDirection, checkDistance, layerMask, drawDebugLines, debugLineColor);
        return returnHit.InPixelSpace();
    }

    public static SensorHit SensorCast_WorldSpace(Vector2 anchorPoint, Vector2 checkDirection, float checkDistance, int layerMask = ~0, bool drawDebugLines = false, Color? debugLineColor = null)
    {
        RaycastHit2D returnSensor = new RaycastHit2D();

        //enforce direction is axis aligned and normalized
        checkDirection = checkDirection.OneAxis();
        
        RaycastHit2D[] allNormalHits = Physics2D.RaycastAll(anchorPoint, checkDirection, maxDistance, layerMask);
        RaycastHit2D normalHit = FindBestCheckCandidate(allNormalHits, checkDirection);

        RaycastHit2D[] allExtensionHits = Physics2D.RaycastAll(anchorPoint - checkDirection * maxDistance, checkDirection, maxDistance, layerMask);
        RaycastHit2D extensionHit = FindBestCheckCandidate(allExtensionHits, checkDirection, SensorType.Extension);
        
        RaycastHit2D[] allRegressionHits = Physics2D.RaycastAll(anchorPoint, -checkDirection, maxDistance, layerMask);
        RaycastHit2D regressionHit = FindBestCheckCandidate(allRegressionHits, checkDirection, SensorType.Regression);

        RaycastHit2D[] hits = {normalHit, extensionHit, regressionHit};

        returnSensor = GetClosestHit(hits);

        if(drawDebugLines)
        {
            if(debugLineColor == null)
            {
                debugLineColor = Color.white;
            }

            Debug.DrawRay(anchorPoint, -checkDirection * checkDistance, (Color)debugLineColor, Time.fixedDeltaTime);
            
            Vector2 offset = new Vector2(checkDirection.y, checkDirection.x);   
            Debug.DrawRay(anchorPoint - offset * 0.5f * 0.1f, offset * 0.1f, Color.white);
            Debug.DrawRay(anchorPoint - checkDirection * 0.5f * 0.1f, checkDirection * 0.1f, Color.white);

            // if(returnSensor.collider != null)
            // {
            //     Debug.DrawLine(anchorPoint, returnSensor.point);             
            // }
            // Debug.DrawRay(anchorPoint, checkDirection * maxDistance, Color.red);
            // Debug.DrawRay(anchorPoint, -checkDirection * maxDistance, Color.red);
            // Debug.DrawRay(anchorPoint - checkDirection * maxDistance, checkDirection * maxDistance, Color.red);
        }

        if(returnSensor.collider == null)
        {
            return new SensorHit();
        }
        else return new SensorHit(returnSensor.collider, returnSensor.point, returnSensor.normal, checkDirection, returnSensor.distance);
    }

    private enum SensorType
    {
        Normal,
        Extension,
        Regression
    };

    private static RaycastHit2D GetClosestHit(RaycastHit2D[] hits)
    {
        int closestIndex = -1;
        for(int i = 0; i < hits.Length; i++)
        {
            if(hits[i].collider == null) continue;

            if(closestIndex == -1)
            {
                closestIndex = i;
            }
            else if(hits[i].distance < hits[closestIndex].distance)
            {
                closestIndex = i;
            }
        }

        if(closestIndex == -1)
        {
            return new RaycastHit2D();
        }

        return hits[closestIndex];
    }

    private static RaycastHit2D FindBestCheckCandidate(RaycastHit2D[] hits, Vector2 checkDir, SensorType sensorType = SensorType.Normal)
    {
        int candidateIndex = -1;

        if(hits.Length == 0)
        {
            return new RaycastHit2D();
        }

        for(int i = 0; i < hits.Length; i++)
        {
            if(hits[i].collider != null)
            {
                if(hits[i].collider.CompareTag("TopSolid") && checkDir != -Vector2.up)
                {
                    continue;
                }
                else if(hits[i].collider.CompareTag("SideBottomSolid") && checkDir == -Vector2.up)
                {
                    continue;
                }

                if(sensorType == SensorType.Extension)
                {
                    hits[i].distance -= maxDistance;
                }
                else if(sensorType == SensorType.Regression)
                {
                    hits[i].distance = -hits[i].distance;
                }

                if(Vector2.Dot(hits[i].normal, checkDir) > 0f) //flip normal if we got the normal on the inside of the collider
                {
                    hits[i].normal = -hits[i].normal;
                }

                if(candidateIndex == -1)
                {
                    candidateIndex = i;
                }
                else if(hits[i].distance < hits[candidateIndex].distance)
                {
                    candidateIndex = i;
                }
            }
        }

        if(candidateIndex < 0 || candidateIndex >= hits.Length)
        {
            return new RaycastHit2D();
        }
        hits[candidateIndex].distance = Mathf.FloorToInt(hits[candidateIndex].distance*PIXELS_PER_UNIT)/PIXELS_PER_UNIT;
        return hits[candidateIndex];
    }
}



public class SensorHit
{
    public Collider2D collider = null;
    public Vector2 castDirection = Vector2.zero;
    public Vector2 point = Vector2.zero;
    public Vector2 normal = Vector2.up; //the normal of the contact edge. if flagged the normal is axis aligned (x = +-1, y = 0) || (x = 0, y = +-1)
    public Vector2 rawNormal = Vector2.up; //always returns the raw normal of the edge.
    public float angle {get{return AngleFromNormal(normal);}} //returns the angle of the normal. Only 90 degree increments if flagged.
    public float signedAngle {get{return SignedAngle(angle);}}
    public float rawAngle {get{return AngleFromNormal(rawNormal);}} //always returns the angle of the edge normal, regardless of flagging.
    public float rawSignedAngle {get{return SignedAngle(rawAngle);}}
    public float distance = 0f;
    public bool hit = false;

    public SensorHit()
    {
        collider = null;
        point = Vector2.zero;
        castDirection = Vector2.zero;
        normal = Vector2.up; 
        rawNormal = normal; 
        distance = 0f;
        hit = false;
    }

    public SensorHit(Collider2D _collider, Vector2 _point, Vector2 _normal, Vector2 _direction, float _distance)
    {
        collider = _collider;
        point = _point;
        normal = _normal;
        rawNormal = _normal;
        distance = _distance;
        castDirection = _direction;
        hit = _collider != null;

        if(_collider != null)
        {
            var flags = _collider.GetComponent<Utils_SpriteShapeAngleFlags>();
            if(flags != null && flags.IsPointFlagged(_point))
            {
                normal = _normal.OneAxis().normalized;
            }
        }
    }

    public SensorHit InPixelSpace()
    {
        SensorHit pixelSpaceSensor = this;

        if(pixelSpaceSensor.hit)
        {
            pixelSpaceSensor.distance = ToPixelSpace(distance);        
        }
        else pixelSpaceSensor.distance = ToPixelSpace(Sensor.maxDistance);
         
        pixelSpaceSensor.point = ToPixelSpace(point);
        return pixelSpaceSensor;
    }

    float AngleFromNormal(Vector2 normal, bool quantizeAngle = true)
    {
        float angle = Vector2.SignedAngle(Vector2.up, normal.normalized);
        float angleRounding = 360f/256f; //original games store angles 0 - 255, edge case not being handled here is 360 degrees wasn't stored in the originals. was just wrapped back to 0. Not a problem, just a footnote.

        if(angle < 0f)
        {
            angle += 360f;
        }

        if(quantizeAngle)
        {
            angle = Mathf.RoundToInt(angle/angleRounding) * angleRounding;        
        }

        return angle;
    }

    float SignedAngle(float angle)
    {
        angle = Mathf.Repeat(angle, 360f);
        return angle > 180f ? angle - 360f : angle;
    }
}
