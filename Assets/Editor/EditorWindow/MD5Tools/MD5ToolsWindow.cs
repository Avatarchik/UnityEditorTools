using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Aspose.Cells;
using System.Threading;
using System.Security.Cryptography;

public class MD5ToolsWindow : EditorWindow
{
    public static UnityEditor.BuildTarget buildTarget = BuildTarget.StandaloneWindows;

    [MenuItem("Helper/MD5Tools Window")]
    public static MD5ToolsWindow NewWindow()
    {
        MD5ToolsWindow newWindow = EditorWindow.GetWindow<MD5ToolsWindow>();
        return newWindow;
    }

    //记录Toolbar按钮的ID  
    private int toolbarID;
    //用于标签显示的信息  
    private string info;
    //Toolbar按钮上的信息  
    private string[] toolbarInfo = new string[] { "PC", "IOS", "Android" }; 

    protected Assembly asm;
    protected void OnEnable()
    {
        autoRepaintOnSceneChange = false;

        if (asm == null)
        {
            asm = Assembly.Load("Assembly-CSharp");
        }
    }


    void OnGUI()
    {
        if (asm == null)
        {
            asm = Assembly.Load("Assembly-CSharp");
        }

        GUILayout.Space(80);

        //绘制Toolbar  
        toolbarID = GUI.Toolbar(new Rect(20, 20, 200, 20), toolbarID, toolbarInfo);
        //根据toolbarID来获得info  
        info = toolbarInfo[toolbarID];
        //绘制标签  
        GUI.Label(new Rect(40, 50, 200, 20), "选择的是" + info + "平台");

        if (toolbarID == 0)
            buildTarget = BuildTarget.StandaloneWindows;
        else if (toolbarID == 1)
            buildTarget = BuildTarget.iOS;
        else if (toolbarID == 2)
            buildTarget = BuildTarget.Android;


        #region 

        GUILayout.Space(20);

        if (GUILayout.Button("生成指定平台的MD5文件", GUILayout.Width(256)))
        {
            string platform = GetPlatformPath(buildTarget);
            Execute(platform);
            AssetDatabase.Refresh();
        }

        GUILayout.Space(20);
        #endregion

    }


    public static void Execute(string platform)
    {
        Dictionary<string, string> DicFileMD5 = new Dictionary<string, string>();
        MD5CryptoServiceProvider md5Generator = new MD5CryptoServiceProvider();

        string dir = System.IO.Path.Combine(Application.dataPath, platform);
        foreach (string filePath in Directory.GetFiles(dir, "*.unity3d", SearchOption.AllDirectories))
        {
            if (filePath.Contains(".meta") || filePath.Contains("VersionMD5") || filePath.Contains(".txt"))
                continue;

            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] hash = md5Generator.ComputeHash(file);
            string strMD5 = System.BitConverter.ToString(hash);
            file.Close();

            string key = filePath.Substring(dir.Length, filePath.Length - dir.Length);

            key = key.Replace("\\", "/");

            if (DicFileMD5.ContainsKey(key) == false)
                DicFileMD5.Add(key, strMD5);
            else
                Debug.LogWarning("<Two File has the same name> name = " + filePath);
        }

        //	string savePath = System.IO.Path.Combine(Application.dataPath, "AssetBundle/") + platform;
        string savePath = Application.dataPath + "/VersionNum/" + platform;
        if (Directory.Exists(savePath) == false)
            Directory.CreateDirectory(savePath);
        // 删除前一版的old数据
        if (File.Exists(savePath + "/VersionMD5-old.txt"))
        {
            System.IO.File.Delete(savePath + "/VersionMD5-old.txt");
        }

        // 如果之前的版本存在，则将其名字改为VersionMD5-old.txt
        if (File.Exists(savePath + "/VersionMD5.txt"))
        {
            System.IO.File.Move(savePath + "/VersionMD5.txt", savePath + "/VersionMD5-old.txt");
        }

        FileStream txtDoc = new FileStream(savePath + "/VersionMD5.txt", FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(txtDoc);

        foreach (KeyValuePair<string, string> pair in DicFileMD5)
        {
            sw.WriteLine("FileName%" + pair.Key + "%MD5%" + pair.Value);
        }

        // 读取旧版本的MD5
        Dictionary<string, string> dicOldMD5 = ReadMD5File(savePath + "/VersionMD5-old.txt");
        // VersionMD5-old中有，而VersionMD5中没有的信息，手动添加到VersionMD5
        foreach (KeyValuePair<string, string> pair in dicOldMD5)
        {
            if (DicFileMD5.ContainsKey(pair.Key) == false)
                DicFileMD5.Add(pair.Key, pair.Value);
        }
        sw.Close();
    }

    //不同平台的Assetbundle存放路径不同，这个可以根据自己需要修改
    public static string GetPlatformPath(UnityEditor.BuildTarget target)
    {
        //string SavePath = "";
        //switch (target)
        //{
        //    case BuildTarget.StandaloneWindows:
        //    case BuildTarget.StandaloneWindows64:
        //        SavePath = "StreamingAssets/";
        //        break;
        //    case BuildTarget.iOS:
        //        SavePath = "Raw/";
        //        break;
        //    case BuildTarget.Android:
        //        SavePath = "assets/";
        //        break;
        //    default:
        //        SavePath = "StreamingAssets/";
        //        break;
        //}

        /**
         * 这里是为了方便Examples演示用，所以不同平台都放在一个目录，实际开发中可以分开存放，可以避免反复切换平台需要反复打包    
         * 不过出包的时候需要把输出的AB包贴到游戏调用的统一目录（比如StreamingAssets）
         */
        string SavePath = "StreamingAssets/";

        if (Directory.Exists(SavePath) == false)
            Directory.CreateDirectory(SavePath);
        return SavePath;
    }

    static Dictionary<string, string> ReadMD5File(string fileName)
    {
        Dictionary<string, string> DicMD5 = new Dictionary<string, string>();

        // 如果文件不存在，则直接返回
        if (System.IO.File.Exists(fileName) == false)
            return DicMD5;

        string[] lineText = File.ReadAllLines(fileName);

        foreach (string x in lineText)
        {
            BundleFile a = GetBundleFile(x);
            a.fileName = a.fileName.Replace("\\", "/");
            if (a != null && DicMD5.ContainsKey(a.fileName) == false)
            {
                DicMD5.Add(a.fileName, a.version);
            }
        }
        return DicMD5;
    }

    static BundleFile GetBundleFile(string _textLine)
    {
        string[] text = _textLine.Split('%');
        if (text[0].Length > 1)
        {
            BundleFile x = new BundleFile();
            x.fileName = text[1];
            x.version = text[3];
            return x;
        }
        return null;
    }
}

public class BundleFile
{
    public string fileName;
    public string version;
    public float size;
}
