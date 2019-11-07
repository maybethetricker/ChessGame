using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AngerCrystal : MotionArtifact
{
    public override void OnArtCreate()
    {
        SetMug(1);

    }
    public override void ArtOnHit()
    {
        GameObject Blood;
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (Vector3.Angle(GameManager.PlayerOnEdit.transform.position - artPosition, artPosition - BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position) < 1)
            {
                Blood = GameManager.OccupiedGround[i].PlayerBlood;
                ArtifactController.instance.ClearHighlight();
                //change:use AimBlood instead of Blood
                //攻击
                int bloodamount = int.Parse(Blood.GetComponent<Text>().text);
                bloodamount -= 5;
                Blood.GetComponent<Text>().text = bloodamount.ToString();
                GameManager.instance.startCoroutine(OnHitAction(artPosition, GameManager.OccupiedGround[i].PlayerOnGround));
                if (bloodamount <= 0)
                {
                    //被攻击者死亡，与之上相似
                    if (GameManager.OccupiedGround[i].Moved)
                        PlayerController.MovedDead++;
                    GameManager.OccupiedGround[i].PlayerBlood.SetActive(false);
                    GameManager.instance.DeleteDiedObject(GameManager.OccupiedGround[i].PlayerBlood);
                    GameObject diedObject=GameManager.OccupiedGround[i].PlayerOnGround;
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    //BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.OccupiedGround[i].OrigColor;
                    GameManager.OccupiedGround.RemoveAt(i);
                    if (diedObject.tag == "Team1")
                        GameManager.instance.TeamDiedSoldiers[0]++;
                    if (diedObject.tag == "Team2")
                        GameManager.instance.TeamDiedSoldiers[1]++;
                    GameManager.instance.DeleteDiedObject(diedObject);
                }   
            }  
        }
        ArtifactController.instance.ClearHighlight();
        ArtifactController.instance.ChangeTurn();
        GameManager.instance.EnemyChecked = false;
    }
    public override void ArtPower()
    {
        SetMug((GameManager.instance.Turn) / 2);
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
        if (GameManager.instance.Turn % 2 == 0)
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
                        if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, GameManager.instance.ArtifactGround.transform.position) < BoardManager.distance / 2)
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
            if (t.name == "Grounds")
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
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, GameManager.instance.ArtifactGround.transform.position) < BoardManager.distance / 2)
                    {
                        i2 = j;
                        j2 = k;
                    }
                }
            if (Mathf.Abs(j2 - j1) <= Range
                && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
            {
                color = new Color(0, 10, 0); ;
                color.a = 0.2f;
                if (t == GameManager.instance.ArtifactGround.transform)
                    continue;
                if (t.gameObject.GetComponent<SpriteRenderer>().color == color)
                    continue;
                if (t.parent == GameManager.instance.ArtifactGround.transform)
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
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, GameManager.instance.ArtifactGround.transform.position) < BoardManager.distance / 2)
                    {
                        i2 = j;
                        j2 = k;
                    }
                }
            if (Mathf.Abs(j2 - j1) <= Range
                && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
            {
                if (BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j] == GameManager.instance.ArtifactGround)
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
                    GameObject diedObject = GameManager.OccupiedGround[i].PlayerOnGround;
                    //Destroy(GameManager.OccupiedGround[i].PlayerBlood);
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    //GameManager.OccupiedGround.RemoveAt(i);
                    if (diedObject.tag == "Team1")
                        GameManager.instance.TeamDiedSoldiers[0]++;
                    if (diedObject.tag == "Team2")
                        GameManager.instance.TeamDiedSoldiers[1]++;
                    diedObject.SetActive(false);
                    //Destroy(GameManager.OccupiedGround[i].PlayerOnGround);
                    Debug.Log("PoisonDiedDeleted");
                    GameManager.instance.DeleteDiedObject(diedObject);
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
            if (counter >= 2 * GameManager.TeamCount)
            {
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
                GameManager.instance.SmallTurn = 0;
                PlayerController.MovedDead = 0;
                oGround = new List<GameManager.GroundStage>();
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                {
                    GameManager.GroundStage GStage = GameManager.OccupiedGround[i];
                    GStage.Moved = false;
                    oGround.Add(GStage);
                }
                GameManager.instance.Turn++;
                GameManager.OccupiedGround = oGround;
                break;
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
    }


}
