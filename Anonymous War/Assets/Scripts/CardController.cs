using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    //[SerializeField]
	private LineRenderer BodyLineRenderer;
    private GameObject head;
    RectTransform canvasRect;
	private Vector3 StartPos;
	private Vector3 EndPos;
    bool isReady;
    string currentWeaponName;
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        canvasRect = GameObject.Find("Canvas(1)").GetComponent<RectTransform>();
        if (BodyLineRenderer == null)
            BodyLineRenderer = GetComponentInChildren<LineRenderer>();
        foreach (Transform t in GetComponentInChildren<Transform>())
        {
            if (t.name == "ArrowHead")
            {
                head = t.gameObject;
            }
        }
        head.SetActive(false);
        BodyLineRenderer.gameObject.SetActive(false);
    }
    void Update()
    {
        if (isReady)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("exit");
                head.SetActive(false);
                BodyLineRenderer.gameObject.SetActive(false);
                AddWeapon();
                isReady = false;
                GameManager.instance.PointerIsready = false;
            }
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, new Vector2(Input.mousePosition.x, Input.mousePosition.y), Camera.main, out EndPos);
            head.transform.position = EndPos;
            BodyLineRenderer.SetPosition(0, StartPos);
            BodyLineRenderer.SetPosition(1, head.transform.localPosition);
            Vector3 direction = EndPos-gameObject.transform.position;
            direction.z = 0;
            float z1 = Vector3.Angle(Vector3.up, direction);
            if (EndPos.x > gameObject.transform.position.x)
            {
                z1 = -z1;
            }
            head.transform.rotation = Quaternion.Euler(0, 0, z1);
        }
    }
    void StartArrowPointer()
    {
        GameManager.instance.PointerIsready = true;
        isReady = true;
        StartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        head.transform.position = StartPos;
        BodyLineRenderer.gameObject.SetActive(true);
        head.SetActive(true);
    }
    /// <summary>
    /// OnMouseDown is called when the user has pressed the mouse button while
    /// over the GUIElement or Collider.
    /// </summary>
    void OnMouseDown()
    {
        if(GameManager.instance.SmoothMoveOnWay)
            return;
        if (GameManager.Stage <= 0 || !GameManager.RealPlayerTeam.Contains("Team" + (GameManager.instance.MovingTeam + 1).ToString()))
            return;
        if(!Root.instance.MouseClickLimit(gameObject,Root.instance.LimitClickException,ref Root.instance.UseLimitClick,Root.instance.LimitClickFinished))
            return;
        StartArrowPointer();
    }
    void AddWeapon()
    {

        for (int i = 0; i < BoardManager.row; i++)
            for (int j = 0; j < BoardManager.col; j++)
            {
                if (BoardManager.Grounds[i][j] != null && Vector3.Distance(BoardManager.Grounds[i][j].transform.position, GameManager.instance.AddWeaponAim) < BoardManager.distance * 0.45)
                {
                    if (Root.instance.UseLimitClick&&BoardManager.Grounds[i][j] != Root.instance.LimitClickException)
                    {
                        Root.instance.flowchart.SetBooleanVariable("RepeatCommand", true);
                        return;
                    }
                    else
                    {
                        Root.instance.flowchart.SetBooleanVariable("FinnishCommand", true);
                        BoardManager.Grounds[i][j].GetComponent<SpriteRenderer>().color = GameManager.instance.OrigGroundColor;
                    }
                    if (!GameManager.UseAI && GameManager.RealPlayerTeam.Count < GameManager.TeamCount)
                    {
                        ProtocolBytes protocol = new ProtocolBytes();
                        protocol.AddString("AddWeapon");
                        protocol.AddString(this.tag);
                        protocol.AddFloat(BoardManager.Grounds[i][j].transform.position.x);
                        protocol.AddFloat(BoardManager.Grounds[i][j].transform.position.y);
                        protocol.AddFloat(BoardManager.Grounds[i][j].transform.position.z);
                        NetMgr.srvConn.Send(protocol);
                    }
                    GameObject aimGround = null;
                    switch (this.tag)
                    {
                        case "Long": aimGround = BoardManager.instance.LongGround; break;
                        case "Short": aimGround = BoardManager.instance.ShortGround; break;
                        case "Drag": aimGround = BoardManager.instance.DragGround; break;
                        case "Ax": aimGround = BoardManager.instance.AxGround; break;
                        case "Shield": aimGround = BoardManager.instance.ShieldGround; break;
                    }
                    GameObject thisweapon=null;
                    foreach(Transform t in BoardManager.Grounds[i][j].GetComponentInChildren<Transform>())
                    {
                        if(t.tag=="Weapon")
                        {
                            thisweapon = t.gameObject;
                            t.gameObject.SetActive(true);
                        }
                    }
                    foreach(Transform t in aimGround.GetComponentInChildren<Transform>())
                    {
                        if(t.tag=="Weapon")
                        {
                            thisweapon.GetComponent<SpriteRenderer>().sprite = t.GetComponent<SpriteRenderer>().sprite;
                            //color?
                        }
                    }
                    BoardManager.Grounds[i][j].tag = aimGround.tag;
                    BoardManager.Grounds[i][j].GetComponent<SpriteRenderer>().color = GameManager.instance.AddWeaponOrigColor;
                    GameManager.instance.AddWeaponAim = new Vector3(0, 0, 0);
                    gameObject.SetActive(false);
                }
            }
    }

}
