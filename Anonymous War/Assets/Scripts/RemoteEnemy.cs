using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteEnemy : PlayerController
{

    /// <summary>
    /// OnMouseDown is called when the user has pressed the mouse button while
    /// over the GUIElement or Collider.
    /// </summary>
    void OnMouseDown()
    {
        //玩家攻击时的受击检测
        if (GameManager.Stage == 2 && Vector2.Distance(GameManager.PlayerOnEdit.transform.position, transform.position) > 0.1f)
        {
            //只有本回合能动的一方可动
            if (!GameManager.RealPlayerTeam.Contains(GameManager.PlayerOnEdit.tag))
                return;
            if ((!CanMoveList.ContainsKey(gameObject)) && (!OnlyLine))
                return;
            if (OnlyLine)
            {
                bool find = false;
                for (int i = 0; i < LineCanAttack.Count; i++)
                {
                    if (LineCanAttack[i].Enemy == gameObject)
                    {
                        find = true;
                        break;
                    }
                }
                if(!find)
                    return;
            }
            //UpdateAttack协议，包含被攻击者位置与是否使用抓勾
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("UpdateAttack");
            protocol.AddFloat(this.transform.position.x);
            protocol.AddFloat(this.transform.position.y);
            protocol.AddFloat(this.transform.position.z);
            if (OnlyLine)
                protocol.AddInt(1);
            else
            {
                protocol.AddInt(0);
            }
            NetMgr.srvConn.Send(protocol);
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
            if (!OnlyLine)
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

    
}
