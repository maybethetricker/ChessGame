using System.Collections;
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
        if(GameManager.Stage==1&&GameManager.PlayerOnEdit.tag=="Team" + (PlayerController.MovingTeam + 1).ToString())
        {
            foreach(KeyValuePair<GameObject,Color> key in PlayerController.CanMoveList)
                key.Key.GetComponent<SpriteRenderer>().color = key.Value;
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                string team = "Team" + (PlayerController.MovingTeam + 1).ToString();
                if (!GameManager.OccupiedGround[i].Moved && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                {
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.OccupiedGround[i].OrigColor;
                }
            }
            for (int i = 0; i < GameManager.OccupiedGround.Count;i++)
            {
                if(GameManager.OccupiedGround[i].PlayerOnGround==GameManager.PlayerOnEdit)
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
            if(!PlayerController.OnlyLine)
                foreach (KeyValuePair<GameObject, Color> key in PlayerController.CanMoveList)
                {
                    if (key.Key.tag == "Monster")
                    {
                        GameManager.instance.TearGround.GetComponent<SpriteRenderer>().color = key.Value;
                        continue;
                    }
                    for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                    {
                        if (GameManager.OccupiedGround[j].PlayerOnGround == key.Key)
                        {
                            BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j].GetComponent<SpriteRenderer>().color = key.Value;
                            break;
                        }
                    }
                }
            else
            {
                foreach (PlayerController.AttackLine line in PlayerController.LineCanAttack)
                {
                    if (line.Enemy == GameManager.PlayerOnEdit||line.Enemy==null)
                        continue;
                    if (line.Enemy.tag == "Monster")
                    {
                        GameManager.instance.TearGround.GetComponent<SpriteRenderer>().color = line.color;
                        continue;
                    }
                    for (int j = 0; j < GameManager.OccupiedGround.Count; j++)
                    {
                        if (GameManager.OccupiedGround[j].PlayerOnGround == line.Enemy)
                        {
                            BoardManager.Grounds[GameManager.OccupiedGround[j].i][GameManager.OccupiedGround[j].j].GetComponent<SpriteRenderer>().color = line.color;
                            break;
                        }
                    }
                }
            }
            PlayerController.OnlyLine = false;
            //同Player controller。Check Turn
            GameManager.Stage = 1;
            PlayerController.SmallTurn++;
            bool teamHaveMove = false;

            if (PlayerController.SmallTurn >= GameManager.TeamCount * 3 - PlayerController.FaintCount-PlayerController.DiedSoldiersTeam1-PlayerController.DIedSoldiersTeam2+PlayerController.MovedDead)
            {
                GameManager.MudSetted = false;
                PlayerController.SmallTurn = 0;
                PlayerController.MovedDead = 0;
                List<GameManager.GroundStage> oGround = new List<GameManager.GroundStage>();
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                {
                    GameManager.GroundStage GStage = GameManager.OccupiedGround[i];
                    GStage.Moved = false;
                    oGround.Add(GStage);
                }
                GameManager.Turn++;
                GameManager.OccupiedGround = oGround;

            }
            int counter = 0;
            while (!teamHaveMove)
            {
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                {
                    string team = "Team" + (PlayerController.MovingTeam + 1).ToString();
                    if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                    {
                        teamHaveMove = true;
                        break;
                    }
                }
                if (!teamHaveMove)
                    PlayerController.MovingTeam = (PlayerController.MovingTeam + 1) % GameManager.TeamCount;
                counter++;
                if (counter > 2 * GameManager.TeamCount)
                {
                    Debug.Log("Bug");
                    break;
                }
            }
            Color color = new Color(255, 255, 0, 0.2f);
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                string team = "Team" + (PlayerController.MovingTeam + 1).ToString();
                if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                {
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = color;
                }
            }
            GameManager.PlayerOnEdit = null;
            PlayerController.EnemyChecked = false;
            PlayerController.CanMoveList = new Dictionary<GameObject, Color>();
        }

    }
    
}
