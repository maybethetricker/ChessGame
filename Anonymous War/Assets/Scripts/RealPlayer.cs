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
        if(GameManager.instance.SmoothMoveOnWay)
            return;
        if(!Root.instance.MouseClickLimit(gameObject,Root.instance.LimitClickException,ref Root.instance.UseLimitClick,Root.instance.LimitClickFinished))
            return;
        if(GameManager.Stage==0)
        {
            if (this.tag != "Team" + (GroundClick.TeamCounter + 1).ToString() || !GameManager.RealPlayerTeam.Contains(this.tag))
                return;
            foreach (GameManager.GroundStage gstage in GameManager.OccupiedGround)
                if (gstage.PlayerOnGround == gameObject && gstage.i != -1)
                    return;
            GameManager.PlayerOnEdit = gameObject;
            StartCoroutine(OnClickJump());

        }
        if (GameManager.Stage == 1 && GameManager.PlayerOnEdit == null)//移动
        {
            //只有本回合能动的一方可动
            if (this.tag != "Team" + (GameManager.instance.MovingTeam + 1).ToString() || !GameManager.RealPlayerTeam.Contains(this.tag))
                return;
            foreach (GameManager.GroundStage gstage in GameManager.OccupiedGround)
                if (gstage.PlayerOnGround == gameObject && gstage.Moved == true)
                    return;
            //标记出移动者并计算可移动范围
            //transform.localScale *= 1.1f;
            GameManager.PlayerOnEdit = gameObject;
            StartCoroutine(OnClickJump());
            foreach (GameManager.GroundStage gstage in GameManager.OccupiedGround)
                if (gstage.PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    CheckRange(gstage.PlayerOnGround, BoardManager.Grounds[gstage.i][gstage.j].transform.position, 1, "Grounds", 0, false,false);
                    break;
                }

        }
        //移动，同上
        else if (GameManager.Stage == 1 && Vector2.Distance(GameManager.PlayerOnEdit.transform.position, transform.position) > 0.1f)
        {
            //只有本回合能动的一方可动
            if (this.tag != "Team" + (GameManager.instance.MovingTeam + 1).ToString() || !GameManager.RealPlayerTeam.Contains(this.tag))
                return;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {

                if (GameManager.OccupiedGround[i].PlayerOnGround == gameObject && GameManager.OccupiedGround[i].Moved == true)
                    return;
            }
            //mistake:haven't change scale back after move, won't change as way of highlighting isn't ready
            //transform.localScale *= 1.1f;
            //GameManager.PlayerOnEdit.transform.localScale /= 1.1f;
            StartCoroutine(OnClickJump());
            GameManager.PlayerOnEdit = gameObject;
            foreach (GameManager.GroundStage gstage in GameManager.OccupiedGround)
                if (gstage.PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    CheckRange(gstage.PlayerOnGround, BoardManager.Grounds[gstage.i][gstage.j].transform.position, 1, "Grounds", 0, false,false);
                    break;
                }
        }
        else if (GameManager.Stage == 2 && Vector2.Distance(GameManager.PlayerOnEdit.transform.position, transform.position) > 0.1f)
        {
            //只有本回合能动的一方可动
            if (!GameManager.RealPlayerTeam.Contains(GameManager.PlayerOnEdit.tag))
                return;

            bool find = false;
            for (int i = 0; i < AimRangeList.Count; i++)
            {
                if (AimRangeList[i].Aim == gameObject)
                {
                    find = true;
                    break;
                }
            }
            if (!find)
                return;
            if (GameManager.RealPlayerTeam.Count < GameManager.TeamCount && (!GameManager.UseAI))
            {
                ProtocolBytes protocol = new ProtocolBytes();
                protocol.AddString("UpdateAttack");
                protocol.AddFloat(this.transform.position.x);
                protocol.AddFloat(this.transform.position.y);
                protocol.AddFloat(this.transform.position.z);
                protocol.AddInt(GameManager.instance.AttackMode);
                NetMgr.srvConn.Send(protocol);
            }
            //获取反击攻击力，反击范围与双方血条
            GameObject thisBlood = null;
            string aimWeapon = "";
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == this.gameObject)
                {
                    Blood = GameManager.OccupiedGround[i].PlayerBlood;
                    aimWeapon = GameManager.OccupiedGround[i].PlayerWeapon;
                    //change:data error
                }
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    thisBlood = GameManager.OccupiedGround[i].PlayerBlood;
                }
            }
            switch (GameManager.instance.AttackMode)
            {
                case 0:
                    Attack(Blood, thisBlood, gameObject.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon,true);
                    break;
                case 1:
                    DragAttack(Blood, thisBlood, attack, aimWeapon);
                    break;
                case 2:
                    ArrowAttack(Blood, thisBlood, gameObject.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
                    break;
            }
        }
    }
}
