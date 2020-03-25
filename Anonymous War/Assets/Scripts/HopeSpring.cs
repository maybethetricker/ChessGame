using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HopeSpring : MotionArtifact
{
    int routine=0;
    public override void OnArtCreate()
    {
        ArtifactController.instance.aiAbleToUse = false;
        GameManager.instance.ArtActFinished = true;
        Color color = GameManager.instance.ArtifactAbleRangeHighlight;
        if(routine==2)
            color = GameManager.instance.HopeSpringDesperateHighlight;
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(artPosition, t.position) < BoardManager.distance * 1.5f
            && GameManager.PlayerOnEdit!=null)
            {
                Debug.Log("inChangeToWhite");
                Debug.Log(GameManager.Stage);
                t.gameObject.GetComponent<SpriteRenderer>().color = GameManager.instance.OrigGroundColor;
                if(t.tag=="Occupied")
                {
                    for (int j = 0; j < GameManager.OccupiedGround.Count;j++)
                    {
                        if(Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position,t.position)<BoardManager.distance*0.5)
                        {
                            GameManager.GroundStage groundStage = GameManager.OccupiedGround[j];
                            groundStage.OrigColor = GameManager.instance.OrigGroundColor;
                            GameManager.OccupiedGround.RemoveAt(j);
                            GameManager.OccupiedGround.Add(groundStage);
                            break;
                        }
                    }
                }
            }
        }
        artPosition = ArtifactController.instance.gameObject.transform.position;
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(artPosition, t.position) < BoardManager.distance / 2 + BoardManager.distance)
            {
                if (Vector3.Distance(artPosition, t.position) < BoardManager.distance / 2)
                    continue;
                t.gameObject.GetComponent<SpriteRenderer>().color = color;
                if(t.tag=="Occupied")
                {
                    for (int j = 0; j < GameManager.OccupiedGround.Count;j++)
                    {
                        if(Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position,t.position)<BoardManager.distance*0.5)
                        {
                            GameManager.GroundStage groundStage = GameManager.OccupiedGround[j];
                            groundStage.OrigColor = color;
                            GameManager.OccupiedGround.RemoveAt(j);
                            GameManager.OccupiedGround.Add(groundStage);
                            break;
                        }
                    }
                }
            }
        }
    }

    public override void ArtOnHit()
    {
        ArtifactController.instance.ClearHighlight();
        PlayerController.AimRangeList = new List<PlayerController.AimNode>();
        foreach(Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.gameObject.name == "Grounds")
                continue;
            if(Vector3.Distance(t.position,GameManager.PlayerOnEdit.transform.position)>1.5*BoardManager.distance)
                continue;
            if(Vector3.Distance(t.position,GameManager.PlayerOnEdit.transform.position)<0.5*BoardManager.distance)
                continue;
            if(t.tag=="Occupied")
                continue;
            PlayerController.AimNode node = new PlayerController.AimNode();
            node.Aim = t.gameObject;
            node.color = t.gameObject.GetComponent<SpriteRenderer>().color;
            t.gameObject.GetComponent<SpriteRenderer>().color = GameManager.instance.AttackAimHighlight;
            node.JudgeHelper = node.Aim;
            PlayerController.AimRangeList.Add(node);
        }
    }

    public override void ArtPower()
    {
        routine++;
        if(routine==3)
        {
            HopeOrDesperate(true);
        }
        else
        {
            HopeOrDesperate(false);
        }
        GameManager.instance.ArtActFinished = true;
    }

    void HopeOrDesperate(bool desperate)
    {
        Debug.Log("HopeOrDesperate");
        if(routine==2)
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
                t.gameObject.GetComponent<SpriteRenderer>().color = GameManager.instance.HopeSpringDesperateHighlight;
                if(t.tag=="Occupied")
                {
                    for (int j = 0; j < GameManager.OccupiedGround.Count;j++)
                    {
                        if(Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position,t.position)<BoardManager.distance*0.5)
                        {
                            GameManager.GroundStage groundStage = GameManager.OccupiedGround[j];
                            groundStage.OrigColor = GameManager.instance.HopeSpringDesperateHighlight;
                            GameManager.OccupiedGround.RemoveAt(j);
                            GameManager.OccupiedGround.Add(groundStage);
                            break;
                        }
                    }
                }
            }
        }
        int bloodChange = 0;
        if(desperate)
        {
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
                    t.gameObject.GetComponent<SpriteRenderer>().color = GameManager.instance.ArtifactAbleRangeHighlight;
                    if (t.tag == "Occupied")
                    {
                        for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                        {
                            if (Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, t.position) < BoardManager.distance * 0.5)
                            {
                                GameManager.GroundStage groundStage = GameManager.OccupiedGround[j];
                                groundStage.OrigColor = GameManager.instance.ArtifactAbleRangeHighlight;
                                GameManager.OccupiedGround.RemoveAt(j);
                                GameManager.OccupiedGround.Add(groundStage);
                                break;
                            }
                        }
                    }
                }
            }
            bloodChange = -5;
            routine = 0;
        }
        else
        {
            bloodChange = 1;
        }
        for (int i = 0; i < GameManager.OccupiedGround.Count;i++)
        {
            if(Vector3.Distance(artPosition,BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position)<BoardManager.distance*1.5f)
            {
                int bloodNum = int.Parse(GameManager.OccupiedGround[i].PlayerBlood.GetComponent<Text>().text);
                bloodNum+=bloodChange;
                if (bloodNum <= 0)
                {
                    //被攻击者死亡，与之上相似
                    if (GameManager.OccupiedGround[i].Moved)
                        PlayerController.MovedDead++;
                    GameManager.OccupiedGround[i].PlayerBlood.SetActive(false);
                    GameManager.instance.DeleteDiedObject(GameManager.OccupiedGround[i].PlayerBlood);
                    GameObject diedObject = GameManager.OccupiedGround[i].PlayerOnGround;
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                    //BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.OccupiedGround[i].OrigColor;
                    GameManager.OccupiedGround.RemoveAt(i);
                    if (diedObject.tag == "Team1")
                        GameManager.instance.TeamDiedSoldiers[0]++;
                    if (diedObject.tag == "Team2")
                        GameManager.instance.TeamDiedSoldiers[1]++;
                    diedObject.SetActive(false);
                    GameManager.instance.DeleteDiedObject(diedObject);
                    continue;
                }
                GameManager.OccupiedGround[i].PlayerBlood.GetComponent<Text>().text = bloodNum.ToString();
                GameManager.instance.startCoroutine(OnHitAction(artPosition, GameManager.OccupiedGround[i].PlayerOnGround));
            }
        }
    }
}
