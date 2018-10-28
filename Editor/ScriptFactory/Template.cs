using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptFactory
{
    [CreateAssetMenu(menuName = "Script Factory Template")]
    public class Template : ScriptableObject
    {
        public string OutputPath = "Assets";
        public Param NamespaceParam = new Param(TemplateKey.Namespace, "Template");
        public Param SuperClassNameParam = new Param(TemplateKey.SuperClass, "SuperClass");
        public List<Param> ParamList = new List<Param>();

        public string Format = DefaultFormat;
        public const string DefaultFormat =
@"using System;
using System.Collections;
using System.Collections.Generic;

namespace #namespace#
{
        public class #class# : #super class#
        {
            public #class#()
            {
                
            }
}";
    }

    [System.Serializable]
    public class Param
    {
        public string Key = "#Key#";
        public string Value;

        public Param() { }
        public Param(string key, string value = null)
        {
            Key = key;
            Value = value;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Key) && !string.IsNullOrEmpty(Value);
        }
    }

    public static class TemplateKey
    {
        public static readonly string Namespace = "#namespace#";
        public static readonly string Class = "#class#";
        public static readonly string SuperClass = "#super class#";
    }
}
