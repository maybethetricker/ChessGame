using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    public InputField idInput;
    public InputField pwInput;
    public Button loginBtn;
    public Button regBtn;
    public Text Warning;
    string host = "114.55.95.111"; 
    int port = 1234; 
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
    }
    void OnRegClick()
    {
        loginBtn.onClick.RemoveAllListeners();
        regBtn.onClick.RemoveAllListeners();
        loginBtn.GetComponentInChildren<Text>().text = "注册";
        regBtn.GetComponentInChildren<Text>().text = "返回登录";
        loginBtn.onClick.AddListener(OnRegConfirm);
        Warning.text = "";
        regBtn.onClick.AddListener(BackToLogin);
    }
    void BackToLogin()
    {
        Warning.text = "";
        loginBtn.onClick.RemoveAllListeners();
        regBtn.onClick.RemoveAllListeners();
        loginBtn.GetComponentInChildren<Text>().text = "登录";
        regBtn.GetComponentInChildren<Text>().text = "注册";
        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
    }
    public void OnLoginClick()
    {
        if (idInput.text == "" || pwInput.text == "")
        {
            Warning.text = "用户名或密码为空";
            return;
        }
        if (NetMgr.srvConn.status != Connection.Status.Connected)
        {
            NetMgr.srvConn.proto = new ProtocolBytes();
            if(!NetMgr.srvConn.Connect(host, port))
                Warning.text = "服务器连接失败";
        }
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        NetMgr.srvConn.Send(protocol, OnLoginBack);
    }
    public void OnLoginBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 0)
        {
            Debug.Log("Login Succeed");
            //Start Your Game
            SceneManager.LoadScene("MainPage");
        }
        else
        {
            Warning.text = "用户名或密码错误";
            NetMgr.srvConn.status = Connection.Status.None;
        }
    }
    public void OnRegConfirm()
    {    //用户名、密码为空
        if (idInput.text == "" || pwInput.text == "")
        {
            Warning.text="用户名密码不能为空 !";
            return;
        }
        if (NetMgr.srvConn.status != Connection.Status.Connected)
        {
            NetMgr.srvConn.proto = new ProtocolBytes();
            NetMgr.srvConn.Connect(host, port);
        }    //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Register"); 
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        Debug.Log("发送 " + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol, OnRegBack);
    }
    public void OnRegBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0; 
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start); 
        if (ret == 0)
        {
            Warning.text="注册成功! 请返回登录";
            NetMgr.srvConn.status = Connection.Status.None;
        }
        else
        {
            Warning.text="注册失败! 请更换用户名";
            NetMgr.srvConn.status = Connection.Status.None;
        }
    }
}
