using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GuideActOrder : MonoBehaviour
{
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        Root.instance.LimitClickException = null;
        Root.instance.UseLimitClick = true;
        Root.instance.SkipPlot.gameObject.SetActive(false);
    }
    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(GameManager.Guide==1)
        {
            Guide1();
        }
        if(GameManager.Guide==2)
            Guide2();
        if(GameManager.Guide==3)
            Guide3();
    }
    void Guide3()
    {
        int Step = Root.instance.flowchart.GetIntegerVariable("GuideStep");
        if(Step==1)
        {
            Root.instance.LimitClickException = null;
            Root.instance.UseLimitClick = true;
        }
        if(Step==2)
        {
            Root.instance.UseLimitClick = false;
        }
        if(Step==3)
        {
            Root.instance.UseLimitClick = false;
            if (GameManager.PlayerOnEdit!=null
            &&GameManager.RealPlayerTeam.Contains(GameManager.PlayerOnEdit.tag)
            && Vector3.Distance(GameManager.instance.ArtifactGround.transform.position, GameManager.PlayerOnEdit.transform.position) < 1.5 * BoardManager.distance)
            {
                Root.instance.flowchart.SetBooleanVariable("FinnishCommand", true);
            }
        }
        if (Step == 4)
        {
            foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
            {
                if (t.name == "Grounds")
                    continue;
                Vector3 artPosition = GameManager.instance.ArtifactGround.transform.position;
                if (Vector3.Angle(GameManager.PlayerOnEdit.transform.position - artPosition, artPosition - t.position) < 1)
                {
                    t.GetComponent<SpriteRenderer>().color = new Color(20, 0, 0, 0.2f);
                }
            }
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -4);
            Root.instance.LimitClickException = GameManager.instance.ArtifactGround;
            Root.instance.LimitClickFinished = delegate ()
            {
                ProtocolBytes prot = new ProtocolBytes();
                prot.AddString("AddScore");
                prot.AddInt(50);
                NetMgr.srvConn.Send(prot);
                Root.instance.UseLimitClick = false;
                Root.instance.OncePlotOpen = true;
                SceneManager.LoadScene("MainPage");
            };
            Root.instance.UseLimitClick = true;
        }
    }
    void Guide2()
    {
        int Step = Root.instance.flowchart.GetIntegerVariable("GuideStep");
        if(Step==1)
        {
            Root.instance.LimitClickException = null;
            Root.instance.UseLimitClick = true;
        }
        else
        {
            Root.instance.UseLimitClick = false;
        }
    }
    void Guide1()
    {
        int Step = Root.instance.flowchart.GetIntegerVariable("GuideStep");
        Color highLightColor = new Color(0, 20, 0, 0.8f);
        //Debug.Log(Step);
        if(Step==1)
        {
            Root.instance.LimitClickException = null;
            Root.instance.UseLimitClick = true;
        }
        if(Step==2)
        {
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -2);
            BoardManager.Grounds[5][3].GetComponent<SpriteRenderer>().color = highLightColor;
            Root.instance.LimitClickException = BoardManager.Grounds[5][3];
            Root.instance.LimitClickFinished = delegate () { BoardManager.Grounds[5][3].GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);};
            Root.instance.UseLimitClick = true;
        }
        if(Step==3)
        {
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -3);
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (Vector3.Distance(BoardManager.Grounds[5][3].transform.position, GameManager.OccupiedGround[i].PlayerOnGround.transform.position) < 0.1f)
                {
                    Root.instance.LimitClickException = GameManager.OccupiedGround[i].PlayerOnGround;
                    break;
                }
            }
            Root.instance.LimitClickFinished = delegate () { };
            Root.instance.UseLimitClick = true;
        }
        if(Step==4)
        {
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -4);
            GameObject skip= GameObject.Find("Skip");
            Root.instance.LimitClickException = skip;
            skip.GetComponent<Image>().color = highLightColor;
            Root.instance.LimitClickFinished = delegate ()
            {
                skip.GetComponent<Image>().color = new Color(255, 255, 255);
            };
            Root.instance.UseLimitClick = true;
        }
        if(Step==5)
        {
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -5);
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (Vector3.Distance(BoardManager.Grounds[4][4].transform.position, GameManager.OccupiedGround[i].PlayerOnGround.transform.position) < 0.1f)
                {
                    Root.instance.LimitClickException = GameManager.OccupiedGround[i].PlayerOnGround;
                    break;
                }
            }
            foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
            {
                if (t.name == "Grounds")
                    continue;
                if (Vector3.Distance(t.position, BoardManager.Grounds[4][4].transform.position) < 0.1f)
                    continue;
                if (Vector3.Distance(t.position, BoardManager.Grounds[5][3].transform.position) < 0.5 * BoardManager.distance)
                    continue;
                if (Vector3.Distance(t.position, BoardManager.Grounds[5][3].transform.position) < 1.5 * BoardManager.distance)
                    t.gameObject.GetComponent<SpriteRenderer>().color = highLightColor;
            }
            Root.instance.LimitClickFinished = delegate ()
            {
                foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
                {
                    if (t.name == "Grounds")
                        continue;
                    if(Vector3.Distance(t.position,BoardManager.Grounds[4][4].transform.position)<0.1f)
                        continue;
                    if (Vector3.Distance(t.position, BoardManager.Grounds[5][3].transform.position) < 0.5 * BoardManager.distance)
                        continue;
                    if (Vector3.Distance(t.position, BoardManager.Grounds[5][3].transform.position) < 1.5 * BoardManager.distance)
                        t.gameObject.GetComponent<SpriteRenderer>().color = new Color(255,255,255);
                }
            };
            Root.instance.UseLimitClick = true;
        }
        if(Step==9)
        {
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -9);
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (Vector3.Distance(BoardManager.Grounds[5][3].transform.position, GameManager.OccupiedGround[i].PlayerOnGround.transform.position) < 0.1f)
                {
                    Root.instance.LimitClickException = GameManager.OccupiedGround[i].PlayerOnGround;
                    break;
                }
            }
            Root.instance.LimitClickFinished = delegate () { };
            Root.instance.UseLimitClick = true;
        }
        if(Step==10)
        {
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -10);
            Root.instance.LimitClickException = BoardManager.Grounds[4][3];
            BoardManager.Grounds[4][3].GetComponent<SpriteRenderer>().color = highLightColor;
            Root.instance.LimitClickFinished = delegate () { 
                BoardManager.Grounds[4][3].GetComponent<SpriteRenderer>().color = new Color(255,255,255);
            };
            Root.instance.UseLimitClick = true;
        }
        if(Step==11)
        {
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -11);
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (Vector3.Distance(BoardManager.Grounds[5][4].transform.position, GameManager.OccupiedGround[i].PlayerOnGround.transform.position) < 0.1f)
                {
                    Root.instance.LimitClickException = GameManager.OccupiedGround[i].PlayerOnGround;
                    break;
                }
            }
            foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
            {
                if (t.name == "Grounds")
                    continue;
                if (Vector3.Distance(t.position, BoardManager.Grounds[5][4].transform.position) < 0.1f)
                    continue;
                if (Vector3.Distance(t.position, BoardManager.Grounds[4][3].transform.position) < 1.5 * BoardManager.distance)
                    continue;
                int i1 = 4, j1 = 3, i2 = 0, j2 = 0, Range = 3;
                for (int j = 0; j < BoardManager.row; j++)
                    for (int k = 0; k < BoardManager.col; k++)
                    {
                        if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, t.position) < BoardManager.distance / 2)
                        {
                            i2 = j;
                            j2 = k;
                            break;
                        }
                    }
                if (Mathf.Abs(j2 - j1) <= Range
                    && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                    || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
                    t.gameObject.GetComponent<SpriteRenderer>().color = highLightColor;
            }
            Root.instance.LimitClickFinished = delegate ()
            {
                foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
                {
                    if (t.name == "Grounds")
                        continue;
                    if (Vector3.Distance(t.position, BoardManager.Grounds[5][4].transform.position) < 0.1f)
                        continue;
                    if (Vector3.Distance(t.position, BoardManager.Grounds[4][3].transform.position) < 1.5 * BoardManager.distance)
                        continue;
                    int i1 = 4, j1 = 3, i2 = 0, j2 = 0, Range = 3;
                    for (int j = 0; j < BoardManager.row; j++)
                        for (int k = 0; k < BoardManager.col; k++)
                        {
                            if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, t.position) < BoardManager.distance / 2)
                            {
                                i2 = j;
                                j2 = k;
                                break;
                            }
                        }
                    if (Mathf.Abs(j2 - j1) <= Range
                        && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                        || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
                        t.gameObject.GetComponent<SpriteRenderer>().color = new Color(255,255,255);
                }
            };
            Root.instance.UseLimitClick = true;
        }
        if(Step==15)
        {
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -15);
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (Vector3.Distance(BoardManager.Grounds[4][3].transform.position, GameManager.OccupiedGround[i].PlayerOnGround.transform.position) < 0.1f)
                {
                    Root.instance.LimitClickException = GameManager.OccupiedGround[i].PlayerOnGround;
                    break;
                }
            }
            Root.instance.LimitClickFinished = delegate () { };
            Root.instance.UseLimitClick = true;
        }
        if(Step==16)
        {
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -16);
            Root.instance.LimitClickException = BoardManager.Grounds[4][2];
            BoardManager.Grounds[4][2].GetComponent<SpriteRenderer>().color = highLightColor;
            Root.instance.LimitClickFinished = delegate () { 
                BoardManager.Grounds[5][3].GetComponent<SpriteRenderer>().color = new Color(255,255,255);
            };
            Root.instance.UseLimitClick = true;
        }
        if(Step==17)
        {
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -17);
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (Vector3.Distance(BoardManager.Grounds[4][4].transform.position, GameManager.OccupiedGround[i].PlayerOnGround.transform.position) < 0.1f)
                {
                    Root.instance.LimitClickException = GameManager.OccupiedGround[i].PlayerOnGround;
                    break;
                }
            }
            foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
            {
                if (t.name == "Grounds")
                    continue;
                if (Vector3.Distance(t.position, BoardManager.Grounds[4][4].transform.position) < 0.1f)
                    continue;
                if (Vector3.Distance(t.position, BoardManager.Grounds[4][2].transform.position) < 0.5 * BoardManager.distance)
                    continue;
                List<GameObject> Surround = new List<GameObject>();
                Vector3 CenterPosition = BoardManager.Grounds[4][2].transform.position;
                //是否在直线上
                foreach (Transform t2 in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
                {
                    if (t2.name == "Grounds")
                        continue;
                    if (Vector3.Distance(CenterPosition, t2.position) < BoardManager.distance / 2 + BoardManager.distance)
                    {
                        if (Vector3.Distance(CenterPosition, t2.position) < BoardManager.distance / 2)
                            continue;
                        if (t2.tag == "Weapon")
                            continue;
                        Surround.Add(t2.gameObject);
                    }
                }
                bool inLine = false;
                foreach (GameObject g in Surround)
                {
                    if (Vector3.Angle(CenterPosition - t.position, CenterPosition - g.transform.position) < 1)
                    {
                        inLine = true;
                    }
                }
                if(!inLine)
                    continue;
                int i1 = 4, j1 = 2, i2 = 0, j2 = 0, Range = 3;
                for (int j = 0; j < BoardManager.row; j++)
                    for (int k = 0; k < BoardManager.col; k++)
                    {
                        if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, t.position) < BoardManager.distance / 2)
                        {
                            i2 = j;
                            j2 = k;
                            break;
                        }
                    }
                if (Mathf.Abs(j2 - j1) <= Range
                    && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                    || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
                    t.gameObject.GetComponent<SpriteRenderer>().color = highLightColor;
            }
            Root.instance.LimitClickFinished = delegate ()
            {
                foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
                {
                    if (t.name == "Grounds")
                        continue;
                    if (Vector3.Distance(t.position, BoardManager.Grounds[4][4].transform.position) < 0.1f)
                        continue;
                    if (Vector3.Distance(t.position, BoardManager.Grounds[4][2].transform.position) < 0.5 * BoardManager.distance)
                        continue;
                    List<GameObject> Surround = new List<GameObject>();
                    Vector3 CenterPosition = BoardManager.Grounds[4][2].transform.position;
                    //是否在直线上
                    foreach (Transform t2 in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
                    {
                        if (t2.name == "Grounds")
                            continue;
                        if (Vector3.Distance(CenterPosition, t2.position) < BoardManager.distance / 2 + BoardManager.distance)
                        {
                            if (Vector3.Distance(CenterPosition, t2.position) < BoardManager.distance / 2)
                                continue;
                            if (t2.tag == "Weapon")
                                continue;
                            Surround.Add(t2.gameObject);
                        }
                    }
                    bool inLine = false;
                    foreach (GameObject g in Surround)
                    {
                        if (Vector3.Angle(CenterPosition - t.position, CenterPosition - g.transform.position) < 1)
                        {
                            inLine = true;
                        }
                    }
                    if (!inLine)
                        continue;
                    int i1 = 4, j1 = 2, i2 = 0, j2 = 0, Range = 3;
                    for (int j = 0; j < BoardManager.row; j++)
                        for (int k = 0; k < BoardManager.col; k++)
                        {
                            if (BoardManager.Grounds[j][k] != null && Vector3.Distance(BoardManager.Grounds[j][k].transform.position, t.position) < BoardManager.distance / 2)
                            {
                                i2 = j;
                                j2 = k;
                                break;
                            }
                        }
                    if (Mathf.Abs(j2 - j1) <= Range
                        && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                        || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
                        t.gameObject.GetComponent<SpriteRenderer>().color = new Color(255,255,255);
                }
            };
            Root.instance.UseLimitClick = true;
        }
        if (Step == 18)
        {
            ProtocolBytes prot = new ProtocolBytes();
            prot.AddString("AddScore");
            prot.AddInt(50);
            NetMgr.srvConn.Send(prot);
            Root.instance.UseLimitClick = false;
            SceneManager.LoadScene("MainPage");
        }
    }
    IEnumerator ClickOnUI(Button button,UnityEngine.Events.UnityAction FinishedAction)
    {
        while (true)
        {
            Debug.Log("Looping");
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("mouse,screen,world");
                Debug.Log(Input.mousePosition+" "+Camera.main.WorldToScreenPoint(Input.mousePosition)+" "+ Camera.main.ScreenToWorldPoint(Input.mousePosition));
                Debug.Log("this");
                Debug.Log(button.transform.position+" "+Camera.main.WorldToScreenPoint(button.transform.position)+" "+ Camera.main.ScreenToWorldPoint(button.transform.position));
                if (Vector3.Distance(Camera.main.WorldToScreenPoint(Input.mousePosition), (button.transform.position))
                < BoardManager.distance / 2)
                {
                    Debug.Log("in");
                    gameObject.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
                    Root.instance.flowchart.SetBooleanVariable("FinnshCommand", true);
                    FinishedAction();
                    break;
                }
                else
                {
                    Root.instance.flowchart.SetBooleanVariable("RepeatCommand", true);
                }
            }
            yield return 0;
        }
    }
}
