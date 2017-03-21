using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using AssetBundles;

public class AbHotUpdateExample : MonoBehaviour
{

    Text _ShowResult;
    InputField _ServerIP;
    string _resServerUrl;
    RawImage _ImageDownload;

	void Start () {

        _ShowResult = GameObject.Find("ShowResult").GetComponent<Text>();
        _ShowResult.gameObject.SetActive(true);

        _ServerIP = GameObject.Find("ServerAssetPath").GetComponent<InputField>();
        _ServerIP.text = "http://www.1ceyou.com/game/web/asset/";

        _ImageDownload = GameObject.Find("RawImage").GetComponent<RawImage>();

        //先加载本地的md5文件，后面检查更新用
        AssetBundleManager.instance.Init(_ServerIP.text);

        //
        //实例里面，初始化的时候，会在本地StreamingAssets里面找MD5文件，在内存里面保留一份本地已有的<AB包名字，MD5码>键值对
        //点击CheckHotUpdate后，会根据上面输入的资源服务器路径，下载服务端MD5文件(并覆盖本地的MD5文件)，然后对比，对于(本地没有的||本地已经有)的AB包，就下载下来
        //下载完成后，AB在缓存中，可以选择输出到资源目录
	}

    public void OnClick_StarCheckHotUpdate()
    {
        //"http://www.1ceyou.com/game/web/asset/";
        _resServerUrl = _ServerIP.text;
        if (string.IsNullOrEmpty(_resServerUrl))
        {
            Debug.LogError("Resources_Server_Url Input is Error !!!");
            return;
        }
        //StartCoroutine(OnUpdateResource());
        StartCoroutine(DownLoadFile((_serverPath, _savePath) =>
            {
                if (string.IsNullOrEmpty(_serverPath))
                {
                    Debug.Log("Do Not Need To DownLoad , Already exist : " + _savePath);
                }
                else
                {
                    Debug.Log("Finish DownLoad_Http From - " + _serverPath + " - To - " + _savePath);
                }
                
                AssetBundle _bundle = AssetBundle.LoadFromFile(_savePath);
                if (_bundle != null)
                {
                    _ImageDownload.texture = _bundle.LoadAsset<Texture>("sugarBoss.png");
                }
            }));
    }

	void Update () {
	}

    //WWW加载到本地cache，然后WWW加载使用 
    IEnumerator OnUpdateResource()
    {
        string dataPath = Application.dataPath + "/StreamingAssets/";
        string listUrl = _resServerUrl + "VersionMD5.txt";
        WWW www = new WWW(listUrl); 
        yield return www;

        if (www.error != null)
        {
            Debug.LogError(www.error);
            yield break;
        }
        
        File.WriteAllBytes(dataPath + "VersionMD5.txt", www.bytes);
        string md5Text = www.text;
        Dictionary<string, string> _dic = AssetBundles.Util.GetMD5DicByFileString(md5Text);

        foreach(KeyValuePair<string, string> pair in _dic)
        {
            string localfile = (dataPath + pair.Key).Trim();
            string path = Path.GetDirectoryName(localfile);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            bool canUpdate = !File.Exists(localfile);
            if (!canUpdate)
            {
                string remoteMd5 = pair.Value.Trim();
                string localMd5 = AssetBundleManager.instance.GetLocalMD5(pair.Key);

                canUpdate = !remoteMd5.Equals(localMd5);
                if (canUpdate)
                {
                    File.Delete(localfile);
                }
            }

            if (canUpdate == true)
            {
                Debug.Log("BeginDownload : " + pair.Key);
                AssetBundleManager.instance.LoadAssetBundleInternal(pair.Key,
                    () =>
                    {
                        string _errorStr = "";
                        LoadedAssetBundle _bundle = AssetBundleManager.GetLoadedAssetBundle(pair.Key, out _errorStr);
                        if (string.IsNullOrEmpty(_errorStr))
                        {
                            _ImageDownload.texture = _bundle.m_AssetBundle.LoadAsset<Texture>("sugarBoss.png");
                        }

                    });
            }
        }

        yield return new WaitForEndOfFrame();
        StartGame();
    }

    //Http下载到本地磁盘，然后WWW加载使用
    IEnumerator DownLoadFile(System.Action<string, string> _callBack)
    {
        string dataPath = Application.dataPath + "/StreamingAssets/";
        string listUrl = _resServerUrl + "VersionMD5.txt";
        WWW www = new WWW(listUrl);
        yield return www;

        if (www.error != null)
        {
            Debug.LogError(www.error);
            yield break;
        }

        //服务端的MD5文件先保存一份到本地，如果资源更新顺利完成，就用这份文件覆盖本地MD5文件。如果不顺利，也能保证本地文件依然是旧版本，不影响下次更新流程
        File.WriteAllBytes(dataPath + "VersionMD5-Server.txt", www.bytes);
        string md5Text = www.text;
        Dictionary<string, string> _dic = AssetBundles.Util.GetMD5DicByFileString(md5Text);

        foreach (KeyValuePair<string, string> pair in _dic)
        {
            string localfile = (dataPath + pair.Key).Trim();
            string path = Path.GetDirectoryName(localfile);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            bool canUpdate = !File.Exists(localfile);
            if (!canUpdate)
            {
                string remoteMd5 = pair.Value.Trim();
                string localMd5 = AssetBundleManager.instance.GetLocalMD5(pair.Key);

                canUpdate = !remoteMd5.Equals(localMd5);
                if (canUpdate)
                {
                    File.Delete(localfile);
                }
            }

            if (canUpdate == true)
            {
                Debug.Log("BeginDownload : " + pair.Key);

                string _fileUrl = _resServerUrl + pair.Key;
                string _savePath = dataPath + pair.Key;;
                Debug.Log("Begin DownLoad_Http - _fileUrl: " + _fileUrl);
                Debug.Log("Begin DownLoad_Http - _savePath: " + _savePath);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_fileUrl);
                request.Method = "GET";
                HttpWebResponse hw = (HttpWebResponse)request.GetResponse();
                Stream stream = hw.GetResponseStream();
                FileStream fileStream = new FileStream(_savePath, FileMode.Create, FileAccess.Write);
                long length = hw.ContentLength;
                long currentNum = 0;
                decimal currentProgress = 0;
                while (currentNum < length)
                {
                    byte[] buffer = new byte[1024];
                    currentNum += stream.Read(buffer, 0, buffer.Length);
                    fileStream.Write(buffer, 0, buffer.Length);
                    if (currentNum % 1024 == 0)
                    {
                        currentProgress = Math.Round(Convert.ToDecimal(Convert.ToDouble(currentNum) / Convert.ToDouble(length) * 100), 4);
                        Debug.Log("当前下载文件大小:" + length.ToString() + "字节   当前下载大小:" + currentNum + "字节 下载进度" + currentProgress.ToString() + "%");
                    }
                    else
                    {
                        Debug.Log("当前下载文件大小:" + length.ToString() + "字节   当前下载大小:" + currentNum + "字节" + "字节 下载进度" + 100 + "%");
                    }
                    yield return false;
                }

                hw.Close();
                stream.Close();
                fileStream.Close();
                if (_callBack != null) _callBack(_fileUrl, _savePath);
            }
            else
            {
                //不用下载，就是本地有，而且是最新
                if (_callBack != null) _callBack("", localfile);
            }
        }

        yield return new WaitForEndOfFrame();

        //把服务端MD5文件同步到本地
        File.WriteAllBytes(dataPath + "VersionMD5.txt", www.bytes);

        StartGame();
    }

    void StartGame()
    {
        Debug.Log("StartGame");

        //本次更新包已经下载完毕 可以进入游戏了
    }
}
