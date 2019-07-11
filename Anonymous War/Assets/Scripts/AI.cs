using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AI : PlayerController
{
    public struct SurroundScore
    {
        public GameObject Aim;
        public GameObject Enemy;
        public int score;
    }
    //分数列表
    public static Dictionary<GameObject, SurroundScore> score;
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
        //要移动到的地块
        GameObject GroundToMove = null;
        int maxScore = 0, thisScore = 0;
        score = new Dictionary<GameObject, SurroundScore>();
        //算分
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (!GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[i].PlayerOnGround.tag))
            {
                SurroundScore surroundscore = new SurroundScore();
                if (GameManager.OccupiedGround[i].Moved)
                {
                    surroundscore.score = -10;
                    surroundscore.Aim = null;
                    surroundscore.Enemy = null;
                    score.Add(GameManager.OccupiedGround[i].PlayerBlood, surroundscore);
                }
                else
                {
                    maxScore = -10;
                    CheckRange(GameManager.OccupiedGround[i].PlayerOnGround, BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position, MP, "Grounds");
                    Color color = BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color;
                    CanMoveList.Add(BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j], color);
                    int enemyScore=0, enemyMaxScore = -10;
                    foreach (KeyValuePair<GameObject, Color> key in CanMoveList)
                    {
                        if(key.Key.tag=="Weapon")
                            continue;
                        if (key.Key!=BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j]&&key.Key.tag == "Occupied")
                            thisScore = -10;
                        else
                        {
                            string groundtag;
                            if(key.Key.tag=="Untagged"||key.Key==BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j])
                                groundtag = GameManager.OccupiedGround[i].PlayerWeapon;
                            else
                                groundtag = key.Key.tag;
                            switch (groundtag)
                            {
                                case "Short":
                                    for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                                    {
                                        if (GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[j].PlayerOnGround.tag))
                                        {
                                            enemyScore = 0;
                                            if (Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, key.Key.transform.position) < BoardManager.distance * 1.5f)
                                            {
                                                enemyScore = 30 - int.Parse(GameManager.OccupiedGround[j].PlayerBlood.GetComponent<Text>().text);
                                                switch (GameManager.OccupiedGround[j].PlayerWeapon)
                                                {
                                                    case "Short":
                                                        break;
                                                    case "Long":
                                                        enemyScore *= 2;
                                                        break;
                                                    case "Drag":
                                                        enemyScore *= 4;
                                                        break;
                                                    case "Tear":
                                                        enemyScore = (30 - int.Parse(GameManager.OccupiedGround[i].PlayerBlood.GetComponent<Text>().text)) / 4;
                                                        break;
                                                }
                                                if (GameManager.OccupiedGround[j].Faint)
                                                    enemyScore *= 2;

                                            }
                                            enemyScore -= (int)Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, key.Key.transform.position);
                                            if (enemyScore > enemyMaxScore)
                                            {
                                                enemyMaxScore = enemyScore;
                                                surroundscore.Enemy = GameManager.OccupiedGround[j].PlayerOnGround;
                                            }
                                        }
                                    }
                                    thisScore = 10 + enemyMaxScore;
                                    break;
                                case "Long":
                                    for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                                    {
                                        if (GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[j].PlayerOnGround.tag))
                                        {
                                            enemyScore = 0;
                                            if (Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, key.Key.transform.position) < BoardManager.distance * 2.5f)
                                            {
                                                enemyScore = 30 - int.Parse(GameManager.OccupiedGround[j].PlayerBlood.GetComponent<Text>().text);
                                                switch (GameManager.OccupiedGround[j].PlayerWeapon)
                                                {
                                                    case "Short":
                                                        if (Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, key.Key.transform.position) < BoardManager.distance * 1.5f)
                                                            enemyScore /= 2;
                                                        else
                                                            enemyScore *= 2;
                                                        break;
                                                    case "Long":
                                                        break;
                                                    case "Drag":
                                                        enemyScore *= 3;
                                                        break;
                                                    case "Tear":
                                                        if(GameManager.OccupiedGround[j].Faint)
                                                            enemyScore *= 5;
                                                        else
                                                            enemyScore = (30 - int.Parse(GameManager.OccupiedGround[i].PlayerBlood.GetComponent<Text>().text))/3;
                                                        break;
                                                }
                                                if(GameManager.OccupiedGround[j].Faint)
                                                    enemyScore *= 2;
                                            }
                                            enemyScore -= (int)Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, key.Key.transform.position);
                                            if (enemyScore > enemyMaxScore)
                                            {
                                                enemyMaxScore = enemyScore;
                                                surroundscore.Enemy = GameManager.OccupiedGround[j].PlayerOnGround;
                                            }
                                        }
                                    }
                                    thisScore = 10 + enemyMaxScore;
                                    break;
                                case "Drag":
                                    thisScore = 1;
                                    break;
                                case "Tear":
                                    foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
                                    {
                                        if(t.name=="Players")
                                            continue;
                                        enemyScore = 0;
                                        if (t.gameObject.tag == "Monster")
                                        {
                                            enemyScore -= (int)Vector3.Distance(t.position, key.Key.transform.position);
                                            if (enemyScore > enemyMaxScore)
                                            {
                                                enemyMaxScore = enemyScore;
                                                surroundscore.Enemy = t.gameObject;
                                            }
                                        }
                                    }
                                    thisScore = 1000+enemyMaxScore;
                                    break;
                            }
                        }
                        if (thisScore > maxScore)
                        {
                            surroundscore.Aim = key.Key;
                            maxScore = thisScore;
                        }
                    }
                    surroundscore.score = maxScore;
                    score.Add(GameManager.OccupiedGround[i].PlayerBlood, surroundscore);
                }
            }
        }
        maxScore = -10;
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if ((!GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[i].PlayerOnGround.tag)) && !GameManager.OccupiedGround[i].Moved)
            {
                if (score[GameManager.OccupiedGround[i].PlayerBlood].score <= maxScore)
                    continue;
                maxScore = score[GameManager.OccupiedGround[i].PlayerBlood].score;
                GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
                if (BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j] == score[GameManager.OccupiedGround[i].PlayerBlood].Aim)
                {
                    GroundToMove = null;
                }
                else
                    GroundToMove = score[GameManager.OccupiedGround[i].PlayerBlood].Aim;
            }
        }
        if (GroundToMove == null)
        {
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
        //对接移动函数，可以不用看了
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

    void AIAttack()
    {
        //如果是抓勾攻击
        if (OnlyLine)
        {
            GameObject PlayerToAttack = null;
            //遍历所有可攻击对象，攻击第一个可攻击的对象
            for (int i = 0; i < LineCanAttack.Count; i++)
            {
                if(LineCanAttack[i].Enemy.tag==GameManager.PlayerOnEdit.tag)
                    continue;
                PlayerToAttack = LineCanAttack[i].Enemy;
                break;
            }
            if(PlayerToAttack==null)
                PlayerToAttack = LineCanAttack[0].Enemy;
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
                if(t.name=="Players")
                    continue;
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
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    PlayerToAttack = score[GameManager.OccupiedGround[i].PlayerBlood].Enemy;
                    break;
                }
            if (PlayerToAttack == null)
            {
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                    if (GameManager.OccupiedGround[i].PlayerOnGround.tag == "Monster")
                    {
                        PlayerToAttack = GameManager.OccupiedGround[i].PlayerOnGround;
                        break;
                    }
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
                if(t.name=="Players")
                    continue;
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
        if ((CanMoveList.Count == 0 && (!OnlyLine)) || (OnlyLine && LineCanAttack.Count == 0))
        {
            OnlyLine = false;
            StopCoroutine(WaitToAttack());
        }
        yield return new WaitForSeconds(1);
        AIAttack();
        CoroutineStarted = false;
    }
    IEnumerator WaitToMove()
    {
        CoroutineStarted = true;
        yield return new WaitForSeconds(3);
        AIMove();
        CoroutineStarted = false;
    }

    
}
