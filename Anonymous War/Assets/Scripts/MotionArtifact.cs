using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionArtifact
{
    public Vector3 artPosition;
    Dictionary<GameObject, Color> groundRange = new Dictionary<GameObject, Color>();
    public virtual void OnArtCreate()
    {

    }

    public virtual void ArtPower()
    {

    }

    public virtual void ArtOnHit()
    {
        
    }

    //same as playercontroller.onhitation
    public IEnumerator OnHitAction(Vector3 attackerposotion,GameObject aim)
    {
        float singleFlameMovement=0.1f;
        if(attackerposotion.x>aim.transform.position.x)
            singleFlameMovement = -0.1f;
        Vector3 nowPosition = aim.transform.position;
        Vector3 OrigPosition = nowPosition;
        for (int i = 0; i < 10;i++)
        {
            if(aim==null)
                break;
            nowPosition.x += singleFlameMovement;
            aim.transform.position = nowPosition;
            yield return 0;
        }
        for (int i = 0; i < 10;i++)
        {
            if(aim==null)
                break;
            nowPosition.y -= singleFlameMovement;
            aim.transform.position = nowPosition;
            yield return 0;
        }
        if(aim!=null)
            aim.transform.position = OrigPosition;
    }

    public List<GameObject> FindAimsSector(Vector3 Center, Vector3 Sectorcenter, int Range, int Breadth)
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
                g.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
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
                g.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
                break;
            }
        }
        Debug.Log(Vector3.Angle(Center - border1, Center - border2));
        groundRange = new Dictionary<GameObject, Color>();
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Angle(Center - border1, Center - t.position) + Vector3.Angle(Center - border2, Center - t.position) > (Breadth) * 60 - 1
                    && Vector3.Angle(Center - border1, Center - t.position) + Vector3.Angle(Center - border2, Center - t.position) < (Breadth) * 60 + 1
                    && Vector3.Dot(Center - border1, Center - t.position) > -0.01
                    && Vector3.Dot(Center - border2, Center - t.position) > -0.01
                    && Vector3.Distance(Center, t.position) < 2.1f * BoardManager.distance)
            {
                groundRange.Add(t.gameObject, t.gameObject.GetComponent<SpriteRenderer>().color);
                t.gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
            }
        }
        System.Timers.Timer timer = new System.Timers.Timer(500);
        timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleTimer);
        timer.AutoReset = false;
        timer.Enabled = true;
        timer.Start();
        List<GameObject> aims = new List<GameObject>();
        foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Players")
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
                if (t.tag == "Monster")
                {
                    continue;
                }
                bool inLine = false;

                if (Vector3.Angle(Center - border1, Center - t.position) + Vector3.Angle(Center - border2, Center - t.position) > (Breadth) * 60 - 1
                    && Vector3.Angle(Center - border1, Center - t.position) + Vector3.Angle(Center - border2, Center - t.position) < (Breadth) * 60 + 1
                    && Vector3.Dot(Center - border1, Center - t.position) > -0.01
                    && Vector3.Dot(Center - border2, Center - t.position) > -0.01)
                {
                    inLine = true;
                }
                if (!inLine)
                    continue;
                else
                {
                    for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                    {
                        if (GameManager.OccupiedGround[j].PlayerOnGround == t.gameObject)
                        {
                            aims.Add(t.gameObject);
                            break;
                        }
                    }

                }

            }
        }
        return aims;
    }

    public GameObject FindMaxHate()
    {
        //int max = 0;
        GameObject MaxHatePlayer = null;
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            //if (GameManager.OccupiedGround[i].Hate > max)
            {
                MaxHatePlayer = GameManager.OccupiedGround[i].PlayerOnGround;
                //max = GameManager.OccupiedGround[i].Hate;
            }
        }
        return MaxHatePlayer;
    }

    void HandleTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
        Debug.Log("ChangeBack");
        foreach (KeyValuePair<GameObject, Color> pair in groundRange)
        {
            pair.Key.GetComponent<SpriteRenderer>().color = pair.Value;
        }

    }
}
