using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuideActOrder : MonoBehaviour
{
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {

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
    }
    void Guide1()
    {
        int Step = Root.instance.flowchart.GetIntegerVariable("GuideStep");
        Color highLightColor = new Color(0, 20, 0, 0.2f);
        //Debug.Log(Step);
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
            Root.instance.LimitClickFinished = delegate () {skip.GetComponent<Image>().color = new Color(255, 255, 255); };
            Root.instance.UseLimitClick = true;
        }
        if(Step==5)
        {
            Root.instance.flowchart.SetIntegerVariable("GuideStep", -5);
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
