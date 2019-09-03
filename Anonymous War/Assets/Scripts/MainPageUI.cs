using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainPageUI : MonoBehaviour
{
    public Button StartGame;
    // Start is called before the first frame update
    void Start()
    {
        StartGame.onClick.AddListener(StartMatch);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void StartMatch()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartMatch");
        NetMgr.srvConn.Send(protocol);
        StartGame.onClick.RemoveAllListeners();
    }


}
