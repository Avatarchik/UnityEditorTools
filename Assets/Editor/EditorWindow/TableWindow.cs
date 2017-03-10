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

public class TableWindow : EditorWindow {

    protected TableProcessProfile profile;

    //一般只执行一次，创建配置文件列表
    [MenuItem("Assets/Create/Custom Assets/CreatTableProcess")]
    static void CreateAnimationTableAsset()
    {
        TableProcessProfile _profile = AssetDatabase.LoadAssetAtPath(EditorHelper.mStrAssetProfileFolder + "TableProcessProfile_System.asset",
                                      typeof(TableProcessProfile)) as TableProcessProfile;
        if (_profile == null)
        {
            _profile = EditorHelper.CreateNewEditorProfile<TableProcessProfile>("TableProcessProfile_System.asset", EditorHelper.mStrAssetProfileFolder, "TableProcessProfile");
        }
    }

    /*
    * 配表规则定死：
    *      第一行是 字段变量名称（中文）
    *      第二行是 字段变量类型（字符串，string,int（暂时只有两种））
    *      第三行是 字段变量名称（英文，代码中可用）
    */

    private const int LINE_NUM_VALUE_NAME_CH = 0;
    private const int LINE_NUM_VALUE_TYPE = 1;
    private const int LINE_NUM_VALUE_NAME_EN = 2;

    public class ExcelTitle
    {
        public ExcelTitle()
        {
            strArrValueNamesCH = null;
            strArrValueType = null;
            strArrValueNamesEN = null;
        }
        public string strArrValueNamesCH;
        public string strArrValueType;
        public string strArrValueNamesEN;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(strArrValueNamesCH) && !string.IsNullOrEmpty(strArrValueType) && !string.IsNullOrEmpty(strArrValueNamesEN);
        }
    };
    //--- <列标，表头信息>
    Dictionary<int, ExcelTitle> mDicExeclTitles = new Dictionary<int, ExcelTitle>();

    //--- <行标，<列标，字段内容>>
    Dictionary<int, Dictionary<int, string>> mDicExcelData = new Dictionary<int, Dictionary<int, string>>();

    [MenuItem("Helper/Table Window")]
    public static TableWindow NewWindow()
    {
        TableWindow newWindow = EditorWindow.GetWindow<TableWindow>();
        return newWindow;
    }

    protected Assembly asm;

    public static string mStrProfileName = "TableProcessProfile_System.asset";
    protected void OnEnable()
    {
        autoRepaintOnSceneChange = false;

        if (asm == null)
        {
            asm = Assembly.Load("Assembly-CSharp");
        }

        LoadAsset(mStrProfileName);
    }


    void OnGUI()
    {
        if (asm == null)
        {
            asm = Assembly.Load("Assembly-CSharp");
        }

        #region excel_2_*.asset

        GUILayout.Space(20);
        //（离散包）从Excel读取数据，然后每个 TableProcessInfo info in profile.tableInfos 打成自己单独的一个*.asset文件 
        
        if (GUILayout.Button("Excel_2_Asset All (excel_2_*.asset)", GUILayout.Width(256)))
        {

            if (profile != null && profile.tableInfos != null)
            {
                foreach (TableProcessInfo info in profile.tableInfos)
                {
                    if (info.IsClientUse)
                    {
                        ExportTable(info);
                        AssetDatabase.Refresh();
                    }
                }
            }
        }

        GUILayout.Space(20);
        #endregion

        #region 从离散包(*.asset)路径读取所有离散包，根据他们共有的标签（assetBundleName）打成一个文件（整包）
        GUILayout.Space(20);

        GUI.Label(new Rect(40, 50, 700, 100), "所有Asset导出成一个整包, 不同平台导出结果不同，但是公用一个目录，导出前会清空导出目录");


        if (profile == null) return;

        if (asm == null)
        {
            asm = Assembly.Load("Assembly-CSharp");
        }

        List<UnityEngine.Object> objs = new List<UnityEngine.Object>();
        if (profile.tableInfos != null)
        {
            foreach (TableProcessInfo info in profile.tableInfos)
            {
                if (info.IsClientUse)
                {
                    string local = EditorHelper.mStrTableAssetFolder + info.ouput_asset_name;
                    Debug.Log(local);
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(local, typeof(UnityEngine.Object));
                    objs.Add(obj);
                }
            }
        }

        if (GUILayout.Button("Package_All (PC)", GUILayout.Width(256)))
        {
            TableData_Package(objs, BuildTarget.StandaloneWindows);
        }

        if (GUILayout.Button("Package_All (Android)", GUILayout.Width(256)))
        {
            TableData_Package(objs, BuildTarget.Android);
        }

        if (GUILayout.Button("Package_All (IOS)", GUILayout.Width(256)))
        {
            TableData_Package(objs, BuildTarget.iOS);
        }
        AssetDatabase.Refresh();
        GUILayout.Space(20);

        #endregion
    }

    void ClearDataAB()
    {
        string _path = Application.streamingAssetsPath + "/table";
        DirectoryInfo dirs = new DirectoryInfo(_path);

        if(dirs==null||(!dirs.Exists))//不存在，就创建一个空目录
		{
            Directory.CreateDirectory(_path);
            AssetDatabase.Refresh();
			return ;
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

    void LoadAsset(string _asset)
    {
        profile = AssetDatabase.LoadAssetAtPath(EditorHelper.mStrAssetProfileFolder + _asset, typeof(TableProcessProfile)) as TableProcessProfile;
        if (profile == null)
        {
            Debug.LogError(EditorHelper.mStrAssetProfileFolder + "mStrProfileName" + _asset + " no found, please create it first!");
        }
        profile.tableInfos.Sort(new TableProcessComparer());
    }

    private class TableProcessComparer : IComparer<TableProcessInfo>
    {
        public int Compare(TableProcessInfo _a, TableProcessInfo _b)
        {
            if (_a != null && _a.input_excel != null && _b != null && _b.input_excel != null)
            {
                string aName = _a.input_excel.name;
                string bName = _b.input_excel.name;
                return string.Compare(aName, bName);
            }
            return 0;
        }
    }

    public static string mStrExcelFolder = "Assets/Tables/";
    public static string mStrScriptableObjectFolder = "Assets/ScriptableObject/";

    public static Worksheet LoadExcelSheet(string _excelFileName, string _sheetName)
    {
        Workbook book = new Workbook(mStrExcelFolder + _excelFileName);
        foreach (Worksheet sheet in book.Worksheets)
        {
            if (sheet.Name == _sheetName)
            {
                return sheet;
            }
        }
        Debug.LogError("can't find the sheet you want " + _sheetName);
        return null;
    }

    void ExportTable(TableProcessInfo _tableInfo)
    {
        Worksheet sheet = EditorHelper.LoadExcelSheet(AssetDatabase.GetAssetPath(_tableInfo.input_excel), _tableInfo.sheet_name);

        if(sheet.Cells.Rows.Count<=LINE_NUM_VALUE_TYPE)
        {
            Debug.LogError(string.Format("sheet.Cells.Rows.Count <= {0}",LINE_NUM_VALUE_TYPE));
        }

        /*
         * row.Index == 0是第一行，行列都是从0开始
         * sheet.Cells.Count 是所有有内容的格子个数，也就是说，标准矩形表格外的格子也会读进来，所以需要筛选
         * sheet.Cells.MaxDataColumn 非空格子的最大列数
         * 
         * 配表规则定死：
         *      第一行是 字段变量名称（中文）
         *      第二行是 字段变量类型（字符串，string,int（暂时只有两种））
         *      第三行是 字段变量名称（英文，代码中可用）
         */

        //TODO: parse title 
        ParseTableTitle(sheet);

        //TODO: parse table content
        ParseTableContent(sheet);

        //TODO: init ScriptableObject
        InitScriptableObjects(_tableInfo);
    }

    private void ParseTableTitle(Worksheet sheet)
    {
        /*
         * 查找表头三行，全部不为空的，为有效列，定出有效列数上限
         */
        mDicExeclTitles.Clear();

        //Line 0 LINE_NUM_VALUE_NAME_CH
        Row row = sheet.Cells.Rows[LINE_NUM_VALUE_NAME_CH];
        //              --- 每行首列如果为空，则为不合法行，即有ID(能检索)才能合法，否则忽略这行
        if (row.GetCellOrNull(0) != null && !string.IsNullOrEmpty(row.GetCellOrNull(0).StringValue))
        {
            for (int j = 0; j <= sheet.Cells.MaxDataColumn; ++j)
            {
                string value = row.GetCellOrNull(j) != null ? row.GetCellOrNull(j).StringValue : "";
                if (mDicExeclTitles.ContainsKey(j))
                {
                    mDicExeclTitles[j].strArrValueNamesCH = value.Trim();
                }
                else
                {
                    ExcelTitle tmpTitle = new ExcelTitle();
                    tmpTitle.strArrValueNamesCH = value.Trim();
                    mDicExeclTitles.Add(j, tmpTitle);
                }
            }
        }

        //Line 1 LINE_NUM_VALUE_TYPE
        row = sheet.Cells.Rows[LINE_NUM_VALUE_TYPE];
        if (row.GetCellOrNull(0) != null && !string.IsNullOrEmpty(row.GetCellOrNull(0).StringValue))
        {
            for (int j = 0; j <= sheet.Cells.MaxDataColumn; ++j)
            {
                string value = row.GetCellOrNull(j) != null ? row.GetCellOrNull(j).StringValue : "";
                if (mDicExeclTitles.ContainsKey(j))
                {
                    mDicExeclTitles[j].strArrValueType = value.Trim();
                }
                else
                {
                    ExcelTitle tmpTitle = new ExcelTitle();
                    tmpTitle.strArrValueType = value.Trim();
                    mDicExeclTitles.Add(j, tmpTitle);
                }
            }
        }

        //Line 2 LINE_NUM_VALUE_NAME_EN
        row = sheet.Cells.Rows[LINE_NUM_VALUE_NAME_EN];
        if (row.GetCellOrNull(0) != null && !string.IsNullOrEmpty(row.GetCellOrNull(0).StringValue))
        {
            for (int j = 0; j <= sheet.Cells.MaxDataColumn; ++j)
            {
                string value = row.GetCellOrNull(j) != null ? row.GetCellOrNull(j).StringValue : "";
                if (mDicExeclTitles.ContainsKey(j))
                {
                    mDicExeclTitles[j].strArrValueNamesEN = value.Trim();
                }
                else
                {
                    ExcelTitle tmpTitle = new ExcelTitle();
                    tmpTitle.strArrValueNamesEN = value.Trim();
                    mDicExeclTitles.Add(j, tmpTitle);
                }
            }
        }
    }

    private void ParseTableContent(Worksheet sheet)
    {
        mDicExcelData.Clear();
        for (int i = LINE_NUM_VALUE_NAME_EN + 1; i < sheet.Cells.Rows.Count; ++i)
        {
            Row tmpRow = sheet.Cells.Rows[i];
            if (tmpRow.GetCellOrNull(0) != null && !string.IsNullOrEmpty(tmpRow.GetCellOrNull(0).StringValue))
            {
                for (int j = 0; j <= sheet.Cells.MaxDataColumn; ++j)
                {
                    string value = tmpRow.GetCellOrNull(j) != null ? tmpRow.GetCellOrNull(j).StringValue : "";
                    if (mDicExeclTitles.ContainsKey(j) && mDicExeclTitles[j].IsValid())
                    {
                        if( mDicExcelData.ContainsKey(i) )
                        {
                            mDicExcelData[i].Add(j, value.Trim());
                        }
                        else
                        {
                            mDicExcelData.Add(i, new Dictionary<int, string>());
                            mDicExcelData[i].Add(j, value.Trim());
                        }
                    }
                }
            }
        }
    }



    private void InitScriptableObjects(TableProcessInfo _tableInfo)
    {
        if (asm == null)
        {
            asm = Assembly.Load("Assembly-CSharp");
        }

        //---
        ScriptableObject oTable
            = EditorHelper.CreateNewEditorProfile<ScriptableObject>(_tableInfo.ouput_asset_name + ".asset",
                                                                    EditorHelper.mStrTableAssetFolder,
                                                                    _tableInfo.table_class_name);
        if (oTable == null)
            return;

        //得到指定资源路径  
        string path = EditorHelper.mStrTableAssetFolder + _tableInfo.ouput_asset_name + ".asset";
        AssetImporter ai = AssetImporter.GetAtPath(path);
        ai.assetBundleName = EditorHelper.mStrTableData_ABName;

        ////---
        System.Type tData = asm.GetType(_tableInfo.data_class_name);

        foreach (KeyValuePair<int, Dictionary<int, string>> pair in mDicExcelData)
        {
            System.Object oData = System.Activator.CreateInstance(tData);
            foreach (KeyValuePair<int, string> rowPair in pair.Value)
            {
                object param = EditorHelper.GetTableValue(mDicExeclTitles[rowPair.Key].strArrValueType, rowPair.Value);
                FieldInfo mInfo = tData.GetField(mDicExeclTitles[rowPair.Key].strArrValueNamesEN, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (mInfo != null)
                {
                    //Debug.Log(mDicExeclTitles[rowPair.Key].strArrValueNamesEN + " = " + param);
                    mInfo.SetValue(oData, param);
                }
            }

            IDataList lst = oTable as IDataList;
            if (lst != null)
            {
                lst.AddDataObj(oData);
            }
        }
        EditorUtility.SetDirty(oTable);
        AssetDatabase.SaveAssets();
    }

    private void TableData_Package(List<UnityEngine.Object> _objs, BuildTarget _buildTarget)
    {
        ClearDataAB();
        //打包为一个文件       
        //--- 不同平台需要各自打包
        BuildPipeline.BuildAssetBundles(EditorHelper.mStrTableAssetBundleTotalFolder,
            BuildAssetBundleOptions.None,
            _buildTarget);

//#if UNITY_EDITOR
//        BuildPipeline.BuildAssetBundles(EditorHelper.mStrTableAssetBundleTotalFolder,
//            BuildAssetBundleOptions.None,
//            _buildTarget);
//        //BuildPipeline.BuildAssetBundles(EditorHelper.mStrTableAssetBundleFolder);

//#elif UNITY_IOS
//            BuildPipeline.BuildAssetBundle(oTable, 
//                null, 
//                EditorHelper.mStrTableAssetBundleFolder + _tableInfo.ouput_asset_name + ".assetbundle", 
//                BuildAssetBundleOptions.CollectDependencies, 
//                BuildTarget.iOS);
//#elif UNITY_ANDROID
//            BuildPipeline.BuildAssetBundle(oTable, 
//                null, 
//                EditorHelper.mStrTableAssetBundleFolder + _tableInfo.ouput_asset_name + ".assetbundle", 
//                BuildAssetBundleOptions.CollectDependencies, 
//                BuildTarget.Android);
//#endif
    }
}
