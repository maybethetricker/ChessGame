using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MonsterController : PlayerController
{

    public override void Die()//怪死，游戏结束
    {
        GameManager.WinnerNotice.SetActive(true);
        if(DiedSoldiersTeam1<3&&DIedSoldiersTeam2<3)
        {
            
            GameManager.Notice.text = "ALL WIN";
        }
        else if(DiedSoldiersTeam1>=3)
        {
            GameManager.Notice.GetComponent<Text>().text = "TEAM2 WIN";
        }
        else
        {
            GameManager.Notice.GetComponent<Text>().text = "TEAM1 WIN";
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
}
