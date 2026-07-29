using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

[InitializeOnLoad]
public static class SpriteShapePointSelectionTracker
{
    static Vector2 dragStart;
    static Vector2 dragEnd;
    static bool dragging;

    static Utils_SpriteShapeAngleFlags flags;

    static SpriteShapePointSelectionTracker()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (Selection.activeGameObject == null)
            return;
        

        var cur_flags = Selection.activeGameObject.GetComponent<Utils_SpriteShapeAngleFlags>();
        if(cur_flags != flags && cur_flags != null && flags != null)
        {
            flags.editorSelectedPoints.Clear();
        }
        flags = cur_flags;
        
        if (!flags || !flags.controller || flags.controller.spline == null)
            return;

        Event e = Event.current;

        HandleMouseInput(sceneView, flags, e);
        DrawSelectionRect(sceneView);
    }

    private static void HandleMouseInput(SceneView sceneView, Utils_SpriteShapeAngleFlags flags, Event e)
    {
        bool shift = e.shift;
        bool ctrl = e.control || e.command;
        bool boxSelectionActive = shift;

        if(!boxSelectionActive) return;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            dragStart = GetGUIMousePosition(e);
            dragEnd   = dragStart;
            dragging  = false;

            if (HandleUtility.nearestControl != 0)
                return;

            if (Tools.current != Tool.None)
                return;
        }


        if (e.type == EventType.MouseDrag && e.button == 0)
        {
            Vector2 mousePos = GetGUIMousePosition(e);

            // Only begin dragging after moving a bit
            if (!dragging && Vector2.Distance(mousePos, dragStart) > 4f)
                dragging = true;

            if (dragging)
            {
                dragEnd = GetGUIMousePosition(e);
                e.Use();
                sceneView.Repaint();
            }
        }


        if (e.type == EventType.MouseUp && e.button == 0)
        {
            if (dragging)
            {
                dragging = false;

                Rect rect = GetScreenRect(dragStart, dragEnd);
                SelectPointsInRect(sceneView, flags, rect, e.shift, e.control || e.command);

                e.Use();
                sceneView.Repaint();
            }
        }

    }

    private static void SelectPointsInRect(
        SceneView sceneView,
        Utils_SpriteShapeAngleFlags flags,
        Rect rect,
        bool shift,
        bool ctrl
    )
    {
        if (!shift && !ctrl)
            flags.editorSelectedPoints.Clear();

        var spline = flags.controller.spline;
        Transform t = flags.controller.transform;

        for (int i = 0; i < spline.GetPointCount(); i++)
        {
            Vector3 world = t.TransformPoint(spline.GetPosition(i));
            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(world);

            if (!rect.Contains(guiPoint))
                continue;

            if (ctrl)
            {
                if (!flags.editorSelectedPoints.Add(i))
                    flags.editorSelectedPoints.Remove(i);
            }
            else
            {
                flags.editorSelectedPoints.Add(i);
            }
        }

        EditorUtility.SetDirty(flags);
    }

    private static void DrawSelectionRect(SceneView sceneView)
    {
        if (!dragging)
            return;

        Rect rect = GetScreenRect(dragStart, dragEnd);

        Handles.BeginGUI();
        Color fill = new Color(0.3f, 0.6f, 1f, 0.15f);
        Color outline = new Color(0.3f, 0.6f, 1f, 0.8f);

        EditorGUI.DrawRect(rect, fill);
        Handles.DrawSolidRectangleWithOutline(rect, Color.clear, outline);
        Handles.EndGUI();
    }

    private static Rect GetScreenRect(Vector2 p1, Vector2 p2)
    {
        float xMin = Mathf.Min(p1.x, p2.x);
        float xMax = Mathf.Max(p1.x, p2.x);
        float yMin = Mathf.Min(p1.y, p2.y);
        float yMax = Mathf.Max(p1.y, p2.y);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static Vector2 GetGUIMousePosition(Event e)
    {
        return e.mousePosition;
    }
}
