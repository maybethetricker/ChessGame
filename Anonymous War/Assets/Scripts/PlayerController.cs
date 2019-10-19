using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour//附着在每个棋子上
{
    public struct AimNode
    {
        public GameObject Aim;
        public Color color;
        public GameObject JudgeHelper;//若与Aim相同则说明存储的是地块，直线攻击时为Null，抓勾时为被抓到的地块
    }//用于直线抓勾记录攻击范围内的棋子的数据，以实现抓勾拉过来的操作
    //public static Dictionary<GameObject, Color> CanMoveList = new Dictionary<GameObject, Color>();//记录移动/攻击范围
    //棋子数据，通过更改颜色标识
    public GameObject Blood;

    public static int attack = 0;
    int range = 0;
    public static int FaintCount = 0;//被晕的棋子数
    public static int MovedDead = 0;
    public static List<AimNode> AimRangeList = new List<AimNode>();//抓勾可抓取范围


    // Update is called once per frame
    void Update()
    {
        //若攻击阶段，则检测攻击范围
        CheckAttack();
    }

    //若攻击阶段，则检测攻击范围
    public void CheckAttack()
    {
        if (GameManager.Stage == 2 && !GameManager.instance.EnemyChecked)
        {
            GameManager.instance.EnemyChecked = true;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    switch (GameManager.OccupiedGround[i].PlayerWeapon)
                    {
                        //change:use position of ground instead of army to check range
                        case "Long": attack = 2; range = 3; CheckRange(GameManager.PlayerOnEdit, BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position, range, "Players", 2, false); GameManager.instance.AttackMode = 2; break;
                        case "Short": attack = 4; range = 1; CheckRange(GameManager.PlayerOnEdit, BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position, range, "Players", 0, false); GameManager.instance.AttackMode = 0; break;
                        case "Drag": attack = 1; range = 3; CheckRange(GameManager.PlayerOnEdit, BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position, range, "Players", 1, true); GameManager.instance.AttackMode = 1; break;
                        case "Tear": attack = 50; range = 2; CheckRange(GameManager.PlayerOnEdit, BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position, range, "Players", 0, false); GameManager.instance.AttackMode = 0; break;
                    }
                    break;
                }
            }
            GameManager.instance.MovingTeam = (GameManager.instance.MovingTeam + 1) % GameManager.TeamCount;
            if (AimRangeList.Count == 0)
            {
                ClearHighlight();
                GameManager.instance.TimerText.text = "20";
                GameManager.instance.Timer.fillAmount = 1;
                ChangeTurn();
                GameManager.instance.EnemyChecked = false;
            }


        }
    }

    //攻击
    public void Attack(GameObject AimBlood, GameObject ThisBlood, Vector3 AimPosition, Vector3 ThisPosition, int Hurt, string AimWeapon)//攻击，参数为
    //对方血条，己方血条，己方攻击力，对方攻击力与反击范围
    {
        Debug.Log("AimWeapon:" + AimWeapon);
        ClearHighlight();
        //change:use AimBlood instead of Blood
        //攻击
        int bloodamount = int.Parse(AimBlood.GetComponent<Text>().text);
        bloodamount -= Hurt;
        AimBlood.GetComponent<Text>().text = bloodamount.ToString();
        //在反击范围内被反击
        //change:use position of ground instead of army to check range
        int aimattack = 0, aimrange = 0;
        switch (AimWeapon)
        {
            //change:use position of ground instead of army to check range
            case "Long": aimattack = 2; aimrange = 3; CheckRange(gameObject, AimPosition, aimrange, "Players", 2, false); break;
            case "Short": aimattack = 4; aimrange = 1; CheckRange(gameObject, AimPosition, aimrange, "Players", 0, false); break;
            case "Drag": aimattack = 1; aimrange = 3; CheckRange(gameObject, AimPosition, aimrange, "Players", 1, true); break;
            case "Tear": aimattack = 50; aimrange = 2; CheckRange(gameObject, AimPosition, aimrange, "Players", 0, false); break;
        }
        bool canHitBack = false;
        for (int i = 0; i < AimRangeList.Count; i++)
        {
            if (AimRangeList[i].Aim.transform.position == ThisPosition)
                canHitBack = true;
        }
        Debug.Log("CanHitBackFormer"+canHitBack);
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            //死亡/眩晕不反击
            if (GameManager.OccupiedGround[i].PlayerOnGround == gameObject && GameManager.OccupiedGround[i].Faint)
                canHitBack = false;
            if (GameManager.PlayerOnEdit.tag == gameObject.tag)
                canHitBack = false;
            if (bloodamount <= 0)
                canHitBack = false;
        }
        ClearHighlight();
        Debug.Log("CanHitBack" + canHitBack);
        if (canHitBack)
        {
            int thisblood = int.Parse(ThisBlood.GetComponent<Text>().text);
            thisblood -= aimattack;
            ThisBlood.GetComponent<Text>().text = thisblood.ToString();
            if (thisblood <= 0)
            {
                //攻击者死亡，剔除棋子状态信息
                Vector3 DiedPosition = new Vector3();
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
                        DiedPosition = BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position;
                        GameManager.OccupiedGround[i].PlayerBlood.SetActive(false);
                        Destroy(GameManager.OccupiedGround[i].PlayerBlood);
                        BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                        //BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.OccupiedGround[i].OrigColor;
                        GameManager.OccupiedGround.RemoveAt(i);
                        break;
                    }
                }
                //统计死亡人数
                if (GameManager.PlayerOnEdit.tag == "Team1")
                    GameManager.instance.TeamDiedSoldiers[0]++;
                if (GameManager.PlayerOnEdit.tag == "Team2")
                    GameManager.instance.TeamDiedSoldiers[1]++;
                ThisDie();
            }
        }
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
                    GameManager.OccupiedGround[i].PlayerBlood.SetActive(false);
                    Destroy(GameManager.OccupiedGround[i].PlayerBlood);
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    //BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.OccupiedGround[i].OrigColor;
                    GameManager.OccupiedGround.RemoveAt(i);
                    break;
                }
            }
            if (gameObject.tag == "Team1")
                GameManager.instance.TeamDiedSoldiers[0]++;
            if (gameObject.tag == "Team2")
                GameManager.instance.TeamDiedSoldiers[1]++;
            Die();

        }
        ChangeTurn();
        GameManager.instance.EnemyChecked = false;


    }
    //抓勾攻击，与普通攻击大体一样，没有统一函数所有看起来比较冗余
    public void DragAttack(GameObject AimBlood, GameObject ThisBlood, int Hurt, string AimWeapon)//抓勾攻击，参数同上
    {
        //寻找对应被拉去的地块
        GameObject surround = null;
        for (int i = 0; i < AimRangeList.Count; i++)
        {
            if (AimRangeList[i].Aim == gameObject)
            {
                if (gameObject.tag == "Monster")
                {
                    GameManager.instance.ArtifactGround.GetComponent<SpriteRenderer>().color = AimRangeList[i].color;
                    surround = AimRangeList[i].JudgeHelper;
                    break;
                }
                for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                {
                    if (GameManager.OccupiedGround[j].PlayerOnGround == gameObject)
                    {
                        BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j].GetComponent<SpriteRenderer>().color = AimRangeList[i].color;
                        break;
                    }
                }
                surround = AimRangeList[i].JudgeHelper;
                AimRangeList.RemoveAt(i);
                break;
            }
        }
        Vector3 OrigPosition = gameObject.transform.position;
        GameManager.GroundStage GStage = new GameManager.GroundStage();
        GStage.PlayerBlood = AimBlood;
        //被抓取
        if (surround.tag != "Occupied" && gameObject.tag != "Monster")//有人在那块地上或拉怪，拉不动
        {

            //下同Ground Click。PlayerMove
            Vector3 playeroffset = new Vector3(0, 0, -0.1f);
            gameObject.transform.position = surround.transform.position + playeroffset;
            int bloodNum = 0;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == gameObject)
                {
                    GStage = GameManager.OccupiedGround[i];
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    bloodNum = int.Parse(AimBlood.GetComponentInChildren<Text>().text);
                    if(GStage.Moved==false&&GStage.InMug==false&&surround.GetComponent<SpriteRenderer>().color==new Color(0,10,0,0.2f))
                    {
                        GStage.Moved = true;
                        GStage.InMug = true;
                        GStage.Faint = true;
                        FaintCount++;
                    }
                    GameManager.OccupiedGround.RemoveAt(i);
                    break;
                }
            }
            string tag = gameObject.tag;
            GameObject anotherObject = null;
            switch (surround.tag)
            {
                case "Long":
                    anotherObject = Instantiate(GameObject.Find("GameManager").GetComponent<GameManager>().LongSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    if (GameManager.RealPlayerTeam.Contains(tag))
                    {
                        anotherObject.AddComponent<RealPlayer>();
                    }
                    else if (GameManager.UseAI)
                    {
                        anotherObject.AddComponent<AI>();
                    }
                    else
                    {
                        anotherObject.AddComponent<RemoteEnemy>();
                    }
                    gameObject.SetActive(false);
                    break;
                case "Short":
                    anotherObject = Instantiate(GameObject.Find("GameManager").GetComponent<GameManager>().ShortSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    if (GameManager.RealPlayerTeam.Contains(tag))
                    {
                        anotherObject.AddComponent<RealPlayer>();
                    }
                    else if (GameManager.UseAI)
                    {
                        anotherObject.AddComponent<AI>();
                    }
                    else
                    {
                        anotherObject.AddComponent<RemoteEnemy>();
                    }
                    gameObject.SetActive(false);
                    break;
                case "Drag":
                    anotherObject = Instantiate(GameObject.Find("GameManager").GetComponent<GameManager>().DragSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    if (GameManager.RealPlayerTeam.Contains(tag))
                    {
                        anotherObject.AddComponent<RealPlayer>();
                    }
                    else if (GameManager.UseAI)
                    {
                        anotherObject.AddComponent<AI>();
                    }
                    else
                    {
                        anotherObject.AddComponent<RemoteEnemy>();
                    }
                    gameObject.SetActive(false);
                    break;
                /*case "Tear":
                    //anotherObject = Instantiate(GameObject.Find("GameManager").GetComponent<GameManager>().TearSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    if (GameManager.RealPlayerTeam.Contains(tag))
                    {
                        anotherObject.AddComponent<RealPlayer>();
                    }
                    else if (GameManager.UseAI)
                    {
                        anotherObject.AddComponent<AI>();
                    }
                    else
                    {
                        anotherObject.AddComponent<RemoteEnemy>();
                    }
                    gameObject.SetActive(false);
                    break;*/
                default:
                    anotherObject = gameObject;
                    anotherObject.transform.Rotate(45, 0, 0);
                    break;
            }
            if (tag == "Team2")
                anotherObject.GetComponentInChildren<SpriteRenderer>().color = new Color(0, 8, 8);
            anotherObject.transform.Rotate(-45, 0, 0);
            anotherObject.tag = tag;
            //AimBlood.transform.position = this.transform.position + offset;
            foreach (Transform t in surround.GetComponentsInChildren<Transform>())
                if (t.tag == "Weapon")
                    t.gameObject.SetActive(false);
            for (int i = 0; i < BoardManager.row; i++)
                for (int j = 0; j < BoardManager.col; j++)
                    if (BoardManager.Grounds[i][j] != null && Vector3.Distance(BoardManager.Grounds[i][j].transform.position, surround.transform.position) < BoardManager.distance / 2)
                    {
                        GStage.i = i;
                        GStage.j = j;
                    }
            GStage.PlayerOnGround = anotherObject;
            foreach (Transform t in anotherObject.GetComponentsInChildren<Transform>())
                if (t.tag == "Blood")
                    GStage.PlayerBlood = t.gameObject;
            GStage.PlayerBlood.GetComponentInChildren<Text>().text = bloodNum.ToString();
            //GStage.PlayerBlood = AimBlood;
            if (surround.tag != "Untagged")
                GStage.PlayerWeapon = surround.tag;
            surround.tag = "Occupied";
            GameManager.OccupiedGround.Add(GStage);
        }
        //GStage.PlayerBlood.transform.localScale *= 1.2f;
        Attack(GStage.PlayerBlood, ThisBlood, OrigPosition, GameManager.PlayerOnEdit.transform.position, Hurt, AimWeapon);
        if (!gameObject.activeSelf)
            Destroy(gameObject);
    }

    public void ArrowAttack(GameObject AimBlood, GameObject ThisBlood, Vector3 AimPosition, Vector3 ThisPosition, int Hurt, string AimWeapon)
    {
        Attack(AimBlood, ThisBlood, AimPosition, ThisPosition, Hurt, AimWeapon);
    }
    public virtual void Die()
    {
        gameObject.SetActive(false);
    }
    void ThisDie()
    {
        GameManager.PlayerOnEdit.SetActive(false);
        Destroy(GameManager.PlayerOnEdit);
        GameManager.PlayerOnEdit = null;
    }
    //确定移动攻击范围
    //确定抓勾范围，加上了一条必须在直线上
    public void CheckRange(GameObject Center, Vector3 CenterPosition, int Range, string Groups, int Mode, bool CanAttackFriendly)//Mode0：只以距离中心的距离为判断标准
    //Mode1：只允许直线攻击，Mode2远程攻击，但是近身一格不在攻击范围内
    {
        ClearHighlight();
        List<GameObject> Surround = new List<GameObject>();
        //是否在直线上
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
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
            if (t.name == Groups)
                continue;
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
                GameObject surroundLine = null;
                bool inLine = false;
                AimNode line = new AimNode();
                line.Aim = t.gameObject;
                Color color = new Color(0, 255, 255, 0.2f);
                if (t.gameObject.tag == "Monster")
                {
                    if(Vector3.Distance(CenterPosition,t.position)>BoardManager.distance *1.5)
                        continue;
                    line.color = GameManager.instance.ArtifactGround.GetComponent<SpriteRenderer>().color;
                    GameManager.instance.ArtifactGround.GetComponent<SpriteRenderer>().color = color;
                    line.JudgeHelper = null;
                    AimRangeList.Add(line);
                    continue;
                }
                foreach (GameObject g in Surround)
                {
                    if (Vector3.Angle(CenterPosition - t.position, CenterPosition - g.transform.position) < 1)
                    {

                        inLine = true;
                        surroundLine = g;
                    }
                }
                if (Mode == 1)
                {
                    if (!inLine)
                        continue;
                }
                if (Mode == 2)
                {
                    bool insurround = false;
                    for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                    {
                        if (GameManager.OccupiedGround[j].PlayerOnGround.transform == t)
                        {
                            if (Surround.Contains(BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j]))
                                insurround = true;
                            break;
                        }
                    }
                    if(insurround)
                        continue;
                }
                if (Groups == "Grounds" && (t.tag == "Occupied" || t.tag == "Weapon"))
                    continue;
                if (Groups == "Players" && t.tag != "Team1" && t.tag != "Team2" && t.tag != "Monster")
                    continue;
                if (Groups == "Players" && !CanAttackFriendly && t.tag == Center.tag)
                {
                    continue;
                }
                if (Groups == "Players" && CanAttackFriendly && Center.transform.position == t.position)
                    continue;
                if (Groups == "Grounds")
                {
                    line.color = t.gameObject.GetComponent<SpriteRenderer>().color;
                    t.gameObject.GetComponent<SpriteRenderer>().color = color;
                }
                else
                {
                    for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                    {
                        if (GameManager.OccupiedGround[j].PlayerOnGround == t.gameObject)
                        {
                            line.color = BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j].GetComponent<SpriteRenderer>().color;
                            BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j].GetComponent<SpriteRenderer>().color = color;
                            break;
                        }
                    }
                }
                if (Mode == 0)
                {
                    if (Groups == "Grounds")
                        line.JudgeHelper = t.gameObject;
                    else
                    {
                        line.JudgeHelper = null;
                    }
                }
                if (Mode == 1)
                    line.JudgeHelper = surroundLine;
                AimRangeList.Add(line);

            }
        }
    }
    public void ChangeTurn()//更换回合
    {
        GameManager.Stage = 1;
        GameManager.instance.SmallTurn++;
        //Debug.Log("SmallTurn:"+GameManager.instance.SmallTurn);
        //若本回合结束更换大回合
        int totalSmallTurns = GameManager.TeamCount * 3 - FaintCount + MovedDead;
        for (int k = 0; k < GameManager.TeamCount;k++)
            totalSmallTurns -= GameManager.instance.TeamDiedSoldiers[k];
        if (GameManager.instance.SmallTurn >= totalSmallTurns)
        {
            //Debug.Log("AddTurn");
            GameManager.MudSetted = false;
            GameManager.instance.SmallTurn = 0;
            MovedDead = 0;
            List<GameManager.GroundStage> oGround = new List<GameManager.GroundStage>();
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                GameManager.GroundStage GStage = GameManager.OccupiedGround[i];
                GStage.Moved = false;
                oGround.Add(GStage);
            }
            GameManager.instance.Turn++;
            GameManager.OccupiedGround = oGround;
        }
        bool teamHaveMove = false;
        int counter = 0;
        //若死人或晕人导致一队可能连续移动（一队全部动不了就再次更改下小回合移动的一方
        while (!teamHaveMove)
        {

            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
                if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                {
                    teamHaveMove = true;
                    break;
                }
            }
            if (!teamHaveMove)
                GameManager.instance.MovingTeam = (GameManager.instance.MovingTeam + 1) % GameManager.TeamCount;
            counter++;
            if (counter > 2 * GameManager.TeamCount)
            {
                Debug.Log("SmallTurn" + GameManager.instance.SmallTurn);
                Debug.Log("faint,MovedDied" + FaintCount + MovedDead);
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                    Debug.Log("position,moved" + BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.transform.position + GameManager.OccupiedGround[i].Moved);
                Debug.Log("Bug");

                break;
            }
        }
        Color color = new Color(255, 255, 0, 0.2f);
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
            if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
            {
                GameManager.GroundStage GStage=GameManager.OccupiedGround[i];
                GStage.OrigColor = BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color;
                GameManager.OccupiedGround[i] = GStage;
                BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = color;
            }
        }
        //change:fix the bug due to moving a same chess contineously
        GameManager.PlayerOnEdit = null;
    }

    public void ClearHighlight()
    {
        foreach (AimNode line in AimRangeList)
        {
            if (line.Aim == null)
            {
                Debug.Log("AimRangeList:Aim is null");
                continue;
            }
            if (line.Aim.tag == "Monster")
            {
                GameManager.instance.ArtifactGround.GetComponent<SpriteRenderer>().color = line.color;
            }
            if (line.Aim == line.JudgeHelper)
                line.Aim.GetComponent<SpriteRenderer>().color = line.color;
            for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
            {
                if (GameManager.OccupiedGround[j].PlayerOnGround == line.Aim)
                {
                    BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j].GetComponent<SpriteRenderer>().color = line.color;
                    break;
                }
            }
        }
        AimRangeList = new List<AimNode>();
    }
}
