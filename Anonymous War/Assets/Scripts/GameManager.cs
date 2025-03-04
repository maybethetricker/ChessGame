﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public Color OrigGroundColor = new Color(255, 255, 255);//white,Board Manager处由于Game Manager可能没有生成，仍需要修改
    public Color OrigButtonColor = new Color(255, 255, 255);//white
    public Color ArtifactAbleRangeHighlight = new Color(0, 10, 0, 0.2f);//green
    public Color HopeSpringDesperateHighlight = new Color(0, 10, 0, 0.6f);//brighter green
    public Color MovablePlayerHighlight = new Color(255,255,0,0.2f);//yellow
    public Color AttackAimHighlight = new Color(0, 255, 255, 0.2f);//blue, move aim is also this
    public Color Team2Color = new Color(0, 8, 8);
    public Color Team3Color=new Color(255,255,0);
    public Color Team4Color;
    public Color GuideHighlight = new Color(0, 10, 0, 0.2f);//green
    public Color ArtifactRangeHighlight = new Color(220, 220, 220, 0.2f);//purple
    public Color AddWeaponOrigColor;
    public Color WinColor;
    public Color LostColor;

    public static GameManager instance;
    public Sprite LongSoldier;
    public Sprite LongSoldier2;
    public Sprite LongSoldier3;
    public Sprite ShortSoldier;
    public Sprite ShortSoldier2;
    public Sprite ShortSoldier3;
    public Sprite DragSoldier;
    public Sprite DragSoldier2;
    public Sprite DragSoldier3;
    public Sprite AxSoldier;
    public Sprite AxSoldier2;
    public Sprite AxSoldier3;
    public Sprite ShieldSoldier;
    public Sprite ShieldSoldier2;
    public Sprite ShieldSoldier3;
    public Sprite BumbSoldier;
    public Sprite BumbSoldier2;
    public Sprite BumbSoldier3;
    public Sprite Bumb;
    public Sprite crystal;
    public Sprite spring;
    public Sprite tower;
    public GameObject fogGround;
    public GameObject fog;
    public GameObject OrigSoldier;
    public GameObject Monster;//怪
    public Image Timer;
    public Text TimerText;
    public Image WinnerNotice;
    public Text WinnerText;
    public Button Back;
    public GameObject TimerHolder;
    public GameObject EnemyCard;
    public struct GroundStage
    {
        //public GameObject Ground;//所在地块
        public int i;//所在地块在数组中的第一个下标
        public int j;//。。。第二个下标
        public GameObject PlayerOnGround;//玩家
        public GameObject PlayerBlood;//血条
        public string PlayerWeapon;//拿了什么武器
        public bool Moved;//是否移动过
        public bool InMug;//是否位于泥地中
        public bool Faint;//是否被眩晕
        public Color OrigColor;
        public int Ability;//0:nothing,1:attack+1,2:blood+10,3:one more moving turn
    }//每个棋子及其所在地块，血条与基本属性
    public static int Stage;
    public static GameObject PlayerOnEdit;//正准备移动或攻击的棋子
    public static List<GroundStage> OccupiedGround = new List<GroundStage>();//棋盘上所有棋子信息
    public static int TeamCount = 2;
    public static List<string> RealPlayerTeam = new List<string>();//哪一队是玩家（其他是AI或者联机模块）
    public int Turn;
    public GameObject ArtifactGround=null;//怪物生成地
    public List<GameObject> randomPlace = new List<GameObject>();//生成怪的范围
    public bool ArtActFinished = false;//本回合是否已扩毒
    public bool ArtPerActActFinished = false;//战争迷雾是否已更新
    public static bool UseAI;
    public bool CoroutineStarted = false;
    public int MovingTeam = 1;//在移动的队伍
    public int SmallTurn;//每回合的小回合
    public bool EnemyChecked;//是否检测了可攻击范围
    public int AttackMode;
    public int[] TeamDiedSoldiers;//各队死亡人数
    public bool InGame = true;
    public GameObject SkipButton;
    public Coroutine timer;
    public static bool IsTraining;
    public static int Mode=1;//随天梯分数提高而有游戏性变化
    public bool SmoothMoveOnWay;
    public bool PointerIsready;
    public Vector3 AddWeaponAim;
    //0:无天降神器
    //1愤怒水晶
    //Button test;
    public static int Guide = -1;//1,2,3...为各教程
    public bool SecondMovingTurn=false;
    public bool Ability1Moved = false;
    public bool Ability2Moved = false;
    public bool ClickJumpOnway = false;//防止连点同一士兵导致错位，但用SmoothMoveOnway的话响应普遍延迟，手感不好
    // Start is called before the first frame update

    public virtual void Start()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
        if(TeamCount==2)
        {
            GameObject enemyCard = Instantiate(EnemyCard, new Vector3(-15, -33, 0), Quaternion.identity, GameObject.Find("Canvas(1)").transform);
            enemyCard.transform.localPosition = new Vector3(-15, 150, 0);
            enemyCard.transform.Rotate(-45, 0, 0);
            if(Mode<3)
                foreach (Transform t in enemyCard.GetComponentInChildren<Transform>())
                {
                    if(Mode<2&&t.tag=="Ax")
                        t.gameObject.SetActive(false);
                    if(t.tag=="BumbMaker")
                        t.gameObject.SetActive(false);
                }
            if(RealPlayerTeam.Contains("Team1"))
                enemyCard.name = "EnemyWeaponCardTeam2";
            else
                enemyCard.name = "EnemyWeaponCardTeam1";
        }
        if(TeamCount==3)
        {
            GameObject enemyCard1 = Instantiate(EnemyCard, new Vector3(-30, -33, 0), Quaternion.identity, GameObject.Find("Canvas(1)").transform);
            GameObject enemyCard2 = Instantiate(EnemyCard, new Vector3(0, -33, 0), Quaternion.identity, GameObject.Find("Canvas(1)").transform);
            enemyCard1.transform.localPosition = new Vector3(-160, 150, 0);
            enemyCard1.transform.Rotate(-45, 0, 0);
            enemyCard1.transform.localScale *= 0.75f;
            enemyCard2.transform.localPosition = new Vector3(140, 150, 0);
            enemyCard2.transform.Rotate(-45, 0, 0);
            enemyCard2.transform.localScale *= 0.75f;
            if (RealPlayerTeam.Contains("Team1"))
            {
                enemyCard1.name = "EnemyWeaponCardTeam2";
                enemyCard1.GetComponentInChildren<Text>().text = "蓝队:";
                enemyCard2.name = "EnemyWeaponCardTeam3";
                enemyCard2.GetComponentInChildren<Text>().text = "红队:";
            }
            else if (RealPlayerTeam.Contains("Team2"))
            {
                enemyCard1.name = "EnemyWeaponCardTeam1";
                enemyCard1.GetComponentInChildren<Text>().text = "白队:";
                enemyCard2.name = "EnemyWeaponCardTeam3";
                enemyCard2.GetComponentInChildren<Text>().text = "红队:";
            }
            else
            {
                enemyCard1.name = "EnemyWeaponCardTeam1";
                enemyCard1.GetComponentInChildren<Text>().text = "白队:";
                enemyCard2.name = "EnemyWeaponCardTeam2";
                enemyCard2.GetComponentInChildren<Text>().text = "蓝队:";
            }
        }
        SkipButton = GameObject.Find("Skip");
        if(Mode==9)
            SkipButton.SetActive(false);
        Timer.fillAmount = 1;
        TimerText.text = "20";
        //初始化变量
        Stage = 0;
        Turn = 0;
        PlayerOnEdit = null;
        OccupiedGround = new List<GroundStage>();
        ArtActFinished = false;
        SmoothMoveOnWay = false;
        GroundClick.TeamCounter = 0;
        MovingTeam = 1;
        SmallTurn = 0;
        PlayerController.FaintCount = 0;
        TeamDiedSoldiers = new int[TeamCount];
        for (int i = 0; i < TeamCount; i++)
            TeamDiedSoldiers[i] = 0;
        EnemyChecked = false;
        PlayerController.AimRangeList = new List<PlayerController.AimNode>();
        PlayerController.MovedDead = 0;
        CoroutineStarted = false;
        Back.onClick.AddListener(delegate { SceneManager.LoadScene("MainPage"); });
        //RealPlayerTeam.Add("Team1");
        //RealPlayerTeam.Add("Team2");
        //UseAI = false;
        //UseAI = true;
        if (Guide > 0)
        {
            if (Guide == 1)
                RealPlayerTeam.Add("Team2");
            if (Guide == 2)
            {
                RealPlayerTeam.Add("Team1");
                Root.instance.flowchart.SendFungusMessage("Guide2Start");
            }
            if (Guide == 3)
            {
                RealPlayerTeam.Add("Team1");
                Root.instance.flowchart.SendFungusMessage("Guide3Start");
            }
            IsTraining = true;
            if (Guide >= 3)
                Mode = 1;
            else
            {
                Mode = 0;
            }
        }
        if (!GameManager.IsTraining)
        {
            timer = StartCoroutine(GameManager.instance.HandleTimer());
        }
        else
        {
            TimerHolder.gameObject.SetActive(false);
        }
        int totalSoldier = TeamCount * 3;
        if (Guide == 1 || Mode==9)
            totalSoldier = TeamCount;
        Debug.Log(totalSoldier);
        for (int i = 0; i < totalSoldier; i++)
        {
            GroundStage groundStage = new GroundStage();
            Vector3 position = new Vector3();
            position.z = 78;
            groundStage.i = groundStage.j = -1;
            if (i < totalSoldier/TeamCount)
            {
                position.x = -80;
            }
            else if(i<totalSoldier/TeamCount*2)
            {
                position.x = 82;
            }
            else if(i<totalSoldier/TeamCount*3)
            {
                position.x = 94;
            }
            else
            {
                position.x=106;
            }
            if (Guide==1 || Mode==9)
                position.y = 70;
            else
            {
                position.y = 50 + (i % 3) * 20 * 2;
            }
            GameObject newPlayer = Instantiate(OrigSoldier, position, Quaternion.identity, GameObject.Find("Players").transform);
            if (i < totalSoldier/TeamCount)
            {
                newPlayer.tag = RealPlayerTeam[0];
            }
            else if(i<totalSoldier/TeamCount*2)
            {
                if (RealPlayerTeam[0] == "Team2")
                    newPlayer.tag = "Team1";
                else
                {
                    newPlayer.tag = "Team2";
                }
            }
            else if(i<totalSoldier/TeamCount*3)
            {
                if (RealPlayerTeam[0] == "Team3")
                    newPlayer.tag = "Team1";
                else
                {
                    newPlayer.tag = "Team3";
                }
            }
            else{
                if (RealPlayerTeam[0] == "Team4")
                    newPlayer.tag = "Team1";
                else
                {
                    newPlayer.tag = "Team4";
                }
            }
            if (newPlayer.tag == "Team2")
                newPlayer.GetComponentInChildren<SpriteRenderer>().color = Team2Color;
            else if(newPlayer.tag == "Team3")
                newPlayer.GetComponentInChildren<SpriteRenderer>().color = Team3Color;
            else if(newPlayer.tag == "Team4")
                newPlayer.GetComponentInChildren<SpriteRenderer>().color = Team4Color;
            newPlayer.transform.Rotate(-45, 0, 0);
            newPlayer.AddComponent<RealPlayer>();
            groundStage.PlayerOnGround = newPlayer;
            groundStage.Moved = false;
            groundStage.InMug = false;
            groundStage.Faint = false;
            if (Mode == 9)
            {
                groundStage.Ability = 3;
                groundStage.PlayerWeapon = "BumbMaker";
            }
            else if (Mode >= 2)
                groundStage.Ability = i % 3 + 1;
            else
            {
                groundStage.Ability = 0;
            }
            //生成血条
            GameObject blood = null;
            foreach (Transform t in newPlayer.GetComponentsInChildren<Transform>())
                if (t.tag == "Blood")
                    blood = t.gameObject;
            if (groundStage.Ability == 2)
            {
                blood.GetComponent<Text>().text = (int.Parse(blood.GetComponent<Text>().text) + 10).ToString();
            }
            foreach (Transform t in newPlayer.GetComponentsInChildren<Transform>())
                if (t.tag == "PlayerSprite")
                {
                    if (Mathf.Abs(groundStage.Ability) == 2)
                        t.gameObject.GetComponent<SpriteRenderer>().sprite = GameManager.instance.ShortSoldier2;
                    else if (Mathf.Abs(groundStage.Ability) == 3)
                    {
                        if (Mode==9)
                            t.gameObject.GetComponent<SpriteRenderer>().sprite = GameManager.instance.BumbSoldier3;
                        else
                            t.gameObject.GetComponent<SpriteRenderer>().sprite = GameManager.instance.ShortSoldier3;
                    }
                }
            groundStage.PlayerBlood = blood;
            OccupiedGround.Add(groundStage);
        }
        /* 
        test = GameObject.Find("Test").GetComponent<Button>();
        test.onClick.AddListener(delegate () {
        for (int i = 0; i < OccupiedGround.Count; i++)
        {
            if(!OccupiedGround[i].Moved)
                OccupiedGround[i].Ground.transform.localScale *= 1.5f;
            }
            Debug.Log(OccupiedGround.Count);
            Debug.Log("stage+team" + Stage + " " + PlayerController.MovingTeam);
            Debug.Log("Turn" + PlayerController.SmallTurn);
            Debug.Log("faint=dead" + PlayerController.FaintCount + PlayerController.DiedSoldiers);
        });*/
    }

    // Update is called once per frame
    void Update()
    {
        if(SmoothMoveOnWay)
            return;
        if (UseAI && GameManager.Stage == 0 && !RealPlayerTeam.Contains("Team" + (GroundClick.TeamCounter + 1).ToString()))
        {
            //等待一会儿后空降
            if (!CoroutineStarted)
                StartCoroutine(WaitToLand());
        }
        if (Mode>=1&&Mode<=8&&Turn == 1 && !ArtActFinished)
        {
            if(Guide==3)
                Root.instance.flowchart.SetBooleanVariable("FinnishCommand",true);
            //降怪前准备
            FindArtifact();
            
        }
        //降怪
        if (Mode >= 1 && Mode<=8 && Turn == 2 && !ArtActFinished)
        {
            if(Guide==3)
                Root.instance.flowchart.SetBooleanVariable("FinnishCommand",true);
            CreateArtifact();
            
        }
        if(InGame)
            CheckWinner();

    }
    void FindArtifact()
    {
        if (RealPlayerTeam.Count < 2 && (!UseAI) && !RealPlayerTeam.Contains("Team1"))
        {
            ArtActFinished = true;
            return;
        }
        //确定降怪点
        int randomx=3;
        int randomy = Random.Range(1, BoardManager.col-1);
        if(randomy==1)
            randomx = Random.Range(3, BoardManager.row-1);
        else if(randomy==2)
            randomx = Random.Range(2, BoardManager.row-1);
        else if(randomy==3)
            randomx = Random.Range(1, BoardManager.row-1);
        else if(randomy==4)
            randomx = Random.Range(1, BoardManager.row-2);    
        else if(randomy==5)
            randomx = Random.Range(1, BoardManager.row-3);
        ArtifactGround = BoardManager.Grounds[randomx][randomy];
        while (ArtifactGround == null)
        {
            randomy = Random.Range(0, BoardManager.col);
            if (randomy == 1)
                randomx = Random.Range(3, BoardManager.row - 1);
            else if (randomy == 2)
                randomx = Random.Range(2, BoardManager.row - 1);
            else if (randomy == 3)
                randomx = Random.Range(1, BoardManager.row - 1);
            else if (randomy == 4)
                randomx = Random.Range(1, BoardManager.row - 2);
            else if (randomy == 5)
                randomx = Random.Range(1, BoardManager.row - 3);
            ArtifactGround = BoardManager.Grounds[randomx][randomy];
        }
        Vector3 problePosition = ArtifactGround.transform.position + new Vector3(-BoardManager.distance, 0, 0);
        if (randomx % 2 == 0)
            problePosition = ArtifactGround.transform.position + new Vector3(BoardManager.distance, 0, 0);
        if (RealPlayerTeam.Count < GameManager.TeamCount && (!UseAI))
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("FindMonster");
            protocol.AddFloat(problePosition.x);
            protocol.AddFloat(problePosition.y);
            protocol.AddFloat(problePosition.z);
            NetMgr.srvConn.Send(protocol);
        }
        randomPlace = new List<GameObject>();
        //在周围标记提示圈
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(problePosition, t.position) < BoardManager.distance / 2 + BoardManager.distance * 2)
            {
                t.gameObject.GetComponent<SpriteRenderer>().color = ArtifactRangeHighlight;
                randomPlace.Add(t.gameObject);
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
            string team = "Team" + (MovingTeam + 1).ToString();
            if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
            {
                BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = MovablePlayerHighlight;
            }
        }
        ArtActFinished = true;
    }
    void CreateArtifact()
    {
        if (RealPlayerTeam.Count < 2 && (!UseAI) && !RealPlayerTeam.Contains("Team1"))
        {
            ArtActFinished = true;
            return;
        }
        //清除提示圈
        for (int i = 0; i < randomPlace.Count; i++)
        {
            randomPlace[i].GetComponent<SpriteRenderer>().color = OrigGroundColor;
            if (randomPlace[i].tag == "Occupied" )
            {
                for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                {
                    if (BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j] == randomPlace[i])
                    {
                        GameManager.GroundStage GStage = GameManager.OccupiedGround[j];
                        GStage.OrigColor = randomPlace[i].GetComponent<SpriteRenderer>().color;
                        GameManager.OccupiedGround[j] = GStage;
                        break;
                    }
                }
            }
        }
        //生成怪物
        int random = Random.Range(0, randomPlace.Count - 1);
        int count = 0;
        if (ArtifactGround.tag == "Occupied"|| ArtifactGround.tag=="Weapon")
        {
            while (randomPlace[random].tag == "Occupied"|| randomPlace[random].tag=="Weapon")
            {
                count++;
                random = Random.Range(0, randomPlace.Count - 1);
                Debug.Log(random);
                if (count > 20)
                {
                    Debug.Log("AllOccupied");
                    break;
                }
            }
            ArtifactGround = randomPlace[random];
        }
        if (RealPlayerTeam.Count < GameManager.TeamCount && (!UseAI))
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("CreateMonster");
            protocol.AddFloat(ArtifactGround.transform.position.x);
            protocol.AddFloat(ArtifactGround.transform.position.y);
            protocol.AddFloat(ArtifactGround.transform.position.z);
            NetMgr.srvConn.Send(protocol);
        }
        Vector3 position = ArtifactGround.transform.position + new Vector3(0, 0, -0.1f);
        //GroundStage GStage = new GroundStage();
        ArtifactGround.tag = "Occupied";
        foreach (Transform t in ArtifactGround.GetComponentsInChildren<Transform>())
            if (t.tag == "Weapon")
                Destroy(t.gameObject);
        GameObject artifact;
        artifact = Instantiate(Monster, position, Quaternion.identity, GameObject.Find("Players").transform);
        artifact.transform.Rotate(-45, 0, 0);
        //monster.GetComponent<MonsterController>().Monster.OnMonsterCreate();
        /*bool containPlayer=false;
        for (int i = 0; i < OccupiedGround.Count; i++)
        {
             if (OccupiedGround[i].Ground == TearGround)
            {
                
                GStage.PlayerBlood = OccupiedGround[i].PlayerBlood;
                GStage.InMug = OccupiedGround[i].InMug;
                GStage.Faint = OccupiedGround[i].Faint;
                GStage.PlayerWeapon = "Tear";
                string tag = OccupiedGround[i].PlayerOnGround.tag;
                PlayerController.CanMoveList.Remove(OccupiedGround[i].PlayerOnGround);
                Destroy(OccupiedGround[i].PlayerOnGround);
                GStage.PlayerOnGround = Instantiate(TearSoldier, position, Quaternion.identity, GameObject.Find("Players").transform);
                GStage.PlayerOnGround.tag = tag;
                if(tag=="Team2")
                    GStage.PlayerOnGround.transform.Rotate(0, 0, 180);
                GStage.PlayerOnGround.tag = tag;
                OccupiedGround.RemoveAt(i);
                containPlayer = true;
                break;
            }
        }
        
        Destroy(TearGround);
        TearGround=Instantiate(Ground_Tear, position, Quaternion.identity, GameObject.Find("Grounds").transform);
        
        GStage.Ground = TearGround;
        if(containPlayer)
        {
            foreach (Transform t in TearGround.GetComponentsInChildren<Transform>())
                if (t.tag == "Weapon")
                    t.gameObject.SetActive(false);
            TearGround.tag = "Occupied";
            GameManager.OccupiedGround.Add(GStage);
        }*/
    }

    void CheckWinner()
    {
        int RemainingTeam = TeamCount;
        int lastTeam=0;
        bool thisTeamLost=false;
        for (int i = 0; i < TeamCount;i++)
        {
            if ((Mode!=9&&TeamDiedSoldiers[i] >= 3)
            ||Mode==9&&TeamDiedSoldiers[i]>=1)
            {
                RemainingTeam--;
                //TeamDiedSoldiers[i] = 0;
                if (RealPlayerTeam.Contains("Team" + (i + 1).ToString()))
                {
                    Debug.Log("you lost");
                    thisTeamLost = true;
                }
            }
            else
            {
                lastTeam = i;
            }
        }
        if (RemainingTeam == 1)
        {
            InGame = false;
            try
            {
                StopCoroutine(timer);
            }
            catch
            {
                Debug.Log("stopTimerError");
            }
            string notice="";
            if (RealPlayerTeam.Contains("Team" + (lastTeam + 1).ToString()))
            {
                notice = "胜 利";
                WinnerNotice.color = WinColor;
                if(!UseAI && !IsTraining)
                {
                    ProtocolBytes protocol = new ProtocolBytes();
                    protocol.AddString("OutOfGame");
                    NetMgr.srvConn.Send(protocol);
                }
                if ((!IsTraining||Guide==2||Guide==3)&&Mode<7)
                {
                    ProtocolBytes prot = new ProtocolBytes();
                    prot.AddString("AddScore");
                    prot.AddInt(50);
                    NetMgr.srvConn.Send(prot);
                }
            }
            else
            {
                notice = "失 败";
                WinnerNotice.color = LostColor;
                if (!IsTraining && Mode>=2 && Mode<7)
                {
                    ProtocolBytes prot = new ProtocolBytes();
                    prot.AddString("AddScore");
                    prot.AddInt(-50);
                    NetMgr.srvConn.Send(prot);
                }
            }
            Root.instance.Quit.gameObject.SetActive(true);
            Root.instance.GiveIn.gameObject.SetActive(false);
            Root.instance.soundManager.ChangeClip();
            WinnerNotice.gameObject.SetActive(true);
            WinnerText.text = notice;
            foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
            {
                if (t.name == "Players")
                    continue;
                if (t.gameObject.GetComponent<AI>())
                    Destroy(t.gameObject.GetComponent<AI>());
                if (t.gameObject.GetComponent<RealPlayer>())
                    Destroy(t.gameObject.GetComponent<RealPlayer>());

            }
        }
        else if(thisTeamLost)
        {
            InGame = false;
            try
            {
                StopCoroutine(timer);
            }
            catch
            {
                Debug.Log("stopTimerError");
            }
            string notice = "";
            notice = "失 败";
            WinnerNotice.color = LostColor;
            Root.instance.Quit.gameObject.SetActive(true);
            Root.instance.GiveIn.gameObject.SetActive(false);
            Root.instance.soundManager.ChangeClip();
            WinnerNotice.gameObject.SetActive(true);
            WinnerText.text = notice;
            foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
            {
                if (t.name == "Players")
                    continue;
                if (t.gameObject.GetComponent<AI>())
                    Destroy(t.gameObject.GetComponent<AI>());
                if (t.gameObject.GetComponent<RealPlayer>())
                    Destroy(t.gameObject.GetComponent<RealPlayer>());

            }
        }
    }

    void AILand()
    {
        for (int i = 0; i < GameManager.OccupiedGround.Count;i++)
        {
            GameObject player = GameManager.OccupiedGround[i].PlayerOnGround;
            if(GameManager.OccupiedGround[i].PlayerOnGround.tag==("Team" + (GroundClick.TeamCounter + 1).ToString())&&GameManager.OccupiedGround[i].i==-1)
            {
                PlayerOnEdit = player;
                break;
            }
        }
        //需要空降到的地块
        GameObject GroundToLand = null;
        List<GameObject> randomLandList = new List<GameObject>();
        //遍历所有有武器且未被其他棋子占据的地块，并在遍历到的第一个地块处降落,因为地上有武器，所有Tag也不能为Weapon
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (t.tag != "Occupied"  && t.tag != "Weapon")
            {
                if(t.tag=="Untagged"&&Mode!=9)
                    continue;
                if(Mode==9 && t.tag != "Untagged")
                    continue;
                if (t.tag == "Drag" &&randomLandList.Count>0)
                    continue;
                if(t.tag=="Ax"&&randomLandList.Count>0)
                    continue;
                randomLandList.Add(t.gameObject);
            }
        }
        int rand = Random.Range(0, randomLandList.Count);
        GroundToLand = randomLandList[rand];
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

    IEnumerator WaitToLand()
    {
        CoroutineStarted = true;
        int second = Random.Range(2, 6);
        if(IsTraining)
            second = 1;
        if (Guide != 1)
        {
            yield return new WaitForSeconds(second);
            AILand();
        }
        else
        {
            yield return new WaitForSeconds(1);
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                GameObject player = GameManager.OccupiedGround[i].PlayerOnGround;
                if (GameManager.OccupiedGround[i].PlayerOnGround.tag==("Team" + (GroundClick.TeamCounter + 1).ToString()) && GameManager.OccupiedGround[i].i == -1)
                {
                    PlayerOnEdit = player;
                    break;
                }
            }
            BoardManager.Grounds[4][4].GetComponent<GroundClick>().PlaceSinglePlayer();
        }
        /*if (SmallTurn >= 3 * TeamCount && Stage == 0)
        {
            SmallTurn = 0;
            Stage = 1;
            Color color = new Color(255, 255, 0, 0.2f);
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                string team = "Team" + (MovingTeam + 1).ToString();
                if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                {
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = color;
                }
            }
        }*/
        CoroutineStarted = false;
    }

    public void DeleteDiedObject(GameObject diedobject)
    {
        Destroy(diedobject);
    }

    public IEnumerator HandleTimer()
    {
        int lastFlameStage=Stage;
        while (true)
        {
            TimerEvent(lastFlameStage);
            lastFlameStage = Stage;
            yield return new WaitForSeconds(1);
        }
    }
    
    public void TimerEvent(int lastFlameStage)
    {
        if(SmoothMoveOnWay || !InGame)
            return;
        if (Stage == 1 && !RealPlayerTeam.Contains("Team" + (MovingTeam + 1).ToString()))
        {
            TimerText.text = "敌方回合";
            Timer.fillAmount = 1;
            return;
        }
        else if (Stage == 2 && RealPlayerTeam.Contains("Team" + (MovingTeam + 1).ToString()))
        {
            TimerText.text = "敌方回合";
            Timer.fillAmount = 1;
            return;
        }
        else if(Stage==0&&!RealPlayerTeam.Contains("Team" + (GroundClick.TeamCounter + 1).ToString()))
        {
            TimerText.text = "敌方回合";
            Timer.fillAmount = 1;
            return;
        }
        else if(TimerText.text=="敌方回合")
        {
            Timer.fillAmount = 1;
            TimerText.text = "20";
        }
        int leftTime = int.Parse(TimerText.text);
        if(Stage==0)
        {
            Timer.fillAmount -= 1f / 20;
            leftTime--;
            if (leftTime <= 0)
            {
                for (int i = 0; i < OccupiedGround.Count; i++)
                {
                    if (RealPlayerTeam.Contains(OccupiedGround[i].PlayerOnGround.tag) && OccupiedGround[i].i<0)
                    {
                        PlayerOnEdit = OccupiedGround[i].PlayerOnGround;
                        break;
                    }
                }
                GameObject GroundToLand = null;
                List<GameObject> randomLandList = new List<GameObject>();
                //遍历所有有武器且未被其他棋子占据的地块，并在遍历到的第一个地块处降落,因为地上有武器，所有Tag也不能为Weapon
                foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
                {
                    if (t.name == "Grounds")
                        continue;
                    if (t.tag != "Occupied" && t.tag != "Untagged" && t.tag != "Weapon")
                    {
                        randomLandList.Add(t.gameObject);
                    }
                }
                int rand = Random.Range(0, randomLandList.Count);
                GroundToLand = randomLandList[rand];
                if(!UseAI && !IsTraining)
                {
                    ProtocolBytes protocol=new ProtocolBytes();
                    protocol.AddString("UpdateLand");
                    for (int i = 0; i < GameManager.OccupiedGround.Count;i++)
                    {
                        if(GameManager.OccupiedGround[i].PlayerOnGround==GameManager.PlayerOnEdit)
                        {
                            Debug.Log("Added");
                            protocol.AddString(GameManager.PlayerOnEdit.tag);
                            protocol.AddFloat(GameManager.PlayerOnEdit.transform.position.y);
                            break;
                        }
                    }
                    protocol.AddFloat(GroundToLand.transform.position.x);
                    protocol.AddFloat(GroundToLand.transform.position.y);
                    protocol.AddFloat(GroundToLand.transform.position.z);
                    NetMgr.srvConn.Send(protocol);
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
            TimerText.text = leftTime.ToString();
            return;
        }
        if(lastFlameStage!=Stage)
        {
            if(Stage==1)
                leftTime = 20;
            else
            {
                leftTime = 10;
            }
            Timer.fillAmount = 1;
        }
        leftTime--;
        if(Stage==1)
            Timer.fillAmount -= 1f / 20;
        else
        {
            Timer.fillAmount -= 1f / 10;
        }
        if(leftTime<=0)
        {
            if(Stage==1)
                leftTime = 10;
            else
            {
                leftTime = 20;
            }
            Timer.fillAmount = 1;
            if ((Stage==1&&RealPlayerTeam.Contains("Team"+(MovingTeam+1).ToString()))
                ||(Stage==2&&PlayerOnEdit!=null&&RealPlayerTeam.Contains(PlayerOnEdit.tag)))
            {
                Debug.Log(PlayerOnEdit);
                Debug.Log(Stage);
                Debug.Log("TimeUpSkip");
                Vector3 groundPositon = new Vector3();
                if (PlayerOnEdit == null)
                {
                    for (int i = 0; i < OccupiedGround.Count; i++)
                    {
                        if (RealPlayerTeam.Contains(OccupiedGround[i].PlayerOnGround.tag) && OccupiedGround[i].Moved == false)
                        {
                            PlayerOnEdit = OccupiedGround[i].PlayerOnGround;
                            groundPositon = BoardManager.Grounds[OccupiedGround[i].i][OccupiedGround[i].j].transform.position;
                            break;
                        }
                    }
                }
                if(Mode==9)
                {
                    foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
                    {
                        if (t.name == "Grounds")
                            continue;
                        if (t.tag != "Occupied" && t.tag != "Weapon")
                        {
                            if(Vector3.Distance(groundPositon,t.position)<BoardManager.distance*1.5)
                            {
                                t.gameObject.GetComponent<GroundClick>().PlayerMove();
                                TimerText.text = leftTime.ToString();
                                return;
                            }
                        }
                    }
                }
                SkipButton.GetComponent<SkipTurn>().SkipOnClick();
            }
        }
        TimerText.text = leftTime.ToString();
    }
    public IEnumerator smoothMove(GameObject MovingObject, Vector3 aimPosition,float speed,UnityEngine.Events.UnityAction finnishAction)//匀速移动
    {
        //change:as I've edited playeronedit in "PlayerController.ChangeTurn,I need one more parameter"
        while (aimPosition != MovingObject.transform.position)
        {
            MovingObject.transform.position = Vector3.MoveTowards(MovingObject.transform.position, aimPosition, speed * Time.deltaTime);
            yield return 0;
            if (MovingObject == null)
            {
                Debug.Log("NullMovingObject");
                break;
            }
        }
        if(MovingObject!=null)
            MovingObject.transform.position = aimPosition;
        finnishAction();
        SmoothMoveOnWay = false;
        //GameManager.PlayerOnEdit.transform.position = transform.position;
    }
    //有些没有继承Mono behavior的脚本引用函数
    public void startCoroutine(IEnumerator ienu)
    {
        StartCoroutine(ienu);
    }
    public GameObject instantiate(GameObject original,Vector3 position,Quaternion rotation)
    {
        return Instantiate(original, position, rotation);
    }
}
