using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Root : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        NetMgr.srvConn.msgDist.AddListener("StartFight", OnMatchBack);
        NetMgr.srvConn.msgDist.AddListener("UpdateMove", NetMove);
        NetMgr.srvConn.msgDist.AddListener("UpdateAttack", NetAttack);
        NetMgr.srvConn.msgDist.AddListener("SkipMove", SkipMove);
        NetMgr.srvConn.msgDist.AddListener("SkipAttack", SkipAttack);
        NetMgr.srvConn.msgDist.AddListener("UpdateLand",NetLand);
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
            if (PlayerToAttack.tag == "Monster")
            {
                Blood = GameObject.Find("MonsterBlood");
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

    void OnMatchBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int team = proto.GetInt(start, ref start);
        GameManager.RealPlayerTeam.Add("Team" + (team + 1).ToString());
        int useAI = proto.GetInt(start, ref start);
        if(useAI==1)
            GameManager.UseAI = true;
        else
            GameManager.UseAI = false;
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
}
