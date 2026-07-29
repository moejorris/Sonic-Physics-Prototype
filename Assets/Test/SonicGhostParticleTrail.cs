using UnityEngine;
using System.Collections.Generic;

public class SonicGhostParticleTrail : MonoBehaviour
{
    [Header("Settings")]
    public SpriteRenderer targetRenderer;
    public Material afterImageMaterial;
    public Gradient colorOverLifetime;
    public float lifetime = 0.5f;
    public float spawnSpeed = 1.0f; // Minimum distance to travel before spawning

    private List<GhostFrame> ghosts = new List<GhostFrame>();
    private Dictionary<Sprite, Mesh> meshCache = new Dictionary<Sprite, Mesh>();

    struct GhostFrame {
        public Matrix4x4 matrix;
        public Sprite sprite;
        public float spawnTime;
    }

    PlayerMovement playerMovement;

    void Start() {
        playerMovement = GetComponentInParent<PlayerMovement>();
        spawnSpeed = playerMovement.movementStats.dash_speed;
    }

    void FixedUpdate()
    {
        // 1. Check Speed
        
        if (Mathf.Abs(playerMovement.GroundSpeed) >= spawnSpeed) {
            SpawnGhost();
        }

    }

    void LateUpdate() {

        // 2. Render and Cleanup
        for (int i = ghosts.Count - 1; i >= 0; i--) {
            float age = Time.time - ghosts[i].spawnTime;
            if (age > lifetime) {
                ghosts.RemoveAt(i);
                continue;
            }
            RenderGhost(ghosts[i], age / lifetime);
        }

    }

    void SpawnGhost() {
        // Handle Flip X/Y by modifying the local scale in the matrix
        Vector3 scale = targetRenderer.transform.lossyScale;
        if (targetRenderer.flipX) scale.x *= -1;
        if (targetRenderer.flipY) scale.y *= -1;

        Matrix4x4 matrix = Matrix4x4.TRS(targetRenderer.transform.position, targetRenderer.transform.rotation, scale);

        ghosts.Add(new GhostFrame {
            matrix = matrix,
            sprite = targetRenderer.sprite,
            spawnTime = Time.time
        });
    }

    void RenderGhost(GhostFrame ghost, float age) {
        if (!meshCache.TryGetValue(ghost.sprite, out Mesh mesh)) {
            mesh = CreateMeshFromSprite(ghost.sprite);
            meshCache.Add(ghost.sprite, mesh);
        }

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetTexture("_MainTex", ghost.sprite.texture);
        // Ensure shader uses "_Color" or adjust to "_RendererColor" for URP
        mpb.SetColor("_Color", colorOverLifetime.Evaluate(age));

        Graphics.DrawMesh(mesh, ghost.matrix, afterImageMaterial, 0, null, 0, mpb);
    }

    Mesh CreateMeshFromSprite(Sprite s) {
        Mesh mesh = new Mesh();
        
        // Offset vertices by pivot to ensure rotation happens around the correct center
        Vector2 pivot = s.pivot / s.pixelsPerUnit;
        Vector3[] vertices = System.Array.ConvertAll(s.vertices, v => new Vector3(v.x, v.y, 0));
        
        mesh.vertices = vertices;
        mesh.uv = s.uv;
        mesh.triangles = System.Array.ConvertAll(s.triangles, t => (int)t);
        mesh.RecalculateBounds();
        return mesh;
    }
}
