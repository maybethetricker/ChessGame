using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkipTurn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(SkipOnClick);
    }

    public void SkipOnClick()
    {
        if(!Root.instance.MouseClickLimit(gameObject,Root.instance.LimitClickException,ref Root.instance.UseLimitClick,Root.instance.LimitClickFinished))
            return;
        if (GameManager.PlayerOnEdit != null && GameManager.RealPlayerTeam.Contains(GameManager.PlayerOnEdit.tag)&&GameManager.Stage!=0)
        {
            if ((!GameManager.UseAI) && GameManager.RealPlayerTeam.Count < GameManager.TeamCount)
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
    }
    public void Skip()//跳过移动或攻击阶段
    {
        if(GameManager.RealPlayerTeam.Contains(GameManager.PlayerOnEdit.tag))
            GameManager.PlayerOnEdit.GetComponent<RealPlayer>().ClearHighlight();
        else
        {
            GameManager.PlayerOnEdit.GetComponent<RemoteEnemy>().ClearHighlight();
        }
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
            GameManager.GroundStage GStage = new GameManager.GroundStage();
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                if (GameManager.OccupiedGround[i].PlayerOnGround == GameManager.PlayerOnEdit)
                {
                    GStage = GameManager.OccupiedGround[i];
                    GStage.Moved = true;
                    GameManager.OccupiedGround[i] = GStage;
                    break;
                }
            }
            if (GStage.Ability == 3 && !GameManager.instance.SecondMovingTurn)
            {
                GameManager.instance.SecondMovingTurn = true;
                GameManager.Stage = 1;
                for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                {
                    string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
                    if (GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                    {
                        GStage = GameManager.OccupiedGround[i];
                        if (GStage.Ability == 3)
                        {
                            GStage.Moved = false;
                            GStage.OrigColor = BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color;
                            BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.instance.MovablePlayerHighlight;
                        }
                        else
                        {
                            if (GStage.Ability == 1)
                                GameManager.instance.Ability1Moved = GStage.Moved;
                            else
                                GameManager.instance.Ability2Moved = GStage.Moved;
                            GStage.Moved = true;
                        }
                        GameManager.OccupiedGround[i] = GStage;
                    }
                }
                GameManager.PlayerOnEdit = null;
            }
            else
            {
                if (GStage.Ability == 3)
                {
                    GameManager.instance.SecondMovingTurn = false;
                    for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
                    {
                        string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
                        if (GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                        {
                            GStage = GameManager.OccupiedGround[i];
                            if (GStage.Ability == 1)
                                GStage.Moved = GameManager.instance.Ability1Moved;
                            else if(GStage.Ability==2)
                                GStage.Moved = GameManager.instance.Ability2Moved;
                            GameManager.OccupiedGround[i] = GStage;
                        }
                    }
                }
                GameManager.Stage = 2;
            }
            return;
        }
        //跳过攻击，删除高亮并更替小回合
        if (GameManager.Stage == 2)
        {
            if (GameManager.RealPlayerTeam.Contains(GameManager.PlayerOnEdit.tag))
            {
                GameManager.PlayerOnEdit.GetComponent<RealPlayer>().ClearHighlight();
                GameManager.PlayerOnEdit.GetComponent<RealPlayer>().ChangeTurn();
            }
            else
            {
                GameManager.PlayerOnEdit.GetComponent<RemoteEnemy>().ClearHighlight();
                GameManager.PlayerOnEdit.GetComponent<RemoteEnemy>().ChangeTurn();
            }
            GameManager.instance.EnemyChecked = false;

        }

    }
}
