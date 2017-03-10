using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelInfoTable : ScriptableObject, IDataList
{
    public List<LevelInfoRef> dataList = new List<LevelInfoRef>();

    public System.Collections.IEnumerable GetEnumerator()
    {
        return dataList;
    }

    public void AddDataObj(System.Object _data)
    {
        dataList.Add((LevelInfoRef)_data);
    }
}

[System.Serializable]
public class LevelInfoRef
{
    public int levelIndex;
    public int nextIndex;
    public int rowNum;
    public int colNum;
    public int iconTypeNum;
    public int levelTargetType;
    public int qualmark1;
    public int qualmark2;
    public int qualmark3;
    public int operationNum;
    public int limitTime;
}
