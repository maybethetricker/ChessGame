using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTower : MotionArtifact
{
    GameObject[][] fogGrounds;
    List<GameObject> towerArea=new List<GameObject>();
    public override void OnArtCreate()
    {
        fogGrounds = new GameObject[BoardManager.row][];
        for (int i = 0; i < BoardManager.row; i++)
        {
            fogGrounds[i] = new GameObject[BoardManager.col];
        }
        GameManager.instance.ArtActFinished = true;
        for (int i = 0; i < BoardManager.row;i++)
            for (int j = 0; j < BoardManager.col;j++)
                if(BoardManager.Grounds[i][j]!=null && BoardManager.Grounds[i][j]!=GameManager.instance.ArtifactGround)
                {
                    fogGrounds[i][j] = GameManager.instance.instantiate(GameManager.instance.fogGround, BoardManager.Grounds[i][j].transform.position+new Vector3(0,5,0), Quaternion.identity);
                }
        List<GameObject> Surround = new List<GameObject>();
        //是否在直线上
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(artPosition, t.position) < BoardManager.distance / 2 + BoardManager.distance)
            {
                if (Vector3.Distance(artPosition, t.position) < BoardManager.distance / 2)
                    continue;
                if (t.tag == "Weapon")
                    continue;
                Surround.Add(t.gameObject);
            }
        }
        int randomDir = Random.Range(0, Surround.Count);
        towerArea = FindAimsSector(artGroundPosition, Surround[randomDir].transform.position, 20, 1, "Grounds");
        for (int i = 0; i < BoardManager.row;i++)
            for (int j = 0; j < BoardManager.col;j++)
                if(fogGrounds[i][j]!=null && towerArea.Contains(BoardManager.Grounds[i][j]))
                {
                    BoardManager.Grounds[i][j].GetComponent<SpriteRenderer>().color=GameManager.instance.ArtifactAbleRangeHighlight;
                    if (BoardManager.Grounds[i][j].tag == "Occupied")
                    {
                        for (int k = 0; k < GameManager.OccupiedGround.Count; k++)
                        {
                            if (Vector3.Distance(GameManager.OccupiedGround[k].PlayerOnGround.transform.position, BoardManager.Grounds[i][j].transform.position) < BoardManager.distance * 0.5)
                            {
                                GameManager.GroundStage groundStage = GameManager.OccupiedGround[k];
                                groundStage.OrigColor = GameManager.instance.ArtifactAbleRangeHighlight;
                                GameManager.OccupiedGround.RemoveAt(k);
                                GameManager.OccupiedGround.Add(groundStage);
                                break;
                            }
                        }
                    }
                    fogGrounds[i][j].SetActive(false);
                }
        ArtPerActionPower();
    }
    public override void ArtPower()
    {
        GameManager.instance.ArtActFinished = true;
    }

    public override void ArtOnHit()
    {
        Debug.Log("LightTowerOnHit");
        for (int i = 0; i < BoardManager.row;i++)
            for (int j = 0; j < BoardManager.col;j++)
                if(fogGrounds[i][j]!=null && towerArea.Contains(BoardManager.Grounds[i][j]))
                {
                    BoardManager.Grounds[i][j].GetComponent<SpriteRenderer>().color=GameManager.instance.OrigGroundColor;
                    if (BoardManager.Grounds[i][j].tag == "Occupied")
                    {
                        for (int k = 0; k < GameManager.OccupiedGround.Count; k++)
                        {
                            if (Vector3.Distance(GameManager.OccupiedGround[k].PlayerOnGround.transform.position, BoardManager.Grounds[i][j].transform.position) < BoardManager.distance * 0.5)
                            {
                                GameManager.GroundStage groundStage = GameManager.OccupiedGround[k];
                                groundStage.OrigColor = GameManager.instance.OrigGroundColor;
                                GameManager.OccupiedGround.RemoveAt(k);
                                GameManager.OccupiedGround.Add(groundStage);
                                break;
                            }
                        }
                    }
                }
        towerArea=FindAimsSector(artGroundPosition, GameManager.PlayerOnEdit.transform.position, 20, 1, "Grounds");
        for (int i = 0; i < BoardManager.row;i++)
            for (int j = 0; j < BoardManager.col;j++)
                if(fogGrounds[i][j]!=null && towerArea.Contains(BoardManager.Grounds[i][j]))
                {
                    BoardManager.Grounds[i][j].GetComponent<SpriteRenderer>().color=GameManager.instance.ArtifactAbleRangeHighlight;
                    if (BoardManager.Grounds[i][j].tag == "Occupied")
                    {
                        for (int k = 0; k < GameManager.OccupiedGround.Count; k++)
                        {
                            if (Vector3.Distance(GameManager.OccupiedGround[k].PlayerOnGround.transform.position, BoardManager.Grounds[i][j].transform.position) < BoardManager.distance * 0.5)
                            {
                                GameManager.GroundStage groundStage = GameManager.OccupiedGround[k];
                                groundStage.OrigColor = GameManager.instance.ArtifactAbleRangeHighlight;
                                GameManager.OccupiedGround.RemoveAt(k);
                                GameManager.OccupiedGround.Add(groundStage);
                                break;
                            }
                        }
                    }
                    fogGrounds[i][j].SetActive(false);
                }
        ArtPerActionPower();
        ArtifactController.instance.ClearHighlight();
        ArtifactController.instance.ChangeTurn();
        GameManager.instance.EnemyChecked = false;
    }

    public override void ArtPerActionPower()
    {
        GameManager.instance.ArtPerActActFinished = true;
        for (int i = 0; i < BoardManager.row; i++)
            for (int j = 0; j < BoardManager.col; j++)
            {
                if (fogGrounds[i][j] != null && !towerArea.Contains(BoardManager.Grounds[i][j]))
                {
                    fogGrounds[i][j].SetActive(true);
                }
                if (fogGrounds[i][j] != null && towerArea.Contains(BoardManager.Grounds[i][j]))
                {
                    fogGrounds[i][j].SetActive(false);
                }
            }
        for (int k = 0; k < GameManager.OccupiedGround.Count; k++)
        {
            if(!GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[k].PlayerOnGround.tag))
                continue;
            int Range;
            Vector3 playerPosition = BoardManager.Grounds[GameManager.OccupiedGround[k].i][GameManager.OccupiedGround[k].j].transform.position;
            switch (GameManager.OccupiedGround[k].PlayerWeapon)
            {
                case "Long": Range = 3; break;
                case "Drag": Range = 3; break;
                default: Range = 1; break;
            }
            List<GameObject> Surround = new List<GameObject>();
            //是否在直线上
            foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
            {
                if (t.name == "Grounds")
                    continue;
                if (Vector3.Distance(playerPosition, t.position) < BoardManager.distance / 2 + BoardManager.distance)
                {
                    if (Vector3.Distance(playerPosition, t.position) < BoardManager.distance / 2)
                        continue;
                    if (t.tag == "Weapon")
                        continue;
                    Surround.Add(t.gameObject);
                }
            }
            for (int i = 0; i < BoardManager.row; i++)
                for (int j = 0; j < BoardManager.col; j++)
                    if (fogGrounds[i][j] != null)
                    {
                        int i1 = 0, j1 = 0, i2 = 0, j2 = 0;
                        for (int m = 0; m < BoardManager.row; m++)
                            for (int n = 0; n < BoardManager.col; n++)
                            {
                                if (BoardManager.Grounds[m][n] != null && Vector3.Distance(BoardManager.Grounds[m][n].transform.position, BoardManager.Grounds[i][j].transform.position) < BoardManager.distance / 2)
                                {
                                    i1 = m;
                                    j1 = n;
                                }
                                if (BoardManager.Grounds[m][n] != null && Vector3.Distance(BoardManager.Grounds[m][n].transform.position, BoardManager.Grounds[GameManager.OccupiedGround[k].i][GameManager.OccupiedGround[k].j].transform.transform.position) < BoardManager.distance / 2)
                                {
                                    i2 = m;
                                    j2 = n;
                                }
                            }
                        if (Mathf.Abs(j2 - j1) <= Range
                            && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                            || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
                        {
                            if(GameManager.OccupiedGround[k].PlayerWeapon=="Drag")
                            {
                                bool inLine = false;
                                foreach (GameObject g in Surround)
                                {
                                    if (Vector3.Angle(playerPosition - BoardManager.Grounds[i][j].transform.position, playerPosition - g.transform.position) < 1)
                                    {
                                        inLine = true;
                                    }
                                }
                                if(inLine==false)
                                    continue;
                            }
                            fogGrounds[i][j].SetActive(false);
                        }
                    }
        }
    }

    public List<GameObject> FindAimsSector(Vector3 Center, Vector3 Sectorcenter, int Range, int Breadth,string Groups)
    {
        List<GameObject> Surround = new List<GameObject>();
        //是否在直线上
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(Center, t.position) < BoardManager.distance / 2 + BoardManager.distance)
            {
                if (Vector3.Distance(Center, t.position) < BoardManager.distance / 2)
                    continue;
                if (t.tag == "Weapon")
                    continue;
                Surround.Add(t.gameObject);

            }
        }
        Vector3 border1 = new Vector3(), border2 = new Vector3();
        foreach (GameObject g in Surround)
        {
            if (Vector3.Angle(Center - Sectorcenter, Center - g.transform.position) < 59 + (Breadth / 2) * 60
            && Vector3.Angle(Center - Sectorcenter, Center - g.transform.position) > 59 + (Breadth / 2 - 1) * 60)
            {
                border1 = g.transform.position;
                break;
            }
        }
        foreach (GameObject g in Surround)
        {
            if (Vector3.Angle(Center - border1, Center - g.transform.position) < 1 + (Breadth) * 60
            && Vector3.Angle(Center - border1, Center - g.transform.position) > (Breadth) * 60 - 1
            && Vector3.Angle(Center - Sectorcenter, Center - g.transform.position) < 1 + (Breadth) * 60)
            {
                border2 = g.transform.position;
                break;
            }
        }
        List<GameObject> aims = new List<GameObject>();
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
                    if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, Center) < BoardManager.distance / 2)
                    {
                        i2 = j;
                        j2 = k;
                    }
                }
            if (Mathf.Abs(j2 - j1) <= Range
                && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
            {
                if(t.tag=="Monster" || t.tag=="Weapon")
                    continue;
                bool inLine = false;
                if (Vector3.Angle(Center - border1, Center - t.position) + Vector3.Angle(Center - border2, Center - t.position) > (Breadth) * 60 - 0.1
                    && Vector3.Angle(Center - border1, Center - t.position) + Vector3.Angle(Center - border2, Center - t.position) < (Breadth) * 60 + 0.1
                    && Vector3.Dot(Center - border1, Center - t.position) > -0.01
                    && Vector3.Dot(Center - border2, Center - t.position) > -0.01)
                {
                    inLine = true;
                }
                if (!inLine)
                    continue;
                else
                {
                    aims.Add(t.gameObject);
                }

            }
        }
        Debug.Log(aims.Count);
        return aims;
    }
}
