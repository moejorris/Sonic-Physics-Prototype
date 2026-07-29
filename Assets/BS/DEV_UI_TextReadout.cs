using TMPro;
using UnityEngine;

public class DEV_UI_TextReadout : MonoBehaviour
{
    public bool convertToPixels = true;
    TextMeshProUGUI textDisplay;
    enum ReadoutType
    {
        FPS,
        PPS,
        GroundSpeed,
        VelX,
        VelY,
    };

    [SerializeField] ReadoutType readout;

    void Awake()
    {
        textDisplay = GetComponent<TextMeshProUGUI>();
    }

    void LateUpdate()
    {
        if(PlayerMovement.Player == null ) return;

        float mult = convertToPixels ? 16f : 1f;

        string output = "";
        switch(readout)
        {
            case ReadoutType.FPS:
            output = "FPS: " + 1f/Time.deltaTime;
            break;

            case ReadoutType.PPS:
            output = "PPS: " + 1f/Time.fixedDeltaTime;
            break;

            case ReadoutType.GroundSpeed:
            output = "GSP: " + PlayerMovement.Player.GroundSpeed * mult;
            break;

            case ReadoutType.VelX:
            output = "VEL X:" + PlayerMovement.Player.Velocity.x * mult;
            break;            
            
            case ReadoutType.VelY:
            output = "VEL Y:" + PlayerMovement.Player.Velocity.y * mult;
            break;
        }

        textDisplay.text = output;
    }
}
