using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Sonic Animation Library", menuName = "Sonic Characters/GenesisSonicAnimations")]
public class GenesisSonicAnimations : ScriptableObject
{
    
    
    [Header("Standing")]
    public SonicSprite[] idle;
    public SonicSprite[] balancing;
    public SonicSprite[] lookUp;
    public SonicSprite[] crouch;

    [Header("Moving")]
    public SonicSprite[] walking;
    public SonicSprite[] running;
    public SonicSprite[] dashing;
    public SonicSprite[] pushing;

    public enum BrakeStyle {Sonic1_CD, Sonic2_3_Knuckles};

    public BrakeStyle brakingStyle = BrakeStyle.Sonic1_CD;
    public SonicSprite[] braking;

    [Header("Ball")]
    public BallSprite[] ball;
    public SonicSprite[] spindash;

    [Header("Airborne")]    
    public SonicSprite[] spring;
    public SonicSprite[] diagonalSpring;
    public SonicSprite[] falling; 
    
}

public class AnimationSet
{
    public SonicSprite[] normal;
    public SonicSprite[] superForm;
}

[Serializable]
public class SonicSprite
{
    [SerializeField] string animName; //this field is purely for the user to be able to identify faster what this set is, rather than having to guess.
    public bool shouldLoop = true;
    public int loopTimes = 0; //leave 0 to loop infinitely
    public bool variableByGroundSpeed = false;
    public float subImage_duration;
    public Sprite[] sprites;
    public Sprite[] superSprites;
}

[Serializable]
public class BallSprite : SonicSprite
{
    public Sprite ballSprite;
}