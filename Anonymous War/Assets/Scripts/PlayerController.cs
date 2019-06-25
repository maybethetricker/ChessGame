using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour//附着在每个棋子上
{
    //几种玩家状态，临时替代一种game object多状态
    public GameObject LongSoldier;
    public GameObject ShortSoldier;
    public GameObject DragSoldier;
    public GameObject TearSoldier;
    public GameObject Tear;
    public struct AttackLine
    {
        public GameObject Enemy;
        public Color color;
        public GameObject Surround;//周围地块，亦即抓勾抓到哪
    }//用于直线抓勾记录攻击范围内的棋子的数据，以实现抓勾拉过来的操作
    public static Dictionary<GameObject, Color> CanMoveList = new Dictionary<GameObject, Color>();//记录移动/攻击范围
    //棋子数据，通过更改颜色标识
    GameObject Blood;
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
    void Update()//若攻击阶段，则检测攻击范围
    {
        if (GameManager.Stage == 2 && !EnemyChecked)
        {
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    switch (GameManager.OccupiedGround[i].PlayerWeapon)
                    {
                        case "Long": attack = 2; range = 2; CheckRange(GameManager.PlayerOnEdit, GameManager.OccupiedGround[i].Ground.transform.position, range, "Players"); break;
                        case "Short": attack = 4; range = 1; CheckRange(GameManager.PlayerOnEdit, GameManager.OccupiedGround[i].Ground.transform.position, range, "Players"); break;
                        case "Drag": attack = 1; range = 3; CheckRangeLine(GameManager.PlayerOnEdit, GameManager.OccupiedGround[i].Ground.transform.position, range, "Players"); OnlyLine = true; break;
                        case "Tear": attack = 50; range = 2; CheckRange(GameManager.PlayerOnEdit, GameManager.OccupiedGround[i].Ground.transform.position, range, "Players"); break;
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
    /// <summary>
    /// OnMouseDown is called when the user has pressed the mouse button while
    /// over the GUIElement or Collider.
    /// </summary>
    void OnMouseDown()//在移动/攻击时点击该回合可操作棋子触发操作
    {
        if (GameManager.Stage == 1 && GameManager.PlayerOnEdit == null)//移动
        {
            //只有本回合能动的一方可动
            if (this.tag != "Team" + (MovingTeam + 1).ToString())
                return;
            foreach (GameManager.GroundStage gstage in GameManager.OccupiedGround)
                if (gstage.PlayerOnGround == gameObject && gstage.Moved == true)
                    return;
            //标记出移动者并计算可移动范围
            transform.localScale *= 1.1f;
            GameManager.PlayerOnEdit = gameObject;
            foreach (GameManager.GroundStage gstage in GameManager.OccupiedGround)
                if (gstage.PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    CheckRange(gstage.PlayerOnGround, gstage.Ground.transform.position, MP, "Grounds");
                    break;
                }

        }
        //移动，同上
        else if (GameManager.Stage == 1 && Vector2.Distance(GameManager.PlayerOnEdit.transform.position, transform.position) > 0.1f && this.tag == "Team" + (MovingTeam + 1).ToString())
        {
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {

                if (GameManager.OccupiedGround[i].PlayerOnGround == gameObject && GameManager.OccupiedGround[i].Moved == true)
                    return;
            }
            transform.localScale *= 1.1f;
            GameManager.PlayerOnEdit.transform.localScale /= 1.1f;
            GameManager.PlayerOnEdit = gameObject;
            foreach (GameManager.GroundStage gstage in GameManager.OccupiedGround)
                if (gstage.PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    CheckRange(gstage.PlayerOnGround, gstage.Ground.transform.position, MP, "Grounds");
                    break;
                }
        }
        //攻击
        else if (GameManager.Stage == 2 && Vector2.Distance(GameManager.PlayerOnEdit.transform.position, transform.position) > 0.1f)
        {
            //获取反击攻击力，反击范围与双方血条
            GameObject thisBlood = null;
            int aimRange = 0;
            int aimAttack = 0;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == this.gameObject)
                {
                    Blood = GameManager.OccupiedGround[i].PlayerBlood;
                    switch (GameManager.OccupiedGround[i].PlayerWeapon)
                    {
                        case "Long": aimAttack = 2; aimRange = 2; break;
                        case "Short": aimAttack = 3; aimRange = 1; break;
                        case "Drag": aimAttack = 1; aimRange = 3; break;
                        case "Tear": aimAttack = 50; aimRange = 1; break;
                    }
                }
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    thisBlood = GameManager.OccupiedGround[i].PlayerBlood;
                }
            }
            if (gameObject.tag == "Monster")
            {
                Blood = GameObject.Find("MonsterBlood");
            }
            //是否直线攻击
            if (CanMoveList.ContainsKey(gameObject) && !OnlyLine)
                Attack(Blood, thisBlood, attack, aimAttack, aimRange);
            if (OnlyLine)
            {

                for (int i = 0; i < LineCanAttack.Count; i++)
                {
                    if (LineCanAttack[i].Enemy == gameObject)
                    {
                        DragAttack(Blood, thisBlood, attack, aimAttack, aimRange);
                        OnlyLine = false;
                        break;
                    }
                }
            }

        }
    }
    //攻击
    void Attack(GameObject AimBlood, GameObject ThisBlood, int Hurt, int aimattack, int aimrange)//攻击，参数为
    //对方血条，己方血条，己方攻击力，对方攻击力与反击范围
    {
        //攻击
        int bloodamount = int.Parse(Blood.GetComponent<Text>().text);
        bloodamount -= Hurt;
        Blood.GetComponent<Text>().text = bloodamount.ToString();
        //在反击范围内被反击
        if (Vector3.Distance(gameObject.transform.localPosition, GameManager.PlayerOnEdit.transform.localPosition) < BoardManager.distance / 2 + BoardManager.distance * aimrange)
        {
            
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
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
                        GameManager.OccupiedGround[i].Ground.tag = "Untagged";
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
                    GameManager.OccupiedGround[i].Ground.tag = "Untagged";
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
    void DragAttack(GameObject AimBlood, GameObject ThisBlood, int Hurt, int aimattack, int aimrange)//抓勾攻击，参数同上
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
        int bloodamount = int.Parse(Blood.GetComponent<Text>().text);
        bloodamount -= Hurt;
        Blood.GetComponent<Text>().text = bloodamount.ToString();
        //反击
        if (Vector3.Distance(gameObject.transform.localPosition, GameManager.PlayerOnEdit.transform.localPosition) < BoardManager.distance / 2 + BoardManager.distance * aimrange)
        {
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == gameObject && GameManager.OccupiedGround[i].Faint)
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
                        GameManager.OccupiedGround[i].Ground.tag = "Untagged";
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
                    GameManager.OccupiedGround[i].Ground.tag = "Untagged";
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


                    anotherObject = Instantiate(LongSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    gameObject.SetActive(false);
                    break;
                case "Short":


                    anotherObject = Instantiate(ShortSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    gameObject.SetActive(false);
                    break;
                case "Drag":


                    anotherObject = Instantiate(DragSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    gameObject.SetActive(false);
                    break;
                case "Tear":
                    gameObject.SetActive(false);
                    anotherObject = Instantiate(TearSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
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
            GStage.Ground = surround;
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
                    GameManager.OccupiedGround[i].Ground.tag = "Untagged";
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
    void CheckRange(GameObject Center, Vector3 CenterPosition, int Range, string Groups)//检测移动与攻击范围，目前是用直接距离将就的，原理为计入
    //Center周围Range范围内的所有地块/敌方单位
    {
        foreach (KeyValuePair<GameObject, Color> key in CanMoveList)
            key.Key.GetComponent<SpriteRenderer>().color = key.Value;
        CanMoveList = new Dictionary<GameObject, Color>();
        LineCanAttack = new List<AttackLine>();
        foreach (Transform t in GameObject.Find(Groups).GetComponentsInChildren<Transform>())
        {
            if (Vector3.Distance(CenterPosition, t.position) < BoardManager.distance / 2 + BoardManager.distance * Range)
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
            if (Vector3.Distance(CenterPosition, t.position) < BoardManager.distance / 2 + BoardManager.distance * Range)
            {
                if (Center.tag == t.tag)
                    continue;
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
                Debug.Log("Died1,2"+DiedSoldiersTeam1+DIedSoldiersTeam2);
                Debug.Log("faint,MovedDied" + FaintCount + MovedDead);
                Debug.Log("Bug");

                break;
            }
        }
    }
    void CreateTear(Vector3 position)//生成致死刀
    {
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
}
