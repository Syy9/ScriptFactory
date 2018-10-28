using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptFactory
{
    [CustomEditor(typeof(Template))]
    public class TemplateEditor : Editor
    {
        Template template;
        DefaultAsset outputFolder;
        SerializedObject editorSerializedObject;

        ReorderableList paramList;
        ReorderableList outputParamList;
        [SerializeField] List<OutputParam> rawOutputParamList = new List<OutputParam>() { new OutputParam() };
        
        void OnEnable()
        {
            template = (Template) target;
            outputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(template.OutputPath);
            editorSerializedObject = new SerializedObject(this);
            var outputParamListProperty = editorSerializedObject.FindProperty("rawOutputParamList");

            var paramListProperty = serializedObject.FindProperty("ParamList");
            paramList = new ReorderableList(serializedObject, paramListProperty);
            paramList.elementHeight = ParamDrawer.Height;
            paramList.drawElementCallback = (rect, index, isActive, isFocused) => {
                var property = paramListProperty.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                EditorGUI.PropertyField(rect, property);
            };

            paramList.drawHeaderCallback = (rect) => {
                EditorGUI.LabelField(rect, "Custom Param");
            };

            paramList.onAddCallback = (list) => {
                paramListProperty.arraySize++;
                list.index = paramListProperty.arraySize -1;
                var property = paramListProperty.GetArrayElementAtIndex(list.index);
                var key = property.FindPropertyRelative("Key");
                key.stringValue = "#Key#";
            };

            outputParamList = new ReorderableList(rawOutputParamList, typeof(OutputParam));
            outputParamList.elementHeight = ParamDrawer.Height;
            outputParamList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var property = outputParamListProperty.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                EditorGUI.PropertyField(rect, property);
            };

            outputParamList.drawHeaderCallback = (rect) => {
                EditorGUI.LabelField(rect, "Output Param");
            };

            outputParamList.onAddCallback = (list) => {
                outputParamListProperty.arraySize++;
                list.index = outputParamListProperty.arraySize -1;
            };
        }

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("■Output Folder", outputFolder, typeof(DefaultAsset), false);
                if (check.changed)
                {
                    var path = AssetDatabase.GetAssetPath(outputFolder);
                    if (AssetDatabase.IsValidFolder(path))
                        template.OutputPath = path;
                }
            }

            EditorGUILayout.Space();
            template.NamespaceParam.Value = EditorGUILayout.TextField(TemplateKey.Namespace, template.NamespaceParam.Value);
            template.SuperClassNameParam.Value = EditorGUILayout.TextField(TemplateKey.SuperClass, template.SuperClassNameParam.Value);

            EditorGUILayout.Space();
            serializedObject.Update();
            paramList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            using(new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("■Format", GUILayout.Width(60));
                if(GUILayout.Button("Reset", EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    template.Format = Template.DefaultFormat;
                }
            }
            
            template.Format = EditorGUILayout.TextArea(template.Format, GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 10));

            EditorGUILayout.Space();
            editorSerializedObject.Update();
            outputParamList.DoLayoutList();
            editorSerializedObject.ApplyModifiedProperties();

            foreach (var outputParam in rawOutputParamList)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(BuildFilePath(outputParam));
                using (new EditorGUI.DisabledScope(true))
                {
                    var content = BuildContent(outputParam);
                    EditorGUILayout.TextArea(content, GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * (Length(content) - 2)));
                }
            }

            EditorGUILayout.Space();
            using( new EditorGUI.DisabledScope(!EnableCreate()))
            {
                if (GUILayout.Button("Create"))
                {
                    int current = 1;
                    int max = rawOutputParamList.Count;
                    foreach (var outputParam in rawOutputParamList)
                    {
                        var path = BuildFilePath(outputParam);
                        if(System.IO.File.Exists((path)))
                        {
                            if(!EditorUtility.DisplayDialog($"Script Factory ({current}/{max})", $"Already file exist. Replace ?\n{path}", "Replace", "Skip"))
                            {
                                continue;
                            }
                        }
                        File.WriteAllText(path, BuildContent(outputParam), Encoding.UTF8);
                    }
                    AssetDatabase.Refresh();
                }
            }
        }

        bool EnableCreate()
        {
            return !string.IsNullOrEmpty(template.NamespaceParam.Value) && 
                !string.IsNullOrEmpty(template.OutputPath) &&
                !rawOutputParamList.Any(p => string.IsNullOrEmpty(p.ClassName)) &&
                rawOutputParamList.Count >= 1;
        }

        string BuildContent(OutputParam outputParam)
        {
            var contents = new StringBuilder(template.Format);
            contents = contents.Replace(template.NamespaceParam.Key, template.NamespaceParam.Value);
            contents = contents.Replace(TemplateKey.Class, outputParam.ClassName);
            contents = contents.Replace(template.SuperClassNameParam.Key, template.SuperClassNameParam.Value);

            foreach (var param in template.ParamList.Where(param => param.IsValid()))
            {
                contents = contents.Replace(param.Key, param.Value);
            }

            return contents.ToString();
        }

        string BuildFilePath(OutputParam outputParam)
        {
            return $"{template.OutputPath}/{outputParam.FileName}.cs";
        }

        private int Length(string content)
        {
            int line = 0;
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '\n') line++;
            }
            return line + 1;
        }
    }

    [System.Serializable]
    public class OutputParam
    {
        public string ClassName = "Template";
        public string FileName;
        public bool IsOverride;
        public List<Param> ParamList = new List<Param>() { new Param() };
    }
}
