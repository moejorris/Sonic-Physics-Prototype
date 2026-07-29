using UnityEngine;
using UnityEngine.U2D;

public class Utils_SpriteShapeColliderCorrection : MonoBehaviour
{

    //Simple Utility script that corrects spline point to collider point calculation errors caused by Unity. 
    // This ensures everything is aligned to where they should be on the grid, rather than having very small diagonals between points.
    void Awake()
    {
        foreach(SpriteShapeController ssc in FindObjectsByType<SpriteShapeController>())
        {
            EdgeCollider2D collider = ssc.GetComponent<EdgeCollider2D>();
            if(collider != null)
            {
                ssc.autoUpdateCollider = false;
                collider.enabled = false;

                Vector2[] points = collider.points;

                for(int i = 0; i < points.Length; i++)
                {
                    points[i].x = Mathf.Round(points[i].x * 10f) / 10f;
                    points[i].y = Mathf.Round(points[i].y * 10f) / 10f;

                }
                collider.points = points;
                collider.enabled = true;
            }
        }
    }
}
