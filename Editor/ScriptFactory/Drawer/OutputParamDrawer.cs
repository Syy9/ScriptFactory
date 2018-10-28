using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptFactory
{
    [CustomPropertyDrawer(typeof(OutputParam))]
    public class OutputParamDrawer : PropertyDrawer
    {
        public static float BaseHeight
        {
            get
            {
                return (EditorGUIUtility.singleLineHeight + 2) * 2 + 5;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUIUtility.labelWidth = 120;
            position.height = EditorGUIUtility.singleLineHeight;
            var classRect = new Rect(position);

            int toggleWidth = 15;
            var toggleRect = new Rect(classRect)
            {
                y = classRect.y + EditorGUIUtility.singleLineHeight + 2,
                width = toggleWidth,
            };

            var fileRect = new Rect(toggleRect)
            {
                x = position.x + toggleRect.width,
                width = position.width - toggleRect.width,
            };

            var className = property.FindPropertyRelative("ClassName");
            var fileName = property.FindPropertyRelative("FileName");
            var isOverride = property.FindPropertyRelative("IsOverride");
            className.stringValue = EditorGUI.TextField(classRect, className.displayName, className.stringValue);

            isOverride.boolValue = EditorGUI.ToggleLeft(toggleRect, "", isOverride.boolValue);
            using (new EditorGUI.DisabledScope(!isOverride.boolValue))
            {
                if (!isOverride.boolValue)
                {
                    fileName.stringValue = className.stringValue;
                }
                EditorGUIUtility.labelWidth -= toggleWidth + 2;
                fileName.stringValue = EditorGUI.TextField(fileRect, "File Name", fileName.stringValue);
                EditorGUIUtility.labelWidth += toggleWidth + 2;
            }
        }
    }
}
