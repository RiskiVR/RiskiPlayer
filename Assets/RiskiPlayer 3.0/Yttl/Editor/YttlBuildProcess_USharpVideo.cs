using System.Linq;
using UdonSharp.Video;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yttl.Udon;

namespace Yttl.Editor
{
    public class YttlBuildProcess_USharpVideo : IProcessSceneWithReport
    {
        public int callbackOrder => default;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var rootGameObjects = scene.GetRootGameObjects();

            var connectors = rootGameObjects
                  .SelectMany(x => x.GetComponentsInChildren<YttlConnector_USharpVideo>(true))
                  .Where(x => !IsEditorOnly(x.transform));

            var videoPlayer = rootGameObjects
                  .SelectMany(x => x.GetComponentsInChildren<USharpVideoPlayer>(true))
                  .FirstOrDefault(x => !IsEditorOnly(x.transform));

            foreach (var connector in connectors)
            {
                if (connector.videoPlayer == null)
                {
                    connector.videoPlayer = videoPlayer;
                }
            }
        }

        private static bool IsEditorOnly(Transform t)
        {
            for (; t; t = t.parent)
                if (t.gameObject.CompareTag("EditorOnly"))
                    return true;

            return false;
        }
    }
}
