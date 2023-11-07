
using UdonSharp;
using UnityEngine;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Yttl.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class YttlManager : UdonSharpBehaviour
    {
        [SerializeField]
        private YttlParser parser;

        [System.NonSerialized]
        public UdonBehaviour listener;

        public void LoadData(VRCUrl url, UdonSharpBehaviour listener)
        {
            if (!TryExtractHost(url.Get(), out var host))
            {
                Debug.LogWarning("[YTTL] 不正なホスト");
                return;
            }

            if (!parser.IsSupportedHost(host))
            {
                Debug.LogWarning("[YTTL] 未対応のホスト");
                return;
            }

            this.listener = (UdonBehaviour)(Component)listener;
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            var data = result.Result;

            var url = result.Url.Get();

            if (!TryExtractHost(url, out var host))
            {
                Debug.LogWarning("[YTTL] 不正なホスト");
                return;
            }

            parser.SetRawDataText(data);

            // tags: { "title", "author", "viewCount", "description" }
            if (parser.TryGetValue(host, "author", out var author))
            {
                Debug.Log($"[YTTL] {nameof(author)}: {author}");
            }

            if (parser.TryGetValue(host, "title", out var title))
            {
                Debug.Log($"[YTTL] {nameof(title)}: {title}");
            }

            if (parser.TryGetValue(host, "viewCount", out var viewCount))
            {
                if (int.TryParse(viewCount, out var partInt))
                {
                    viewCount = $"{partInt:#,0}";
                }

                Debug.Log($"[YTTL] {nameof(viewCount)}: {viewCount}");
            }

            if (parser.TryGetValue(host, "description", out var description))
            {
                Debug.Log($"[YTTL] {nameof(description)}: {description}");
            }

            Yttl_OnDataLoaded(author, title, viewCount, description);
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogWarning($"[YTTL] 動画情報がダウンロードできない Error: {result.Error} (ErrorCode: {result.ErrorCode})");
        }

        private bool TryExtractHost(string urlStr, out string host)
        {
            var index1 = urlStr.IndexOf("://");

            if (index1 == -1)
            {
                Debug.LogWarning("[YTTL] 不正なURL `://`");
                host = string.Empty;
                return false;
            }

            index1 += "://".Length;

            if (urlStr.Substring(index1).StartsWith("www."))
            {
                index1 += "www.".Length;
            }

            var index2 = urlStr.IndexOf("/", index1);

            if (index2 == -1)
            {
                index2 = urlStr.IndexOf("?", index1);
            }

            if (index2 == -1)
            {
                Debug.LogWarning("[YTTL] 未対応のURL `/` or `?`");
                host = string.Empty;
                return false;
            }

            host = urlStr.Substring(index1, index2 - index1);
            return true;
        }

        private void Yttl_OnDataLoaded(string author, string title, string viewCount, string description)
        {
            if (listener != null)
            {
                listener.SetProgramVariable(nameof(author), author);
                listener.SetProgramVariable(nameof(title), title);
                listener.SetProgramVariable(nameof(viewCount), viewCount);
                listener.SetProgramVariable(nameof(description), description);
                listener.SendCustomEvent(nameof(Yttl_OnDataLoaded));
            }
        }
    }
}
