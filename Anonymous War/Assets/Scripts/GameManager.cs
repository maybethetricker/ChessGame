using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject LongSoldier;
    public GameObject ShortSoldier;
    public GameObject DragSoldier;
    public GameObject TearSoldier;
    public GameObject Tear;//致死刀
    public GameObject Monster;//怪
    public GameObject Blood;//血条prefab
    public static GameObject WinnerNotice;//获胜提示信息
    public static Text Notice;
    public Button Restart;
    public struct GroundStage{
        //public GameObject Ground;//所在地块
        public int i;//所在地块在数组中的第一个下标
        public int j;//。。。第二个下标
        public GameObject PlayerOnGround;//玩家
        public GameObject PlayerBlood;//血条
        public string PlayerWeapon;//拿了什么武器
        public bool Moved;//是否移动过
        public bool InMug;//是否位于泥地中
        public bool Faint;//是否被眩晕
    }//每个棋子及其所在地块，血条与基本属性
    public static int Stage;
    public static GameObject PlayerOnEdit;//正准备移动或攻击的棋子
    public static List<GroundStage> OccupiedGround = new List<GroundStage>();//棋盘上所有棋子信息
    public static int TeamCount = 2;
    public static List<string> RealPlayerTeam=new List<string>();//哪一队是玩家（其他是AI或者联机模块）
    public static int Turn;
    GameObject TearGround;//怪物生成地，因为之前不是怪所有名字有点不对
    GameObject MonsterBlood;
    List<GameObject> randomPlace = new List<GameObject>();//生成怪的范围
    public static bool MudSetted = false;//本回合是否已扩毒
    public static bool TearCreated;//致死刀至多一把
    //Button test;
    // Start is called before the first frame update
    
    void Start()
    {
        //胜利提示框
        WinnerNotice=GameObject.Find("WinnerNotice");
        Notice = GameObject.Find("Notice").GetComponent<Text>();
        WinnerNotice.SetActive(false);
        Restart.onClick.AddListener(delegate ()
        {
            SceneManager.LoadScene("Game");
        });
        //初始化静态变量
        Stage =0;
        Turn = 0;
        PlayerOnEdit = null;
        OccupiedGround = new List<GroundStage>();
        MudSetted = false;
        GroundClick.SoldierCount = 0;
        GroundClick.TeamCounter = 0;
        PlayerController.MovingTeam = 1;
        PlayerController.SmallTurn = 0;
        PlayerController.FaintCount = 0;
        PlayerController.DiedSoldiersTeam1 = 0;
        PlayerController.DIedSoldiersTeam2 = 0;
        PlayerController.CanMoveList = new Dictionary<GameObject, Color>();
        PlayerController.EnemyChecked = false;
        PlayerController.LineCanAttack = new List<PlayerController.AttackLine>();
        PlayerController.OnlyLine = false;
        PlayerController.MovedDead = 0;
        AI.CoroutineStarted = false;
        RealPlayerTeam.Add("Team1");
        RealPlayerTeam.Add("Team2");
        TearCreated = false;
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
        if(GameManager.Stage==0&&!RealPlayerTeam.Contains("Team"+(GroundClick.TeamCounter+1).ToString()))
        {
            //等待一会儿后空降
            if(!AI.CoroutineStarted)
                StartCoroutine(WaitToLand());
        }
        if(Turn==3&&!MudSetted)
        {
            //降怪前准备
            FindMonster();
        }
        //降怪
        if(Turn==4&&!MudSetted)
            CreateMonster();
        //扩毒
        if(Turn>4&&!MudSetted)
            SetMug((Turn - 2)/2);
        //双方死完，都输
        if(PlayerController.DiedSoldiersTeam1==3&&PlayerController.DIedSoldiersTeam2==3)
            AllLose();

    }
    void FindMonster()
    {
        //寻找大致降怪范围
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (Vector3.Magnitude(t.position) < BoardManager.distance / 2 + BoardManager.distance * 4)
            {
                if(t.tag=="Weapon")
                    continue;
                if(!randomPlace.Contains(t.gameObject))
                    randomPlace.Add(t.gameObject);
            }
        }
        //确定降怪点
        int random = Random.Range(0, randomPlace.Count-1);
        TearGround = randomPlace[random];
        Vector3 problePosition=new Vector3(-BoardManager.distance,0,0);
        if(random%2==0)
            problePosition = TearGround.transform.position + new Vector3(BoardManager.distance,0,0);
        randomPlace = new List<GameObject>();
        //在周围标记提示圈
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (Vector3.Distance(problePosition, t.position) < BoardManager.distance / 2 + BoardManager.distance * 2)
            {
                t.gameObject.GetComponent<SpriteRenderer>().color = new Color(0,0,10);
                randomPlace.Add(t.gameObject);
            }
        }
        MudSetted = true;
    }
    void CreateMonster()
    {
        //清除提示圈
        for (int i = 0; i < randomPlace.Count;i++)
            randomPlace[i].GetComponent<SpriteRenderer>().color = new Color(255,255,255);
        //生成怪物
        int random=Random.Range(0, randomPlace.Count-1);
        int count=0;
        if (TearGround.tag == "Occupied")
        {
            while (randomPlace[random].tag=="Occupied")
            {
                count++;
                random = Random.Range(0, randomPlace.Count - 1);
                Debug.Log(random);
                if (count > 10)
                    break;
            }
            TearGround = randomPlace[random];
        }
        Vector3 position = TearGround.transform.position;
        //GroundStage GStage = new GroundStage();
        TearGround.tag = "Occupied";
        foreach(Transform t in TearGround.GetComponentsInChildren<Transform>())
            if(t.tag=="Weapon")
                Destroy(t.gameObject);
        Instantiate(Monster, position, Quaternion.identity,GameObject.Find("Players").transform);
        Vector3 offset=new Vector3(0, -0.3f, 0);
        MonsterBlood=Instantiate(Blood,position+offset,Quaternion.identity,GameObject.Find("Canvas").transform);
        MonsterBlood.GetComponent<Text>().text = "50";
        MonsterBlood.name = "MonsterBlood";
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
        
        SetMug(1);
    }

    void SetMug(int Range)
    {
        if (Turn % 2 == 0)
        {
            //防止一出毒就被毒扩入的误差
            for (int i = 0; i < OccupiedGround.Count; i++)
            {
                GroundStage GStage = OccupiedGround[i];
                int i1 = 0, j1 = 0, i2 = 0, j2 = 0;
                i1 = OccupiedGround[i].i;
                j1 = OccupiedGround[i].j;
                for (int j = 0; j < BoardManager.row; j++)
                    for (int k = 0; k < BoardManager.col; k++)
                    {
                        if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, TearGround.transform.position) < BoardManager.distance / 2)
                        {
                            i2 = j;
                            j2 = k;
                        }
                    }
                if (Mathf.Abs(j2 - j1) <= Range - 1
                    && ((j1 >= j2 && (i1 >= i2 - Range + 1 && i1 <= i2 + Range + j2 - j1 - 1))
                    || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 + 1 && i1 <= i2 + Range - 1))))
                    continue;
                else
                {
                    GStage.InMug = false;
                    OccupiedGround[i] = GStage;
                }
            }
        }
        //标记毒
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            int i1 = 0, j1 = 0, i2 = 0, j2 = 0;
            for (int j = 0; j < BoardManager.row; j++)
                for (int k = 0; k < BoardManager.col; k++)
                {
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, t.position) < BoardManager.distance / 2)
                    {
                        i1 = j;
                        j1 = k;
                    }
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, TearGround.transform.position) < BoardManager.distance / 2)
                    {
                        i2 = j;
                        j2 = k;
                    }
                }
            if (Mathf.Abs(j2 - j1) <= Range
                && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
            {
                if (t == TearGround.transform)
                    continue;
                if (t.gameObject.GetComponent<SpriteRenderer>().color == new Color(0, 10, 0))
                    continue;
                if (t.parent == TearGround.transform)
                    continue;
                t.gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 10, 0);
                randomPlace.Add(t.gameObject);
            }
        }
        List<GroundStage> oGround = new List<GroundStage>();
        PlayerController.FaintCount = 0;
        //玩家进入掉血眩晕
        for (int i = 0; i < OccupiedGround.Count; i++)
        {
            GroundStage GStage = OccupiedGround[i];
            GStage.Faint = false;
            int i1 = 0, j1 = 0, i2 = 0, j2 = 0;
            i1 = OccupiedGround[i].i;
            j1 = OccupiedGround[i].j;
            for (int j = 0; j < BoardManager.row; j++)
                for (int k = 0; k < BoardManager.col; k++)
                {
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, TearGround.transform.position) < BoardManager.distance / 2)
                    {
                        i2 = j;
                        j2 = k;
                    }
                }
            if (Mathf.Abs(j2 - j1) <= Range
                && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
            {
                if (BoardManager.Grounds[OccupiedGround[i].i][OccupiedGround[i].j] == TearGround)
                {
                    GStage.InMug = false;
                    oGround.Add(GStage);
                    continue;
                }
                int bloodamount = int.Parse(OccupiedGround[i].PlayerBlood.GetComponent<Text>().text);
                bloodamount -= 1;
                OccupiedGround[i].PlayerBlood.GetComponent<Text>().text = bloodamount.ToString();
                //死亡
                if (bloodamount <= 0)
                {
                    Destroy(GameManager.OccupiedGround[i].PlayerBlood);
                    BoardManager.Grounds[OccupiedGround[i].i][OccupiedGround[i].j].tag = "Untagged";
                    if (PlayerController.CanMoveList.ContainsKey(OccupiedGround[i].PlayerOnGround))
                        PlayerController.CanMoveList.Remove(OccupiedGround[i].PlayerOnGround);
                    if (OccupiedGround[i].PlayerOnGround.tag == "Team1")
                        PlayerController.DiedSoldiersTeam1++;
                    if (OccupiedGround[i].PlayerOnGround.tag == "Team2")
                        PlayerController.DIedSoldiersTeam2++;
                    if (PlayerController.DiedSoldiersTeam1 == 3 || PlayerController.DIedSoldiersTeam2 == 3)
                        CreateTear(OccupiedGround[i].PlayerOnGround.transform.position);
                    Destroy(OccupiedGround[i].PlayerOnGround);
                    continue;
                }
                if (OccupiedGround[i].InMug == false)
                {
                    GStage.InMug = true;
                    GStage.Moved = true;
                    GStage.Faint = true;
                    Debug.Log("faint" + GStage.PlayerOnGround.transform.position);
                    PlayerController.FaintCount++;
                }
            }
            else
            {
                if (OccupiedGround[i].InMug == true)
                {
                    GStage.InMug = false;
                }
            }
            oGround.Add(GStage);
        }
        OccupiedGround = oGround;
        int counter = 0;
        //若都被晕住则开始新回合
        bool teamHaveMove = false;
        MudSetted = true;
        while (!teamHaveMove)
        {
            for (int i = 0; i < OccupiedGround.Count; i++)
            {
                string team = "Team" + (PlayerController.MovingTeam + 1).ToString();
                if (OccupiedGround[i].Moved == false && OccupiedGround[i].PlayerOnGround.tag == team)
                {
                    teamHaveMove = true;
                    break;
                }
            }
            if (!teamHaveMove)
                PlayerController.MovingTeam = (PlayerController.MovingTeam + 1) % TeamCount;
            counter++;
            if (counter >= 2 * GameManager.TeamCount)
            {
                Debug.Log("Died1,2" + PlayerController.DiedSoldiersTeam1 + PlayerController.DIedSoldiersTeam2);
                Debug.Log("faint,MovedDied" + PlayerController.FaintCount + PlayerController.MovedDead);
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                    Debug.Log("position,moved" + BoardManager.Grounds[OccupiedGround[i].i][OccupiedGround[i].j].transform.transform.position + GameManager.OccupiedGround[i].Moved);
                Debug.Log("ProbleBug");
                if (counter >= 10)
                    break;
                MudSetted = false;
                PlayerController.SmallTurn = 0;
                PlayerController.MovedDead = 0;
                oGround = new List<GameManager.GroundStage>();
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                {
                    GameManager.GroundStage GStage = OccupiedGround[i];
                    GStage.Moved = false;
                    oGround.Add(GStage);
                }
                Turn++;
                OccupiedGround = oGround;
                break;
            }
        }

    }

    void AllLose()
    {
        WinnerNotice.SetActive(true);
        Notice.text = "You All Losed!";

    }
    public void CreateTear(Vector3 position)//产生致死刀
    {
        if(TearCreated)
            return;
        TearCreated = true;
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if(Vector3.Distance(position,t.position)<0.1f)
            {
                Instantiate(Tear, position, Quaternion.identity, t);
                t.gameObject.tag = "Tear";
                break;
            }
        }
    }

    void AILand()
    {
        //需要空降到的地块
        GameObject GroundToLand=null;
        //遍历所有有武器且未被其他棋子占据的地块，并在遍历到的第一个地块处降落,因为地上有武器，所有Tag也不能为Weapon
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if(t.tag!="Occupied"&&t.tag!="Untagged"&&t.tag!="Weapon")
            {
                GroundToLand = t.gameObject;
                break;
            }
        }
        
        //对接降落函数，可以不用看了
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if(Vector3.Distance(GroundToLand.transform.position, t.position) < BoardManager.distance / 2)
            {
                t.gameObject.GetComponent<GroundClick>().PlaceSinglePlayer();
                break;
            }
        }
    }

    IEnumerator WaitToLand()
    {
        AI.CoroutineStarted = true;
        yield return new WaitForSeconds(1);
        AILand();
        AI.CoroutineStarted = false;
    }
}
