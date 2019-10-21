using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fungus;

public class Root : MonoBehaviour
{
    public static Root instance;
    public Button Quit;
    public GameObject StaticUI;
    public GameObject Notice;
    public Text NoticeText;
    public Button ConfirmNotice;
    public Flowchart flowchart;
    public bool UseLimitClick;
    public GameObject LimitClickException;
    //public GameObject BlockUIPanel;
    public delegate void VoidDelegate();
    public VoidDelegate LimitClickFinished;

    //public GameObject Plot;
    //public Text PlotText;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(StaticUI);
        Notice.SetActive(false);
        //Plot.SetActive(false);
        Quit.onClick.AddListener(QuitGame);
        NetMgr.srvConn.msgDist.AddListener("StartFight", OnMatchBack);
        NetMgr.srvConn.msgDist.AddListener("SetBoard", NetSetBoard);
        NetMgr.srvConn.msgDist.AddListener("UpdateMove", NetMove);
        NetMgr.srvConn.msgDist.AddListener("UpdateAttack", NetAttack);
        NetMgr.srvConn.msgDist.AddListener("SkipMove", SkipMove);
        NetMgr.srvConn.msgDist.AddListener("SkipAttack", SkipAttack);
        NetMgr.srvConn.msgDist.AddListener("UpdateLand", NetLand);
        NetMgr.srvConn.msgDist.AddListener("FindMonster", NetFindArtifact);
        NetMgr.srvConn.msgDist.AddListener("CreateMonster", NetCreateArtifact);
        NetMgr.srvConn.msgDist.AddListener("EndGame", EndGame);
        NetMgr.srvConn.msgDist.AddListener("Logout", OnLogoutBack);
        NetMgr.srvConn.msgDist.AddListener("WinGame", WinGame);
        NetMgr.srvConn.msgDist.AddListener("GetScore", GetScore);
    }

    // Update is called once per frame
    void Update()
    {
        NetMgr.Update();
    }

    void NetLand(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Vector3 GroundPosition;
        GroundPosition.x = proto.GetFloat(start, ref start);
        GroundPosition.y = proto.GetFloat(start, ref start);
        GroundPosition.z = proto.GetFloat(start, ref start);
        //需要空降到的地块
        GameObject GroundToLand = null;
        //降落到对应地块上
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(t.position, GroundPosition) < BoardManager.distance / 2)
            {
                if (t.tag == "Weapon")
                    continue;
                GroundToLand = t.gameObject;
                break;
            }
        }
        //对接降落函数，可以不用看了
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(GroundToLand.transform.position, t.position) < BoardManager.distance / 2)
            {
                t.gameObject.GetComponent<GroundClick>().PlaceSinglePlayer();
                break;
            }
        }
    }
    void NetMove(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Vector3 MoverPosition, GroundPosition;
        MoverPosition.x = proto.GetFloat(start, ref start);
        MoverPosition.y = proto.GetFloat(start, ref start);
        MoverPosition.z = proto.GetFloat(start, ref start);
        GroundPosition.x = proto.GetFloat(start, ref start);
        GroundPosition.y = proto.GetFloat(start, ref start);
        GroundPosition.z = proto.GetFloat(start, ref start);
        //找到待移动棋子
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (Vector3.Distance(MoverPosition, GameManager.OccupiedGround[i].PlayerOnGround.transform.position) < BoardManager.distance / 2)
            {
                GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
                break;
            }

        }
        //要移动到的地块
        GameObject GroundToMove = null;
        //移动到对应地块上
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(t.position, GroundPosition) < BoardManager.distance / 2)
            {
                if (t.tag == "Weapon")
                    continue;
                GroundToMove = t.gameObject;
                break;
            }
        }
        if (GroundToMove == null)
        {
            Debug.Log("error");
        }
        //对接移动函数
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(GroundToMove.transform.position, t.position) < BoardManager.distance / 2)
            {
                t.gameObject.GetComponent<GroundClick>().PlayerMove();
                break;
            }
        }
    }

    void NetAttack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Vector3 EnemyPosition;
        EnemyPosition.x = proto.GetFloat(start, ref start);
        EnemyPosition.y = proto.GetFloat(start, ref start);
        EnemyPosition.z = proto.GetFloat(start, ref start);
        int attackMode = proto.GetInt(start, ref start);
        GameObject PlayerToAttack = null;
        //攻击对应棋子
        foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Players")
                continue;
            if (Vector3.Distance(EnemyPosition, t.position) < BoardManager.distance / 2)
            {
                PlayerToAttack = t.gameObject;
                break;
            }
        }
        GameObject Blood = null;
        int attack = 0;

        //对接攻击函数
        //获取反击攻击力，反击范围与双方血条
        GameObject thisBlood = null;
        string aimWeapon = "";
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (GameManager.OccupiedGround[i].PlayerOnGround == PlayerToAttack)
            {
                Blood = GameManager.OccupiedGround[i].PlayerBlood;
                aimWeapon = GameManager.OccupiedGround[i].PlayerWeapon;
            }
            if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
            {
                switch (GameManager.OccupiedGround[i].PlayerWeapon)
                {
                    case "Long": attack = 2; break;
                    case "Short": attack = 4; break;
                    case "Drag": attack = 1; break;
                    case "Tear": attack = 50; break;
                }
                thisBlood = GameManager.OccupiedGround[i].PlayerBlood;
            }
        }
        //对接攻击函数
        foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Players")
                continue;
            if (Vector3.Distance(PlayerToAttack.transform.position, t.position) < BoardManager.distance / 2)
            {
                if (t.tag == "Monster")
                        t.gameObject.GetComponent<ArtifactController>().Artifact.ArtOnHit();
                else
                {
                    if (attackMode == 0)
                    {
                        t.gameObject.GetComponent<RealPlayer>().Attack(Blood, thisBlood, PlayerToAttack.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
                        break;
                    }
                    else if (attackMode == 1)
                    {
                        if(GameManager.RealPlayerTeam.Contains(t.tag))
                            t.gameObject.GetComponent<RealPlayer>().DragAttack(Blood, thisBlood, attack, aimWeapon);
                        else
                            t.gameObject.GetComponent<RemoteEnemy>().DragAttack(Blood, thisBlood, attack, aimWeapon);    
                        break;
                    }
                    else
                    {
                        t.gameObject.GetComponent<RealPlayer>().ArrowAttack(Blood, thisBlood, PlayerToAttack.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
                        break;
                    }
                }
            }
        }

    }

    void NetFindArtifact(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Color color;
        //确定降怪点
        Vector3 problePosition = new Vector3();
        problePosition.x = proto.GetFloat(start, ref start);
        problePosition.y = proto.GetFloat(start, ref start);
        problePosition.z = proto.GetFloat(start, ref start);
        GameManager.instance.randomPlace = new List<GameObject>();
        //在周围标记提示圈
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(problePosition, t.position) < BoardManager.distance / 2 + BoardManager.distance * 2)
            {
                color = new Color(220, 220, 220);
                color.a = 0.2f;
                t.gameObject.GetComponent<SpriteRenderer>().color = color;
                GameManager.instance.randomPlace.Add(t.gameObject);
                if (t.tag == "Occupied")
                {
                    for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                    {
                        if (BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j] == t.gameObject)
                        {
                            GameManager.GroundStage GStage = GameManager.OccupiedGround[j];
                            GStage.OrigColor = t.gameObject.GetComponent<SpriteRenderer>().color;
                            GameManager.OccupiedGround[j] = GStage;
                            break;
                        }
                    }
                }
            }
        }
        color = new Color(255, 255, 0, 0.2f);
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
            if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
            {
                BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = color;
            }
        }
        GameManager.MudSetted = true;

    }
    void NetCreateArtifact(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        GameObject monster;
        //清除提示圈
        for (int i = 0; i < GameManager.instance.randomPlace.Count; i++)
        {
            Color color = new Color(255, 255, 255);
            GameManager.instance.randomPlace[i].GetComponent<SpriteRenderer>().color = color;
            if (GameManager.instance.randomPlace[i].tag == "Occupied")
            {
                for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                {
                    if (BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j] == GameManager.instance.randomPlace[i])
                    {
                        GameManager.GroundStage GStage = GameManager.OccupiedGround[j];
                        GStage.OrigColor = GameManager.instance.randomPlace[i].GetComponent<SpriteRenderer>().color;
                        GameManager.OccupiedGround[j] = GStage;
                        break;
                    }
                }
            }
        }
        Vector3 tearPosition = new Vector3();
        tearPosition.x = proto.GetFloat(start, ref start);
        tearPosition.y = proto.GetFloat(start, ref start);
        tearPosition.z = proto.GetFloat(start, ref start);
        //生成怪物
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(t.position, tearPosition) < BoardManager.distance / 2)
            {
                GameManager.instance.ArtifactGround = t.gameObject;
                break;
            }

        }
        Vector3 position = GameManager.instance.ArtifactGround.transform.position + new Vector3(0, 0, -0.1f);
        //GroundStage GStage = new GroundStage();
        GameManager.instance.ArtifactGround.tag = "Occupied";
        foreach (Transform t in GameManager.instance.ArtifactGround.GetComponentsInChildren<Transform>())
            if (t.tag == "Weapon")
                Destroy(t.gameObject);
        monster = Instantiate(GameManager.instance.Monster, position, Quaternion.identity, GameObject.Find("Players").transform);
        monster.transform.Rotate(-45, 0, 0);
        Vector3 offset = new Vector3(6, -12f, -2f);
        //monster.GetComponent<MonsterController>().Monster.OnMonsterCreate();
    }
    void OnMatchBack(ProtocolBase protocol)
    {
        Root.instance.Notice.SetActive(false);
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int team = proto.GetInt(start, ref start);
        GameManager.RealPlayerTeam = new List<string>();
        GameManager.IsTraining = false;
        GameManager.RealPlayerTeam.Add("Team" + (team + 1).ToString());
        int useAI = proto.GetInt(start, ref start);
        if (useAI == 1)
            GameManager.UseAI = true;
        else
            GameManager.UseAI = false;
        int mode = proto.GetInt(start, ref start);
        GameManager.Mode = mode;
        GameManager.Guide = -1;
        Quit.GetComponentInChildren<Text>().text = "投降";
        Quit.onClick.RemoveAllListeners();
        Quit.onClick.AddListener(delegate ()
        {
            ProtocolBytes prot = new ProtocolBytes();
            prot.AddString("EndGame");
            string winnerNotice = "";
            if (GameManager.RealPlayerTeam.Contains("Team1"))
            {
                prot.AddInt(2);
            }
            else
            {
                prot.AddInt(1);
            }
            winnerNotice = "失败";
            if (GameManager.RealPlayerTeam.Count < 2 && (!GameManager.UseAI))
            {

                NetMgr.srvConn.Send(prot);
            }
            Quit.GetComponentInChildren<Text>().text = "退出";
            Quit.onClick.RemoveAllListeners();
            Quit.onClick.AddListener(delegate () { Application.Quit(); });
            ShowNotice(winnerNotice, "返回", delegate () {
                SceneManager.LoadScene("MainPage");
            });
            //SceneManager.LoadScene("MainPage");
        });
        SceneManager.LoadScene("Game");
    }

    void SkipMove(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Vector3 position;
        position.x = proto.GetFloat(start, ref start);
        position.y = proto.GetFloat(start, ref start);
        position.z = proto.GetFloat(start, ref start);
        //找到待移动棋子
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (Vector3.Distance(position, GameManager.OccupiedGround[i].PlayerOnGround.transform.position) < BoardManager.distance / 2)
            {
                GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
                break;
            }

        }
        GameObject.Find("Skip").GetComponent<SkipTurn>().Skip();
    }

    void SkipAttack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        GameObject.Find("Skip").GetComponent<SkipTurn>().Skip();
    }

    void EndGame(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string winnerNotice = "";
        int winCase = proto.GetInt(start, ref start);
        switch (winCase)
        {
            case 1:
                if(GameManager.RealPlayerTeam.Contains("Team1"))
                    winnerNotice = "胜利";
                else
                {
                    winnerNotice = "失败";
                }
                break;
            case 2:
                if(GameManager.RealPlayerTeam.Contains("Team2"))
                    winnerNotice = "胜利";
                else
                {
                    winnerNotice = "失败";
                }
                break;
        }
        if (winnerNotice == "胜利")
        {
            ProtocolBytes prot = new ProtocolBytes();
            prot.AddString("AddScore");
            prot.AddInt(50);
            NetMgr.srvConn.Send(prot);
        }
        ShowNotice(winnerNotice, "返回", delegate ()
        {
            SceneManager.LoadScene("MainPage");
        });
        Quit.GetComponentInChildren<Text>().text = "退出";
        Quit.onClick.RemoveAllListeners();
        Quit.onClick.AddListener(delegate () { Application.Quit(); });
    }

    void NetSetBoard(ProtocolBase protocol)
    {
        Debug.Log("ReceiveSETBOARD");
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        BoardManager.row = 7;
        BoardManager.col = 7;
        int[][] randomlist = new int[BoardManager.row][];
        BoardManager.Grounds = new GameObject[BoardManager.row][];
        for (int i = 0; i < BoardManager.row; i++)
        {
            BoardManager.Grounds[i] = new GameObject[BoardManager.col];
            randomlist[i] = new int[BoardManager.col];
        }
        for (int i = 0; i < BoardManager.row; i++)
        {
            for (int j = 0; j < BoardManager.col; j++)
            {
                randomlist[i][j] = proto.GetInt(start, ref start);
            }
        }
        BoardManager board = GameObject.Find("BoardManager").GetComponent<BoardManager>();
        board.InstantiateBoard(randomlist);
    }

    public void ShowNotice(string text,string ButtonText,UnityEngine.Events.UnityAction action)
    {
        Notice.SetActive(true);
        NoticeText.text = text;
        ConfirmNotice.GetComponentInChildren<Text>().text = ButtonText;
        ConfirmNotice.onClick.AddListener(delegate(){
            action();
            Notice.SetActive(false);
        });
    }

    public void ShowPlot(string plotText)
    {
        //PlotText.text = plotText;
        
    }

    void OnLogoutBack(ProtocolBase protocol)
    {
        ShowNotice("网络断开或您的账号在其他地方登录", "好的", QuitGame);
    }
    void QuitGame()
    {
        Application.Quit();
    }

    void WinGame(ProtocolBase protocol)
    {
        ProtocolBytes prot = new ProtocolBytes();
        prot.AddString("AddScore");
        prot.AddInt(50);
        NetMgr.srvConn.Send(prot);
        string winnerNotice = "";
        winnerNotice = "胜利";
        ShowNotice(winnerNotice, "返回", delegate ()
        {
            SceneManager.LoadScene("MainPage");
        });
    }

    public int FindMode(int score)
    {
        int mode = 0;
        if(score>=150)
            mode = 1;
        return mode;
    }

    void GetScore(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        try
        {
            GameObject.Find("Score").GetComponent<Text>().text = proto.GetInt(start, ref start).ToString();
        }
        catch
        {
            Debug.Log("SetScoreError");
        }
    }
    public bool MouseClickLimit(GameObject clickedObject,GameObject except,ref bool useLimit,VoidDelegate FinishedAction)
    {
        if(!useLimit)
            return true;
        if(except==null)
            return false;
        if (clickedObject != except)
        {
            Root.instance.flowchart.SetBooleanVariable("RepeatCommand", true);
            return false;
        }
        else
        {
            useLimit = false;
            Root.instance.flowchart.SetBooleanVariable("FinnishCommand", true);
            FinishedAction();
            return true;
        }
    }
}
