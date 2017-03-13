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
        StartCoroutine(OnUpdateResource());
    }

	void Update () {
	}

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
        string filesText = www.text;
        string[] files = filesText.Split('\n');

        for (int i = 0; i < files.Length; i++)
        {
            if (string.IsNullOrEmpty(files[i]))
            {
                continue;
            }
            string _line = files[i].Trim('\r');
            string[] keyValue = _line.Split('%');
            string fileName = keyValue[1];
            string localfile = (dataPath + fileName).Trim();

            string path = Path.GetDirectoryName(localfile);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            bool canUpdate = !File.Exists(localfile);
            if (!canUpdate)
            {
                string remoteMd5 = keyValue[3].Trim();
                string localMd5 = AssetBundleManager.instance.GetMD5(fileName);

                canUpdate = !remoteMd5.Equals(localMd5);
                if (canUpdate)
                {
                    File.Delete(localfile);
                }
            }
            if (canUpdate)
            {
                Debug.Log("BeginDownload : " + fileName);
                AssetBundleManager.instance.LoadAssetBundleInternal(fileName, 
                    ()=>
                    {
                        string _errorStr = "";
                        LoadedAssetBundle _bundle = AssetBundleManager.GetLoadedAssetBundle(fileName, out _errorStr);
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

    void StartGame()
    {
        Debug.Log("StartGame");

        //本次更新包已经下载完毕 可以进入游戏了

        //把下载的资源包输出到资源路径下（可选，不输出的话，是在Cache里面）
        SaveAssetBundle();
    }

    void SaveAssetBundle()
    {

    }
}
