using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Text;
using UnityEngine.UI;

public class TableDataPack : MonoBehaviour
{

    BinaryWriter bw;
    Text _text;

	void Start () {

        _text = GameObject.Find("ReadTableResult").GetComponent<Text>();
        _text.text = "点击LoadTable开始读表: \n";
	}

    public void Onclick()
    {
        Debug.Log("Onclick");
        _text.text = "这里是读表结果: \n";
        StartCoroutine(LoadTableData());
    }

    IEnumerator LoadTableData()
    {
        
        string shortURL = "table/table_data";
        string path = "";

#if UNITY_ANDROID && !UNITY_EDITOR
        _text.text += "平台类型: UNITY_ANDROID && !UNITY_EDITOR\n";
        path = "jar:file://" + Application.dataPath + "!/assets/" + shortURL;
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        _text.text += "平台类型: UNITY_STANDALONE_WIN || UNITY_EDITOR\n";
        path = "file:///" + Path.Combine(Application.streamingAssetsPath, shortURL);
#elif UNITY_IPHONE
        _text.text += "平台类型: UNITY_IPHONE\n";
        path = "file://" + Application.dataPath + "/Raw/" + shortURL;
#else
        path = string.Empty;
#endif


        _text.text += path + " \n";

        WWW configData = new WWW(path);
        yield return configData;
        if (!string.IsNullOrEmpty(configData.error))
        {
            _text.text += configData.error + " \n";
            Debug.LogError(configData.error);
            configData = null;
            yield break;
        }

        HeroDataTable heroDataTable = configData.assetBundle.LoadAsset<HeroDataTable>("HeroDataTable.asset");

        Debug.Log("----  Read Data Form assetbundle");
        for (int i = 0; i < heroDataTable.dataList.Count; ++i)
        {
            string _tmp = heroDataTable.dataList[i].id
                + heroDataTable.dataList[i].name
                + heroDataTable.dataList[i].describe
                + heroDataTable.dataList[i].avatarID
                + heroDataTable.dataList[i].speak;
            Debug.Log(_tmp);

            if (_text != null)
            {
                _text.text += " \n" + _tmp;
            }
        }
        Debug.Log("---- End Of Read Assetbundle");
    }

	// Update is called once per frame
	void Update () {
	}
}
