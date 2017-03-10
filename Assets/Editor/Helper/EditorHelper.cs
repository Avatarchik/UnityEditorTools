using System.IO;
using UnityEditor;
using UnityEngine;
using Aspose.Cells;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum TableValueType
{
    INT,
    LONG,
    FLOAT,
    BOOL,
    STRING,
    ENUM,
    CUSTOM,
};


public class EditorHelper
{
    public static BuildTarget buildTarget =
#if UNITY_ANDROID
        BuildTarget.Android;
#elif UNITY_STANDALONE_OSX || UNITY_IPHONE
        BuildTarget.iOS;
#else
        BuildTarget.StandaloneWindows;
#endif

    //Unity 5.x 每个资源都会有一个ABName，相同ABName的资源会被打入同一个AB包中
    public static string mStrTableData_ABName = "table_data";

    public static string mStrExcelFolder = "Assets/_Share/Tables/";
    public static string mStrScriptableObjectFolder = "Assets/_Share/ScriptableObject/";

    public static string mStrTableAssetFolder = "Assets/_Share/_TableAsset/";

    //数据表格整包路径
    /*
     * Application.streamingAssetsPath 也就是 StreamingAssets 下面用 _Table命名子文件夹就找不到，不懂为什么
     * + "/_Table"就找不到 真的不懂为什么
     */
    public static string mStrTableAssetBundleTotalFolder = Application.streamingAssetsPath + "/table";

    public static string mStrAssetProfileFolder = "Assets/_Share/_Profile/";


    public static T CreateNewEditorProfile<T>(string _profileName, string _path = "", string _class = "") where T : ScriptableObject
    {
        string path = Path.Combine(_path, _profileName);

        if (string.IsNullOrEmpty(_profileName))
        {
            Debug.LogError("can't create asset, the name is empty");
            return null;
        }

        if (_class.Length != 0)
        {
            ScriptableObject obj = ScriptableObject.CreateInstance(_class);
            if (obj is T)
            {
                AssetDatabase.CreateAsset(obj, path);
                Selection.activeObject = obj;
                return obj as T;
            }
            return null;
        }
        return null;
    }

    public static WorksheetCollection LoadExcelSheets(string _excelFilePath)
    {
        if (!string.IsNullOrEmpty(_excelFilePath))
        {
            Workbook book = new Workbook(_excelFilePath);
            return book.Worksheets;
        }
        return null;
    }

    public static Worksheet LoadExcelSheet(string _excelFilePath, string _sheetName)
    {
        Workbook book = new Workbook(_excelFilePath);
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

    public static object GetTableValue(string _valueType, string _valueString)
    {
        if (string.Compare(TableValueType.INT.ToString(), _valueType, true) == 0)
        {
            int value = 0;
            _valueString.Trim();

            if (int.TryParse(_valueString, out value))
            {
                return value;
            }
            return null;
        }
        else if (string.Compare(TableValueType.LONG.ToString(), _valueType, true) == 0)
        {
            long value = 0;
            _valueString.Trim();

            if (long.TryParse(_valueString, out value))
            {
                return value;
            }
            return null;
        }
        else if (string.Compare(TableValueType.FLOAT.ToString(), _valueType, true) == 0)
        {
            float value = 0;
            _valueString.Trim();

            if (float.TryParse(_valueString, out value))
            {
                return value;
            }
            return null;
        }
        else if (string.Compare(TableValueType.BOOL.ToString(), _valueType, true) == 0)
        {
            bool value = false;
            _valueString.Trim();

            if (bool.TryParse(_valueString, out value))
            {
                return value;
            }
            return null;
        }
        else if (string.Compare(TableValueType.STRING.ToString(), _valueType, true) == 0)
        {
            _valueString.TrimEnd();
            return _valueString;
        }
        else if (string.Compare(TableValueType.CUSTOM.ToString(), _valueType, true) == 0)
        {
            return _valueString;
        }

        return null;
    }

    static public void SetValue(string _valueType, FieldInfo mInfo, object c, object param)
    {
        if (string.Compare(TableValueType.INT.ToString(), _valueType, true) == 0)
        {
            List<int> temp = mInfo.GetValue(c) as List<int>;
            if (temp == null)
                temp = new List<int>();
            if (param != null)
                temp.Add((int)param);
            mInfo.SetValue(c, temp);
        }
        else if (string.Compare(TableValueType.STRING.ToString(), _valueType, true) == 0)
        {
            List<string> temp = mInfo.GetValue(c) as List<string>;
            if (temp == null)
                temp = new List<string>();
            if (param != null && !string.IsNullOrEmpty((string)param))
                temp.Add((string)param);
            mInfo.SetValue(c, temp);
        }
    }
}
