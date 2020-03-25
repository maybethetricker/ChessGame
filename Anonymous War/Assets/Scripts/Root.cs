﻿using System.Collections;
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
    public Button SkipPlot;
    public bool OncePlotOpen = false;

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
        SkipPlot.onClick.AddListener(delegate () {
            //Debug.Log("skipplot");
            foreach(Block block in flowchart.GetExecutingBlocks())
            {
                //Debug.Log(block.name);
                block.Stop();
            }
            flowchart.SetBooleanVariable("Finnished", true);
        });
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
        NetMgr.srvConn.msgDist.AddListener("HopeSpringMove", HopeSpringMove);
        NetMgr.srvConn.msgDist.AddListener("ChooseArtifact", netChooseArtifact);
        NetMgr.srvConn.msgDist.AddListener("AddWeapon", NetAddWeapon);
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
        Vector3 GroundPosition, playerPosition;
        playerPosition.x = proto.GetFloat(start, ref start);
        playerPosition.y = proto.GetFloat(start, ref start);
        playerPosition.z = proto.GetFloat(start, ref start);
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
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if(GameManager.OccupiedGround[i].i!=-1)
                continue;
            Debug.Log(Mathf.Abs(GameManager.OccupiedGround[i].PlayerOnGround.transform.position.y - playerPosition.y));
            if(Mathf.Abs(GameManager.OccupiedGround[i].PlayerOnGround.transform.position.y-playerPosition.y)<1f)
                GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
        }
        if(GameManager.PlayerOnEdit==null)
            Debug.Log("NullPlayer");
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
                    case "Long": attack = 3; break;
                    case "Short": attack = 4; break;
                    case "Drag": attack = 1; break;
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
                        t.gameObject.GetComponent<RealPlayer>().Attack(Blood, thisBlood, PlayerToAttack.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon,true);
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
                t.gameObject.GetComponent<SpriteRenderer>().color = GameManager.instance.ArtifactRangeHighlight;
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
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
            if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
            {
                BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.instance.MovablePlayerHighlight;
            }
        }
        GameManager.instance.ArtActFinished = true;

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
            GameManager.instance.randomPlace[i].GetComponent<SpriteRenderer>().color = GameManager.instance.OrigGroundColor;
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
    void netChooseArtifact(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int kind = proto.GetInt(start, ref start);
        ArtifactController.instance.NetArtifactCreate(kind);
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
        string winnerNotice = "胜利";
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
        if(score>=300)
            mode = 2;
        if(score>=500)
            mode = 3;
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
            GameObject.Find("MainPageUI").GetComponent<MainPageUI>().OpenGuidePlot();
        }
        catch
        {
            Debug.Log("SetScoreError");
            ProtocolBytes prot = new ProtocolBytes();
            prot.AddString("GetScore");
            NetMgr.srvConn.Send(prot);
        }
    }
    void HopeSpringMove(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Vector3 aimPosition;
        aimPosition.x = proto.GetFloat(start, ref start);
        aimPosition.y = proto.GetFloat(start, ref start);
        aimPosition.z = proto.GetFloat(start, ref start);
        GameManager.instance.ArtifactGround.tag = "Untagged";
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(t.position, aimPosition) < BoardManager.distance / 2)
            {
                if (t.tag == "Weapon")
                    continue;
                GameManager.instance.ArtifactGround = t.gameObject;
                break;
            }
        }
        GameManager.instance.ArtifactGround.tag = "Occupied";
        ArtifactController.instance.ClearHighlight();
        StartCoroutine(GameManager.instance.smoothMove(ArtifactController.instance.gameObject, aimPosition + new Vector3(0, 0, -0.1f), 30, delegate ()
        {
            ArtifactController.instance.Artifact.OnArtCreate();
            ArtifactController.instance.ChangeTurn();
            GameManager.instance.EnemyChecked = false;
        }));
    }
    public bool MouseClickLimit(GameObject clickedObject,GameObject except,ref bool useLimit,VoidDelegate FinishedAction)
    {
        if(!useLimit)
            return true;
        Debug.Log("ClickLimited");
        if(except==null)
            return false;
        if (clickedObject != except)
        {
            Root.instance.flowchart.SetBooleanVariable("RepeatCommand", true);
            return false;
        }
        else
        {
            Debug.Log("limitFinished");
            useLimit = false;
            Root.instance.flowchart.SetBooleanVariable("FinnishCommand", true);
            FinishedAction();
            return true;
        }
    }
    void NetAddWeapon(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string weaponName = proto.GetString(start, ref start);
        Vector3 aimPosition;
        aimPosition.x = proto.GetFloat(start, ref start);
        aimPosition.y = proto.GetFloat(start, ref start);
        aimPosition.z = proto.GetFloat(start, ref start);
        for (int i = 0; i < BoardManager.row; i++)
            for (int j = 0; j < BoardManager.col; j++)
            {
                if (BoardManager.Grounds[i][j] != null && Vector3.Distance(BoardManager.Grounds[i][j].transform.position, aimPosition) < BoardManager.distance * 0.45)
                {
                    GameObject aimGround = null;
                    switch (weaponName)
                    {
                        case "Long": aimGround = BoardManager.instance.LongGround; break;
                        case "Short": aimGround = BoardManager.instance.ShortGround; break;
                        case "Drag": aimGround = BoardManager.instance.DragGround; break;
                        case "Ax": aimGround = BoardManager.instance.AxGround; break;
                        case "Shield": aimGround = BoardManager.instance.ShieldGround; break;
                    }
                    GameObject thisweapon = null;
                    foreach (Transform t in BoardManager.Grounds[i][j].GetComponentInChildren<Transform>())
                    {
                        if (t.tag == "Weapon")
                        {
                            thisweapon = t.gameObject;
                            t.gameObject.SetActive(true);
                        }
                    }
                    foreach (Transform t in aimGround.GetComponentInChildren<Transform>())
                    {
                        if (t.tag == "Weapon")
                        {
                            thisweapon.GetComponent<SpriteRenderer>().sprite = t.GetComponent<SpriteRenderer>().sprite;
                            //color?
                        }
                    }
                    BoardManager.Grounds[i][j].tag = aimGround.tag;
                    foreach(Transform t in GameObject.Find("EnemyWeaponCard").GetComponentInChildren<Transform>())
                    {
                        if(t.tag==weaponName)
                            t.gameObject.SetActive(false);
                    }
                }
            }
    }
}
