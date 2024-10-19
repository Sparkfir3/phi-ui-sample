using Sparkfire.UI;
using UnityEditor;
using UnityEngine;

namespace Sparkfire.Utility
{
    public static class CreateCustomGameObjects
    {
        [MenuItem("GameObject/UI/Parallelogram Image")]
        public static void CreateParallelogramImageObject(MenuCommand menuCommand)
        {
            GameObject gameObject = new GameObject("Parallelogram Image");
            gameObject.AddComponent<RectTransform>();
            gameObject.AddComponent<CanvasRenderer>();
            gameObject.AddComponent<ParallelogramImage>();

            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
            Selection.activeObject = gameObject;
        }
    }
}
