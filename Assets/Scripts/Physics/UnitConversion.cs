using UnityEngine;

public static class UnitConversion
{
    public const float PIXELS_PER_UNIT = 16f;
    const float SUBPIXEL = 1f / 256f;

    //converts subpixels to a float decimal value. eg 128 subpixels => 0.5f
    public static float SubpixelToFloat(int value) => value * SUBPIXEL;

    public static Vector2 ToPixelSpace(Vector2 worldSpaceVector)
    {
        return new Vector2(ToPixelSpace(worldSpaceVector.x), ToPixelSpace(worldSpaceVector.y));
    }

    public static float ToPixelSpace(float worldSpaceValue)
    {
        return worldSpaceValue * PIXELS_PER_UNIT;
    }

    public static Vector2 ToWorldSpace(Vector2 pixelSpaceVector)
    {
        return new Vector2(ToWorldSpace(pixelSpaceVector.x), ToWorldSpace(pixelSpaceVector.y));
    }

    public static float ToWorldSpace(float pixelSpaceValue)
    {
        return pixelSpaceValue / PIXELS_PER_UNIT;
    }
}
