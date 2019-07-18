using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealPlayer : PlayerController
{
        /// <summary>
    /// OnMouseDown is called when the user has pressed the mouse button while
    /// over the GUIElement or Collider.
    /// </summary>
    void OnMouseDown()//在移动/攻击时点击该回合可操作棋子触发操作
    {
        if (GameManager.Stage == 1 && GameManager.PlayerOnEdit == null)//移动
        {
            //只有本回合能动的一方可动
            if (this.tag != "Team" + (MovingTeam + 1).ToString() ||  !GameManager.RealPlayerTeam.Contains(this.tag))
                return;
            foreach (GameManager.GroundStage gstage in GameManager.OccupiedGround)
                if (gstage.PlayerOnGround == gameObject && gstage.Moved == true)
                    return;
            //标记出移动者并计算可移动范围
            //transform.localScale *= 1.1f;
            GameManager.PlayerOnEdit = gameObject;
            foreach (GameManager.GroundStage gstage in GameManager.OccupiedGround)
                if (gstage.PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    CheckRange(gstage.PlayerOnGround, BoardManager.Grounds[gstage.i][gstage.j].transform.position, MP, "Grounds");
                    break;
                }

        }
        //移动，同上
        else if (GameManager.Stage == 1 && Vector2.Distance(GameManager.PlayerOnEdit.transform.position, transform.position) > 0.1f)
        {
            //只有本回合能动的一方可动
            if (this.tag != "Team" + (MovingTeam + 1).ToString() || !GameManager.RealPlayerTeam.Contains(this.tag))
                return;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {

                if (GameManager.OccupiedGround[i].PlayerOnGround == gameObject && GameManager.OccupiedGround[i].Moved == true)
                    return;
            }
//mistake:haven't change scale back after move, won't change as way of highlighting isn't ready
            //transform.localScale *= 1.1f;
            //GameManager.PlayerOnEdit.transform.localScale /= 1.1f;
            GameManager.PlayerOnEdit = gameObject;
            foreach (GameManager.GroundStage gstage in GameManager.OccupiedGround)
                if (gstage.PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    CheckRange(gstage.PlayerOnGround, BoardManager.Grounds[gstage.i][gstage.j].transform.position, MP, "Grounds");
                    break;
                }
        }
        else if (GameManager.Stage == 2 && Vector2.Distance(GameManager.PlayerOnEdit.transform.position, transform.position) > 0.1f)
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
            if (GameManager.RealPlayerTeam.Count < 2 && (!GameManager.UseAI))
            {
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
            }
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
}
