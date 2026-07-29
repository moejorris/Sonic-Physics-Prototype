using UnityEngine;

public class ParticleSysSpriteUpdater : MonoBehaviour
{
    int maxSprites = 5;
    [SerializeField] SpriteRenderer spriteRenderer;
    ParticleSystem pS;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pS = GetComponent<ParticleSystem>();

        for(int i = 0; i < maxSprites; i++)
        {
            pS.textureSheetAnimation.AddSprite(spriteRenderer.sprite);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // pS.Stop();
        var main = pS.main;
        if(pS.textureSheetAnimation.GetSprite(0) == spriteRenderer.sprite) return;
        for(int i = maxSprites - 2; i > -1; i--)
        {
            if(i < maxSprites - 1)
            {
                pS.textureSheetAnimation.SetSprite(i + 1, pS.textureSheetAnimation.GetSprite(i));
            }
        }
        main.startSize = spriteRenderer.sprite.rect.width/spriteRenderer.sprite.pixelsPerUnit;
        pS.textureSheetAnimation.SetSprite(0, spriteRenderer.sprite);

        // pS.Play();

        // pS.textureSheetAnimation.AddSprite(spriteRenderer.sprite);

        // if(pS.textureSheetAnimation.spriteCount > 3)
        // {
        //     pS.textureSheetAnimation.RemoveSprite(0);
        // }
    }
}
