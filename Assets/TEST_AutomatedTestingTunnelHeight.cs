using System.Collections.Generic;
using UnityEngine;

public class TEST_AutomatedTestingTunnelHeight : MonoBehaviour
{
    Vector2 startPos;
    [SerializeField] float currentHeight;
    [SerializeField] float currentBest;
    [SerializeField] List<float> heights = new List<float>();
    enum GroundSpeedOverride
    {
        Zero,
        HalfRunSpeed,
        RunSpeed,
        DashSpeed,
        SpeedCap
    }

    [SerializeField] GroundSpeedOverride overrideSpeed = GroundSpeedOverride.Zero;

    bool startRecording = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = new Vector3(502,21.5f);
        ResetPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        InputManager.Instance.OverrideMoveInput(Vector2.right);

        if(PlayerMovement.Player.Velocity.y > 0f && !PlayerMovement.Player.isGrounded())
        {
            startRecording = true;
        }

        if(PlayerMovement.Player.Position.y > currentHeight && startRecording)
        {
            currentHeight = PlayerMovement.Player.Position.y;
        }

        if(PlayerMovement.Player.Velocity.y < 0f && startRecording)
        {
            
            if(currentHeight > currentBest)
            {
                if(currentBest != 0)
                {
                    Debug.Log("New record!: " + currentHeight);
                }
                else Debug.Log("Baseline: " + currentHeight);

                currentBest = currentHeight;
            }
            
            heights.Add(currentHeight);
        
            currentHeight = 0;

            ResetPlayer();
        }
    }

    void ResetPlayer()
    {
        startRecording = false;
        PlayerMovement.Player.SetPosition(startPos);
        PlayerMovement.Player.ForceStateChange(PlayerMovement.PlayerState.Grounded);
        SonicCamera.instance.WarpCam(startPos);
        if(overrideSpeed != GroundSpeedOverride.Zero)
        {
            PlayerMovementStats ms = PlayerMovement.Player.movementStats;

            float groundSpeed = 0f;
            switch(overrideSpeed)
            {
                case GroundSpeedOverride.HalfRunSpeed:
                groundSpeed = ms.top_speed / 2f;
                break;

                case GroundSpeedOverride.RunSpeed:
                groundSpeed = ms.top_speed;
                break;

                case GroundSpeedOverride.DashSpeed:
                groundSpeed = ms.dash_speed;
                break;

                case GroundSpeedOverride.SpeedCap:
                groundSpeed = 1f;
                break;
            }
            // PlayerMovement.Player.SetVelocity(groundSpeed * Vector2.right);
            PlayerMovement.Player.SetGroundSpeed(groundSpeed);
        }

    }
}
