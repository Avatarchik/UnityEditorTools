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


    #region TablePack Define

    //Unity 5.x 每个资源都会有一个ABName，相同ABName的资源会被打入同一个AB包中
    public static string mStrTableData_ABName = "table_data";//Excel表格的标签

    public static string mStrExcelFolder = "Assets/_Share/Tables/";
    public static string mStrScriptableObjectFolder = "Assets/_Share/ScriptableObject/";

    public static string mStrTableAssetFolder = "Assets/_Share/TableAsset/";


    /*
     * Application.streamingAssetsPath 也就是 StreamingAssets 下面用 _Table命名子文件夹就找不到，不懂为什么
     * + "/_Table"就找不到 真的不懂为什么
     */
    public static string mStrTableAssetBundleTotalFolder = EditorHelper.bundlePath + "/Table";//数据表格整包路径
    //public static string mStrTableAssetBundleTotalFolder = EditorHelper.bundlePath;//数据表格整包路径

    public static string mStrAssetProfileFolder = "Assets/_Share/_Profile/";

    #endregion

    #region 生成TableProcessProfile //一般只执行一次，创建配置文件列表
    protected TableProcessProfile profile;
    public static string mStrProfileName = "TableProcessProfile_System.asset";

    //一般只执行一次，创建配置文件列表
    [MenuItem("Assets/Create/Custom Assets/CreatTableProcess")]
    static void CreateExcelTableAsset()
    {
        TableProcessProfile _profile = AssetDatabase.LoadAssetAtPath(mStrAssetProfileFolder + mStrProfileName,
                                      typeof(TableProcessProfile)) as TableProcessProfile;
        if (_profile == null)
        {
            _profile = EditorHelper.CreateNewEditorProfile<TableProcessProfile>("TableProcessProfile_System.asset", mStrAssetProfileFolder, "TableProcessProfile");
        }
    }
    #endregion

    protected Assembly asm;

    [MenuItem("Helper/Table Window")]
    public static TableWindow NewWindow()
    {
        //TableWindow newWindow = EditorWindow.GetWindow<TableWindow>();
        //GetWindow<TableWindow>();
        return GetWindow<TableWindow>();
    }

    /*
    * 配表规则：
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

        #region excel_2_*.asset

        GUILayout.Space(20);
        //（离散包）从Excel读取数据，然后每个 TableProcessInfo info in profile.tableInfos 打成自己单独的一个*.asset文件 
        // 并给他们 assetBundleName 都赋值 EditorHelper.mStrTableData_ABName
        
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
        #endregion

        #region 设置标签（assetBundleName）
        GUILayout.Space(20);
        if (GUILayout.Button("Set Table assetBundleName ", GUILayout.Width(400)))
        {
            SetAssetBundleName();
            AssetDatabase.Refresh();
        }
        #endregion

        #region 从离散包(*.asset)路径读取所有离散包，根据他们共有的标签（assetBundleName）打成一个文件（整包）
        GUILayout.Space(20);
        if (GUILayout.Button("Package_Table (根据PlayerSetting里面的平台输出到相应的目录)", GUILayout.Width(400)))
        {
            TableData_Package(EditorHelper.buildTarget);
            AssetDatabase.Refresh();
        }

        #endregion
    }

    void LoadAsset(string _asset)
    {
        profile = AssetDatabase.LoadAssetAtPath(mStrAssetProfileFolder + _asset, typeof(TableProcessProfile)) as TableProcessProfile;
        if (profile == null)
        {
            Debug.LogError(mStrAssetProfileFolder + "mStrProfileName" + _asset + " no found, please create it first!");
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
         * 配表规则：
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
                                                                    mStrTableAssetFolder,
                                                                    _tableInfo.table_class_name);
        if (oTable == null)
            return;

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

    private void SetAssetBundleName()
    {
        DirectoryInfo dirs = new DirectoryInfo(mStrTableAssetFolder);
        if (dirs == null || (!dirs.Exists))
        {
            Debug.LogError(mStrTableAssetFolder + " 路径为不存在");
            return;
        }

        FileInfo[] files = dirs.GetFiles();
        for (int i = 0; i < files.Length; ++i)
        {
            if (!files[i].Name.EndsWith(".meta"))
            {
                AssetImporter ai = AssetImporter.GetAtPath(mStrTableAssetFolder + files[i].Name);
                ai.assetBundleName = mStrTableData_ABName;
                ai.assetBundleVariant = "unity3d";
                ai.SaveAndReimport();
            }
        }
    }

    private void ClearDataAB()
    {
        string _path = mStrTableAssetBundleTotalFolder;
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

    private void TableData_Package(BuildTarget _buildTarget)
    {

        ClearDataAB();      

        DirectoryInfo dirs = new DirectoryInfo(mStrTableAssetFolder);

        if (dirs == null || (!dirs.Exists))
        {
            Debug.LogError(mStrTableAssetFolder + " 路径为不存在");
            return;
        }

        FileInfo[] files = dirs.GetFiles();

        AssetBundleBuild[] builds = new AssetBundleBuild[1];
        builds[0].assetBundleName = mStrTableData_ABName;
        builds[0].assetBundleVariant = "unity3d";

        List<string> _assetNames = new List<string>();
        for (int i = 0; i < files.Length; ++i)
        {
            if (!files[i].Name.EndsWith(".meta"))
            {
                _assetNames.Add(mStrTableAssetFolder + files[i].Name);
            }
        }
        builds[0].assetNames = _assetNames.ToArray();

        UnityEngine.AssetBundleManifest _abM = BuildPipeline.BuildAssetBundles(mStrTableAssetBundleTotalFolder, 
            builds, 
            BuildAssetBundleOptions.None,
            _buildTarget);

        if (_abM != null)
        {
            EditorUtility.DisplayDialog("Excel已经打包完成", "", "确定");
            AssetDatabase.Refresh();
        }
        else
        {
            EditorUtility.DisplayDialog("Excel打包出错", "", "确定");
        }
    }
}
