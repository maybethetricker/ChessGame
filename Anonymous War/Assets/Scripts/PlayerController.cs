using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour//附着在每个棋子上
{
    //几种玩家状态，临时替代一种game object多状态
    public struct AttackLine
    {
        public GameObject Enemy;
        public Color color;
        public GameObject Surround;//周围地块，亦即抓勾抓到哪
    }//用于直线抓勾记录攻击范围内的棋子的数据，以实现抓勾拉过来的操作
    public static Dictionary<GameObject, Color> CanMoveList = new Dictionary<GameObject, Color>();//记录移动/攻击范围
    //棋子数据，通过更改颜色标识
    public GameObject Blood;
    public static int MP = 1;//棋子移动范围
    //public static List<CanMove> CanMoveList = new List<CanMove>();
    public static bool EnemyChecked;//是否检测了可攻击范围
    public static int attack = 0;
    int range = 0;
    public static int MovingTeam = 1;//在移动的队伍
    public static int SmallTurn;//每回合的小回合
    public static int FaintCount = 0;//被晕的棋子数
    public static int DiedSoldiersTeam1 = 0;//死亡的队1人数
    public static int DIedSoldiersTeam2 = 0;//。。。队2。。
    public static List<AttackLine> LineCanAttack = new List<AttackLine>();//抓勾可抓取范围
    public static bool OnlyLine = false;//是否是抓勾攻击方式
    public static int MovedDead = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //若攻击阶段，则检测攻击范围
        CheckAttack();
    }

    //若攻击阶段，则检测攻击范围
    public void CheckAttack()
    {
        if (GameManager.Stage == 2 && !EnemyChecked)
        {
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    switch (GameManager.OccupiedGround[i].PlayerWeapon)
                    {
//change:use position of ground instead of army to check range
                        case "Long": attack = 2; range = 2; CheckRange(GameManager.PlayerOnEdit, BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position, range, "Players"); break;
                        case "Short": attack = 4; range = 1; CheckRange(GameManager.PlayerOnEdit, BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position, range, "Players"); break;
                        case "Drag": attack = 1; range = 3; CheckRangeLine(GameManager.PlayerOnEdit, BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position, range, "Players"); OnlyLine = true; break;
                        case "Tear": attack = 50; range = 2; CheckRange(GameManager.PlayerOnEdit, BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position, range, "Players"); break;
                        default:attack=0;range=0;CanMoveList=new Dictionary<GameObject, Color>();break;
                    }
                    break;
                }
            }

            MovingTeam = (MovingTeam + 1) % GameManager.TeamCount;
            if ((CanMoveList.Count == 0 && (!OnlyLine)) || (OnlyLine && LineCanAttack.Count == 0))
            {
                OnlyLine = false;
                ChangeTurn();
            }
            else
                EnemyChecked = true;

        }
    }

    //攻击
    public void Attack(GameObject AimBlood, GameObject ThisBlood, int Hurt, int aimattack, int aimrange)//攻击，参数为
    //对方血条，己方血条，己方攻击力，对方攻击力与反击范围
    {
        //change:use AimBlood instead of Blood
        //攻击
        int bloodamount = int.Parse(AimBlood.GetComponent<Text>().text);
        bloodamount -= Hurt;
        AimBlood.GetComponent<Text>().text = bloodamount.ToString();
        //在反击范围内被反击
        //change:use position of ground instead of army to check range
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            int i1 = 0, j1 = 0;
            for (int j = 0; j < BoardManager.row; j++)
                for (int k = 0; k < BoardManager.col; k++)
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, this.transform.position) < BoardManager.distance / 2)
                    {
                        i1 = j;
                        j1 = k;
                    }
            if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                if (Mathf.Abs(GameManager.OccupiedGround[i].j - j1) > aimrange
                    || (j1 >= GameManager.OccupiedGround[i].j && (i1 < GameManager.OccupiedGround[i].i - aimrange || i1 > GameManager.OccupiedGround[i].i + aimrange + GameManager.OccupiedGround[i].j - j1))
                    || (j1 < GameManager.OccupiedGround[i].j && (i1 < GameManager.OccupiedGround[i].i - aimrange + GameManager.OccupiedGround[i].j - j1 || i1 > GameManager.OccupiedGround[i].i + aimrange)))
                    goto AfterHurt;
            //眩晕不反击
            if (GameManager.OccupiedGround[i].PlayerOnGround == gameObject && GameManager.OccupiedGround[i].Faint)
                goto AfterHurt;
        }
        int thisblood = int.Parse(ThisBlood.GetComponent<Text>().text);
        thisblood -= aimattack;
        ThisBlood.GetComponent<Text>().text = thisblood.ToString();
        if (thisblood <= 0)
        {
            //攻击者死亡，剔除棋子状态信息
            for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                if (GameManager.OccupiedGround[j].PlayerOnGround == GameManager.PlayerOnEdit && (GameManager.OccupiedGround[j].Moved))
                {
                    MovedDead++;
                    break;
                }
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {

                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    Destroy(GameManager.OccupiedGround[i].PlayerBlood);
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    GameManager.OccupiedGround.RemoveAt(i);
                    break;
                }
            }
            if (CanMoveList.ContainsKey(GameManager.PlayerOnEdit))
                CanMoveList.Remove(GameManager.PlayerOnEdit);
            //统计死亡人数
            if (GameManager.PlayerOnEdit.tag == "Team1")
                DiedSoldiersTeam1++;
            if (GameManager.PlayerOnEdit.tag == "Team2")
                DIedSoldiersTeam2++;
            if (DiedSoldiersTeam1 == 3 || DIedSoldiersTeam2 == 3)
                CreateTear(GameManager.PlayerOnEdit.transform.position);
            ThisDie();
        }

    AfterHurt:
        if (bloodamount <= 0)
        {
            //被攻击者死亡，与之上相似
            for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                if (GameManager.OccupiedGround[j].PlayerOnGround == gameObject)
                {
                    if (!GameManager.OccupiedGround[j].Moved)
                        break;

                    MovedDead++;
                    break;
                }
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {

                if (GameManager.OccupiedGround[i].PlayerOnGround == gameObject)
                {
                    Destroy(GameManager.OccupiedGround[i].PlayerBlood);
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    GameManager.OccupiedGround.RemoveAt(i);
                    break;
                }
            }
            if (CanMoveList.ContainsKey(gameObject))
                CanMoveList.Remove(gameObject);

            if (gameObject.tag == "Team1")
                DiedSoldiersTeam1++;
            if (gameObject.tag == "Team2")
                DIedSoldiersTeam2++;
            if (DiedSoldiersTeam1 == 3 || DIedSoldiersTeam2 == 3)
                CreateTear(gameObject.transform.position);
            Die();

        }
        ChangeTurn();
        foreach (KeyValuePair<GameObject, Color> key in CanMoveList)
            key.Key.GetComponent<SpriteRenderer>().color = key.Value;
        EnemyChecked = false;


    }
    //抓勾攻击，与普通攻击大体一样，没有统一函数所有看起来比较冗余
    public void DragAttack(GameObject AimBlood, GameObject ThisBlood, int Hurt, int aimattack, int aimrange)//抓勾攻击，参数同上
    {
        //寻找对应被拉去的地块
        GameObject surround = null;
        for (int i = 0; i < LineCanAttack.Count; i++)
        {
            if (LineCanAttack[i].Enemy == gameObject)
            {
                surround = LineCanAttack[i].Surround;
                if (surround.tag != "Occupied" && surround.tag != "Untagged")
                    LineCanAttack.RemoveAt(i);
                break;
            }
        }
        //攻击
        int bloodamount = int.Parse(AimBlood.GetComponent<Text>().text);
        bloodamount -= Hurt;
        AimBlood.GetComponent<Text>().text = bloodamount.ToString();
        //反击
//change:use position of ground instead of army to check range
        
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            int i1 = 0, j1 = 0;
            for (int j = 0; j < BoardManager.row; j++)
                for (int k = 0; k < BoardManager.col; k++)
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, this.transform.position) < BoardManager.distance / 2)
                    {
                        i1 = j;
                        j1 = k;
                    }
            if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                if (Mathf.Abs(GameManager.OccupiedGround[i].j - j1) > aimrange
                    || (j1 >= GameManager.OccupiedGround[i].j && (i1 < GameManager.OccupiedGround[i].i - aimrange || i1 > GameManager.OccupiedGround[i].i + aimrange + GameManager.OccupiedGround[i].j - j1))
                    || (j1 < GameManager.OccupiedGround[i].j && (i1 < GameManager.OccupiedGround[i].i - aimrange + GameManager.OccupiedGround[i].j - j1 || i1 > GameManager.OccupiedGround[i].i + aimrange)))
                    goto AfterHurt;
            if (GameManager.OccupiedGround[i].PlayerOnGround == gameObject && GameManager.OccupiedGround[i].Faint)
                goto AfterHurt;
            if(GameManager.PlayerOnEdit.tag==gameObject.tag)
                goto AfterHurt;
        }
        int thisblood = int.Parse(ThisBlood.GetComponent<Text>().text);
        thisblood -= aimattack;
        ThisBlood.GetComponent<Text>().text = thisblood.ToString();
        if (thisblood <= 0)
        {
            for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                if (GameManager.OccupiedGround[j].PlayerOnGround == GameManager.PlayerOnEdit && GameManager.OccupiedGround[j].Moved)
                    MovedDead++;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {

                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    Destroy(GameManager.OccupiedGround[i].PlayerBlood);
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    GameManager.OccupiedGround.RemoveAt(i);
                    break;
                }
            }

            if (GameManager.PlayerOnEdit.tag == "Team1")
            {
                DiedSoldiersTeam1++;
            }
            if (GameManager.PlayerOnEdit.tag == "Team2")
                DIedSoldiersTeam2++;
            if (DiedSoldiersTeam1 == 3 || DIedSoldiersTeam2 == 3)
                CreateTear(GameManager.PlayerOnEdit.transform.position);
            ThisDie();
        }
    AfterHurt:
    //被抓取
        if (surround.tag != "Occupied" && gameObject.tag != "Monster")//有人在那块地上或拉怪，拉不动
        {
            //下同Ground Click。PlayerMove
            gameObject.transform.position = surround.transform.position;
            bool inMug = false;
            bool faint = false;
            bool moved = false;
            string WeaponTag = "";
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == gameObject)
                {
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    inMug = GameManager.OccupiedGround[i].InMug;
                    faint = GameManager.OccupiedGround[i].Faint;
                    moved = GameManager.OccupiedGround[i].Moved;
                    WeaponTag = GameManager.OccupiedGround[i].PlayerWeapon;
                    GameManager.OccupiedGround.RemoveAt(i);
                    break;
                }
            }
            string tag = gameObject.tag;
            Vector3 offset = new Vector3(0, -BoardManager.distance / 3, 0);
            GameObject anotherObject = null;
            switch (surround.tag)
            {
                case "Long":


                    anotherObject = Instantiate(GameObject.Find("GameManager").GetComponent<GameManager>().LongSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    if(tag=="Team1")
                        anotherObject.AddComponent<RealPlayer>();
                    else
                    {
                        anotherObject.AddComponent<AI>();
                    }
                    gameObject.SetActive(false);
                    break;
                case "Short":


                    anotherObject = Instantiate(GameObject.Find("GameManager").GetComponent<GameManager>().ShortSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    if(tag=="Team1")
                        anotherObject.AddComponent<RealPlayer>();
                    else
                    {
                        anotherObject.AddComponent<AI>();
                    }
                    gameObject.SetActive(false);
                    break;
                case "Drag":


                    anotherObject = Instantiate(GameObject.Find("GameManager").GetComponent<GameManager>().DragSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    if(tag=="Team1")
                        anotherObject.AddComponent<RealPlayer>();
                    else
                    {
                        anotherObject.AddComponent<AI>();
                    }
                    gameObject.SetActive(false);
                    break;
                case "Tear":
                    
                    anotherObject = Instantiate(GameObject.Find("GameManager").GetComponent<GameManager>().TearSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    if(tag=="Team1")
                        anotherObject.AddComponent<RealPlayer>();
                    else
                    {
                        anotherObject.AddComponent<AI>();
                    }
                    gameObject.SetActive(false);
                    break;
                default:
                    anotherObject = gameObject;
                    if (tag == "Team2")
                        gameObject.transform.Rotate(0, 0, 180);
                    break;
            }
            if (tag == "Team2")
                anotherObject.transform.Rotate(0, 0, 180);
            anotherObject.tag = tag;
            anotherObject.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
            AimBlood.transform.position = this.transform.position + offset;
            foreach (Transform t in surround.GetComponentsInChildren<Transform>())
                if (t.tag == "Weapon")
                    t.gameObject.SetActive(false);
            GameManager.GroundStage GStage = new GameManager.GroundStage();
            for (int i = 0; i < BoardManager.row;i++)
            for (int j = 0; j < BoardManager.col;j++)
                if (BoardManager.Grounds[i][j]!=null&&Vector3.Distance(BoardManager.Grounds[i][j].transform.position, surround.transform.position) < BoardManager.distance / 2)
                {
                    GStage.i = i;
                    GStage.j = j;
                }
            GStage.PlayerOnGround = anotherObject;
            GStage.PlayerBlood = AimBlood;
            GStage.InMug = inMug;
            GStage.Faint = faint;
            if (surround.tag != "Untagged")
                GStage.PlayerWeapon = surround.tag;
            else
                GStage.PlayerWeapon = WeaponTag;
            surround.tag = "Occupied";
            GStage.Moved = moved;
            GameManager.OccupiedGround.Add(GStage);

        }
        //死亡与回合轮换，同Attack
        if (bloodamount <= 0)
        {
            for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                if (GameManager.OccupiedGround[j].PlayerOnGround == gameObject && GameManager.OccupiedGround[j].Moved)
                    MovedDead++;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {

                if (GameManager.OccupiedGround[i].PlayerOnGround == gameObject)
                {
                    Destroy(GameManager.OccupiedGround[i].PlayerBlood);
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    GameManager.OccupiedGround.RemoveAt(i);
                    break;
                }
            }
            for (int i = 0; i < LineCanAttack.Count; i++)
            {
                if (LineCanAttack[i].Enemy == gameObject)
                {
                    LineCanAttack.RemoveAt(i);
                    break;
                }
            }

            if (gameObject.tag == "Team1")
                DiedSoldiersTeam1++;
            if (gameObject.tag == "Team2")
                DIedSoldiersTeam2++;
            if (DiedSoldiersTeam1 == 3 || DIedSoldiersTeam2 == 3)
                CreateTear(gameObject.transform.position);
            Die();
        }
        ChangeTurn();
        foreach (AttackLine line in LineCanAttack)
            if(line.Enemy!=GameManager.PlayerOnEdit)
                line.Enemy.GetComponent<SpriteRenderer>().color = line.color;
        EnemyChecked = false;
        if (!gameObject.activeSelf)
            Destroy(gameObject);

        CanMoveList = new Dictionary<GameObject, Color>();
    }
    public virtual void Die()
    {
        gameObject.SetActive(false);
    }
    void ThisDie()
    {
        Destroy(GameManager.PlayerOnEdit);
        GameManager.PlayerOnEdit = null;
    }
    //确定移动攻击范围
    public void CheckRange(GameObject Center, Vector3 CenterPosition, int Range, string Groups)//检测移动与攻击范围，目前是用直接距离将就的，原理为计入
    //Center周围Range范围内的所有地块/敌方单位
    {
        foreach (KeyValuePair<GameObject, Color> key in CanMoveList)
            key.Key.GetComponent<SpriteRenderer>().color = key.Value;
        CanMoveList = new Dictionary<GameObject, Color>();
        LineCanAttack = new List<AttackLine>();
        foreach (Transform t in GameObject.Find(Groups).GetComponentsInChildren<Transform>())
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
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, CenterPosition) < BoardManager.distance / 2)
                    {
                        i2 = j;
                        j2 = k;
                    }
                }
            if (Mathf.Abs(j2 - j1) <= Range
                && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
            {
                if (Groups == "Grounds" && t.tag == "Occupied")
                    continue;
                if (Groups == "Players" && Center.tag == t.tag)
                    continue;

                CanMoveList.Add(t.gameObject, t.gameObject.GetComponent<SpriteRenderer>().color);
                t.gameObject.GetComponent<SpriteRenderer>().color = new Color(10, 0, 0);
            }
        }
    }
    //确定抓勾范围，加上了一条必须在直线上
    void CheckRangeLine(GameObject Center, Vector3 CenterPosition, int Range, string Groups)//类似上方，只检测直线上的敌人，用于限定直线攻击
    {
        LineCanAttack = new List<AttackLine>();
        CanMoveList = new Dictionary<GameObject, Color>();
        List<GameObject> Surround = new List<GameObject>();
        //是否在直线上
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            
            if (Vector3.Distance(CenterPosition, t.position) < BoardManager.distance / 2 + BoardManager.distance)
            {
                if (Vector3.Distance(CenterPosition, t.position) < BoardManager.distance / 2)
                    continue;
                if (t.tag == "Weapon")
                    continue;
                Surround.Add(t.gameObject);

            }
        }//是否在范围内
        foreach (Transform t in GameObject.Find(Groups).GetComponentsInChildren<Transform>())
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
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, CenterPosition) < BoardManager.distance / 2)
                    {
                        i2 = j;
                        j2 = k;
                    }
                }
            if (Mathf.Abs(j2 - j1) <= Range
                && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
            {
                //是否允许队友攻击
                if(Center==t.gameObject)
                    continue;
                //if (Center.tag == t.tag)
                //continue;
                GameObject surroundLine = null;
                bool inLine = false;
                foreach (GameObject g in Surround)
                    if (Vector3.Angle(CenterPosition - t.position, CenterPosition - g.transform.position) < 1)
                    {
                        inLine = true;
                        surroundLine = g;
                    }
                if (!inLine)
                    continue;

                AttackLine line = new AttackLine();
                line.Enemy = t.gameObject;
                line.color = t.gameObject.GetComponent<SpriteRenderer>().color;
                line.Surround = surroundLine;
                LineCanAttack.Add(line);
                t.gameObject.GetComponent<SpriteRenderer>().color = new Color(10, 0, 0);
            }
        }
    }
    void ChangeTurn()//更换回合
    {
        
        GameManager.Stage = 1;
        SmallTurn++;
        //若本回合结束更换大回合
        if (SmallTurn >= GameManager.TeamCount * 3 - FaintCount - DiedSoldiersTeam1 - DIedSoldiersTeam2 + MovedDead)
        {
            GameManager.MudSetted = false;
            SmallTurn = 0;
            MovedDead = 0;
            List<GameManager.GroundStage> oGround = new List<GameManager.GroundStage>();
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                GameManager.GroundStage GStage = GameManager.OccupiedGround[i];
                GStage.Moved = false;
                oGround.Add(GStage);
            }
            
            GameManager.Turn++;
            GameManager.OccupiedGround = oGround;
        }
        bool teamHaveMove = false;
        int counter = 0;
        //若死人或晕人导致一队可能连续移动（一队全部动不了就再次更改下小回合移动的一方
        while (!teamHaveMove)
        {

            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                string team = "Team" + (MovingTeam + 1).ToString();
                if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                {
                    teamHaveMove = true;
                    break;
                }
            }
            if (!teamHaveMove)
                MovingTeam = (MovingTeam + 1) % GameManager.TeamCount;
            counter++;
            if (counter > 2 * GameManager.TeamCount)
            {
                Debug.Log("SmallTurn" + SmallTurn);
                Debug.Log("Died1,2"+DiedSoldiersTeam1+DIedSoldiersTeam2);
                Debug.Log("faint,MovedDied" + FaintCount + MovedDead);
                for (int i = 0; i < GameManager.OccupiedGround.Count;i++)
                    Debug.Log("position,moved" + BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.transform.position+GameManager.OccupiedGround[i].Moved);
                Debug.Log("Bug");

                break;
            }
        }
//change:fix the bug due to moving a same chess contineously
        GameManager.PlayerOnEdit = null;
    }

    void CreateTear(Vector3 position)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().CreateTear(position);
    }
}
