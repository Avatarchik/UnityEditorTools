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


public class UITexturePack : EditorWindow
{
    #region UITexturePack Define
    const string mStrUITextureFolder = "Assets/UITexture/";//贴图存放路径

    const string mStrUITexture_ABName = "uitexture";//UITexture的标签 (Assets/UITexture 路径下的贴图)
    //static string mStrUITextureAssetBundleTotalFolder = EditorHelper.bundlePath + "/UITexture";//UITexture整包路径
    static string mStrUITextureAssetBundleTotalFolder = EditorHelper.bundlePath;//UITexture整包路径

    #endregion

    //[MenuItem("Helper/UITexturePack")]
    //public static void StartBuild()
    //{

    //    UITexture_Package(EditorHelper.buildTarget);
    //}

    [MenuItem("Helper/SetUITextureABName")]
    public static void StartBuild()
    {
        SetAssetBundleName();
    }

    private static void SetAssetBundleName()
    {
        DirectoryInfo dirs = new DirectoryInfo(mStrUITextureFolder);
        if (dirs == null || (!dirs.Exists))
        {
            Debug.LogError(mStrUITextureFolder + " 路径为不存在");
            return;
        }

        FileInfo[] files = dirs.GetFiles();
        for (int i = 0; i < files.Length; ++i)
        {
            if (!files[i].Name.EndsWith(".meta"))
            {
                AssetImporter ai = AssetImporter.GetAtPath(mStrUITextureFolder + files[i].Name);
                ai.assetBundleName = mStrUITexture_ABName;
                ai.assetBundleVariant = "unity3d";
                ai.SaveAndReimport();
            }
        }
    }

    private static void ClearDataAB()
    {
        string _path = mStrUITextureAssetBundleTotalFolder;
        DirectoryInfo dirs = new DirectoryInfo(_path);

        if (dirs == null || (!dirs.Exists))//不存在，就创建一个空目录
        {
            Directory.CreateDirectory(_path);
            AssetDatabase.Refresh();
            return;
        }

        //存在就清空
        FileInfo[] files = dirs.GetFiles();
        if (files != null)
        {
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i] != null)
                {
                    files[i].Delete();
                    files[i] = null;
                }
            }
            files = null;
        }
        AssetDatabase.Refresh();
    }

    private static void UITexture_Package(BuildTarget _buildTarget)
    {
        ClearDataAB();

        DirectoryInfo dirs = new DirectoryInfo(mStrUITextureFolder);

        if (dirs == null || (!dirs.Exists))
        {
            Debug.LogError(mStrUITextureFolder + " 此路径找不到可以打包的UITexture");
            return;
        }

        //AssetBundleBuild[] builds = new AssetBundleBuild[1];
        //builds[0].assetBundleName = mStrUITexture_ABName;

        //FileInfo[] files = dirs.GetFiles();
        //List<string> _assetNames = new List<string>();
        //for (int i = 0; i < files.Length; ++i)
        //{
        //    if (!files[i].Name.EndsWith(".meta"))
        //    {
        //        _assetNames.Add(mStrUITextureFolder + files[i].Name);
        //    }
        //}
        //builds[0].assetNames = _assetNames.ToArray();

        //UnityEngine.AssetBundleManifest _abM = BuildPipeline.BuildAssetBundles(mStrUITextureAssetBundleTotalFolder,
        //    builds,
        //    BuildAssetBundleOptions.None,
        //    _buildTarget);

        //if (_abM != null)
        //{
        //    EditorUtility.DisplayDialog("UITexture已经打包完成", "", "确定");
        //    AssetDatabase.Refresh();
        //}
        //else
        //{
        //    EditorUtility.DisplayDialog("UITexture打包出错", "", "确定");
        //}

        //Unity 5.0.2f1 AssetBundleBuild[] 不认后缀.png（其实除了*.asset *.unity ，其他的貌似都不认）
        //如果不传buildMap，让API自己去找ABName 标签打的话，是可以的，目前没有解决方案，想换个新版本试试
        //暂时用下面Unity4.x的方法打贴图
        List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
        var paths = Directory.GetFiles(mStrUITextureFolder, "*", SearchOption.AllDirectories);
        foreach (var item in paths)
        {
            var addon = AssetDatabase.LoadAssetAtPath(item.Replace(@"\", "/"), typeof(UnityEngine.Object));
            if (addon != null)
            {
                assets.Add(addon);
            }
        }
        string UITextureBundleName = "UITexture.unity3d";
        bool ok = BuildPipeline.BuildAssetBundle(null, 
            assets.ToArray(), 
            mStrUITextureAssetBundleTotalFolder + "/" + UITextureBundleName, 
            BuildAssetBundleOptions.None, 
            _buildTarget);
        if (ok)
        {
            EditorUtility.DisplayDialog("UITexture已经打包完成", "", "确定");
            AssetDatabase.Refresh();
        }
        else
        {
            EditorUtility.DisplayDialog("UITexture打包出错", "", "确定");
        }

        
    }
}
