using System;
using System.Collections;

public interface IDataList {

    void AddDataObj(Object _data);
    IEnumerable GetEnumerator();
}

