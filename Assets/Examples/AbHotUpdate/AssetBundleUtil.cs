using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Collections;
using System.Collections.Generic;
namespace AssetBundles
{
    public class Util
    {
        public Util()
        {
        }


        public static Dictionary<string, string> GetMD5DicByFilePath(string _filePath)
        {
            Dictionary<string, string> _dic = new Dictionary<string, string>();

            if (System.IO.File.Exists(_filePath) == false)
            {
                Debug.LogError(_filePath + " not Exists !!!");
                return _dic;
            }

            string[] lineText = File.ReadAllLines(_filePath);

            foreach (string x in lineText)
            {
                string[] _strs = x.Split('%');
                if (_strs.Length != 4)
                {
                    Debug.LogError("Error Line in file : " + _filePath + " , Line : " + x);
                    continue;
                }

                //FileName%table_data.unity3%MD5%21-B9-03-7B-3B-EA-16-E3-FD-65-8C-47-A6-E8-59-BC
                if (_strs.Length == 4 && _strs[0] == "FileName" && _strs[2] == "MD5")
                {
                    string _fileName = _strs[1];
                    string _md5 = _strs[3];
                    if (!string.IsNullOrEmpty(_fileName) && !string.IsNullOrEmpty(_md5))
                    {
                        if (!_dic.ContainsKey(_fileName))
                            _dic.Add(_fileName, _md5);
                        else _dic[_fileName] = _md5;
                    }
                }
                else
                {
                    Debug.LogError("Error Line in file : " + _filePath + " , Line : " + x);
                    continue;
                }
            }
            return _dic;
        }
    }
}