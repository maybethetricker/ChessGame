using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : PlayerController
{
    //防止协程多次启动
    public static bool CoroutineStarted = false;

    // Update is called once per frame
    void Update()
    {
        //检测攻击范围
        CheckAttack();
        
        if(GameManager.Stage==1&&this.tag == "Team" + (MovingTeam + 1).ToString() && !GameManager.RealPlayerTeam.Contains(this.tag))
        {
            //等待一会儿后移动
            if(!CoroutineStarted)
                StartCoroutine(WaitToMove());
        }
        if(GameManager.Stage == 2 &&  !GameManager.RealPlayerTeam.Contains(GameManager.PlayerOnEdit.tag))
        {
            //等待一会儿后攻击
            if(!CoroutineStarted)
                StartCoroutine(WaitToAttack());
        }

    }

    /// <summary>
    /// OnMouseDown is called when the user has pressed the mouse button while
    /// over the GUIElement or Collider.
    /// </summary>
    void OnMouseDown()
    {
        //玩家攻击时的受击检测，与AI逻辑无关，可不看
        if (GameManager.Stage == 2 && Vector2.Distance(GameManager.PlayerOnEdit.transform.position, transform.position) > 0.1f)
        {
            //只有本回合能动的一方可动
            if ( !GameManager.RealPlayerTeam.Contains(GameManager.PlayerOnEdit.tag))
                return;
            //获取反击攻击力，反击范围与双方血条
            GameObject thisBlood = null;
            int aimRange = 0;
            int aimAttack = 0;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == this.gameObject)
                {
                    Blood = GameManager.OccupiedGround[i].PlayerBlood;
                    switch (GameManager.OccupiedGround[i].PlayerWeapon)
                    {
                        case "Long": aimAttack = 2; aimRange = 2; break;
                        case "Short": aimAttack = 4; aimRange = 1; break;
                        case "Drag": aimAttack = 1; aimRange = 3; break;
                        case "Tear": aimAttack = 50; aimRange = 0; break;
                    }
//change:data error
                }
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    thisBlood = GameManager.OccupiedGround[i].PlayerBlood;
                }
            }
            if (gameObject.tag == "Monster")
            {
                Blood = GameObject.Find("MonsterBlood");
            }
            //是否直线攻击
            if (CanMoveList.ContainsKey(gameObject) && !OnlyLine)
                Attack(Blood, thisBlood, attack, aimAttack, aimRange);
            if (OnlyLine)
            {

                for (int i = 0; i < LineCanAttack.Count; i++)
                {
                    if (LineCanAttack[i].Enemy == gameObject)
                    {
                        DragAttack(Blood, thisBlood, attack, aimAttack, aimRange);
                        OnlyLine = false;
                        break;
                    }
                }
            }

        }
    }

    void AIMove()
    {
        //遍历所有未移动的AI棋子，并移动遍历到的第一颗棋子
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if((!GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[i].PlayerOnGround.tag))&&!GameManager.OccupiedGround[i].Moved)
            {
                GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
                //检测周围可移动地块
                CheckRange(GameManager.OccupiedGround[i].PlayerOnGround, BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position, MP, "Grounds");
                break;
            }

        }
        //要移动到的地块
        GameObject GroundToMove=null;
        //遍历所有可移动地块，移动到第一个可移动到的地块上
        foreach (KeyValuePair<GameObject, Color> key in CanMoveList)
        {
            GroundToMove = key.Key;
            break;
        }

        //对接移动函数，可以不用看了
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if(Vector3.Distance(GroundToMove.transform.position, t.position) < BoardManager.distance / 2)
            {
                t.gameObject.GetComponent<GroundClick>().PlayerMove();
                break;
            }
        }
    }
    
    void AIAttack()
    {
        //如果是抓勾攻击
        if (OnlyLine)
        {
            GameObject PlayerToAttack = null;
            //遍历所有可攻击对象，攻击第一个可攻击的对象
            for (int i = 0; i < LineCanAttack.Count; i++)
            {
                PlayerToAttack = LineCanAttack[i].Enemy;
                break;
            }
            //对接攻击函数，可以不用看了
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
                if (Vector3.Distance(PlayerToAttack.transform.position, t.position) < BoardManager.distance / 2)
                {
                    if(t.tag=="Monster")
                        t.gameObject.GetComponent<MonsterController>().DragAttack(Blood, thisBlood, attack, aimAttack, aimRange);
                    else if(t.tag!=GameManager.PlayerOnEdit.tag)
                        t.gameObject.GetComponent<RealPlayer>().DragAttack(Blood, thisBlood, attack, aimAttack, aimRange);
                    else
                    {
                        t.gameObject.GetComponent<AI>().DragAttack(Blood, thisBlood, attack, aimAttack, aimRange);
                    }
                    OnlyLine = false;
                    break;
                }
            }
        }
        else
        {
            GameObject PlayerToAttack = null;
            //遍历所有可攻击对象，攻击第一个可攻击的对象
            foreach (KeyValuePair<GameObject, Color> key in CanMoveList)
            {
                PlayerToAttack = key.Key;
                break;
            }
            //对接攻击函数，可以不用看了
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
                if (Vector3.Distance(PlayerToAttack.transform.position, t.position) < BoardManager.distance / 2)
                {
                    if(t.tag=="Monster")
                        t.gameObject.GetComponent<MonsterController>().Attack(Blood, thisBlood, attack, aimAttack, aimRange);
                    else
                        t.gameObject.GetComponent<RealPlayer>().Attack(Blood, thisBlood, attack, aimAttack, aimRange);
                    break;
                }
            }
        }
    }

    

    IEnumerator WaitToAttack()
    {
        CoroutineStarted = true;
        yield return new WaitForSeconds(1);
        AIAttack();
        CoroutineStarted = false;
    }
    IEnumerator WaitToMove()
    {
        CoroutineStarted = true;
        yield return new WaitForSeconds(1);
        AIMove();
        CoroutineStarted = false;
    }

    
}
