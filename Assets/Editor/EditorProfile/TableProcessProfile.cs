
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class TableProcessInfo
{
	public string ouput_asset_name;
    public string table_class_name;
	public string data_class_name;
    public string sheet_name = "CharacterTable";
    public bool xmlExec = false;
    public bool newExec = false;

    // excel
	public Object input_excel;

    public bool IsClientUse
    {
        get
        {
            return !string.IsNullOrEmpty(table_class_name);
        }
    }
}

public class TableProcessProfile : ScriptableObject {

	public List<TableProcessInfo> tableInfos;
}
