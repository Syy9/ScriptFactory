using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptFactory
{
    [CustomPropertyDrawer(typeof(Param))]
    public class ParamDrawer : PropertyDrawer
    {
        public static float Height {
            get {
                return (EditorGUIUtility.singleLineHeight + 2) * 2 + 5;
            }
        }

        Param param;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUIUtility.labelWidth = 120;
            position.height = EditorGUIUtility.singleLineHeight;
            var keyRect = new Rect(position);

            var valueRect = new Rect(keyRect)
            {
                y = keyRect.y + EditorGUIUtility.singleLineHeight + 2
            };

            var key = property.FindPropertyRelative("Key");
            var value = property.FindPropertyRelative("Value");
            key.stringValue = EditorGUI.TextField(keyRect, key.displayName, key.stringValue);
            value.stringValue = EditorGUI.TextField(valueRect, value.displayName, value.stringValue);
        }
    }

}
