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
        GameManager.ArtActFinished = true;
        Color color = new Color(0, 10, 0, 0.2f);
        if(routine==2)
            color = new Color(0, 10, 0, 0.6f);
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(artPosition, t.position) < BoardManager.distance / 2 + BoardManager.distance 
            || (GameManager.PlayerOnEdit!=null &&Vector3.Distance(GameManager.PlayerOnEdit.transform.position, t.position) < BoardManager.distance / 2 + BoardManager.distance))
            {
                t.gameObject.GetComponent<SpriteRenderer>().color = new Color(255,255,255);
                if(t.tag=="Occupied")
                {
                    for (int j = 0; j < GameManager.OccupiedGround.Count;j++)
                    {
                        if(Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position,t.position)<BoardManager.distance*0.5)
                        {
                            GameManager.GroundStage groundStage = GameManager.OccupiedGround[j];
                            groundStage.OrigColor = new Color(255,255,255);
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
        Color color = new Color(0, 255, 255, 0.2f);
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
            t.gameObject.GetComponent<SpriteRenderer>().color = color;
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
        GameManager.ArtActFinished = true;
    }

    void HopeOrDesperate(bool desperate)
    {
        Debug.Log("HopeOrDesperate");
        Color willDesperateColor = new Color(0, 10, 0, 0.6f);
        Color normalColor = new Color(0, 10, 0, 0.2f);
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
                t.gameObject.GetComponent<SpriteRenderer>().color = willDesperateColor;
                if(t.tag=="Occupied")
                {
                    for (int j = 0; j < GameManager.OccupiedGround.Count;j++)
                    {
                        if(Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position,t.position)<BoardManager.distance*0.5)
                        {
                            GameManager.GroundStage groundStage = GameManager.OccupiedGround[j];
                            groundStage.OrigColor = willDesperateColor;
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
                    t.gameObject.GetComponent<SpriteRenderer>().color = normalColor;
                    if (t.tag == "Occupied")
                    {
                        for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                        {
                            if (Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, t.position) < BoardManager.distance * 0.5)
                            {
                                GameManager.GroundStage groundStage = GameManager.OccupiedGround[j];
                                groundStage.OrigColor = normalColor;
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
                GameManager.OccupiedGround[i].PlayerBlood.GetComponent<Text>().text = bloodNum.ToString();
                GameManager.instance.startCoroutine(OnHitAction(artPosition, GameManager.OccupiedGround[i].PlayerOnGround));
            }
        }
    }
}
