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

    // Update is called once per frame
    void Update()
    {
        //检测攻击范围
        CheckAttack();
        if (GameManager.Stage == 1 && this.tag == "Team" + (GameManager.instance.MovingTeam + 1).ToString() && !GameManager.RealPlayerTeam.Contains(this.tag))
        {

            //等待一会儿后移动
            if (!GameManager.instance.CoroutineStarted)
                StartCoroutine(WaitToMove());
        }
        if (GameManager.Stage == 2 && !GameManager.RealPlayerTeam.Contains(GameManager.PlayerOnEdit.tag))
        {
            //等待一会儿后攻击
            if (!GameManager.instance.CoroutineStarted)
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
            if (gameObject.tag == "Monster")
            {
                Blood = GameObject.Find("MonsterBlood");
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

    void AIMove()
    {
        //要移动到的地块
        GameObject GroundToMove = null;
        int maxScore = 0, thisScore = 0, minScore = 0;
        score = new Dictionary<GameObject, SurroundScore>();
        int PlayerBloodSum = 0, EnemyBloodSum = 0;
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[i].PlayerOnGround.tag))
                PlayerBloodSum += int.Parse(GameManager.OccupiedGround[i].PlayerBlood.GetComponentInChildren<Text>().text);
            else
            {
                EnemyBloodSum += int.Parse(GameManager.OccupiedGround[i].PlayerBlood.GetComponentInChildren<Text>().text);
            }
        }
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
                    CheckRange(GameManager.OccupiedGround[i].PlayerOnGround, BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].transform.position, 1, "Grounds", 0, false);
                    Color color = BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color;
                    AimNode node = new AimNode();
                    node.Aim = BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j];
                    node.JudgeHelper = node.Aim;
                    node.color = color;
                    AimRangeList.Add(node);
                    int enemyScore = 0, enemyMaxScore = -10;
                    foreach (AimNode Node in AimRangeList)
                    {
                        if (Node.Aim.tag == "Weapon")
                            continue;
                        if (Node.Aim != BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j] && Node.Aim.tag == "Occupied")
                            thisScore = -10;
                        else
                        {
                            string groundtag;
                            if (Node.Aim.tag == "Untagged" || Node.Aim == BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j])
                                groundtag = GameManager.OccupiedGround[i].PlayerWeapon;
                            else
                                groundtag = Node.Aim.tag;
                            switch (groundtag)
                            {
                                case "Short":
                                    for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                                    {
                                        if (GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[j].PlayerOnGround.tag))
                                        {
                                            enemyScore = 0;
                                            if (Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, Node.Aim.transform.position) < BoardManager.distance * 1.5f)
                                            {
                                                enemyScore = 40 - int.Parse(GameManager.OccupiedGround[j].PlayerBlood.GetComponent<Text>().text);
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
                                                        enemyScore = (40 - int.Parse(GameManager.OccupiedGround[i].PlayerBlood.GetComponent<Text>().text)) / 4;
                                                        break;
                                                }
                                                if (GameManager.OccupiedGround[j].Faint)
                                                    enemyScore *= 2;

                                            }
                                            enemyScore -= (int)(Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, Node.Aim.transform.position) / BoardManager.distance);
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
                                            int i1 = 0, j1 = 0, i2 = 0, j2 = 0, Range = 3;
                                            for (int l = 0; l < BoardManager.row; l++)
                                                for (int k = 0; k < BoardManager.col; k++)
                                                {
                                                    if (BoardManager.Grounds[l][k] != null && Vector3.Distance(BoardManager.Grounds[l][k].transform.position, Node.Aim.transform.position) < BoardManager.distance / 2)
                                                    {
                                                        i1 = l;
                                                        j1 = k;
                                                    }
                                                    if (BoardManager.Grounds[l][k] != null && Vector3.Distance(BoardManager.Grounds[l][k].transform.position, GameManager.OccupiedGround[j].PlayerOnGround.transform.position) < BoardManager.distance / 2)
                                                    {
                                                        i2 = l;
                                                        j2 = k;
                                                    }
                                                }
                                            if (Mathf.Abs(j2 - j1) <= Range
                                                && ((j1 >= j2 && (i1 >= i2 - Range && i1 <= i2 + Range + j2 - j1))
                                                || (j1 < j2 && (i1 >= i2 - Range + j2 - j1 && i1 <= i2 + Range))))
                                            {
                                                if (Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, Node.Aim.transform.position) < BoardManager.distance * 1.5f)
                                                    enemyScore = 1;
                                                else
                                                {
                                                    enemyScore = 40 - int.Parse(GameManager.OccupiedGround[j].PlayerBlood.GetComponent<Text>().text);
                                                    switch (GameManager.OccupiedGround[j].PlayerWeapon)
                                                    {
                                                        case "Short":
                                                            enemyScore *= 2;
                                                            break;
                                                        case "Long":
                                                            break;
                                                        case "Drag":
                                                            enemyScore *= 3;
                                                            break;
                                                        case "Tear":
                                                            if (GameManager.OccupiedGround[j].Faint)
                                                                enemyScore *= 5;
                                                            else
                                                                enemyScore = (40 - int.Parse(GameManager.OccupiedGround[i].PlayerBlood.GetComponent<Text>().text)) / 3;
                                                            break;
                                                    }
                                                    if (GameManager.OccupiedGround[j].Faint)
                                                        enemyScore *= 2;
                                                }
                                            }
                                            enemyScore -= (int)(Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, Node.Aim.transform.position) / BoardManager.distance);
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
                                        if (t.name == "Players")
                                            continue;
                                        enemyScore = 0;
                                        if (t.gameObject.tag == "Monster")
                                        {
                                            enemyScore -= (int)(Vector3.Distance(t.position, Node.Aim.transform.position) / BoardManager.distance);
                                            if (enemyScore > enemyMaxScore)
                                            {
                                                enemyMaxScore = enemyScore;
                                                surroundscore.Enemy = t.gameObject;
                                            }
                                        }
                                    }
                                    thisScore = 1000 + enemyMaxScore;
                                    break;
                            }
                        }
                        if (thisScore > maxScore)
                        {
                            surroundscore.Aim = Node.Aim;
                            maxScore = thisScore;
                        }
                    }
                    surroundscore.score = maxScore;
                    score.Add(GameManager.OccupiedGround[i].PlayerBlood, surroundscore);
                    AimRangeList.Remove(node);
                }
            }
        }
        GameObject possibleEnemy = null;
        maxScore = -10;
        if (PlayerBloodSum < EnemyBloodSum - 4)
        {
            maxScore = 10000;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if ((!GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[i].PlayerOnGround.tag)) && !GameManager.OccupiedGround[i].Moved)
                {
                    if (score[GameManager.OccupiedGround[i].PlayerBlood].score >= maxScore)
                        continue;
                    maxScore = score[GameManager.OccupiedGround[i].PlayerBlood].score;
                    GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
                    possibleEnemy = score[GameManager.OccupiedGround[i].PlayerBlood].Enemy;
                    if (BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j] == score[GameManager.OccupiedGround[i].PlayerBlood].Aim)
                    {
                        GroundToMove = null;
                    }
                    else
                        GroundToMove = score[GameManager.OccupiedGround[i].PlayerBlood].Aim;
                }
            }
        }
        else
        {
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if ((!GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[i].PlayerOnGround.tag)) && !GameManager.OccupiedGround[i].Moved)
                {
                    if (score[GameManager.OccupiedGround[i].PlayerBlood].score <= maxScore)
                        continue;
                    maxScore = score[GameManager.OccupiedGround[i].PlayerBlood].score;
                    GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
                    possibleEnemy = score[GameManager.OccupiedGround[i].PlayerBlood].Enemy;
                    if (BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j] == score[GameManager.OccupiedGround[i].PlayerBlood].Aim)
                    {
                        GroundToMove = null;
                    }
                    else
                        GroundToMove = score[GameManager.OccupiedGround[i].PlayerBlood].Aim;
                }
            }
        }
        if (GroundToMove == null)
        {
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
                if (!GameManager.OccupiedGround[i].Moved && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
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
        //对接移动函数，可以不用看了
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
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (GameManager.OccupiedGround[i].PlayerOnGround.transform == GameManager.PlayerOnEdit.transform)
            {
                if (!score.ContainsKey(GameManager.OccupiedGround[i].PlayerBlood))
                {
                    SurroundScore surroundscore = new SurroundScore();
                    surroundscore.Aim = GroundToMove;
                    surroundscore.score = maxScore;
                    surroundscore.Enemy = possibleEnemy;
                    score.Add(GameManager.OccupiedGround[i].PlayerBlood, surroundscore);
                }
            }
        }
    }

    void AIAttack()
    {
        GameObject PlayerToAttack = null;
        if (GameManager.instance.AttackMode == 1)
        {
            //遍历所有可攻击对象，攻击第一个可攻击的对象
            for (int i = 0; i < AimRangeList.Count; i++)
            {
                if (AimRangeList[i].Aim.tag == GameManager.PlayerOnEdit.tag)
                    continue;
                PlayerToAttack = AimRangeList[i].Aim;
                break;
            }
            if (PlayerToAttack == null)
                PlayerToAttack = AimRangeList[0].Aim;
        }
        else
        {
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    PlayerToAttack = score[GameManager.OccupiedGround[i].PlayerBlood].Enemy;
                    break;
                }
            if (PlayerToAttack == null)
            {
                foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
                {
                    if (t.name == "Players")
                        continue;
                    if (t.gameObject.tag == "Monster")
                    {
                        PlayerToAttack = t.gameObject;
                        break;
                    }
                }

            }
        }


        //对接攻击函数，可以不用看了
        //获取反击攻击力，反击范围与双方血条
        GameObject thisBlood = null;
        string aimWeapon = "";
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (GameManager.OccupiedGround[i].PlayerOnGround == PlayerToAttack)
            {
                Blood = GameManager.OccupiedGround[i].PlayerBlood;
                aimWeapon = GameManager.OccupiedGround[i].PlayerWeapon;
            }
            if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
            {
                thisBlood = GameManager.OccupiedGround[i].PlayerBlood;
            }
        }
        if (PlayerToAttack.tag == "Monster")
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
        //对接攻击函数

        foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
        {
            if (t.name == "Players")
                continue;
            if (Vector3.Distance(PlayerToAttack.transform.position, t.position) < BoardManager.distance / 2)
            {
                if (GameManager.instance.AttackMode == 1)
                {
                    if (t.tag == "Monster")
                        t.gameObject.GetComponent<MonsterController>().DragAttack(Blood, thisBlood, attack, aimWeapon);
                    else if (t.tag != GameManager.PlayerOnEdit.tag)
                        t.gameObject.GetComponent<RealPlayer>().DragAttack(Blood, thisBlood, attack, aimWeapon);
                    else
                    {
                        t.gameObject.GetComponent<AI>().DragAttack(Blood, thisBlood, attack, aimWeapon);
                    }
                    break;
                }
                if (GameManager.instance.AttackMode == 0)
                {
                    if (t.tag == "Monster")
                        t.gameObject.GetComponent<MonsterController>().Attack(Blood, thisBlood, PlayerToAttack.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
                    else if (t.tag != GameManager.PlayerOnEdit.tag)
                        t.gameObject.GetComponent<RealPlayer>().Attack(Blood, thisBlood, PlayerToAttack.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
                    else
                    {
                        t.gameObject.GetComponent<AI>().Attack(Blood, thisBlood, PlayerToAttack.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
                    }
                    break;
                }
                if (GameManager.instance.AttackMode == 2)
                {
                    if (t.tag == "Monster")
                        t.gameObject.GetComponent<MonsterController>().ArrowAttack(Blood, thisBlood, PlayerToAttack.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
                    else if (t.tag != GameManager.PlayerOnEdit.tag)
                        t.gameObject.GetComponent<RealPlayer>().ArrowAttack(Blood, thisBlood, PlayerToAttack.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
                    else
                    {
                        t.gameObject.GetComponent<AI>().ArrowAttack(Blood, thisBlood, PlayerToAttack.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
                    }
                    break;
                }
            }
        }

    }



    IEnumerator WaitToAttack()
    {
        GameManager.instance.CoroutineStarted = true;
        if (AimRangeList.Count == 0)
        {
            StopCoroutine(WaitToAttack());
        }
        yield return new WaitForSeconds(1);
        AIAttack();
        GameManager.instance.CoroutineStarted = false;
    }
    IEnumerator WaitToMove()
    {
        GameManager.instance.CoroutineStarted = true;
        yield return new WaitForSeconds(3);
        AIMove();
        GameManager.instance.CoroutineStarted = false;
    }
}
