using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
[ExecuteAlways]

public class EdgeRenderer : MonoBehaviour
{
    Collider2D coll;
    Vector2[] points;
    private Material gizmoMaterial;
    private MaterialPropertyBlock propertyBlock;

    [Header("Overlay Settings")]
    public bool useOverlay = true;
    public Color rightSideTint = new Color(0, 0, 0, 0.5f); // Darker
    public Color leftSideTint = new Color(1, 1, 1, 0.2f);  // Brighter
    public float overlayThickness = 0.5f; // How far "inward" the tint goes
    [Serializable]
    public class EdgeSpriteData
    {
        public Sprite sprite;
        public Color spriteColor = new Color(1,1,1,1);

        [Tooltip("The direction of surface that the sprite appears on")]
        public Vector2 normal;
        public float normalTolerance = 0f;
        public Vector3 rotationOffset;
        [Tooltip("Should the sprite match the angle of the surface?")]
        public bool alignToSurfaceNormal;

        [Header("Shadow Settings")]
        public bool drawShadow = false;
        public bool alignShadowToSurfaceNormal = false;
        public Color shadowColor = new Color(0, 0, 0, 0.5f);
        public Vector3 shadowOffset = new Vector3(0, -1f);
        
        [HideInInspector] public Mesh cachedMesh;
        [HideInInspector] public MaterialPropertyBlock cachedMpB;
        public bool forceRegenMesh;
    }

    [SerializeField]
    List<EdgeSpriteData> edgeSprites = new List<EdgeSpriteData>();





    void UpdatePoints()
    {
        if(coll == null)
        {
            coll = GetComponent<Collider2D>();
        }
        if(coll is BoxCollider2D)
        {
            float halfWidth = (coll as BoxCollider2D).bounds.extents.x;
            float halfHeight = (coll as BoxCollider2D).bounds.extents.y;

            Vector2 upLeft = new Vector3(-halfWidth, halfHeight);
            Vector2 botLeft = new Vector3(-halfWidth, -halfHeight);
            points = new Vector2[]
            {
                upLeft,
                -botLeft,
                -upLeft,
                botLeft
            };
        }
        else if(coll is EdgeCollider2D)
        {
            points = (coll as EdgeCollider2D).points;
        }
        else if(coll is PolygonCollider2D)
        {
            
        }
        else if(coll is CircleCollider2D)
        {
            
        }
        else if(coll is CompositeCollider2D)
        {
            
        }
        else if(coll is TilemapCollider2D)
        {
            
        }
    }
    
    void Update()
    {

        points = new Vector2[]{};
        coll = null;
        if(points.Length == 0)
        {
            UpdatePoints();
        }
        if(edgeSprites.Count != 0)
        {
            DrawEdgeSprites();
        }
        DrawSideOverlay();
    }

private Mesh overlayMesh;
private Vector3[] meshVertices;
private Color[] meshColors;
private int[] meshTriangles;

void DrawSideOverlay()
{
    if (!useOverlay || points == null || points.Length < 3) return;

    if (overlayMesh == null) overlayMesh = new Mesh();
    
    int count = points.Length;
    
    // 1. DETERMINE WINDING ORDER
    // We calculate the signed area to see if the points are CW or CCW
    float area = 0;
    for (int i = 0; i < count; i++)
    {
        Vector2 p1 = points[i];
        Vector2 p2 = points[(i + 1) % count];
        area += (p2.x - p1.x) * (p2.y + p1.y);
    }
    // if area > 0, it's CW. If area < 0, it's CCW.
    float windingSign = (area > 0) ? 1f : -1f;

    if (meshVertices == null || meshVertices.Length != count * 2)
    {
        meshVertices = new Vector3[count * 2];
        meshColors = new Color[count * 2];
        meshTriangles = new int[count * 6];
        overlayMesh.Clear(); 
    }

    for (int i = 0; i < count; i++)
    {
        Vector2 pCurrent = points[i];
        Vector2 pPrev = points[(i + count - 1) % count];
        Vector2 pNext = points[(i + 1) % count];

        Vector2 dirIn = (pCurrent - pPrev).normalized;
        Vector2 dirOut = (pNext - pCurrent).normalized;
        
        // 2. APPLY WINDING SIGN 
        // This ensures "Inward" is always toward the collider's center
        Vector2 nIn = new Vector2(-dirIn.y, dirIn.x) * windingSign;
        Vector2 nOut = new Vector2(-dirOut.y, dirOut.x) * windingSign;

        Vector2 miter = (nIn + nOut).normalized;
        if (miter.sqrMagnitude < 0.01f) miter = nIn;

        float dot = Vector2.Dot(miter, nIn);
        float angleCorrection = Mathf.Clamp((dot > 0.01f) ? (1f / dot) : 1f, 1f, 2f);
        float actualThickness = overlayThickness * angleCorrection;

        // 3. COLOR BASED ON WORLD NORMAL
        // We use nIn (the edge normal) to check if it's a "left" or "right" face
        Color edgeColor = Color.clear;
        if (nIn.x > 0.1f) edgeColor = rightSideTint;
        else if (nIn.x < -0.1f) edgeColor = leftSideTint;
        
        Color innerColor = new Color(edgeColor.r, edgeColor.g, edgeColor.b, 0);

        int vIdx = i * 2;
        meshVertices[vIdx] = pCurrent; 
        meshVertices[vIdx + 1] = pCurrent - (miter * actualThickness);

        meshColors[vIdx] = edgeColor;
        meshColors[vIdx + 1] = innerColor;

        int tIdx = i * 6;
        int nextV = ((i + 1) % count) * 2;

        meshTriangles[tIdx] = vIdx;
        meshTriangles[tIdx + 1] = vIdx + 1;
        meshTriangles[tIdx + 2] = nextV;
        meshTriangles[tIdx + 3] = vIdx + 1;
        meshTriangles[tIdx + 4] = nextV + 1;
        meshTriangles[tIdx + 5] = nextV;
    }

    overlayMesh.vertices = meshVertices;
    overlayMesh.colors = meshColors;
    overlayMesh.triangles = meshTriangles;
    overlayMesh.RecalculateBounds();

    if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
    propertyBlock.SetTexture("_MainTex", Texture2D.whiteTexture);
    propertyBlock.SetColor("_Color", Color.white);

    Graphics.DrawMesh(overlayMesh, transform.position, transform.rotation, gizmoMaterial, 0, null, 0, propertyBlock);
}
    private Mesh CreateSpriteMesh(Sprite sprite)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[sprite.vertices.Length];

        // 1. Get the pivot in units (relative to bottom-left of the sprite rect)
        Vector2 pivotUnits = sprite.pivot / sprite.pixelsPerUnit;
        
        // 2. Get the sprite's layout rect (where the pixels actually are)
        // sprite.rect.size / PPU gives us the width/height in Unity units
        Vector2 rectSizeUnits = sprite.rect.size / sprite.pixelsPerUnit;
        
        // 3. Calculate the offset from the pivot to the geometric center
        // We subtract the pivot from the center point (half-width, half-height)
        Vector2 pivotOffset = new Vector2(rectSizeUnits.x / 2f, rectSizeUnits.y / 2f) - pivotUnits;

        for (int i = 0; i < vertices.Length; i++) 
        {
            // 4. Shift the raw vertex (which is relative to center) by the pivot offset
            vertices[i] = new Vector3(
                sprite.vertices[i].x + pivotOffset.x, 
                sprite.vertices[i].y + pivotOffset.y, 
                0);
        }

        mesh.vertices = vertices;
        mesh.uv = sprite.uv;
        mesh.triangles = Array.ConvertAll(sprite.triangles, t => (int)t);

        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++) {
            normals[i] = Vector3.back;
        }
        mesh.normals = normals;

        return mesh;
    }

    void DrawEdgeSprites()
    {
        for(int i = 0; i < edgeSprites.Count; i++)
        {
            if(gizmoMaterial == null)
            gizmoMaterial = new Material(Shader.Find("Sprites/Default"));
            
            if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

            EdgeSpriteData data = edgeSprites[i];
            if(data.sprite == null) continue;

            float worldWidth = data.sprite.bounds.size.x;

            // Caching the mesh to prevent memory leaks and lag
            if (data.cachedMesh == null || data.forceRegenMesh) data.cachedMesh = CreateSpriteMesh(data.sprite);

            for(int p = 0; p < points.Length; p++)
            {
                int nextPoint = p == points.Length - 1 ? 0 : p + 1;
                Vector2 p1 = points[p];
                Vector2 p2 = points[nextPoint];

                Vector2 normal = (points[nextPoint] - points[p]).CCTangentSafe();
                
                if(Vector2.Dot(normal, data.normal.normalized) < 1f - data.normalTolerance)
                continue;

                Quaternion surfaceRotation = Quaternion.LookRotation(Vector3.forward + data.rotationOffset, normal);
                Quaternion rotation = Quaternion.Euler(data.rotationOffset + transform.eulerAngles);
                float segmentLength = Vector2.Distance(p1, p2);

                for(float d = 0; d < segmentLength; d += worldWidth)
                {
                    Vector3 position = Vector3.Lerp(p1, p2, d/segmentLength) + transform.position;

                    // gizmoMaterial.mainTexture = data.sprite.texture;
                    gizmoMaterial.mainTexture = null;

                    if (data.drawShadow)
                    {
                        // Set color in the property block instead of the material
                        propertyBlock.SetColor("_Color", data.shadowColor);
                        propertyBlock.SetTexture("_MainTex", data.sprite.texture);                  
                        Vector3 sPos = position + data.shadowOffset;
                        
                        Graphics.DrawMesh(data.cachedMesh, sPos, data.alignShadowToSurfaceNormal ? surfaceRotation : rotation, gizmoMaterial, 0, null, 0, propertyBlock);
                    }
                    
                    // Draw using the material
                    propertyBlock.SetColor("_Color", data.spriteColor);  
                    propertyBlock.SetTexture("_MainTex", data.sprite.texture);                  
                    Graphics.DrawMesh(data.cachedMesh, position, data.alignToSurfaceNormal ? surfaceRotation : rotation, gizmoMaterial, 0, null, 0, propertyBlock);                
                }
            }
        }
    }

    public float GetYFromX(Vector2 p1, Vector2 p2, float x)
    {
        // 1. Calculate the percentage (t) of how far x is between p1.x and p2.x
        float t = (x - p1.x) / (p2.x - p1.x);

        // 2. Use that same percentage to find the y value
        // Mathf.Lerp handles the y1 + t * (y2 - y1) math for you
        return Mathf.Lerp(p1.y, p2.y, t);
    }
}
