using UnityEngine;

public static class PhysicsConsts
{
    public static float FIXED_TIMESTEP = 1f/60f;
    public static float MAX_GROUNDED_SNAP_DISTANCE = 14f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitFixedTimestep() //ensures fixed update runs 60 times per second, matches original games' physics once per frame at a fixed FPS of 60 
    {
        Time.fixedDeltaTime = FIXED_TIMESTEP;
        Time.maximumDeltaTime = FIXED_TIMESTEP; //prevents physics from breaking if the game lags or runs at a low framerate
    }
}
