using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImageSystem : MonoBehaviour
{
    public bool emit = true;
    [SerializeField] bool alwaysRun;
    [SerializeField] bool isRunning = false;
    [SerializeField] int maxAfterImages = 10;
    int targetAfterImages = 0;
    [SerializeField] List<SpriteRenderer> afterImages = new List<SpriteRenderer>();

    public SpriteRenderer sourceSpriteRenderer;
    
    [SerializeField] Color startColor;
    [SerializeField] Gradient colorOverLifetime;
    public float emissionRateOverTime = 50f;
    public float lifeTime = 0.3f;

    bool allInactive = false;
    void Awake()
    {
        targetAfterImages = Mathf.CeilToInt(emissionRateOverTime * lifeTime);
        float createAmount = targetAfterImages > maxAfterImages && maxAfterImages > 0 ? maxAfterImages : targetAfterImages;


        transform.position = sourceSpriteRenderer.transform.position;
        for(int i = 0; i < createAmount; i++)
        {
            AddSpriteObject("afterImage_" + i);
        }
    }

    void Update()
    {
        if(alwaysRun && gameObject.activeSelf)
        {
            emit = alwaysRun;
        }

        //update the list size incase the max after images amount was changed after Awake.
        if(emit && !isRunning)
        {
            StartCoroutine("DrawAfterImages");
        }

        if(emit)
        {
            if(afterImages.Count != targetAfterImages)
            {
                UpdateListSize();
            }
        }

        if(isRunning || !allInactive)
        {
            //update each alpha/fade value. Also checks if all the sprites are inactive to fix a fail safe.

            allInactive = true;

            for(int i = 0; i < afterImages.Count; i++)
            {
                if(afterImages[i] == null) continue;

                Color curColor = colorOverLifetime.Evaluate(Mathf.Abs((afterImages[i].sortingOrder + sourceSpriteRenderer.sortingOrder) / 60f)/lifeTime) * startColor;
                afterImages[i].color = curColor;

                if(afterImages[i].color.a <= 0)
                {
                    afterImages[i].gameObject.SetActive(false);
                }

                if(afterImages[i].gameObject.activeSelf)
                {
                    allInactive = false;
                }
            }

            if(allInactive)
            {
                isRunning = false;
            }
        }
    }

    void OnDisable()
    {
        emit = false;
    }

    void UpdateListSize()
    {
        if(afterImages.Count > maxAfterImages)
        {
            while(afterImages.Count > maxAfterImages)
            {
                SpriteRenderer sr = afterImages[afterImages.Count - 1];
                afterImages.RemoveAt(afterImages.Count-1);
                Destroy(sr.gameObject);
            }
            // Debug.Log("Size reduction valid?: " + (afterImages.Count == maxAfterImages));
        }
        else if(afterImages.Count < targetAfterImages)
        {
            for(int i = afterImages.Count - 1; i < targetAfterImages; i++)
            {
                AddSpriteObject("afterImage_" + (i + 1));
            }
            // Debug.Log("Size increase valid?: " + (afterImages.Count == targetAfterImages));
        }
    }

    void AddSpriteObject(string objectName = "afterImage")
    {
        GameObject go = new GameObject();
        // go.transform.parent = transform;
        afterImages.Add(go.AddComponent<SpriteRenderer>());
        go.name = objectName;
        go.SetActive(false);
    }

    IEnumerator DrawAfterImages()
    {
        targetAfterImages = Mathf.CeilToInt(emissionRateOverTime * lifeTime);

        isRunning = true;
        int i = 0;

        while(emit && afterImages.Count == targetAfterImages) //continuously spawn new after images, following the spawn rate (after images spawned per sec.)
        {
            yield return new WaitForSeconds(1/emissionRateOverTime);

            if(afterImages[i] == null || sourceSpriteRenderer == null || afterImages.Count != maxAfterImages)
            {
                isRunning = false;
                // emit = false;
                break;
            }
            else
            {
                afterImages[i].flipX = sourceSpriteRenderer.flipX;
                afterImages[i].flipY = sourceSpriteRenderer.flipY;
                afterImages[i].sprite = sourceSpriteRenderer.sprite;
                afterImages[i].color = startColor;
                afterImages[i].transform.position = sourceSpriteRenderer.transform.position;
                afterImages[i].transform.rotation = sourceSpriteRenderer.transform.rotation;
                afterImages[i].sortingOrder = sourceSpriteRenderer.sortingOrder-1;
                afterImages[i].sortingLayerName = sourceSpriteRenderer.sortingLayerName;
                afterImages[i].gameObject.SetActive(true);

                for(int a = 0; a < afterImages.Count; a++)
                {
                    afterImages[a].sortingOrder--;
                }


                i++;
                if(i >= afterImages.Count)
                {
                    i = 0;
                }
            }
        }

        isRunning = false;
    }
}
