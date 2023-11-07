
using TMPro;
using UdonSharp;
using UdonSharp.Video;
using UnityEngine;
using UnityEngine.UI;

namespace Yttl.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class YttlConnector_USharpVideo : UdonSharpBehaviour
#if !COMPILER_UDONSHARP
        , IYttlConnector
#endif
    {
        [SerializeField]
        YttlManager manager;

        public USharpVideoPlayer videoPlayer;

        [SerializeField]
        private Text uiText;

        [SerializeField]
        private TextMeshPro textMesh;

        [SerializeField]
        private TextMeshProUGUI textMeshUGUI;

        [SerializeField]
        private Color authorColor = new Color(255f / 255f, 184f / 255f, 100f / 255f, 1f);

        [SerializeField]
        private int authorSize = -1;

        [SerializeField]
        private Color titleColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f);

        [SerializeField]
        private int titleSize = -1;

        [SerializeField]
        private Color viewCountColor = new Color(92f / 255f, 92f / 255f, 92f / 255f, 1f);

        [SerializeField]
        private int viewCountSize = -1;

        private void Start()
        {
            if (videoPlayer == null)
            {
                Debug.LogError($"[YTTL] {nameof(USharpVideoPlayer)}がセットされていない");
                return;
            }

            videoPlayer.RegisterCallbackReceiver(this);
        }

        public void OnUSharpVideoLoadStart()
        {
            if (manager == null)
            {
                Debug.LogError("[YTTL] Yttl Managerがセットされていない");
                return;
            }

            ClearTexts();

            var url = videoPlayer.GetCurrentURL();
            manager.LoadData(url, this);
        }

        [System.NonSerialized]
        public string author;

        [System.NonSerialized]
        public string title;

        [System.NonSerialized]
        public string viewCount;

        [System.NonSerialized]
        public string description;

        public void Yttl_OnDataLoaded()
        {
            var resultTmp = new string[3];

            /*

            if (!string.IsNullOrEmpty(author))
            {
                if (authorColor.a > 0f)
                {
                    author = $"<color={C2CT(authorColor)}>{author}</color>";
                }

                if (authorSize > -1)
                {
                    author = $"<size={authorSize}>{author}</size>";
                }

                resultTmp[0] = author;
            }

            */

            if (!string.IsNullOrEmpty(title))
            {
                /*
                
                if (titleColor.a > 0f)
                {
                    title = $"<color={C2CT(titleColor)}>{title}</color>";
                }

                if (titleSize > -1)
                {
                    title = $"<size={titleSize}>{title}</size>";
                }

                */

                resultTmp[1] = title;
            }

            /*

            if (!string.IsNullOrEmpty(viewCount))
            {
                if (viewCountColor.a > 0f)
                {
                    viewCount = $"<color={C2CT(viewCountColor)}>{viewCount}</color>";
                }

                if (viewCountSize > -1)
                {
                    viewCount = $"<size={viewCountSize}>{viewCount} views</size>";
                }

                resultTmp[2] = viewCount;
            }

            */

            var result = string.Join(" ", resultTmp);

            if (!string.IsNullOrWhiteSpace(result))
            {
                if (uiText != null)
                {
                    uiText.text = result;
                }

                if (textMesh != null)
                {
                    textMesh.text = result;
                }

                if (textMeshUGUI != null)
                {
                    textMeshUGUI.text = result;
                }
            }
        }

        private void ClearTexts()
        {
            if (uiText != null)
            {
                uiText.text = string.Empty;
            }

            if (textMesh != null)
            {
                textMesh.text = string.Empty;
            }

            if (textMeshUGUI != null)
            {
                textMeshUGUI.text = string.Empty;
            }
        }

        private string C2CT(Color c)
        {
            var r = Mathf.RoundToInt(c.r * 255);
            var g = Mathf.RoundToInt(c.g * 255);
            var b = Mathf.RoundToInt(c.b * 255);

            return $"#{r:x2}{g:x2}{b:x2}";
        }
    }
}
