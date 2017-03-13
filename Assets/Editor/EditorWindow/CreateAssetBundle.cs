using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class CreateAssetBundle : Editor
{

    //[MenuItem("Helper/AB_BuildAll")]
    //static void Build()
    //{
    //    string path = EditorHelper.bundlePath;
    //    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        
    //    #region TableData
        
    //    #endregion

    //    #region UITexture

    //    #endregion

    //    AssetDatabase.Refresh();
    //}

    [MenuItem("Helper/Clean Cache")]
    public static void CleanCache()
    {
        Caching.CleanCache();
    }

}