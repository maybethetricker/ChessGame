using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainPageUI : MonoBehaviour
{
    public Button StartGame;
    public Button Help;
    public GameObject HelpPage;
    public Button CloseHelp;
    // Start is called before the first frame update
    void Start()
    {
        StartGame.onClick.AddListener(StartMatch);
        Help.onClick.AddListener(OpenHelp);
        CloseHelp.onClick.AddListener(closeHelp);
        HelpPage.SetActive(false);
        CloseHelp.gameObject.SetActive(false);
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

    void OpenHelp()
    {
        HelpPage.SetActive(true);
        CloseHelp.gameObject.SetActive(true);
    }
    void closeHelp()
    {
        HelpPage.SetActive(false);
        CloseHelp.gameObject.SetActive(false);
    }
}
