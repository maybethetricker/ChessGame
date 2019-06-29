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
            if (this.tag != "Team" + (MovingTeam + 1).ToString() || this.tag != "Team" + (GameManager.RealPlayerTeam + 1).ToString())
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
                    CheckRange(gstage.PlayerOnGround, gstage.Ground.transform.position, MP, "Grounds");
                    break;
                }

        }
        //移动，同上
        else if (GameManager.Stage == 1 && Vector2.Distance(GameManager.PlayerOnEdit.transform.position, transform.position) > 0.1f)
        {
            //只有本回合能动的一方可动
            if (this.tag != "Team" + (MovingTeam + 1).ToString() || this.tag != "Team" + (GameManager.RealPlayerTeam + 1).ToString())
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
                    CheckRange(gstage.PlayerOnGround, gstage.Ground.transform.position, MP, "Grounds");
                    break;
                }
        }
        
    }
}
