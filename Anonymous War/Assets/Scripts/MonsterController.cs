using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MonsterController : PlayerController
{

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        CheckAttack();
        //扩毒
        if(GameManager.Turn>4&&!GameManager.MudSetted)
            SetMug((GameManager.Turn - 2)/2);
    }

    public override void Die()//怪死，游戏结束
    {
        GameManager.WinnerNotice.SetActive(true);
        if(DiedSoldiersTeam1<3&&DIedSoldiersTeam2<3)
        {
            
            GameManager.Notice.text = "ALL WIN";
        }
        else if(DiedSoldiersTeam1>=3)
        {
            GameManager.Notice.GetComponent<Text>().text = "TEAM2 WIN";
        }
        else
        {
            GameManager.Notice.GetComponent<Text>().text = "TEAM1 WIN";
        }
    }

    /// <summary>
    /// OnMouseDown is called when the user has pressed the mouse button while
    /// over the GUIElement or Collider.
    /// </summary>
    void OnMouseDown()
    {
        //玩家攻击时的受击检测，与AI逻辑无关，可不看
        if (GameManager.Stage == 2 && Vector2.Distance(GameManager.PlayerOnEdit.transform.position, transform.position) > 0.1f)
        {
            //只有本回合能动的一方可动
            if (!GameManager.RealPlayerTeam.Contains(GameManager.PlayerOnEdit.tag))
                return;
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
                        case "Short": aimAttack = 4; aimRange = 1; break;
                        case "Drag": aimAttack = 1; aimRange = 3; break;
                        case "Tear": aimAttack = 50; aimRange = 0; break;
                    }
//change:data error
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

    public void SetMug(int Range)
    {
        if (GameManager.Turn % 2 == 0)
        {
            //防止一出毒就被毒扩入的误差
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                GameManager.GroundStage GStage = GameManager.OccupiedGround[i];
                int i1 = 0, j1 = 0, i2 = 0, j2 = 0;
                i1 = GameManager.OccupiedGround[i].i;
                j1 = GameManager.OccupiedGround[i].j;
                for (int j = 0; j < BoardManager.row; j++)
                    for (int k = 0; k < BoardManager.col; k++)
                    {
                        if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, GameManager.instance.TearGround.transform.position) < BoardManager.distance / 2)
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
                    GameManager.OccupiedGround[i] = GStage;
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
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, GameManager.instance.TearGround.transform.position) < BoardManager.distance / 2)
                    {
                        i2 = j;
                        j2 = k;
                    }
                }
            if (Mathf.Abs(j2 - j1) <= Range
                && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
            {
                if (t == GameManager.instance.TearGround.transform)
                    continue;
                if (t.gameObject.GetComponent<SpriteRenderer>().color == new Color(0, 10, 0))
                    continue;
                if (t.parent == GameManager.instance.TearGround.transform)
                    continue;
                t.gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 10, 0);
                GameManager.instance.randomPlace.Add(t.gameObject);
            }
        }
        List<GameManager.GroundStage> oGround = new List<GameManager.GroundStage>();
        PlayerController.FaintCount = 0;
        //玩家进入掉血眩晕
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            GameManager.GroundStage GStage = GameManager.OccupiedGround[i];
            GStage.Faint = false;
            int i1 = 0, j1 = 0, i2 = 0, j2 = 0;
            i1 = GameManager.OccupiedGround[i].i;
            j1 = GameManager.OccupiedGround[i].j;
            for (int j = 0; j < BoardManager.row; j++)
                for (int k = 0; k < BoardManager.col; k++)
                {
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, GameManager.instance.TearGround.transform.position) < BoardManager.distance / 2)
                    {
                        i2 = j;
                        j2 = k;
                    }
                }
            if (Mathf.Abs(j2 - j1) <= Range
                && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
            {
                if (BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j] == GameManager.instance.TearGround)
                {
                    GStage.InMug = false;
                    oGround.Add(GStage);
                    continue;
                }
                int bloodamount = int.Parse(GameManager.OccupiedGround[i].PlayerBlood.GetComponent<Text>().text);
                bloodamount -= 1;
                GameManager.OccupiedGround[i].PlayerBlood.GetComponent<Text>().text = bloodamount.ToString();
                //死亡
                if (bloodamount <= 0)
                {
                    Destroy(GameManager.OccupiedGround[i].PlayerBlood);
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    if (PlayerController.CanMoveList.ContainsKey(GameManager.OccupiedGround[i].PlayerOnGround))
                        PlayerController.CanMoveList.Remove(GameManager.OccupiedGround[i].PlayerOnGround);
                    if (GameManager.OccupiedGround[i].PlayerOnGround.tag == "Team1")
                        PlayerController.DiedSoldiersTeam1++;
                    if (GameManager.OccupiedGround[i].PlayerOnGround.tag == "Team2")
                        PlayerController.DIedSoldiersTeam2++;
                    if (PlayerController.DiedSoldiersTeam1 == 3 || PlayerController.DIedSoldiersTeam2 == 3)
                        GameManager.instance.CreateTear(GameManager.OccupiedGround[i].PlayerOnGround.transform.position);
                    Destroy(GameManager.OccupiedGround[i].PlayerOnGround);
                    continue;
                }
                if (GameManager.OccupiedGround[i].InMug == false)
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
                if (GameManager.OccupiedGround[i].InMug == true)
                {
                    GStage.InMug = false;
                }
            }
            oGround.Add(GStage);
        }
        GameManager.OccupiedGround = oGround;
        int counter = 0;
        //若都被晕住则开始新回合
        bool teamHaveMove = false;
        GameManager.MudSetted = true;
        while (!teamHaveMove)
        {
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                string team = "Team" + (PlayerController.MovingTeam + 1).ToString();
                if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                {
                    teamHaveMove = true;
                    break;
                }
            }
            if (!teamHaveMove)
                PlayerController.MovingTeam = (PlayerController.MovingTeam + 1) % GameManager.TeamCount;
            counter++;
            if (counter >= 2 * GameManager.TeamCount)
            {
                Debug.Log("Died1,2" + PlayerController.DiedSoldiersTeam1 + PlayerController.DIedSoldiersTeam2);
                Debug.Log("faint,MovedDied" + PlayerController.FaintCount + PlayerController.MovedDead);
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                    Debug.Log("position,moved" + BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.transform.position + GameManager.OccupiedGround[i].Moved);
                Debug.Log("ProbleBug");
                if (counter >= 10)
                    break;
                GameManager.MudSetted = false;
                PlayerController.SmallTurn = 0;
                PlayerController.MovedDead = 0;
                oGround = new List<GameManager.GroundStage>();
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                {
                    GameManager.GroundStage GStage =GameManager.OccupiedGround[i];
                    GStage.Moved = false;
                    oGround.Add(GStage);
                }
                GameManager.Turn++;
                GameManager.OccupiedGround = oGround;
                break;
            }
        }

    }
}
