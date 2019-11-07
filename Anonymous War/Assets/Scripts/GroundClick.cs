using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundClick : MonoBehaviour//附着在每个地块上，用于初始化棋子位置与棋子移动（在已知可到达地块后点击
//对应地块确认移动及详细状态变化

{
    //几种玩家状态，临时替代一种game object多状态
    public Sprite LongSoldier;
    public Sprite ShortSoldier;
    public Sprite DragSoldier;
    int BloodCount=33;
    public static int TeamCounter = 0;//用于队伍轮转
    // Start is called before the first frame update
    void Start()
    {
        BloodCount = 33;
    }

    // Update is called once per frame
    void Update()
    {
        //空降玩家
        //if(GameManager.Stage==0)
        //PlacePlayer();
        //按所在地块移动


    }
    /// <summary>
    /// OnMouseDown is called when the user has pressed the mouse button while
    /// over the GUIElement or Collider.
    /// </summary>
    void OnMouseDown()
    {
        if(GameManager.instance.SmoothMoveOnWay)
            return;
        if(!Root.instance.MouseClickLimit(gameObject,Root.instance.LimitClickException,ref Root.instance.UseLimitClick,Root.instance.LimitClickFinished))
            return;
        if (GameManager.Stage == 0)
        {
            if (this.tag == "Occupied" || this.tag == "Untagged")
                return;
            if (GameManager.RealPlayerTeam.Contains("Team" + (TeamCounter + 1).ToString()))
            {
                PlaceSinglePlayer();
                if ((!GameManager.UseAI) && GameManager.RealPlayerTeam.Count < GameManager.TeamCount)
                {
                    ProtocolBytes protocol = new ProtocolBytes();
                    protocol.AddString("UpdateLand");
                    protocol.AddFloat(this.transform.position.x);
                    protocol.AddFloat(this.transform.position.y);
                    protocol.AddFloat(this.transform.position.z);
                    NetMgr.srvConn.Send(protocol);
                }
            }
        }
        //按所在地块移动
        if (GameManager.Stage == 1 && GameManager.PlayerOnEdit != null)
        {
            if (GameManager.RealPlayerTeam.Contains("Team" + (GameManager.instance.MovingTeam + 1).ToString()))
            {
                bool find = false;
                for (int i = 0; i < PlayerController.AimRangeList.Count; i++)
                {
                    if (PlayerController.AimRangeList[i].Aim == gameObject)
                    {
                        find = true;
                        break;
                    }
                }
                if (!find)
                    return;
                if ((!GameManager.UseAI) && (GameManager.RealPlayerTeam.Count < GameManager.TeamCount))
                {
                    //Update Move协议，包含移动者位置与待移动地块位置
                    ProtocolBytes protocol = new ProtocolBytes();
                    protocol.AddString("UpdateMove");
                    protocol.AddFloat(GameManager.PlayerOnEdit.transform.position.x);
                    protocol.AddFloat(GameManager.PlayerOnEdit.transform.position.y);
                    protocol.AddFloat(GameManager.PlayerOnEdit.transform.position.z);
                    protocol.AddFloat(this.transform.position.x);
                    protocol.AddFloat(this.transform.position.y);
                    protocol.AddFloat(this.transform.position.z);
                    NetMgr.srvConn.Send(protocol);
                }
                PlayerMove();
            }

        }
    }
    public void PlaceSinglePlayer()//空降单个玩家
    {
        if(GameManager.PlayerOnEdit==null)
            return;
        GameManager.instance.SmoothMoveOnWay = true;
        GameObject newPlayer = GameManager.PlayerOnEdit;
        Vector3 playeroffset = new Vector3(0, 0, -0.1f);
        Vector3 offset = new Vector3(6, -12f, -2f);
        GameManager.GroundStage GStage = new GameManager.GroundStage();
        for (int i = 0; i < GameManager.OccupiedGround.Count;i++)
        {
            if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
            {
                GStage = GameManager.OccupiedGround[i];
                GameManager.OccupiedGround.RemoveAt(i);
                break;
            }
        }
        StartCoroutine(GameManager.instance.smoothMove(newPlayer, transform.position + playeroffset, 100, delegate ()
        {
            switch (this.gameObject.tag)
            {
                case "Long":
                    foreach (Transform t in newPlayer.GetComponentsInChildren<Transform>())
                        if (t.tag == "PlayerSprite")
                        {
                            Debug.Log("inChangeToLong");
                            t.gameObject.GetComponent<SpriteRenderer>().sprite = LongSoldier;
                        }
                    break;
                case "Drag":
                    foreach (Transform t in newPlayer.GetComponentsInChildren<Transform>())
                        if (t.tag == "PlayerSprite")
                            t.gameObject.GetComponent<SpriteRenderer>().sprite = DragSoldier;
                    break;
            }
            this.gameObject.tag = "Occupied";
            foreach (Transform t in GetComponentsInChildren<Transform>())
                if (t.tag == "Weapon")
                    t.gameObject.SetActive(false);
            GameManager.instance.SmallTurn++;
            TeamCounter = (TeamCounter + 1) % GameManager.TeamCount;
            //For Guide
            if (GameManager.Guide == 1 && GameManager.instance.SmallTurn == 1)
            {
                Root.instance.flowchart.SendFungusMessage("Guide1Start");
            }
            if (((GameManager.Guide != 1 && GameManager.instance.SmallTurn >= 3 * GameManager.TeamCount)
            || (GameManager.Guide == 1 && GameManager.instance.SmallTurn >= GameManager.TeamCount))
            && GameManager.Stage == 0)
            {
                GameManager.PlayerOnEdit = null;
                GameManager.instance.SmallTurn = 0;
                GameManager.Stage = 1;
                if (!GameManager.IsTraining)
                {
                    GameManager.instance.Timer.gameObject.SetActive(true);
                    GameManager.instance.timer = StartCoroutine(GameManager.instance.HandleTimer());
                }
                Color color = new Color(255, 255, 0, 0.2f);
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                {
                    string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
                    if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                    {
                        BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = color;
                    }
                }
            }
            GameManager.instance.SmoothMoveOnWay = false;
        }));

        //只能降空地
        //newPlayer = Instantiate(EmptySoldier, this.transform.position, Quaternion.identity,GameObject.Find("Players").transform);
        //一队一个轮流
        switch (TeamCounter)
        {
            case 0: newPlayer.tag = "Team1"; break;
            case 1: newPlayer.tag = "Team2"; break;
            case 2: newPlayer.tag = "Team3"; break;
            case 3: newPlayer.tag = "Team4"; break;
        }
        Destroy(newPlayer.GetComponent<RealPlayer>());
        if (GameManager.RealPlayerTeam.Contains(newPlayer.tag))
        {
            newPlayer.AddComponent<RealPlayer>();
        }
        else if(GameManager.Guide>0)
        {
            newPlayer.AddComponent<Trainer>();
        }
        else if (GameManager.UseAI)
        {
            newPlayer.AddComponent<AI>();
        }
        else
        {
            newPlayer.AddComponent<RemoteEnemy>();
        }
        //foreach (Transform t in blood.GetComponentsInChildren<Transform>())
        {
            // if (t.name == "blood")
            //{ blood = t.gameObject; break; }
        }
        //储存玩家状态
        
            GStage.PlayerBlood.GetComponentInChildren<Text>().text = BloodCount.ToString();
        for (int i = 0; i < BoardManager.row; i++)
            for (int j = 0; j < BoardManager.col; j++)
                if (BoardManager.Grounds[i][j] != null && Vector3.Distance(BoardManager.Grounds[i][j].transform.position, this.transform.position) < BoardManager.distance / 2)
                {
                    GStage.i = i;
                    GStage.j = j;
                }
        GStage.PlayerOnGround = newPlayer;
        GStage.PlayerWeapon = this.tag;
        GStage.OrigColor = BoardManager.Grounds[GStage.i][GStage.j].GetComponent<SpriteRenderer>().color;
        GameManager.OccupiedGround.Add(GStage);
        
    }
    public void PlayerMove()//玩家移动
    //棋子移动，若该地块位于已检测到的移动范围内，则移动，参数为待移动棋子
    {
        GameManager.instance.SmoothMoveOnWay = true;
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
            if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
            {
                BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.OccupiedGround[i].OrigColor;
            }
        }
        GameManager.GroundStage GStage = new GameManager.GroundStage();
        int bloodNum = 0;
        //读取并修改玩家状态
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
            {
                GStage = GameManager.OccupiedGround[i];
                BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].tag = "Untagged";
                bloodNum = int.Parse(GStage.PlayerBlood.GetComponentInChildren<Text>().text);
                GameManager.OccupiedGround.RemoveAt(i);
                break;
            }
        }
        string tag = GameManager.PlayerOnEdit.tag;
        Vector3 playeroffset = new Vector3(0, 0, -0.1f);
        //player.transform.position = transform.position;
        //匀速移动
        StartCoroutine(GameManager.instance.smoothMove(GameManager.PlayerOnEdit, this.transform.position + playeroffset, 30, delegate ()
        {
            switch (this.tag)
            {
                case "Long":
                    foreach (Transform t in GameManager.PlayerOnEdit.GetComponentsInChildren<Transform>())
                        if (t.tag == "PlayerSprite")
                            t.gameObject.GetComponent<SpriteRenderer>().sprite = LongSoldier;
                    break;
                case "Short":
                    foreach (Transform t in GameManager.PlayerOnEdit.GetComponentsInChildren<Transform>())
                        if (t.tag == "PlayerSprite")
                            t.gameObject.GetComponent<SpriteRenderer>().sprite = ShortSoldier;
                    break;
                case "Drag":
                    foreach (Transform t in GameManager.PlayerOnEdit.GetComponentsInChildren<Transform>())
                        if (t.tag == "PlayerSprite")
                            t.gameObject.GetComponent<SpriteRenderer>().sprite = DragSoldier;
                    break;
            }
            this.tag = "Occupied";
            foreach (Transform t in GetComponentsInChildren<Transform>())
                if (t.tag == "Weapon")
                    t.gameObject.SetActive(false);
            GameManager.Stage = 2;
            GameManager.instance.SmoothMoveOnWay = false;
        }));
        //player.transform.position = Vector3.Lerp(player.transform.position, this.transform.position, 0.2f);
        //切换武器状态

        if (tag == "Team2")
            GameManager.PlayerOnEdit.GetComponentInChildren<SpriteRenderer>().color = new Color(0, 8, 8);
        //更换并储存状态
        GameManager.PlayerOnEdit.tag = tag;
        //StartCoroutine(SmoothMove(blood, transform.position + offset));
        //blood.transform.position = transform.position + offset;
        //blood.transform.position = this.transform.position + offset;
        for (int i = 0; i < BoardManager.row; i++)
            for (int j = 0; j < BoardManager.col; j++)
                if (BoardManager.Grounds[i][j] != null && Vector3.Distance(BoardManager.Grounds[i][j].transform.position, this.transform.position) < BoardManager.distance / 2)
                {
                    GStage.i = i;
                    GStage.j = j;
                }
        GStage.PlayerOnGround = GameManager.PlayerOnEdit;
        foreach (Transform t in GameManager.PlayerOnEdit.GetComponentsInChildren<Transform>())
            if (t.tag == "Blood")
                GStage.PlayerBlood = t.gameObject;
        GStage.PlayerBlood.GetComponentInChildren<Text>().text = bloodNum.ToString();
        GStage.Faint = false;
        if (this.tag != "Untagged")
            GStage.PlayerWeapon = this.tag;
        GStage.Moved = true;
        GameManager.OccupiedGround.Add(GStage);

        //Same as PlayerController.ClearHighlight
        foreach (PlayerController.AimNode line in PlayerController.AimRangeList)
        {
            if (line.Aim == null)
            {
                Debug.Log("AimRangeList:Aim is null");
                continue;
            }
            if (line.Aim.tag == "Monster")
            {
                GameManager.instance.ArtifactGround.GetComponent<SpriteRenderer>().color = line.color;
            }
            if (line.Aim == line.JudgeHelper)
                line.Aim.GetComponent<SpriteRenderer>().color = line.color;
            for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
            {
                if (GameManager.OccupiedGround[j].PlayerOnGround == line.Aim)
                {
                    BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j].GetComponent<SpriteRenderer>().color = line.color;
                    break;
                }
            }
        }
        PlayerController.AimRangeList = new List<PlayerController.AimNode>();
    }
    
}
