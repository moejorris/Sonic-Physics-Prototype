using System.Collections;
using UnityEngine;

public class PlayerTrail 
{
    PlayerMovement playerMovement;
    TrailRenderer trailRenderer;
    float trailTime = 0f;
    float trailDecayTime = 0.5f;
    Color color;

    bool isRunning = false;

    public PlayerTrail(Color trailColor, PlayerMovement playerMovement)
    {
        this.playerMovement = playerMovement;

        GameObject trailParent = new GameObject("TrailVFX");
        trailParent.transform.parent = playerMovement.transform;
        trailParent.transform.localPosition = Vector2.zero;

        trailRenderer = trailParent.AddComponent<TrailRenderer>();
        
        trailColor.a = 0.75f;

        color = trailColor;
        
        trailRenderer.startColor = trailColor;

        trailColor.a = 0;
        trailRenderer.endColor = trailColor;

        trailRenderer.startWidth = playerMovement.movementStats.ballHeight * 1.2f;
        trailRenderer.endWidth = trailRenderer.startWidth;

        trailRenderer.time = trailDecayTime/2f;

        trailRenderer.numCapVertices = 5;
        trailRenderer.minVertexDistance = 0f;

        trailRenderer.emitting = false;

        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void ActivateTrail()
    {
        if(GenesisSonicAnimator.instance)
        {
            trailRenderer.sortingOrder = GenesisSonicAnimator.instance.GetPlayerSpriteRenderer().sortingOrder - 1;        
        }


        trailTime = 0f;
        if(!isRunning)
        {
            playerMovement.StartCoroutine(UpdateTrailColor());
        }
    }

    IEnumerator UpdateTrailColor()
    {
        isRunning = true;
        trailRenderer.emitting = true;
        while (trailTime < trailDecayTime)
        {
            float alpha = 1f - (trailTime/trailDecayTime);
            
            Color newColor = color;
            newColor.a = alpha;

            trailRenderer.startColor = newColor;

            newColor.a = 0f;
            trailRenderer.endColor = newColor;

            yield return new WaitForEndOfFrame();
            trailTime = Mathf.Min(trailTime + Time.deltaTime, trailDecayTime);
        }
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        isRunning = false;
    }
}
