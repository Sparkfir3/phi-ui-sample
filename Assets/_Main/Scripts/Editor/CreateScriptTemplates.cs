using UnityEditor;

namespace Sparkfire.Utility
{
    public static class CreateScriptTemplates
    {
        private const string FILE_PATH = "Assets/_Main/Scripts/Editor/ScriptTemplates/";
        
        [MenuItem("Assets/Create/Script/MonoBehaviour", priority = 40)]
        public static void CreateMonoBehaviour()
        {
            string path = FILE_PATH + "MonoBehaviour.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewMonoBehaviour.cs");
        }
        
        [MenuItem("Assets/Create/Script/ScriptableObject", priority = 40)]
        public static void CreateScriptableObject()
        {
            string path = FILE_PATH + "ScriptableObject.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewScriptableObject.cs");
        }

        [MenuItem("Assets/Create/Script/StaticClass", priority = 40)]
        public static void CreateStaticClass()
        {
            string path = FILE_PATH + "StaticClass.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewStaticClass.cs");
        }

        [MenuItem("Assets/Create/Script/MonoBehaviourEditor", priority = 50)]
        public static void CreateMonoBehaviourEditor()
        {
            string path = FILE_PATH + "MonoBehaviourEditor.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewMonoBehaviourEditor.cs");
        }
    }
}
