using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Root : MonoBehaviour
{
    public Button Quit;
    public GameObject StaticUI;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(StaticUI);
        Quit.onClick.AddListener(delegate () {Application.Quit(); });
        NetMgr.srvConn.msgDist.AddListener("StartFight", OnMatchBack);
        NetMgr.srvConn.msgDist.AddListener("UpdateMove", NetMove);
        NetMgr.srvConn.msgDist.AddListener("UpdateAttack", NetAttack);
        NetMgr.srvConn.msgDist.AddListener("SkipMove", SkipMove);
        NetMgr.srvConn.msgDist.AddListener("SkipAttack", SkipAttack);
        NetMgr.srvConn.msgDist.AddListener("UpdateLand",NetLand);
        NetMgr.srvConn.msgDist.AddListener("FindMonster", NetFindMonster);
        NetMgr.srvConn.msgDist.AddListener("CreateMonster", NetCreateMonster);
        NetMgr.srvConn.msgDist.AddListener("EndGame", EndGame);
    }

    // Update is called once per frame
    void Update()
    {
        NetMgr.Update();
    }

    void NetLand(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Vector3 GroundPosition;
        GroundPosition.x = proto.GetFloat(start, ref start);
        GroundPosition.y = proto.GetFloat(start, ref start);
        GroundPosition.z = proto.GetFloat(start, ref start);
        //需要空降到的地块
        GameObject GroundToLand=null;
        //降落到对应地块上
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if(t.name=="Grounds")
                continue;
            if (Vector3.Distance(t.position, GroundPosition) < BoardManager.distance / 2)
            {
                if (t.tag == "Weapon")
                    continue;
                GroundToLand = t.gameObject;
                break;
            }
        }
        //对接降落函数，可以不用看了
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if(t.name=="Grounds")
                continue;
            if(Vector3.Distance(GroundToLand.transform.position, t.position) < BoardManager.distance / 2)
            {
                t.gameObject.GetComponent<GroundClick>().PlaceSinglePlayer();
                break;
            }
        }
    }
    void NetMove(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Vector3 MoverPosition, GroundPosition;
        MoverPosition.x = proto.GetFloat(start, ref start);
        MoverPosition.y = proto.GetFloat(start, ref start);
        MoverPosition.z = proto.GetFloat(start, ref start);
        GroundPosition.x = proto.GetFloat(start, ref start);
        GroundPosition.y = proto.GetFloat(start, ref start);
        GroundPosition.z = proto.GetFloat(start, ref start);
        //找到待移动棋子
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (Vector3.Distance(MoverPosition, GameManager.OccupiedGround[i].PlayerOnGround.transform.position) < BoardManager.distance / 2)
            {
                GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
                break;
            }

        }
        //要移动到的地块
        GameObject GroundToMove = null;
        //移动到对应地块上
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if(t.name=="Grounds")
                continue;
            if (Vector3.Distance(t.position, GroundPosition) < BoardManager.distance / 2)
            {
                if (t.tag == "Weapon")
                    continue;
                GroundToMove = t.gameObject;
                break;
            }
        }
        if (GroundToMove == null)
        {
            Debug.Log("error");
        }
        //对接移动函数
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if(t.name=="Grounds")
                continue;
            if (Vector3.Distance(GroundToMove.transform.position, t.position) < BoardManager.distance / 2)
            {
                t.gameObject.GetComponent<GroundClick>().PlayerMove();
                break;
            }
        }
    }

    void NetAttack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Vector3 EnemyPosition;
        EnemyPosition.x = proto.GetFloat(start, ref start);
        EnemyPosition.y = proto.GetFloat(start, ref start);
        EnemyPosition.z = proto.GetFloat(start, ref start);
        int UseDrag = proto.GetInt(start, ref start);
        GameObject PlayerToAttack = null;
        //攻击对应棋子
        foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
        {
            if(t.name=="Players")
                continue;
            if (Vector3.Distance(EnemyPosition, t.position) < BoardManager.distance / 2)
            {
                PlayerToAttack = t.gameObject;
                break;
            }
        }
        GameObject Blood=null;
        int attack = 0;
        //如果是抓勾攻击
        if (UseDrag == 1)
        {
            //对接攻击函数
            //获取反击攻击力，反击范围与双方血条
            GameObject thisBlood = null;
            int aimRange = 0;
            int aimAttack = 0;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == PlayerToAttack)
                {
                    Blood = GameManager.OccupiedGround[i].PlayerBlood;
                    switch (GameManager.OccupiedGround[i].PlayerWeapon)
                    {
                        case "Long": aimAttack = 2; aimRange = 2; break;
                        case "Short": aimAttack = 4; aimRange = 1; break;
                        case "Drag": aimAttack = 1; aimRange = 3; break;
                        case "Tear": aimAttack = 50; aimRange = 0; break;
                    }
                }
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    switch (GameManager.OccupiedGround[i].PlayerWeapon)
                    {
                        case "Long": attack = 2;break;
                        case "Short": attack = 4; break;
                        case "Drag": attack = 1;break;
                        case "Tear": attack = 50;break;
                    }
                    thisBlood = GameManager.OccupiedGround[i].PlayerBlood;
                }
            }
            if (PlayerToAttack.tag == "Monster")
            {
                Blood = GameObject.Find("MonsterBlood");
            }
            //对接攻击函数
            foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
            {
                if(t.name=="Players")
                    continue;
                if (Vector3.Distance(PlayerToAttack.transform.position, t.position) < BoardManager.distance / 2)
                {
                    if (t.tag == "Monster")
                        t.gameObject.GetComponent<MonsterController>().DragAttack(Blood, thisBlood, attack, aimAttack, aimRange);
                    else if (t.tag != GameManager.PlayerOnEdit.tag)
                        t.gameObject.GetComponent<RealPlayer>().DragAttack(Blood, thisBlood, attack, aimAttack, aimRange);
                    else
                    {
                        t.gameObject.GetComponent<RemoteEnemy>().DragAttack(Blood, thisBlood, attack, aimAttack, aimRange);
                    }
                    PlayerController.OnlyLine = false;
                    break;
                }
            }
        }
        else
        {
            //对接攻击函数
            //获取反击攻击力，反击范围与双方血条
            GameObject thisBlood = null;
            int aimRange = 0;
            int aimAttack = 0;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == PlayerToAttack)
                {
                    Blood = GameManager.OccupiedGround[i].PlayerBlood;
                    switch (GameManager.OccupiedGround[i].PlayerWeapon)
                    {
                        case "Long": aimAttack = 2; aimRange = 2; break;
                        case "Short": aimAttack = 4; aimRange = 1; break;
                        case "Drag": aimAttack = 1; aimRange = 3; break;
                        case "Tear": aimAttack = 50; aimRange = 0; break;
                    }
                }
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    switch (GameManager.OccupiedGround[i].PlayerWeapon)
                    {
                        case "Long": attack = 2;break;
                        case "Short": attack = 4; break;
                        case "Drag": attack = 1;break;
                        case "Tear": attack = 50;break;
                    }
                    thisBlood = GameManager.OccupiedGround[i].PlayerBlood;
                }
            }
            if (gameObject.tag == "Monster")
            {
                Blood = GameObject.Find("MonsterBlood");
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                {
                    if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                    {
                        GameManager.GroundStage gstage = GameManager.OccupiedGround[i];
                        gstage.Hate += attack;
                        GameManager.OccupiedGround[i] = gstage;
                        break;
                    }
                }
            }
            if(Blood==null)
                Debug.Log("NullBlood");
            else
            {
                Debug.Log("HasBlood");
            }
            //对接攻击函数
            foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
            {
                if(t.name=="Players")
                continue;
                if (Vector3.Distance(PlayerToAttack.transform.position, t.position) < BoardManager.distance / 2)
                {
                    if (t.tag == "Monster")
                        t.gameObject.GetComponent<MonsterController>().Attack(Blood, thisBlood, attack, aimAttack, aimRange);
                    else
                        t.gameObject.GetComponent<RealPlayer>().Attack(Blood, thisBlood, attack, aimAttack, aimRange);
                    break;
                }
            }
        }
    }

    void NetFindMonster(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Color color;
        //确定降怪点
        Vector3 problePosition=new Vector3();
        problePosition.x = proto.GetFloat(start, ref start);
        problePosition.y = proto.GetFloat(start, ref start);
        problePosition.z = proto.GetFloat(start, ref start);
        GameManager.instance.randomPlace = new List<GameObject>();
        //在周围标记提示圈
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(problePosition, t.position) < BoardManager.distance / 2 + BoardManager.distance * 2)
            {
                color = new Color(220, 220, 220);
                color.a = 0.2f;
                t.gameObject.GetComponent<SpriteRenderer>().color = color;
                GameManager.instance.randomPlace.Add(t.gameObject);
                if (t.tag == "Occupied")
                {
                    for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                    {
                        if (BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j] == t.gameObject)
                        {
                            GameManager.GroundStage GStage = GameManager.OccupiedGround[j];
                            GStage.OrigColor = t.gameObject.GetComponent<SpriteRenderer>().color;
                            GameManager.OccupiedGround[j] = GStage;
                            break;
                        }
                    }
                }
            }
        }
        color = new Color(255, 255, 0, 0.2f);
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            string team = "Team" + (PlayerController.MovingTeam + 1).ToString();
            if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
            {
                BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = color;
            }
        }
        GameManager.MudSetted = true;

    }
    void NetCreateMonster(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        GameObject monster;
        //清除提示圈
        for (int i = 0; i < GameManager.instance.randomPlace.Count; i++)
        {
            Color color = new Color(255, 255, 255);
            GameManager.instance.randomPlace[i].GetComponent<SpriteRenderer>().color = color;
            if (GameManager.instance.randomPlace[i].tag == "Occupied")
            {
                for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                {
                    if (BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j] == GameManager.instance.randomPlace[i])
                    {
                        GameManager.GroundStage GStage = GameManager.OccupiedGround[j];
                        GStage.OrigColor = GameManager.instance.randomPlace[i].GetComponent<SpriteRenderer>().color;
                        GameManager.OccupiedGround[j] = GStage;
                        break;
                    }
                }
            }
        }
        Vector3 tearPosition = new Vector3();
        tearPosition.x = proto.GetFloat(start, ref start);
        tearPosition.y = proto.GetFloat(start, ref start);
        tearPosition.z = proto.GetFloat(start, ref start);
        //生成怪物
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if(t.name=="Grounds")
                continue;
            if(Vector3.Distance(t.position,tearPosition)<BoardManager.distance/2)
            {
                GameManager.instance.TearGround = t.gameObject;
                break;
            }
            
        }
        Vector3 position = GameManager.instance.TearGround.transform.position + new Vector3(0, 0, -0.1f);
        //GroundStage GStage = new GroundStage();
        GameManager.instance.TearGround.tag = "Occupied";
        foreach (Transform t in GameManager.instance.TearGround.GetComponentsInChildren<Transform>())
            if (t.tag == "Weapon")
                Destroy(t.gameObject);
        monster = Instantiate(GameManager.instance.Monster, position, Quaternion.identity, GameObject.Find("Players").transform);
        monster.transform.Rotate(-45, 0, 0);
        Vector3 offset = new Vector3(6, -12f, -2f);
        GameManager.instance.MonsterBlood = Instantiate(GameManager.instance.Blood, position + offset, Quaternion.identity, GameObject.Find("Canvas").transform);
        GameManager.instance.MonsterBlood.GetComponent<Text>().text = "50";
        GameManager.instance.MonsterBlood.name = "MonsterBlood";
        //monster.GetComponent<MonsterController>().Monster.OnMonsterCreate();
    }
    void OnMatchBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int team = proto.GetInt(start, ref start);
        GameManager.RealPlayerTeam = new List<string>();
        GameManager.RealPlayerTeam.Add("Team" + (team + 1).ToString());
        int useAI = proto.GetInt(start, ref start);
        if (useAI == 1)
            GameManager.UseAI = true;
        else
            GameManager.UseAI = false;
        Quit.GetComponentInChildren<Text>().text = "投降";
        Quit.onClick.RemoveAllListeners();
        Quit.onClick.AddListener(delegate ()
        {
            ProtocolBytes prot = new ProtocolBytes();
            prot.AddString("EndGame");
            string winnerNotice = "";
            if (GameManager.RealPlayerTeam.Contains("Team1"))
            {
                prot.AddInt(2);
                winnerNotice = "队伍2胜利";
            }
            else
            {
                prot.AddInt(1);
                winnerNotice = "队伍1胜利";
            }
            if (GameManager.RealPlayerTeam.Count < 2 && (!GameManager.UseAI))
            {

                NetMgr.srvConn.Send(prot);
            }
            Quit.GetComponentInChildren<Text>().text = "退出";
            Quit.onClick.RemoveAllListeners();
            Quit.onClick.AddListener(delegate () { Application.Quit(); });
            GameManager.WinnerNotice.SetActive(true);
            GameManager.Notice.text = winnerNotice;
            //SceneManager.LoadScene("MainPage");
        });
        SceneManager.LoadScene("Game");
    }

    void SkipMove(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Vector3 position;
        position.x = proto.GetFloat(start, ref start);
        position.y = proto.GetFloat(start, ref start);
        position.z = proto.GetFloat(start, ref start);
        //找到待移动棋子
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (Vector3.Distance(position, GameManager.OccupiedGround[i].PlayerOnGround.transform.position) < BoardManager.distance / 2)
            {
                GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
                break;
            }

        }
        GameObject.Find("Skip").GetComponent<SkipTurn>().Skip();
    }

    void SkipAttack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        GameObject.Find("Skip").GetComponent<SkipTurn>().Skip();
    }

    void EndGame(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string winnerNotice="";
        int winCase = proto.GetInt(start, ref start);
        switch(winCase)
        {
            case 0:
                winnerNotice ="合作胜利";
                break;
            case 1:
                winnerNotice ="队伍1胜利";
                break;
            case 2:
                winnerNotice ="队伍2胜利";
                break;
            case 3:
                winnerNotice ="全员失败!";
                break;
        }
        GameManager.WinnerNotice.SetActive(true);
        GameManager.Notice.text = winnerNotice;
        Quit.GetComponentInChildren<Text>().text = "退出";
        Quit.onClick.RemoveAllListeners();
        Quit.onClick.AddListener(delegate () { Application.Quit(); });
    }
}
