using UnityEngine;
using static UnitConversion;
using System.Runtime.CompilerServices;

public static class Vector2Extensions
{
    /// <summary>
    ///     Returns the normalized clockwise tangent of the vector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 TangentSafe(this Vector2 v)
    {
        Vector2 n = v.normalized;
        return new Vector2(n.y, -n.x);
    }

    /// <summary>
    ///     Returns the clockwise tangent of the vector. 
    ///     Assumes the vector is already normalized.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Tangent(this Vector2 v)
    {
        return new Vector2(v.y, -v.x);
    }

    /// <summary>
    ///     Returns the normalized counter-clockwise tangent of the vector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 CCTangentSafe(this Vector2 v)
    {
        Vector2 n = v.normalized;
        return new Vector2(-n.y, n.x);
    }

    /// <summary>
    ///     Returns the counter-clockwise tangent of the vector. 
    ///     Assumes the vector is already normalized.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 CCTangent(this Vector2 v)
    {
        return new Vector2(-v.y, v.x);
    }

    /// <summary>
    ///     Returns the vector with the axes flipped. 
    ///     Not a tangent.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static Vector2 FlipAxes(this Vector2 v)
    {
        return new Vector2(v.y, v.x);
    }

    /// <summary>
    ///     Returns vector but zeroes out the smaller axis.
    ///     If they are equal, the y axis will be preserved and the x will be zero. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 OneAxis(this Vector2 v)
    {

        if(Mathf.Abs(v.y) >= Mathf.Abs(v.x))
        {
            v.x = 0f;
        }
        else v.y = 0f;


        return v;
    }

    /// <summary>
    ///     Returns vector but it's axes are turned positive.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Abs(this Vector2 v)
    {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }

    /// <summary>
    ///     Returns the angle from the vector passed in. 
    ///     (e.g: Vector2.right => 0f, Vector2.up => 90f, Vector2.left => 180f, etc.)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float AngleDeg(this Vector2 v) //for ground angle
    {
        return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
    }

    public static Vector2 SnapToPixel(this Vector2 v)
    {
        float snapTo = 1f/PIXELS_PER_UNIT;
        float x = Mathf.Round(v.x/snapTo)*snapTo;
        float y = Mathf.Round(v.y/snapTo)*snapTo;

        return new Vector2(x,y);
    }
}
