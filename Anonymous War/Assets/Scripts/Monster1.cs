using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Monster1 : MonsterBase
{
    public override void OnMonsterCreate()
    {
        SetMug(1);
        
    }
    public override void MonsterAttack()
    {   
        SetMug((GameManager.Turn) / 2);
        /* 
        GameObject centerAim = FindMaxHate();
        if(centerAim!=null)
        {
            Debug.Log("MonsterHit");
            List<GameObject> aims = FindAimsSector(monsterPosition, centerAim.transform.position, 4, 1);
            //for (int i = 0; i < aims.Count;i++)
            {
                //aims[i].GetComponentInChildren<SpriteRenderer>().color = new Color(0,0,0);
            }
            MonsterHit(aims, 3, "");
        }*/
    }

    public void SetMug(int Range)
    {
        Color color;
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
            if(t.name=="Grounds")
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
                color=new Color(0, 10, 0);;
                color.a = 0.2f;
                if (t == GameManager.instance.TearGround.transform)
                    continue;
                if (t.gameObject.GetComponent<SpriteRenderer>().color == color)
                    continue;
                if (t.parent == GameManager.instance.TearGround.transform)
                    continue;
                t.gameObject.GetComponent<SpriteRenderer>().color = color;
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
                    GameManager.OccupiedGround[i].PlayerBlood.SetActive(false);
                    GameManager.instance.DeleteDiedObject(GameManager.OccupiedGround[i].PlayerBlood);
                    //Destroy(GameManager.OccupiedGround[i].PlayerBlood);
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    if (PlayerController.CanMoveList.ContainsKey(GameManager.OccupiedGround[i].PlayerOnGround))
                        PlayerController.CanMoveList.Remove(GameManager.OccupiedGround[i].PlayerOnGround);
                    if (GameManager.OccupiedGround[i].PlayerOnGround.tag == "Team1")
                        PlayerController.DiedSoldiersTeam1++;
                    if (GameManager.OccupiedGround[i].PlayerOnGround.tag == "Team2")
                        PlayerController.DIedSoldiersTeam2++;
                    if (PlayerController.DiedSoldiersTeam1 == 3 || PlayerController.DIedSoldiersTeam2 == 3)
                        GameManager.instance.CreateTear(BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position);
                    GameManager.OccupiedGround[i].PlayerOnGround.SetActive(false);
                    //Destroy(GameManager.OccupiedGround[i].PlayerOnGround);
                    GameManager.instance.DeleteDiedObject(GameManager.OccupiedGround[i].PlayerOnGround);
                    continue;
                }
                if (GameManager.OccupiedGround[i].InMug == false)
                {
                    GStage.InMug = true;
                    GStage.Moved = true;
                    GStage.Faint = true;
                    GStage.OrigColor = BoardManager.Grounds[GStage.i][GStage.j].GetComponent<SpriteRenderer>().color;
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
                {
                    GameManager.MudSetted = true;
                    break;
                }
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
        color = new Color(255, 255, 0, 0.2f);
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            string team = "Team" + (PlayerController.MovingTeam + 1).ToString();
            if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
            {
                BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = color;
            }
        }
    }

    
}
