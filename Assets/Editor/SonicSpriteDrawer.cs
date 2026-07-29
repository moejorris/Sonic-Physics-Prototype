using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SonicSprite))]
public class SonicSpriteDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            GetLabel(property)
        );

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float y = position.y + EditorGUIUtility.singleLineHeight + 2;

            DrawField(ref y, position, property, "animName");
            DrawField(ref y, position, property, "shouldLoop");
            if(property.FindPropertyRelative("shouldLoop").boolValue == true)
            {
                DrawField(ref y, position, property, "loopTimes");
            }
            DrawField(ref y, position, property, "subImage_duration");
            DrawField(ref y, position, property, "variableByGroundSpeed");
            DrawField(ref y, position, property, "sprites");
            DrawField(ref y, position, property, "superSprites");

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    string GetLabel(SerializedProperty property)
    {
        var nameProp = property.FindPropertyRelative("animName");
        return string.IsNullOrEmpty(nameProp.stringValue)
            ? "New Animation"
            : nameProp.stringValue;
    }

    void DrawField(ref float y, Rect position, SerializedProperty property, string name)
    {
        var prop = property.FindPropertyRelative(name);
        float height = EditorGUI.GetPropertyHeight(prop, true);

        EditorGUI.PropertyField(
            new Rect(position.x, y, position.width, height),
            prop,
            true
        );

        y += height + 2;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded)
        {
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("animName"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("shouldLoop"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("loopTimes"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("subImage_duration"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("variableByGroundSpeed"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("sprites"), true);
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("superSprites"), true);
        }

        return height + 6;
    }
}
