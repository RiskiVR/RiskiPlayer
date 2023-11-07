
using UdonSharp;
using UnityEngine;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Yttl.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class YttlParser : UdonSharpBehaviour
    {
        private VRCUrl defineFileUrl = new VRCUrl("https://raw.githubusercontent.com/ureishi/yttl-data/v1/yttl.txt");

        private string rawDataText;

        private string Text => rawDataText;

        private string[] hosts;
        private string[][] labels;
        private string[][][][] tagses;

        private void Start()
        {
            LoadDefineFile();
        }

        public void LoadDefineFile()
        {
            VRCStringDownloader.LoadUrl(defineFileUrl, (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            Init(result.Result);
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogWarning($"[YTTL] 定義データがダウンロードできない Error: {result.Error} (ErrorCode: {result.ErrorCode})");
        }

        private bool inited = false;

        private void Init(string defineText)
        {
            if (inited)
            {
                return;
            }

            if (string.IsNullOrEmpty(defineText))
            {
                Debug.LogWarning("[YTTL] 定義データが空");
                return;
            }

            if (!TryParseDefine(defineText))
            {
                Debug.LogWarning("[YTTL] 定義データを解釈できない");
                return;
            }

            Debug.Log("[YTTL] 定義データ読み込み完了");

            inited = true;
        }

        private bool parsed = false;

        private bool TryParseDefine(string tagDefineText)
        {
            if (parsed)
            {
                return true;
            }

            var lines = tagDefineText.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            var hostCount = 0;
            foreach (var line in lines)
            {
                if (line.StartsWith("//"))
                {
                    continue;
                }

                if (line.StartsWith("://"))
                {
                    hostCount++;
                }
            }

            hosts = new string[hostCount];
            labels = new string[hostCount][];
            tagses = new string[hostCount][][][];

            var countI = 0;
            for (var i = 0; i < lines.Length; i++)
            {
                var lineI = lines[i];

                if (lineI.StartsWith("//"))
                {
                    continue;
                }

                if (lineI.StartsWith("://"))
                {
                    hosts[countI] = lineI.Substring(3);

                    var labelCount = 0;

                    for (var j = i + 1; j < lines.Length; j++)
                    {
                        var lineJ = lines[j];

                        if (lineJ.StartsWith("://"))
                        {
                            break;
                        }
                        else if (lineJ.StartsWith(":"))
                        {
                            labelCount++;
                        }
                    }

                    labels[countI] = new string[labelCount];
                    tagses[countI] = new string[labelCount][][];

                    var countJ = 0;
                    for (var j = i + 1; j < lines.Length; j++)
                    {
                        var lineJ = lines[j];

                        if (lineJ.StartsWith("//"))
                        {
                            continue;
                        }

                        if (lineJ.StartsWith("://"))
                        {
                            break;
                        }
                        else if (lineJ.StartsWith(":"))
                        {
                            labels[countI][countJ] = lineJ.Substring(1);

                            var tagCount = 0;

                            for (var k = j + 1; k < lines.Length; k++)
                            {
                                var lineK = lines[k];

                                if (lineK.StartsWith("//"))
                                {
                                    continue;
                                }

                                if (lineK.StartsWith(":"))
                                {
                                    break;
                                }

                                tagCount++;
                            }

                            tagses[countI][countJ] = new string[tagCount][];

                            var countK = 0;
                            for (var k = j + 1; k < lines.Length; k++)
                            {
                                var lineK = lines[k];

                                if (lineK.StartsWith("//"))
                                {
                                    continue;
                                }

                                if (lineK.StartsWith(":"))
                                {
                                    break;
                                }

                                if (lineK.Split(new[] { '\t' }).Length == 1)
                                {
                                    tagses[countI][countJ][countK] = new[] { lineK };
                                }
                                else if (lineK.Split(new[] { '\t' }).Length == 2)
                                {
                                    tagses[countI][countJ][countK] = lineK.Split(new[] { '\t' });
                                }
                                else
                                {
                                    Debug.LogWarning("[YTTL] 定義の最後の行にはtab文字がちょうど1つ必要");
                                    return false;
                                }

                                countK++;
                            }

                            countJ++;
                        }
                    }

                    countI++;
                }
            }

            parsed = true;
            return true;
        }

        public void SetRawDataText(string rawDataText)
        {
            this.rawDataText = rawDataText;
        }

        public bool IsSupportedHost(string host)
        {
            if (!inited)
            {
                Debug.LogWarning("[YTTL] 初期化前");
                return false;
            }

            return System.Array.IndexOf(hosts, host) != -1;
        }

        public bool TryGetValue(string host, string label, out string result)
        {
            if (!inited)
            {
                Debug.LogWarning("[YTTL] 初期化前");
                result = default;
                return false;
            }

            if (string.IsNullOrEmpty(Text))
            {
                Debug.LogWarning("[YTTL] DataTextが空");
                result = default;
                return false;
            }

            var hostIndex = System.Array.IndexOf(hosts, host);

            if (hostIndex == -1)
            {
                Debug.Log($"[YTTL] 対応していないサイト `{host}`");
                result = default;
                return false;
            }

            var labelIndex = System.Array.IndexOf(labels[hostIndex], label);

            if (labelIndex == -1)
            {
                Debug.Log($"[YTTL] 対応していないラベル `{label}`");
                result = default;
                return false;
            }

            var tags = tagses[hostIndex][labelIndex];

            int tagIndex = 0;

            var currentTag = tags[tagIndex++];

            var tmpIndex = 0;

            while (currentTag.Length == 1)
            {
                var cTag = currentTag[0];
                tmpIndex = Text.IndexOf(cTag, tmpIndex);

                if (tmpIndex != -1)
                {
                    tmpIndex += cTag.Length;

                    if (tmpIndex > 0 && Text[tmpIndex - 1] == '\\')
                    {
                        continue;
                    }
                }
                else
                {
                    Debug.Log($"[YTTL] 中間要素が文字列中に見つからない `{cTag}`");
                    result = default;
                    return false;
                }

                if (tagIndex < tags.Length)
                {
                    currentTag = tags[tagIndex++];
                }
                else
                {
                    Debug.LogWarning($"[YTTL] 最後の要素が1つしかない `{currentTag}`");
                    result = default;
                    return false;
                }
            }

            if (currentTag.Length == 2)
            {
                var sTag = currentTag[0];
                var tTag = currentTag[1];

                if (string.IsNullOrEmpty(sTag) || string.IsNullOrEmpty(tTag))
                {
                    Debug.LogWarning("[YTTL] 最後の行の要素は始点と終点のどちらも1文字以上でなければならない");
                }

                var start = Text.IndexOf(sTag, tmpIndex);

                if (start == -1)
                {
                    Debug.LogWarning($"[YTTL] 始点が文字列中に見つからない `{sTag}`");
                    result = default;
                    return false;
                }

                start += sTag.Length;

                tmpIndex = start;

                int terminal;

                while (true)
                {
                    terminal = Text.IndexOf(tTag, tmpIndex);

                    if (terminal == -1)
                    {
                        Debug.LogWarning($"[YTTL] 終点が文字列中に見つからない `{tTag}`");
                        result = default;
                        return false;
                    }
                    else if (terminal == tmpIndex)
                    {
                        result = string.Empty;
                        return true;
                    }
                    else
                    {
                        if (Text[terminal - 1] == '\\')
                        {
                            tmpIndex += terminal - tmpIndex;
                            continue;
                        }
                    }

                    break;
                }

                result = Text.Substring(start, terminal - start);
            }
            else
            {
                Debug.LogWarning("[YTTL] 最後の行の要素が2つでない");
                result = default;
                return false;
            }

            var tmpResultPart = result.Split(new[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < tmpResultPart.Length; i++)
            {
                var part = tmpResultPart[i];

                if (part.Length == 0)
                {
                    continue;
                }

                if ("\"\\/bfnrt".IndexOf(part[0]) != -1)
                {
                    if (!TryUnescape($"\\{part[0]}", out var unesc))
                    {
                        result = default;
                        return false;
                    }

                    tmpResultPart[i] = unesc + part.Substring(1);
                }
                else if (part.StartsWith("u"))
                {
                    if (part.Length >= "\\uxxxx".Length - 1)
                    {
                        if (!TryUnescape($"\\{part.Substring(0, "\\uxxxx".Length - 1)}", out var unesc))
                        {
                            result = default;
                            return false;
                        }

                        tmpResultPart[i] = unesc + part.Substring("\\uxxxx".Length - 1);
                    }
                    else
                    {
                        Debug.LogWarning("[YTTL] Unescapeに必要な文字数に達していない");
                        result = default;
                        return false;
                    }
                }
                else
                {
                    if (i > 0)
                    {
                        Debug.LogWarning("[YTTL] 不正なエスケープ");
                        result = default;
                        return false;
                    }
                }
            }

            result = string.Concat(tmpResultPart);

            return true;
        }

        private static bool TryUnescape(string esc, out char result)
        {
            if (string.IsNullOrEmpty(esc))
            {
                Debug.LogWarning("[YTTL] Unescapeに失敗 - 変換対象がnullまたは空");
                result = default;
                return false;
            }

            if (esc.Length < 2)
            {
                Debug.LogWarning("[YTTL] Unescapeに失敗 - 文字が不足");
                result = default;
                return false;
            }

            if (esc[0] != '\\')
            {
                Debug.LogWarning("[YTTL] Unescapeに失敗 - バックスラッシュから始まっていない");
                result = default;
                return false;
            }

            var eTag = esc[1];

            switch (eTag)
            {
                case '"':
                    result = '\"';
                    return true;
                case '\\':
                    result = '\\';
                    return true;
                case '/':
                    result = '/';
                    return true;
                case 'b':
                    result = '\b';
                    return true;
                case 'f':
                    result = '\f';
                    return true;
                case 'n':
                    result = '\n';
                    return true;
                case 'r':
                    result = '\r';
                    return true;
                case 't':
                    result = '\t';
                    return true;
                case 'u':
                    if (esc.Length < 6)
                    {
                        Debug.LogWarning("[YTTL] Unicode escapeのunescapeに失敗 - 文字が不足");
                        result = default;
                        return false;
                    }

                    if (!int.TryParse(esc.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out var utf16))
                    {
                        Debug.LogWarning("[YTTL] Unicode escapeのunescapeに失敗 - 16進数ではない");
                        result = default;
                        return false;
                    }

                    result = (char)utf16;
                    return true;
                default:
                    Debug.LogWarning("[YTTL] Unescapeに失敗");
                    result = default;
                    return false;
            }
        }
    }
}
