using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Fungus;

public class MainPageUI : MonoBehaviour
{
    public Button StartGame;
    public Button Help;
    public GameObject HelpPage;
    public Button CloseHelp;
    public Button StartWithAI;
    public Text ScoreText;
    // Start is called before the first frame update
    void Start()
    {
        ProtocolBytes prot = new ProtocolBytes();
        prot.AddString("SetScore");
        prot.AddInt(0);
        NetMgr.srvConn.Send(prot);
        ScoreText.text = "-1";
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetScore");
        NetMgr.srvConn.Send(protocol);
        StartGame.onClick.RemoveAllListeners();
        StartGame.onClick.AddListener(StartMatch);
        Help.onClick.AddListener(OpenHelp);
        CloseHelp.onClick.AddListener(closeHelp);
        HelpPage.SetActive(false);
        CloseHelp.gameObject.SetActive(false);
        StartWithAI.onClick.AddListener(StartFightWithAI);
    }

    // Update is called once per frame
    void Update()
    {
        if (int.Parse(ScoreText.text) == 0 && !Root.instance.flowchart.GetBooleanVariable("Started"))
        {
            Root.instance.flowchart.SendFungusMessage("Beginer1");
            Root.instance.flowchart.SetBooleanVariable("Started", true);
        }
        if (Root.instance.flowchart.GetBooleanVariable("Finnished"))
        {
            //open guide
            Root.instance.flowchart.SetBooleanVariable("Finnished", true);
            GameManager.RealPlayerTeam = new List<string>();
            if(int.Parse(ScoreText.text) == 0)
                GameManager.Guide = 1;
            if(int.Parse(ScoreText.text) == 50)
                GameManager.Guide = 2;
            if(int.Parse(ScoreText.text) == 100)
                GameManager.Guide = 3;
            GameManager.UseAI = true;
            Root.instance.Quit.gameObject.SetActive(false);
            SceneManager.LoadScene("Guide");
        }

    }

    void StartMatch()
    {
        if(int.Parse(ScoreText.text)<0)
        {
            Root.instance.ShowNotice("网络较差，请稍后尝试", "好的", delegate () { });
            return;
        }
        int score = int.Parse(ScoreText.text);
        //StartGame.GetComponentInChildren<Text>().text = "匹配中";
        //StartWithAI.gameObject.SetActive(false);
        Root.instance.ShowNotice("匹配中", "取消", delegate ()
        {
            ProtocolBytes protocol2 = new ProtocolBytes();
            protocol2.AddString("StopMatch");
            NetMgr.srvConn.Send(protocol2);
        });
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartMatch");
        protocol.AddInt(Root.instance.FindMode(score));
        NetMgr.srvConn.Send(protocol);
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
    void StartFightWithAI()
    {
        if(int.Parse(ScoreText.text)<0)
        {
            Root.instance.ShowNotice("网络较差，请稍后尝试", "好的", delegate () { });
            return;
        }
        int score = int.Parse(ScoreText.text);
        GameManager.Mode = Root.instance.FindMode(score);
        GameManager.IsTraining = true;
        int rand = Random.Range(0, 2);
        GameManager.RealPlayerTeam = new List<string>();
        GameManager.RealPlayerTeam.Add("Team" + (rand + 1).ToString());
        GameManager.UseAI = true;
        GameManager.Guide = -1;
        Root.instance.Quit.GetComponentInChildren<Text>().text = "投降";
        Root.instance.Quit.onClick.RemoveAllListeners();
        Root.instance.Quit.onClick.AddListener(delegate ()
        {
            string winnerNotice = "";
            winnerNotice = "失败";
            Root.instance.Quit.GetComponentInChildren<Text>().text = "退出";
            Root.instance.Quit.onClick.RemoveAllListeners();
            Root.instance.Quit.onClick.AddListener(delegate () { Application.Quit(); });
            Root.instance.ShowNotice(winnerNotice, "返回", delegate () {
                SceneManager.LoadScene("MainPage");
            });
            //SceneManager.LoadScene("MainPage");
        });
        SceneManager.LoadScene("Game");
    }
}
