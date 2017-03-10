using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HeroDataTable : ScriptableObject, IDataList
{
    public List<HeroData> dataList = new List<HeroData>();

    public System.Collections.IEnumerable GetEnumerator()
    {
        return dataList;
    }

    public void AddDataObj(System.Object _data)
    {
        dataList.Add((HeroData)_data);
    }
}

[System.Serializable]
public class HeroData
{
    public int id;
    public string name;
    public string describe;
    public int avatarID;
    public string speak;
}
