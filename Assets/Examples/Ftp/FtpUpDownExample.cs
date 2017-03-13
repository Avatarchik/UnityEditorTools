using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using System.Collections.Generic;
using ConvertData;
using System.Threading;
using System.Net;

public class FtpUpDownExample : MonoBehaviour
{

    Text _ShowResult;
    InputField _ServerIP;
    InputField _UserID;
    InputField _Password;

    string ftpServerIP = "";
    string ftpUserID = "";
    string ftpPassword = "";

	void Start () {

        _ShowResult = GameObject.Find("ShowResult").GetComponent<Text>();
        _ShowResult.text = "";

        _ServerIP = GameObject.Find("FtpServerPath").GetComponent<InputField>();
        _ServerIP.text = "";
        _UserID = GameObject.Find("UserID").GetComponent<InputField>();
        _UserID.text = "";
        _Password = GameObject.Find("Password").GetComponent<InputField>();
        _Password.text = "";

        FtpUpDown _ftp = new FtpUpDown(ftpServerIP, ftpUserID, ftpPassword);
        if (_ftp == null)
        {
            Debug.LogError("Ftp Init Error !!!");
            return;
        }

	}

    public void OnClick_ShowFtpFileList()
    {
        ftpServerIP = _ServerIP.text;
        if (string.IsNullOrEmpty(ftpServerIP))
        {
            Debug.LogError("ftpServerIP Input is Error !!!");
            return;
        }

        ftpUserID = _UserID.text;
        if (string.IsNullOrEmpty(ftpUserID))
        {
            Debug.LogError("ftpUserID Input is Error !!!");
            return;
        }

        ftpPassword = _Password.text;
        if (string.IsNullOrEmpty(ftpPassword))
        {
            Debug.LogError("ftpPassword Input is Error !!!");
            return;
        }

        ConvertData.FtpUpDown _ftp = new FtpUpDown(ftpServerIP, ftpUserID, ftpPassword);
        if(_ftp == null)
        {
            return;
        }

        string[] _list = _ftp.GetFileList();
        for (int i = 0; i < _list.Length;++i )
        {
            _ShowResult.text += _list[i] + " \n";
        }
    }

	void Update () {
	}
}
