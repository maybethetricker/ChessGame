using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundClick : MonoBehaviour//附着在每个地块上，用于初始化棋子位置与棋子移动（在已知可到达地块后点击
//对应地块确认移动及详细状态变化

{
    //几种玩家状态，临时替代一种game object多状态
    public GameObject LongSoldier;
    public GameObject ShortSoldier;
    public GameObject DragSoldier;
    public GameObject TearSoldier;
    public GameObject EmptySoldier;
    public GameObject Blood;//血条prefab
    GameObject blood;
    public static int SoldierCount=0;
    
    int BloodCount;
    public static int TeamCounter=0;//用于队伍轮转
    // Start is called before the first frame update
    void Start()
    {
        BloodCount = 21;
    }

    // Update is called once per frame
    void Update()
    {
        //空降玩家
        if(GameManager.Stage==0)
            PlacePlayer();
        //按所在地块移动
        if (GameManager.Stage == 1 && GameManager.PlayerOnEdit != null)
        {
            if (Input.GetMouseButtonDown(0)&&PlayerController.MovingTeam==0)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0;
                if (Mathf.Abs(Vector3.Distance(mousePosition, this.gameObject.transform.position)) < BoardManager.distance / 2)
                    PlayerMove();
            }
        }

    }

    void PlacePlayer()
    {

        if (Input.GetMouseButtonUp(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            //相当于onmouse
            if (Mathf.Abs(Vector3.Distance(mousePosition, this.gameObject.transform.position)) < BoardManager.distance / 2 && this.gameObject.tag != "Occupied")
            {
                //不能降空地
                if (this.tag == "Untagged")
                    return;
                if(TeamCounter==0)
                    PlaceSinglePlayer();
            }

        }
        /*if(Input.touchCount>0)
        {
            Touch myTouch = Input.touches[0];
            Vector3 touchPosition = myTouch.position;
            if(myTouch.phase==TouchPhase.Began&&Mathf.Abs(Vector3.Distance(touchPosition,this.gameObject.transform.position))<0.4f&&this.gameObject.tag != "Occupied")
            {
                switch(this.gameObject.tag)
                {
                    case "Long":Instantiate(LongSoldier,this.transform.position,Quaternion.identity);break;
                    case "Short":Instantiate(ShortSoldier,this.transform.position,Quaternion.identity);break;
                    case "Drag":Instantiate(DragSoldier,this.transform.position,Quaternion.identity);break;
                }
                this.GetComponent<GameObject>().SetActive(false);
                this.gameObject.tag = "Occupied";
                SoldierCount++;
            }
        }     */
        //结束空降阶段
        if(SoldierCount>=3*GameManager.TeamCount)
            GameManager.Stage = 1;
    }
    public void PlaceSinglePlayer()//空降单个玩家
    {

        
        GameObject newPlayer = null;
        
        switch (this.gameObject.tag)
        {
            case "Long": newPlayer = Instantiate(LongSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform); break;
            case "Short": newPlayer = Instantiate(ShortSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform); break;
            case "Drag": newPlayer = Instantiate(DragSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform); break;
        }
        //只能降空地
        //newPlayer = Instantiate(EmptySoldier, this.transform.position, Quaternion.identity,GameObject.Find("Players").transform);
        //一队一个轮流
        switch (TeamCounter)
        {
            case 0: newPlayer.tag = "Team1"; newPlayer.AddComponent<RealPlayer>(); break;
            case 1: newPlayer.tag = "Team2"; newPlayer.transform.Rotate(0, 0, 180); newPlayer.AddComponent<AI>(); break;
            case 2: newPlayer.tag = "Team3"; break;
            case 3: newPlayer.tag = "Team4"; break;
        }
        //生成血条
        GameObject canvas = GameObject.Find("Canvas");
        Vector3 offset = new Vector3(0, -0.3f, 0);
        blood = Instantiate(Blood, this.transform.position + offset, Quaternion.identity, canvas.transform);
        //foreach (Transform t in blood.GetComponentsInChildren<Transform>())
        {
            // if (t.name == "blood")
            //{ blood = t.gameObject; break; }
        }
        blood.GetComponentInChildren<Text>().text = BloodCount.ToString();
        foreach (Transform t in GetComponentsInChildren<Transform>())
            if (t.tag == "Weapon")
                t.gameObject.SetActive(false);
        //储存玩家状态
        GameManager.GroundStage GStage = new GameManager.GroundStage();
        GStage.PlayerBlood = blood;
        GStage.Ground = this.gameObject;
        GStage.PlayerOnGround = newPlayer;
        GStage.PlayerWeapon = this.tag;
        GStage.Moved = false;
        GStage.InMug = false;
        GStage.Faint = false;
        GameManager.OccupiedGround.Add(GStage);
        SoldierCount++;
        this.gameObject.tag = "Occupied";
        TeamCounter = (TeamCounter + 1) % GameManager.TeamCount;

    }
    public void PlayerMove()//玩家移动
    //棋子移动，若该地块位于已检测到的移动范围内，则移动，参数为待移动棋子
    {

        if (PlayerController.CanMoveList.ContainsKey(gameObject))//检测在可移动范围内
        {
            string WeaponTag="";
            bool inMug = false;
            //读取并修改玩家状态
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    GameManager.OccupiedGround[i].Ground.tag = "Untagged";
                    blood = GameManager.OccupiedGround[i].PlayerBlood;
                    WeaponTag = GameManager.OccupiedGround[i].PlayerWeapon;
                    inMug = GameManager.OccupiedGround[i].InMug;
                    GameManager.OccupiedGround.RemoveAt(i);
                    break;
                }
            }
            string tag=GameManager.PlayerOnEdit.tag;
            Vector3 offset = new Vector3(0, -BoardManager.distance / 3, 0);
            //player.transform.position = transform.position;
            //匀速移动
            StartCoroutine(SmoothMove(GameManager.PlayerOnEdit,this.transform.position));
            //player.transform.position = Vector3.Lerp(player.transform.position, this.transform.position, 0.2f);
            //切换武器状态
            switch (this.tag)
            {
                case "Long":

                    Destroy(GameManager.PlayerOnEdit);
                    GameManager.PlayerOnEdit = Instantiate(LongSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    break;
                case "Short":

                    Destroy(GameManager.PlayerOnEdit);
                    GameManager.PlayerOnEdit = Instantiate(ShortSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    break;
                case "Drag":

                    Destroy(GameManager.PlayerOnEdit);
                    GameManager.PlayerOnEdit = Instantiate(DragSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    break;
                case "Tear":
                    Destroy(GameManager.PlayerOnEdit);
                    GameManager.PlayerOnEdit = Instantiate(TearSoldier, this.transform.position, Quaternion.identity, GameObject.Find("Players").transform);
                    break;
                default:
                    if (tag == "Team2")
                        GameManager.PlayerOnEdit.transform.Rotate(0, 0, 180);
                    break;
            }
            if (tag == "Team2")
            {
                GameManager.PlayerOnEdit.transform.Rotate(0, 0, 180);
                GameManager.PlayerOnEdit.AddComponent<AI>();
            }
            else
            {
                GameManager.PlayerOnEdit.AddComponent<RealPlayer>();
            }
            //更换并储存状态
            GameManager.PlayerOnEdit.tag = tag;
            blood.transform.position = this.transform.position + offset;
            foreach (Transform t in GetComponentsInChildren<Transform>())
                if (t.tag == "Weapon")
                    t.gameObject.SetActive(false);
            GameManager.GroundStage GStage = new GameManager.GroundStage();
            GStage.Ground = this.gameObject;
            GStage.PlayerOnGround = GameManager.PlayerOnEdit;
            GStage.PlayerBlood = blood;
            GStage.InMug = inMug;
            GStage.Faint = false;
            if(this.tag!="Untagged")
                GStage.PlayerWeapon = this.tag;
            else
                GStage.PlayerWeapon = WeaponTag;
            GStage.Moved = true;
            GameManager.OccupiedGround.Add(GStage);
            
            this.tag = "Occupied";
            GameManager.Stage = 2;
            
            foreach(KeyValuePair<GameObject,Color> key in PlayerController.CanMoveList)
                key.Key.GetComponent<SpriteRenderer>().color = key.Value;
        }
    }
    IEnumerator SmoothMove(GameObject MovingObject,Vector3 aimPosition)//匀速移动
    {
//change:as I've edited playeronedit in "PlayerController.ChangeTurn,I need one more parameters"
        while (aimPosition!=MovingObject.transform.position)
        {
            
            MovingObject.transform.position=Vector3.MoveTowards(MovingObject.transform.position,aimPosition,3*Time.deltaTime);  
            yield return 0;
            if(MovingObject==null)
                break;
        }
        //GameManager.PlayerOnEdit.transform.position = transform.position;
    }
}
