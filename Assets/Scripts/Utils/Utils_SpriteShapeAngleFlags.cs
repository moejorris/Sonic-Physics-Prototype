using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteAlways]
public class Utils_SpriteShapeAngleFlags : MonoBehaviour
{
    public SpriteShapeController controller;

    [Serializable]
    public class PointFlags
    {
        public bool flagIncoming;
        [Range(0f, 0.5f)]
        public float incomingBias;

        public bool flagOutgoing;
        [Range(0f, 0.5f)]
        public float outgoingBias;

        Vector2 startPoint = Vector2.zero;
        Vector2 endPoint = Vector2.zero;
    }

    public List<PointFlags> points = new List<PointFlags>();

    #if UNITY_EDITOR
    [HideInInspector]
    public HashSet<int> editorSelectedPoints = new();
    #endif


    void OnValidate()
    {
        controller = GetComponent<SpriteShapeController>();
        SyncPointCount();
    }

    public void SyncPointCount()
    {
        if (!controller)
            return;

        var spline = controller.spline;
        int count = spline.GetPointCount();

        while (points.Count < count)
            points.Add(new PointFlags());

        while (points.Count > count)
            points.RemoveAt(points.Count - 1);
    }

    public bool IsPointFlagged(Vector2 worldSpacePoint)
    {
        return false;
        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();

        if(edgeCollider == null) return false;

        Vector2 pointAlongCollider = edgeCollider.ClosestPoint(worldSpacePoint);

        int[] pointIndices = GetTwoNearestPointsIndex(pointAlongCollider);
        
        Vector2[] colliderPoints = edgeCollider.points;

        if(colliderPoints.Length == 0 || pointIndices.Length == 0 || pointIndices[0] == -1) return false;

        int firstPoint = pointIndices[0] > pointIndices[1] ? pointIndices[1] : pointIndices[0];
        int secondPoint = firstPoint == pointIndices[1] ? pointIndices[0] : pointIndices[1];

        Vector2 diff = colliderPoints[secondPoint] - colliderPoints[firstPoint];

        Vector2 alpha = pointAlongCollider/diff;

        bool isIncoming = alpha.magnitude > 0.5f;

        bool isFlagged = false;

        if(isIncoming)
        {
            isFlagged = alpha.magnitude >= points[secondPoint].incomingBias;
        }
        else
        {
            isFlagged = alpha.magnitude <= points[firstPoint].outgoingBias;
        }


        return isFlagged;
    }

    int[] GetTwoNearestPointsIndex(Vector2 worldSpacePoint) //Returns the first and second closest points, with closest being [0] and second closest being [1]
    {
        int closest = 0;
        int secondClosest = 1;
        float dist = 1000f;

        Vector2[] colliderPoints = GetComponent<EdgeCollider2D>().points;

        if(colliderPoints.Length == 0) return new int[] {-1};

        for(int i = 0; i < points.Count; i++)
        {
            float curDist = Vector2.Distance(colliderPoints[i], worldSpacePoint);
            if(curDist < dist)
            {
                dist = curDist;
                secondClosest = closest;
                closest = i;

            }
        }

        return new int[] {closest,secondClosest};
    }

    int GetClosestPointIndex(Vector2 worldSpacePoint)
    {
        EdgeCollider2D coll = GetComponent<EdgeCollider2D>();
        Vector2 closestPoint = coll.ClosestPoint(worldSpacePoint);

        float dist = 100f;
        float checkDist = 0;
        Vector2[] points = coll.points;
        int index = 0;
        for(int i = 0; i < points.Length; i++)
        {
            checkDist = Vector2.Distance(closestPoint, transform.TransformPoint(points[i]));
            if(checkDist < dist)
            {
                dist = checkDist;
                index = i;
            }
        }

        return index;
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        
    foreach (int i in editorSelectedPoints)
    {
        Vector3 pos = transform.TransformPoint(controller.spline.GetPosition(i));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, 0.25f);
    }


        if (!controller) return;

        EdgeCollider2D edge = controller.GetComponent<EdgeCollider2D>();
        if (!edge || edge.points.Length < 2) return;

        Vector3[] colliderWorldPoints = new Vector3[edge.points.Length];
        for (int i = 0; i < edge.points.Length; i++)
            colliderWorldPoints[i] = transform.TransformPoint(edge.points[i]);

        var spline = controller.spline;
        int splineCount = spline.GetPointCount();

        for (int i = 0; i < points.Count; i++)
        {
            var pointData = points[i];

            Vector3 splinePos = transform.TransformPoint(spline.GetPosition(i));

            // Handle incoming edge (previous spline point → current)
            if (pointData.flagIncoming && i > 0)
            {
                Vector3 prevSplinePos = transform.TransformPoint(spline.GetPosition(i - 1));
                DrawFlaggedColliderSegment(splinePos, prevSplinePos, pointData.incomingBias, true);
            }

            // Handle outgoing edge (current → next spline point)
            if (pointData.flagOutgoing && i < splineCount - 1)
            {
                Vector3 nextSplinePos = transform.TransformPoint(spline.GetPosition(i + 1));
                DrawFlaggedColliderSegment(splinePos, nextSplinePos, pointData.outgoingBias, false);
            }
        }
        
    }

    /// <summary>
    /// Draws the flagged portion of collider points between two spline points
    /// </summary>
    private void DrawFlaggedColliderSegment(Vector3 start, Vector3 end, float bias, bool reverse = false)
    {
        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
        Vector2[] colliderPoints = edgeCollider.points;
        if (colliderPoints.Length < 2) return;

        int startPointIndex = GetClosestPointIndex(start);
        int endPointIndex = GetClosestPointIndex(end);

        List<Vector2> pointsToCheck = new List<Vector2>();
        int loopStart = startPointIndex > endPointIndex ? endPointIndex : startPointIndex;
        int loopEnd = loopStart == startPointIndex ? endPointIndex : startPointIndex;

        if(reverse)
        {
            loopStart += Mathf.Min(loopEnd - 1, Mathf.RoundToInt((loopEnd - loopStart) * (1f-bias)));
        }
        else
        {
            loopEnd = Mathf.Max(loopStart + 1, loopStart + Mathf.RoundToInt((loopEnd - loopStart) * bias));
        }



        Color color = reverse ? 0.75f * Color.red + Color.white * 0.25f : Color.red;

        for(int i = loopStart; i <= loopEnd; i++)
        {
            Vector2 worldPoint = transform.TransformPoint(edgeCollider.points[i]);
            Vector2 nextPoint = transform.TransformPoint(edgeCollider.points[i + 1]);

            if(i < loopEnd)
            {
                DrawEdge(worldPoint, nextPoint, 0.1f, color);
                Vector2 norm = (worldPoint - nextPoint).TangentSafe().OneAxis();

                Gizmos.DrawRay((worldPoint + nextPoint) * 0.5f, norm);
            }
            pointsToCheck.Add(worldPoint);
        }

        return;
    }

    /// <summary>
    /// Draw a thick cube along a linear segment
    /// </summary>
    private void DrawEdge(Vector3 point1, Vector3 point2, float thickness, Color color)
    {
        Matrix4x4 matrix = Gizmos.matrix;
        Vector2 tangent = (point2 - point1);
        tangent = tangent.TangentSafe();

        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, tangent);
        Vector3 center = (point1 + point2) / 2f;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

        Vector3 size = new Vector3(Vector3.Distance(point1, point2), thickness, 0.01f);

        Gizmos.color = color;
        Gizmos.DrawCube(Vector3.zero, size);

        Gizmos.matrix = matrix;
    }
    #endif
}
