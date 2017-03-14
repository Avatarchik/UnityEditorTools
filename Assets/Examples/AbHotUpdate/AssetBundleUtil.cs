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

            return GetMD5DicByFileString(File.ReadAllText(_filePath));
        }

        public static Dictionary<string, string> GetMD5DicByFileString(string _fileString)
        {
            Dictionary<string, string> _dic = new Dictionary<string, string>();

            string[] lineText = _fileString.Split('\n');
            foreach (string x in lineText)
            {
                if (!IsValidMd5Text(x))
                {
                    continue;
                }

                string[] _keyValue = x.Trim('\r').Split('%');

                string _fileName = _keyValue[1];
                string _md5 = _keyValue[3];
                if (!_dic.ContainsKey(_fileName))
                    _dic.Add(_fileName, _md5);
                else _dic[_fileName] = _md5;
            }
            return _dic;
        }

        public static bool IsValidMd5Text(string _md5TextLine)
        {
            if (string.IsNullOrEmpty(_md5TextLine))
            {
                //Debug.LogError("Null String : AssetBundles.Util.IsValidMd5Text(string)");
                return false;
            }

            string[] _keyValue = _md5TextLine.Trim('\r').Split('%');

            //FileName%table_data.unity3%MD5%21-B9-03-7B-3B-EA-16-E3-FD-65-8C-47-A6-E8-59-BC
            if (_keyValue.Length == 4 && _keyValue[0] == "FileName" && _keyValue[2] == "MD5"
                && !string.IsNullOrEmpty(_keyValue[1]) && !string.IsNullOrEmpty(_keyValue[3]))
            {
                return true;
            }
            Debug.LogError("Error _md5Text : " + _md5TextLine);
            return false;
        }
    }
}