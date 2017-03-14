using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
namespace AssetBundles
{

    public class LoadedAssetBundle
    {
        public AssetBundle m_AssetBundle;
        public int m_ReferencedCount;

        public LoadedAssetBundle(AssetBundle assetBundle)
        {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;
        }
    }

    public class LoadingAssetBundle
    {
        public string m_AssetBundleName;
        public WWW m_DownloadingWWW;
        public System.Action m_Action_Complete_Download;

        public LoadingAssetBundle(string _abName)
        {
            m_AssetBundleName = _abName;
            m_DownloadingWWW = null;
            m_Action_Complete_Download = null;
        }
    }

    public class AssetBundleManager : MonoBehaviour
    {
        public static string m_BaseDownloadingURL = "";
        static Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
        static Dictionary<string, LoadingAssetBundle> m_LoadingAssetBundles = new Dictionary<string, LoadingAssetBundle>();
        static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string>();

        static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();  

        public static AssetBundleManager instance = null;

        public void Awake()
        {
            instance = this;
        }

        #region 本地MD5文件缓存 获取更新后可以销毁
        Dictionary<string, string> mDicAbMD5 = new Dictionary<string, string>();
        public void Init(string _url)
        {
            m_BaseDownloadingURL = _url;
            string _md5FilePath = Application.dataPath + "/StreamingAssets/VersionMD5.txt";
            mDicAbMD5 = AssetBundles.Util.GetMD5DicByFilePath(_md5FilePath);
        }

        public string GetLocalMD5(string _resName)
        {
            if (mDicAbMD5.ContainsKey(_resName))
            {
                return mDicAbMD5[_resName];
            }
            return "";
        }

        #endregion

        public static string BaseDownloadingURL
        {
            get { return m_BaseDownloadingURL; }
            set { m_BaseDownloadingURL = value; }
        }

        static public LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
        {
            if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
            {
                return null;
            }

            LoadedAssetBundle bundle = null;
            m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle == null)
            {
                return null;
            }

            //依赖文件也下载完成，才算下载完成，这里Demo演示比较简单，省略掉了

            return bundle;
        }

        public bool LoadAssetBundleInternal(string assetBundleName, System.Action _completeDownloadCallBack)
        {
            // Already loaded.
            LoadedAssetBundle bundle = null;
            m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle != null)
            {
                bundle.m_ReferencedCount++;
                return true;
            }

            LoadingAssetBundle _loadingBundle = null;
            m_LoadingAssetBundles.TryGetValue(assetBundleName, out _loadingBundle);
            if (_loadingBundle != null)
            {
                if (_completeDownloadCallBack != null) _loadingBundle.m_Action_Complete_Download += _completeDownloadCallBack;
                return true;
            }

            _loadingBundle = new LoadingAssetBundle(assetBundleName);
            if (_completeDownloadCallBack != null) _loadingBundle.m_Action_Complete_Download += _completeDownloadCallBack;
            m_LoadingAssetBundles.Add(assetBundleName, _loadingBundle);
            string url = m_BaseDownloadingURL + assetBundleName;
            _loadingBundle.m_DownloadingWWW = WWW.LoadFromCacheOrDownload(url, 1);

            return false;
        }


        #region Unload assetbundle and its dependencies.
        // Unload assetbundle and its dependencies.
        static public void UnloadAssetBundle(string assetBundleName)
        {
            UnloadAssetBundleInternal(assetBundleName);
            UnloadDependencies(assetBundleName);
        }

        static protected void UnloadDependencies(string assetBundleName)
        {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies))
            {
                return;
            }

            // Loop dependencies.
            foreach (var dependency in dependencies)
            {
                UnloadAssetBundleInternal(dependency);
            }

            m_Dependencies.Remove(assetBundleName);
        }

        static protected void UnloadAssetBundleInternal(string assetBundleName)
        {
            string error;
            LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out error);
            if (bundle == null)
            {
                return;
            }

            if (--bundle.m_ReferencedCount == 0)
            {
                bundle.m_AssetBundle.Unload(false);
                m_LoadedAssetBundles.Remove(assetBundleName);
            }
        }

        #endregion
        void Update()
        {
            // Collect all the finished WWWs.
            var keysToRemove = new List<string>();
            foreach (var keyValue in m_LoadingAssetBundles)
            {
                WWW download = keyValue.Value.m_DownloadingWWW;
                if (download == null) continue;

                // If downloading fails.
                if (download.error != null)
                {
                    m_DownloadingErrors.Add(keyValue.Key, string.Format("Failed downloading bundle {0} from {1}: {2}", keyValue.Key, download.url, download.error));
                    keysToRemove.Add(keyValue.Key);
                    continue;
                }

                // If downloading succeeds.
                if (download.isDone)
                {
                    AssetBundle bundle = download.assetBundle;
                    if (bundle == null)
                    {
                        m_DownloadingErrors.Add(keyValue.Key, string.Format("{0} is not a valid asset bundle.", keyValue.Key));
                        keysToRemove.Add(keyValue.Key);
                        continue;
                    }

                    if (!m_LoadedAssetBundles.ContainsKey(keyValue.Key))
                    {
                        m_LoadedAssetBundles.Add(keyValue.Key, new LoadedAssetBundle(download.assetBundle));
                    }
                    else
                    {
                        m_LoadedAssetBundles[keyValue.Key] = new LoadedAssetBundle(download.assetBundle);
                    }

                    keysToRemove.Add(keyValue.Key);

                    if (keyValue.Value.m_Action_Complete_Download != null)
                    {
                        keyValue.Value.m_Action_Complete_Download();
                    }
                }
            }

            // Remove the finished WWWs.
            foreach (var key in keysToRemove)
            {
                LoadingAssetBundle _loadingBundle = m_LoadingAssetBundles[key];
                WWW download = _loadingBundle.m_DownloadingWWW;
                download.Dispose();
                m_LoadingAssetBundles.Remove(key);
            }
        }
    }
}