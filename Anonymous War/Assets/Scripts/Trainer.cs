using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trainer : PlayerController
{

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Guide == 1)
            AIGuide1();
    }
    void AIGuide1()
    {
        int Step = Root.instance.flowchart.GetIntegerVariable("GuideStep");
        if(Step==6)
        {
            
        }
    }
    void TrainnerMove(GameObject GroundToMove)
    {
        foreach (Transform t in GameObject.Find("Grounds").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Grounds")
                continue;
            if (Vector3.Distance(GroundToMove.transform.position, t.position) < BoardManager.distance / 2)
            {
                t.gameObject.GetComponent<GroundClick>().PlayerMove();
                break;
            }
        }
        //score = new Dictionary<GameObject, SurroundScore>();
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (GameManager.OccupiedGround[i].PlayerOnGround.transform == GameManager.PlayerOnEdit.transform)
            {
                //SurroundScore surroundscore = new SurroundScore();
                //surroundscore.MaxScoreAim = GroundToMove;
                //surroundscore.MaxScore = maxScore;
                //surroundscore.MaxScoreEnemy = possibleEnemy;
                //score.Add(GameManager.OccupiedGround[i].PlayerBlood, surroundscore);
            }
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
