using UnityEngine;
using System;
using System.Collections.Generic;

public class BG_Parallax : MonoBehaviour
{
    //TODO: Fix being able to see outside of the background at certain resolutions (Like 1920x1080). This has to do with the Pixel Perfect camera altering the ortho size.
    static readonly float[] GHZ_SCROLL_TABLE =
    {
        0.75f,
        0.7f,
        0.375f,
        0.25f,
        0.125f,
        0.0625f
    };

    [SerializeField] float verticalScrollMult = 0.10f;

    #region ParallaxItem
    [Serializable]
    class ParallaxItem
    {
        public Sprite sprite;
        [SerializeField] float autoScroll = 0f;

        [HideInInspector] public float scrollMult;

        float autoOffset;
        float width;
        float localY;

        Transform a;
        Transform b;

        public void InitItem(
            Transform parent,
            float yOffset,
            int order,
            Material _material = null
        )
        {
            width = sprite.bounds.size.x;
            localY = yOffset;

            a = CreatePiece(parent, order, _material);
            b = CreatePiece(parent, order, _material);
        }

        Transform CreatePiece(Transform parent, int order, Material _material = null)
        {
            GameObject go = new GameObject(sprite.name);
            go.transform.parent = parent;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.drawMode = SpriteDrawMode.Simple;

            if(_material != null)
            {
                sr.material = _material;
            }

            return go.transform;
        }

        public void UpdatePosition(
            Camera cam,
            float verticalOffset
        )
        {
            autoOffset += autoScroll * Time.deltaTime;

            float camHalfWidth = cam.orthographicSize * cam.aspect;
            float camLeft = cam.transform.position.x - camHalfWidth;

            float parallaxX =
                -cam.transform.position.x * scrollMult + autoOffset;

            float x = camLeft - width + Mathf.Repeat(parallaxX, width);

            float y =
                cam.transform.position.y
                - cam.orthographicSize
                + localY
                + verticalOffset;

            a.position = new Vector3(x, y, 0);
            b.position = new Vector3(x + width, y, 0);
        }
    }
    #endregion

    [SerializeField] Material spriteMaterial;
    [SerializeField] float paletteSwapFps = 12f;
    float paletteSwapTimer = 0f;
    [SerializeField] float startYOffset = -1f;
    [SerializeField] List<ParallaxItem> parallaxItems = new();
    Camera gameCam;
    float totalHeight;

    void Start()
    {
        gameCam = Camera.main;

        float curY = startYOffset;
        totalHeight = 0f;

        for (int i = 0; i < parallaxItems.Count; i++)
        {
            ParallaxItem item = parallaxItems[i];
            item.scrollMult = GetScrollMult(i);

            item.InitItem(
                transform,
                curY,
                -999 - i,
                spriteMaterial
            );

            curY += item.sprite.bounds.size.y;
            totalHeight += item.sprite.bounds.size.y;
        }
    }

    void LateUpdate()
    {
        paletteSwapTimer += Time.deltaTime;
        if(paletteSwapTimer >= 1f/paletteSwapFps)
        {
            paletteSwapTimer = 0f;
            float frames = spriteMaterial.GetFloat("_NumFrames");
            float paletteFrame = spriteMaterial.GetFloat("_Frame") - 1;

            if(paletteFrame < 1)
            {
                paletteFrame = frames - 1;
            }
            else if(paletteFrame > frames)
            {
                paletteFrame = 1;
            }

            spriteMaterial.SetFloat("_Frame", paletteFrame);
        }


        float camHeight = gameCam.orthographicSize * 2f;

        float maxOffset = Mathf.Max(0, totalHeight - camHeight);

        float desiredOffset =
            -gameCam.transform.position.y * verticalScrollMult;

        float verticalOffset =
            Mathf.Clamp(desiredOffset, -maxOffset, 0f);

        for (int i = 0; i < parallaxItems.Count; i++)
            parallaxItems[i].UpdatePosition(gameCam, verticalOffset);
    }

    float GetScrollMult(int index)
    {
        return index < GHZ_SCROLL_TABLE.Length
            ? GHZ_SCROLL_TABLE[index]
            : GHZ_SCROLL_TABLE[^1];
    }
}
