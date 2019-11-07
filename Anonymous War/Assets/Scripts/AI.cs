using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AI : PlayerController
{
    public struct SurroundScore
    {
        public GameObject MaxScoreAim;
        public GameObject MaxScoreEnemy;
        public int MaxScore;
        public GameObject AverageScoreEnemy;
        public GameObject AveScoreAim;
    }
    //分数列表
    public static Dictionary<GameObject, SurroundScore> score;
    //防止协程多次启动

    // Update is called once per frame
    void Update()
    {
        if(GameManager.instance.SmoothMoveOnWay)
            return;
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
        if(GameManager.instance.SmoothMoveOnWay)
            return;
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

    void AIMove()
    {
        //要移动到的地块
        GameObject GroundToMove = null;
        int maxScore = 0, thisScore = 0;
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
                surroundscore.AveScoreAim = null;
                surroundscore.AverageScoreEnemy = null;
                if (GameManager.OccupiedGround[i].Moved)
                {
                    surroundscore.MaxScore = -10;
                    surroundscore.MaxScoreAim = null;
                    surroundscore.MaxScoreEnemy = null;
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
                    int enemyScore = 0, enemyMaxScore = 0;
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
                                                        surroundscore.AverageScoreEnemy=GameManager.OccupiedGround[j].PlayerOnGround;
                                                        surroundscore.AveScoreAim = Node.Aim;
                                                        break;
                                                    case "Long":
                                                        enemyScore *= 3;
                                                        break;
                                                    case "Drag":
                                                        enemyScore *= 4;
                                                        break;
                                                }
                                                if (GameManager.OccupiedGround[j].Faint)
                                                    enemyScore *= 2;

                                            }
                                            enemyScore -= (int)(Vector3.Distance(GameManager.OccupiedGround[j].PlayerOnGround.transform.position, Node.Aim.transform.position) / BoardManager.distance);
                                            if (enemyScore > enemyMaxScore)
                                            {
                                                enemyMaxScore = enemyScore;
                                                surroundscore.MaxScoreEnemy = GameManager.OccupiedGround[j].PlayerOnGround;
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
                                                    enemyScore = 0;
                                                else
                                                {
                                                    enemyScore = 40 - int.Parse(GameManager.OccupiedGround[j].PlayerBlood.GetComponent<Text>().text);
                                                    switch (GameManager.OccupiedGround[j].PlayerWeapon)
                                                    {
                                                        case "Short":
                                                            enemyScore *= 3;
                                                            break;
                                                        case "Long":
                                                            surroundscore.AverageScoreEnemy=GameManager.OccupiedGround[j].PlayerOnGround;
                                                            surroundscore.AveScoreAim = Node.Aim;
                                                            break;
                                                        case "Drag":
                                                            enemyScore *= 2;
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
                                                surroundscore.MaxScoreEnemy = GameManager.OccupiedGround[j].PlayerOnGround;
                                            }
                                        }
                                    }
                                    thisScore = 10 + enemyMaxScore;
                                    break;
                                case "Drag":
                                    thisScore = 1;
                                    break;
                            }
                        }
                        if (GameManager.instance.ArtifactGround != null && Vector3.Distance(Node.Aim.transform.position,GameManager.instance.ArtifactGround.transform.position)<BoardManager.distance*1.5)
                        {
                            int EnemyCanHit=0, FriendlyCanHit=0;
                            for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                            {
                                if (Vector3.Angle(GameManager.OccupiedGround[i].PlayerOnGround.transform.position - Node.Aim.transform.position, Node.Aim.transform.position - BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j].transform.position) < 1)
                                {
                                    if(GameManager.OccupiedGround[j].PlayerOnGround.tag==GameManager.OccupiedGround[i].PlayerOnGround.tag)
                                        FriendlyCanHit++;
                                    else
                                    {
                                        EnemyCanHit++;
                                    }
                                }
                            }
                            enemyScore =(EnemyCanHit - FriendlyCanHit) * 50;
                            if (enemyScore > enemyMaxScore)
                            {
                                enemyMaxScore = enemyScore;
                                foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
                                {
                                    if (t.name == "Players")
                                        continue;
                                    if (Vector3.Distance(GameManager.instance.ArtifactGround.transform.position, t.gameObject.transform.position) < BoardManager.distance / 2)
                                    {
                                        surroundscore.MaxScoreEnemy = t.gameObject;
                                        break;
                                    }
                                }
                            }
                            thisScore = enemyScore;

                        }
                        if(surroundscore.MaxScoreEnemy==null&&surroundscore.AveScoreAim==null)
                        {
                            surroundscore.AveScoreAim = Node.Aim;
                        }
                        if (thisScore > maxScore)
                        {
                            surroundscore.MaxScoreAim = Node.Aim;
                            maxScore = thisScore;
                        }
                    }
                    surroundscore.MaxScore = maxScore;
                    score.Add(GameManager.OccupiedGround[i].PlayerBlood, surroundscore);
                    AimRangeList.Remove(node);
                }
            }
        }
        GameObject possibleEnemy = null;
        maxScore = -10;
        if (PlayerBloodSum < EnemyBloodSum - 4)
        {
            Debug.Log("Act");
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if ((!GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[i].PlayerOnGround.tag)) && !GameManager.OccupiedGround[i].Moved)
                {
                    if (score[GameManager.OccupiedGround[i].PlayerBlood].AveScoreAim == null)
                    {
                        if(GameManager.PlayerOnEdit==null)
                            GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
                        continue;
                    }
                    GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
                    possibleEnemy = score[GameManager.OccupiedGround[i].PlayerBlood].AverageScoreEnemy;
                    if (BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j] == score[GameManager.OccupiedGround[i].PlayerBlood].AveScoreAim)
                    {
                        GroundToMove = null;
                    }
                    else
                        GroundToMove = score[GameManager.OccupiedGround[i].PlayerBlood].AveScoreAim;
                }
            }
        }
        if(PlayerBloodSum >= EnemyBloodSum - 4||(GroundToMove==null&&possibleEnemy==null))
        {
            Debug.Log("Normal");
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if ((!GameManager.RealPlayerTeam.Contains(GameManager.OccupiedGround[i].PlayerOnGround.tag)) && !GameManager.OccupiedGround[i].Moved)
                {
                    if (score[GameManager.OccupiedGround[i].PlayerBlood].MaxScore <= maxScore)
                        continue;
                    maxScore = score[GameManager.OccupiedGround[i].PlayerBlood].MaxScore;
                    GameManager.PlayerOnEdit = GameManager.OccupiedGround[i].PlayerOnGround;
                    possibleEnemy = score[GameManager.OccupiedGround[i].PlayerBlood].MaxScoreEnemy;
                    if (BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j] == score[GameManager.OccupiedGround[i].PlayerBlood].MaxScoreAim)
                    {
                        GroundToMove = null;
                    }
                    else
                        GroundToMove = score[GameManager.OccupiedGround[i].PlayerBlood].MaxScoreAim;
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
        score = new Dictionary<GameObject, SurroundScore>();
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            if (GameManager.OccupiedGround[i].PlayerOnGround.transform == GameManager.PlayerOnEdit.transform)
            {
                SurroundScore surroundscore = new SurroundScore();
                surroundscore.MaxScoreAim = GroundToMove;
                surroundscore.MaxScore = maxScore;
                surroundscore.MaxScoreEnemy = possibleEnemy;
                score.Add(GameManager.OccupiedGround[i].PlayerBlood, surroundscore);
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
                    PlayerToAttack = score[GameManager.OccupiedGround[i].PlayerBlood].MaxScoreEnemy;
                    break;
                }
            /* 
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

            }*/
        }
        bool canAttack = false;
        for (int i = 0; i < AimRangeList.Count; i++)
        {
            if (AimRangeList[i].Aim == PlayerToAttack)
            {
                canAttack = true;
                break;
            }
        }
        if(!canAttack)
        {
            PlayerToAttack = AimRangeList[0].Aim;
        }
        //对接攻击函数，可以不用看了
        //获取反击攻击力，反击范围与双方血条
        //PlayerToAttack.transform.localScale *= 1.1f;
        if (PlayerToAttack.tag == "Monster")
        {
            ArtifactController.instance.OnMouseDown();
        }
        else
        {
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
            //对接攻击函数
            foreach (Transform t in GameObject.Find("Players").GetComponentsInChildren<Transform>())
            {
                if (t.name == "Players")
                    continue;
                if (Vector3.Distance(PlayerToAttack.transform.position, t.position) < BoardManager.distance / 2)
                {
                    if (GameManager.instance.AttackMode == 1)
                    {
                        if (t.tag != GameManager.PlayerOnEdit.tag)
                            t.gameObject.GetComponent<RealPlayer>().DragAttack(Blood, thisBlood, attack, aimWeapon);
                        else
                        {
                            t.gameObject.GetComponent<AI>().DragAttack(Blood, thisBlood, attack, aimWeapon);
                        }
                        break;
                    }
                    if (GameManager.instance.AttackMode == 0)
                    {
                        if (t.tag != GameManager.PlayerOnEdit.tag)
                            t.gameObject.GetComponent<RealPlayer>().Attack(Blood, thisBlood, PlayerToAttack.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
                        else
                        {
                            t.gameObject.GetComponent<AI>().Attack(Blood, thisBlood, PlayerToAttack.transform.position, GameManager.PlayerOnEdit.transform.position, attack, aimWeapon);
                        }
                        break;
                    }
                    if (GameManager.instance.AttackMode == 2)
                    {
                        if (t.tag != GameManager.PlayerOnEdit.tag)
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
    }



    IEnumerator WaitToAttack()
    {
        GameManager.instance.CoroutineStarted = true;
        if (AimRangeList.Count == 0)
        {
            StopCoroutine(WaitToAttack());
        }
        int second = Random.Range(2, 4);
        if(GameManager.IsTraining)
            second = 1;
        yield return new WaitForSeconds(second);
        AIAttack();
        GameManager.instance.CoroutineStarted = false;
    }
    IEnumerator WaitToMove()
    {
        GameManager.instance.CoroutineStarted = true;
        int second = Random.Range(3, 5);
        if(GameManager.IsTraining)
            second = 2;
        yield return new WaitForSeconds(second);
        AIMove();
        GameManager.instance.CoroutineStarted = false;
    }
}
