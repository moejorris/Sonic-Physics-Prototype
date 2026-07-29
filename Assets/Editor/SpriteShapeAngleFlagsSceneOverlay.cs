using UnityEditor;
using UnityEngine;
using System.Linq;

[InitializeOnLoad]
public static class SpriteShapeAngleFlagsSceneOverlay
{
    static SpriteShapeAngleFlagsSceneOverlay()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (Selection.activeGameObject == null)
            return;

        var flags = Selection.activeGameObject.GetComponent<Utils_SpriteShapeAngleFlags>();
        if (!flags || flags.editorSelectedPoints.Count == 0)
            return;

        Handles.BeginGUI();

        GUILayout.BeginArea(
            new Rect(10, sceneView.position.height - 250, 260, 180),
            GUI.skin.window
        );

        DrawPanel(flags);

        GUILayout.EndArea();
        Handles.EndGUI();
    }

    private static void DrawPanel(Utils_SpriteShapeAngleFlags flags)
    {
        var selected = flags.editorSelectedPoints
            .Where(i => i >= 0 && i < flags.points.Count)
            .ToList();

        if (selected.Count == 0)
            return;

        GUILayout.Label(
            $"Angle Flags ({selected.Count} point{(selected.Count > 1 ? "s" : "")} selected)",
            EditorStyles.boldLabel
        );

        DrawEdgeSection(
            "Incoming Edge",
            selected,
            flags,
            incoming: true
        );

        GUILayout.Space(6);

        DrawEdgeSection(
            "Outgoing Edge",
            selected,
            flags,
            incoming: false
        );
    }

    private static void DrawEdgeSection(
        string label,
        System.Collections.Generic.List<int> selected,
        Utils_SpriteShapeAngleFlags flags,
        bool incoming
    )
    {
        GUILayout.Label(label, EditorStyles.miniBoldLabel);

        var values = selected
            .Select(i => incoming ? flags.points[i].flagIncoming : flags.points[i].flagOutgoing)
            .Distinct()
            .ToList();

        bool mixed = values.Count > 1;
        bool flagValue = values.First();

        EditorGUI.showMixedValue = mixed;
        bool newFlag = EditorGUILayout.Toggle("Flag Angle", flagValue);
        EditorGUI.showMixedValue = false;

        if (newFlag != flagValue || mixed)
        {
            Undo.RecordObject(flags, "Toggle Angle Flag");
            foreach (int i in selected)
            {
                if (incoming)
                    flags.points[i].flagIncoming = newFlag;
                else
                    flags.points[i].flagOutgoing = newFlag;
            }
            EditorUtility.SetDirty(flags);
        }

        using (new EditorGUI.DisabledScope(!newFlag))
        {
            var biasValues = selected
                .Select(i => incoming ? flags.points[i].incomingBias : flags.points[i].outgoingBias)
                .Distinct()
                .ToList();

            bool biasMixed = biasValues.Count > 1;
            float biasValue = biasValues.First();

            EditorGUI.showMixedValue = biasMixed;
            float newBias = EditorGUILayout.Slider("Bias", biasValue, 0f, .5f);
            EditorGUI.showMixedValue = false;

            if (!Mathf.Approximately(newBias, biasValue) || biasMixed)
            {
                Undo.RecordObject(flags, "Change Angle Bias");
                foreach (int i in selected)
                {
                    if (incoming)
                        flags.points[i].incomingBias = newBias;
                    else
                        flags.points[i].outgoingBias = newBias;
                }
                EditorUtility.SetDirty(flags);
            }
        }
    }
}
