using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject LongSoldier;
    public GameObject ShortSoldier;
    public GameObject DragSoldier;
    public GameObject TearSoldier;
    public GameObject Short;
    public GameObject Long;
    public GameObject Drag;
    public GameObject Tear;//致死刀
    public GameObject Monster;//怪
    public GameObject Blood;//血条prefab
    public static GameObject WinnerNotice;//获胜提示信息
    public static Text Notice;
    public Button Restart;
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
        public int Hate;
    }//每个棋子及其所在地块，血条与基本属性
    public static int Stage;
    public static GameObject PlayerOnEdit;//正准备移动或攻击的棋子
    public static List<GroundStage> OccupiedGround = new List<GroundStage>();//棋盘上所有棋子信息
    public static int TeamCount = 2;
    public static List<string> RealPlayerTeam = new List<string>();//哪一队是玩家（其他是AI或者联机模块）
    public int Turn;
    public GameObject ArtifactGround=null;//怪物生成地
    public List<GameObject> randomPlace = new List<GameObject>();//生成怪的范围
    public static bool MudSetted = false;//本回合是否已扩毒
    public static bool UseAI;
    public bool CoroutineStarted = false;
    public int MovingTeam = 1;//在移动的队伍
    public int SmallTurn;//每回合的小回合
    public bool EnemyChecked;//是否检测了可攻击范围
    public int AttackMode;
    public int[] TeamDiedSoldiers=new int[TeamCount];//各队死亡人数
    //Button test;
    // Start is called before the first frame update

    void Start()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
        //胜利提示框
        WinnerNotice = GameObject.Find("WinnerNotice");
        Notice = GameObject.Find("Notice").GetComponent<Text>();
        WinnerNotice.SetActive(false);
        Restart.onClick.AddListener(delegate ()
        {
            SceneManager.LoadScene("MainPage");
        });
        //初始化静态变量
        Stage = 0;
        Turn = 0;
        PlayerOnEdit = null;
        OccupiedGround = new List<GroundStage>();
        MudSetted = false;
        GroundClick.TeamCounter = 0;
        MovingTeam = 1;
        SmallTurn = 0;
        PlayerController.FaintCount = 0;
        for (int i = 0; i < TeamCount;i++)
            TeamDiedSoldiers[i] = 0;
        EnemyChecked = false;
        PlayerController.AimRangeList = new List<PlayerController.AimNode>();
        PlayerController.MovedDead = 0;
        CoroutineStarted = false;
        RealPlayerTeam.Add("Team1");
        //RealPlayerTeam.Add("Team2");
        //UseAI = false;
        UseAI = true;
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
        if (UseAI && GameManager.Stage == 0 && !RealPlayerTeam.Contains("Team" + (GroundClick.TeamCounter + 1).ToString()))
        {
            //等待一会儿后空降
            if (!CoroutineStarted)
                StartCoroutine(WaitToLand());
        }
        if (Turn == 1 && !MudSetted)
        {
            //降怪前准备
            FindArtifact();
        }
        //降怪
        if (Turn == 2 && !MudSetted)
            CreateArtifact();

        CheckWinner();

    }
    void FindArtifact()
    {
        if (RealPlayerTeam.Count < 2 && (!UseAI) && RealPlayerTeam.Contains("Team2"))
        {
            MudSetted = true;
            return;
        }
        Color color;
        //确定降怪点
        int randomx = Random.Range(0, BoardManager.row);
        int randomy = Random.Range(0, BoardManager.col);
        ArtifactGround = BoardManager.Grounds[randomx][randomy];
        while (ArtifactGround == null)
        {
            randomx = Random.Range(0, BoardManager.row);
            randomy = Random.Range(0, BoardManager.col);
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
                color = new Color(220, 220, 220);
                color.a = 0.2f;
                t.gameObject.GetComponent<SpriteRenderer>().color = color;
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
        color = new Color(255, 255, 0, 0.2f);
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            string team = "Team" + (MovingTeam + 1).ToString();
            if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
            {
                BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = color;
            }
        }
        MudSetted = true;
    }
    void CreateArtifact()
    {
        if (RealPlayerTeam.Count < 2 && (!UseAI) && RealPlayerTeam.Contains("Team2"))
        {
            MudSetted = true;
            return;
        }
        GameObject artifact;
        //清除提示圈
        for (int i = 0; i < randomPlace.Count; i++)
        {
            Color color = new Color(255, 255, 255);
            randomPlace[i].GetComponent<SpriteRenderer>().color = color;
            if (randomPlace[i].tag == "Occupied")
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
        if (ArtifactGround.tag == "Occupied")
        {
            while (randomPlace[random].tag == "Occupied")
            {
                count++;
                random = Random.Range(0, randomPlace.Count - 1);
                Debug.Log(random);
                if (count > 20)
                    break;
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
        for (int i = 0; i < TeamCount;i++)
        {
            if(TeamDiedSoldiers[i]>=3)
                RemainingTeam--;
            else
            {
                lastTeam = i;
            }
        }
        if (RemainingTeam == 1)
        {
            GameManager.WinnerNotice.SetActive(true);
            foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
            {
                if (t.name == "Players")
                    continue;
                if (t.gameObject.GetComponent<AI>())
                    Destroy(t.gameObject.GetComponent<AI>());
                if (t.gameObject.GetComponent<RealPlayer>())
                    Destroy(t.gameObject.GetComponent<RealPlayer>());

            }
            lastTeam++;
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("EndGame");
            protocol.AddInt(lastTeam);
            GameManager.Notice.GetComponent<Text>().text = "队伍"+lastTeam+"胜利";
            NetMgr.srvConn.Send(protocol);
            if (GameManager.UseAI)
            {
                Button Quit = GameObject.Find("Quit").GetComponent<Button>();
                Quit.GetComponentInChildren<Text>().text = "退出";
                Quit.onClick.RemoveAllListeners();
                Quit.onClick.AddListener(delegate () { Application.Quit(); });
            }
        }
    }

    void AILand()
    {
        //需要空降到的地块
        GameObject GroundToLand = null;
        List<GameObject> randomLandList = new List<GameObject>();
        //遍历所有有武器且未被其他棋子占据的地块，并在遍历到的第一个地块处降落,因为地上有武器，所有Tag也不能为Weapon
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (t.tag != "Occupied" && t.tag != "Untagged" && t.tag != "Weapon")
            {
                if (t.tag == "Drag")
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
        yield return new WaitForSeconds(1);
        AILand();
        if (SmallTurn >= 3 * TeamCount && Stage == 0)
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
        }
        CoroutineStarted = false;
    }

    public void DeleteDiedObject(GameObject diedobject)
    {
        Destroy(diedobject);
    }
}
