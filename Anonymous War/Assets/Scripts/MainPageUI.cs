using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Fungus;

public class MainPageUI : MonoBehaviour
{
    public Button StartGame;
    public Button StartWithAI;
    public Text ScoreText;
    public Button ChangeRank;
    public InputField ChangeRankInput;
    public Button OpenHint;
    public Button ExtraMode;
    public GameObject hint;
    public GameObject ButtonHolder;
    // Start is called before the first frame update
    void Start()
    {
        Root.instance.SkipPlot.gameObject.SetActive(true);
        ProtocolBytes prot = new ProtocolBytes();
        prot.AddString("SetScore");
        prot.AddInt(300);
        //NetMgr.srvConn.Send(prot);
        ScoreText.text = "-1";
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetScore");
        NetMgr.srvConn.Send(protocol);
        StartGame.onClick.RemoveAllListeners();
        StartGame.onClick.AddListener(StartMatch);
        StartWithAI.onClick.AddListener(StartFightWithAI);
        ChangeRankInput.gameObject.SetActive(false);
        ChangeRank.onClick.AddListener(ChangeScore);
        ExtraMode.onClick.AddListener(StartExtraMode);
        if(Root.instance.Authority!=0)
            ChangeRank.gameObject.SetActive(false);
        Root.instance.flowchart.SetBooleanVariable("Started", false);
        Root.instance.flowchart.SetBooleanVariable("Finnished", false);
        hint.SetActive(false);
        OpenHint.onClick.AddListener(delegate { hint.SetActive(true); });
        StartCoroutine(DynamicButtonCreate(new Vector3(0,0,0),100,false));
    }

    // Update is called once per frame
    void Update()
    {
        if (Root.instance.flowchart.GetBooleanVariable("Finnished"))
        {
            //open guide
            Root.instance.flowchart.SetBooleanVariable("Finnished", true);
            GameManager.RealPlayerTeam = new List<string>();
            if(int.Parse(ScoreText.text) == 0)
                GameManager.Guide = 1;
            else if(int.Parse(ScoreText.text) == 50)
                GameManager.Guide = 3;
            else
            {
                return;
            }
            GameManager.UseAI = true;
            Root.instance.flowchart.SetBooleanVariable("RepeatCommand", false);
            Root.instance.flowchart.SetBooleanVariable("FinnishCommand", false);
            Root.instance.flowchart.SetIntegerVariable("GuideStep", 0);
            Root.instance.soundManager.ChangeClip();
            SceneManager.LoadScene("Guide");
        }

    }

    public void OpenGuidePlot()
    {
        if (int.Parse(ScoreText.text) == 0 && !Root.instance.flowchart.GetBooleanVariable("Started"))
        {
            Root.instance.flowchart.SendFungusMessage("Beginer1");
            Root.instance.flowchart.SetBooleanVariable("Started", true);
        }
        if(int.Parse(ScoreText.text) == 50 && !Root.instance.flowchart.GetBooleanVariable("Started"))
        {
            Root.instance.flowchart.SendFungusMessage("Beginer2");
            Root.instance.flowchart.SetBooleanVariable("Started", true);
        }
        if(int.Parse(ScoreText.text) == 100 && !Root.instance.flowchart.GetBooleanVariable("Started")
        &&Root.instance.OncePlotOpen)
        {
            Root.instance.flowchart.SendFungusMessage("Beginer4");
            Root.instance.flowchart.SetBooleanVariable("Started", true);
            Root.instance.OncePlotOpen = false;
        }
        if(int.Parse(ScoreText.text) == 250 || int.Parse(ScoreText.text)==450)
            Root.instance.OncePlotOpen = true;
        if(int.Parse(ScoreText.text) == 300 && !Root.instance.flowchart.GetBooleanVariable("Started")
        &&Root.instance.OncePlotOpen)
        {
            Root.instance.flowchart.SendFungusMessage("Beginer5");
            Root.instance.flowchart.SetBooleanVariable("Started", true);
            Root.instance.OncePlotOpen = false;
        }
        if(int.Parse(ScoreText.text) == 500 && !Root.instance.flowchart.GetBooleanVariable("Started")
        &&Root.instance.OncePlotOpen)
        {
            Root.instance.flowchart.SendFungusMessage("Beginer6");
            Root.instance.flowchart.SetBooleanVariable("Started", true);
            Root.instance.OncePlotOpen = false;
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
        GameManager.TeamCount = 2;
        GameManager.UseAI = true;
        GameManager.Guide = -1;
        Root.instance.soundManager.ChangeClip();
        Root.instance.Quit.gameObject.SetActive(false);
        Root.instance.GiveIn.gameObject.SetActive(true);
        Root.instance.GiveIn.onClick.RemoveAllListeners();
        Root.instance.GiveIn.onClick.AddListener(delegate ()
        {
            string winnerNotice = "";
            winnerNotice = "失 败";
            Root.instance.Quit.gameObject.SetActive(true);
            Root.instance.GiveIn.gameObject.SetActive(false);
            Root.instance.OptionPanel.SetActive(false);
            Root.instance.soundManager.ChangeClip();
            GameManager.instance.WinnerNotice.color = GameManager.instance.LostColor;
            GameManager.instance.WinnerText.text = winnerNotice;
            GameManager.instance.WinnerNotice.gameObject.SetActive(true);
            //SceneManager.LoadScene("MainPage");
        });
        SceneManager.LoadScene("Game");
    }

    void ChangeScore()
    {
        ScoreText.text = "";
        ChangeRankInput.gameObject.SetActive(true);
        ChangeRank.GetComponentInChildren<Text>().text="确认";
        ChangeRank.onClick.RemoveAllListeners();
        ChangeRank.onClick.AddListener(delegate {
            int score;
            try{
                score = int.Parse(ChangeRankInput.text);
                if(!System.Text.RegularExpressions.Regex.IsMatch(ChangeRankInput.text,"^[0-9]+$"))
                    return;
            } 
            catch{
                Debug.Log("Wrong input");
                ChangeRank.GetComponentInChildren<Text>().text="更改天梯分";
                ProtocolBytes prot2 = new ProtocolBytes();
                prot2.AddString("GetScore");
                NetMgr.srvConn.Send(prot2);
                ChangeRank.onClick.RemoveAllListeners();
                ChangeRank.onClick.AddListener(ChangeScore);
                ChangeRankInput.gameObject.SetActive(false);
                return;
            }
            ProtocolBytes prot = new ProtocolBytes();
            prot.AddString("SetScore");
            prot.AddInt(score);
            NetMgr.srvConn.Send(prot);
            prot = new ProtocolBytes();
            prot.AddString("GetScore");
            NetMgr.srvConn.Send(prot);
            ChangeRank.GetComponentInChildren<Text>().text="更改天梯分";
            ChangeRank.onClick.RemoveAllListeners();
            ChangeRank.onClick.AddListener(ChangeScore);
            ChangeRankInput.gameObject.SetActive(false);
        });
    }
    void StartExtraMode()
    {
        StartCoroutine(DynamicButtonCreate(new Vector3(-284,0,0),120,true));
        
    }

    IEnumerator DynamicButtonCreate(Vector3 Aim,float Speed,bool OpenScene)
    {
        while (Vector3.Distance(ButtonHolder.transform.position,Aim)>0.1f)
        {
            ButtonHolder.transform.position = Vector3.MoveTowards(ButtonHolder.transform.position, Aim, Speed * Time.deltaTime);
            yield return 0;
        }
        ButtonHolder.transform.position = Aim;
        if(OpenScene)
            SceneManager.LoadScene("RelaxMode");
    }
}
