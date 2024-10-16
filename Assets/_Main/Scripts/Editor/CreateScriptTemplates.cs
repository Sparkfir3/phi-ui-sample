using UnityEditor;

namespace Sparkfire.Utility
{
    public class CreateScriptTemplates
    {
        private const string FILE_PATH = "Assets/Library/Editor/ScriptTemplates/";
        
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
    }
}
