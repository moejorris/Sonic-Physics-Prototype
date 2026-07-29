using System.Collections;
using UnityEngine;

public class PaletteSwapAnimator : MonoBehaviour
{
    public bool animate = true;
    public int animationFPS = 60;
    public int framesPerColor = 2;
    [SerializeField] Color[] baseColors;

    float frameTime = 0f;
    
    [System.Serializable]
    class ColorSwaps
    {
        public Color[] swapColors;
    }

    [SerializeField] ColorSwaps[] replacementColors;
    [SerializeField] int currentColorSet;
    bool animationStatus = false;

    Material material;
    SpriteRenderer spriteRenderer;
    Shader shader;

    void Start()
    {
        InitShader();
    }

    void Update()
    {
        bool canAnimate = spriteRenderer && material && baseColors.Length > 0 && replacementColors.Length > 0 && replacementColors[currentColorSet].swapColors.Length > 0;

        if(canAnimate)
        {
            if(!animationStatus)
            {
                StartCoroutine(AnimateColors());
            }
        }
        else
        {
            animate = false;
            if(animationStatus)
            {
                StopCoroutine(AnimateColors());
                animationStatus = false;
            }
        }

        if(!animate)
        {
            DisableColors();
        }
    }

    IEnumerator AnimateColors()
    {
        animationStatus = true;
        while(animate)
        {
            frameTime = 1f/((float) animationFPS);

            UpdateColors(currentColorSet);

            yield return new WaitForSeconds(frameTime * framesPerColor);
            currentColorSet++;
            if(currentColorSet >= replacementColors.Length)
            {
                currentColorSet = 0;
            }
        }
        animationStatus = false;
    }

    void InitShader()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        material = spriteRenderer.material;

        for(int i = 0; i < Mathf.Min(baseColors.Length, 4); i++)
        {
            material.SetColor("_Base" + i+1, baseColors[i]);
        }            
    }

    void UpdateColors(int swapIndex)
    {
        // for(int i = 0; i < Mathf.Min(baseColors.Length, 4); i++)
        // {
        //     material.SetColor("_Swap" + i, replacementColors[swapIndex].swapColors[i]);
        // }
        material.SetColor("_Swap1", replacementColors[swapIndex].swapColors[0]);
        // material.SetColor("_Swap1", Color.black);
        material.SetColor("_Swap2", replacementColors[swapIndex].swapColors[1]);
        material.SetColor("_Swap3", replacementColors[swapIndex].swapColors[2]);
        material.SetColor("_Swap4", replacementColors[swapIndex].swapColors[3]);
    }

    void DisableColors()
    {
        for(int i = 0; i < Mathf.Min(baseColors.Length, 4); i++)
        {
            material.SetColor("_Swap" + (i + 1), Color.black);
        }
    }
}
