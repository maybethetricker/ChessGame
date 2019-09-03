﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MonsterController : PlayerController
{
    public MonsterBase Monster;
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        ChooseMonster();
        Monster.monsterPosition = gameObject.transform.position;
        Monster.OnMonsterCreate();
    }
    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        CheckAttack();
        //扩毒
        if (GameManager.instance.Turn > 2 && !GameManager.MudSetted)
        {
            Monster.MonsterAttack();
            //SetMug((GameManager.Turn) / 2);
        }
    }
    void ChooseMonster()
    {
        Monster = new Monster1();
    }
    public override void Die()//怪死，游戏结束
    {
        GameManager.WinnerNotice.SetActive(true);
        foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Players")
                continue;
            if (t.gameObject.GetComponent<AI>())
                Destroy(t.gameObject.GetComponent<AI>());
            if (t.gameObject.GetComponent<RealPlayer>())
                Destroy(t.gameObject.GetComponent<RealPlayer>());

        }
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("EndGame");
        if (DiedSoldiersTeam1 < 3 && DiedSoldiersTeam2 < 3)
        {
            protocol.AddInt(0);
            GameManager.Notice.text = "合作胜利";
        }
        else if (DiedSoldiersTeam1 >= 3)
        {
            protocol.AddInt(2);
            GameManager.Notice.GetComponent<Text>().text = "队伍2胜利";
        }
        else
        {
            protocol.AddInt(1);
            GameManager.Notice.GetComponent<Text>().text = "队伍1胜利";
        }
        NetMgr.srvConn.Send(protocol);
        if (GameManager.UseAI)
        {
            Button Quit = GameObject.Find("Quit").GetComponent<Button>();
            Quit.GetComponentInChildren<Text>().text = "退出";
            Quit.onClick.RemoveAllListeners();
            Quit.onClick.AddListener(delegate () { Application.Quit(); });
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
            if (GameManager.RealPlayerTeam.Count < 2 && (!GameManager.UseAI))
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
            switch (GameManager.instance.AttackMode)
            {
                case 0:
                    Attack(Blood, thisBlood, gameObject.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
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
