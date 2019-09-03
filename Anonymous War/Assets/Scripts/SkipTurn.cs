﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkipTurn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            if (GameManager.PlayerOnEdit != null && GameManager.RealPlayerTeam.Contains(GameManager.PlayerOnEdit.tag))
            {
                if ((!GameManager.UseAI) && GameManager.RealPlayerTeam.Count < 2)
                {
                    if (GameManager.Stage == 1)
                    {
                        ProtocolBytes protocol = new ProtocolBytes();
                        protocol.AddString("SkipMove");
                        protocol.AddFloat(GameManager.PlayerOnEdit.transform.position.x);
                        protocol.AddFloat(GameManager.PlayerOnEdit.transform.position.y);
                        protocol.AddFloat(GameManager.PlayerOnEdit.transform.position.z);
                        NetMgr.srvConn.Send(protocol);
                    }
                    else
                    {
                        ProtocolBytes protocol = new ProtocolBytes();
                        protocol.AddString("SkipAttack");
                        NetMgr.srvConn.Send(protocol);
                    }
                }
                Skip();
            }
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Skip()//跳过移动或攻击阶段
    {
        //跳过移动，修改状态为已移动并删除高亮
        if (GameManager.Stage == 1 && GameManager.PlayerOnEdit.tag == "Team" + (GameManager.instance.MovingTeam + 1).ToString())
        {
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
                if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                {
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.OccupiedGround[i].OrigColor;
                }
            }
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    GameManager.GroundStage GStage = GameManager.OccupiedGround[i];
                    GStage.Moved = true;
                    GameManager.OccupiedGround[i] = GStage;
                    break;
                }
            }
            GameManager.Stage = 2;
            return;
        }
        //跳过攻击，删除高亮并更替小回合
        if (GameManager.Stage == 2)
        {
            GameManager.PlayerOnEdit.GetComponent<RealPlayer>().ClearHighlight();
            GameManager.PlayerOnEdit.GetComponent<RealPlayer>().ChangeTurn();
            GameManager.instance.EnemyChecked = false;

        }

    }
}
