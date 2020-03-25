using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ArtifactController : PlayerController
{
    public MotionArtifact Artifact;
    public int ArtifactType;
    public static ArtifactController instance;
    public bool aiAbleToUse;
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        if(instance==null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
        if(!GameManager.IsTraining && !GameManager.UseAI && GameManager.RealPlayerTeam.Contains("Team1"))
        {
            GameManager.instance.ArtActFinished = true;
            ArtifactType = 0;
            return;
        }
        ArtifactType=ChooseArtifact();
        if(!GameManager.IsTraining&&!GameManager.UseAI)
        {
            ProtocolBytes protocol=new ProtocolBytes();
            protocol.AddString("ChooseArtifact");
            protocol.AddInt(ArtifactType);
            NetMgr.srvConn.Send(protocol);
        }
        Artifact.artPosition = gameObject.transform.position;
        for (int i = 0; i < BoardManager.row;i++)
            for (int j = 0; j < BoardManager.col;j++)
                if(BoardManager.Grounds[i][j]!=null && Vector3.Distance(Artifact.artPosition,BoardManager.Grounds[i][j].transform.position)<BoardManager.distance/2)
                    Artifact.artGroundPosition = BoardManager.Grounds[i][j].transform.position;
        aiAbleToUse = true;
        Artifact.OnArtCreate();
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
            if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
            {
                BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.instance.MovablePlayerHighlight;
            }
        }
    }
    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        CheckAttack();
        //扩毒
        if (GameManager.instance.Turn > 2 && !GameManager.instance.ArtActFinished)
        {
            Artifact.ArtPower();
            for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
            {
                string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
                if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
                {
                    BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.instance.MovablePlayerHighlight;
                }
            }
            //SetMug((GameManager.Turn) / 2);
        }
        if (GameManager.instance.Turn >= 2 && !GameManager.instance.ArtPerActActFinished)
        {
            Artifact.ArtPerActionPower();
        }
    }
    public void NetArtifactCreate(int kind)
    {
        ArtifactType = kind;
        switch (kind)
        {
            case 1:Artifact=new AngerCrystal();gameObject.GetComponent<SpriteRenderer>().sprite=GameManager.instance.crystal;break;
            case 2:Artifact=new HopeSpring();gameObject.GetComponent<SpriteRenderer>().sprite=GameManager.instance.spring;break;
            case 3:Artifact = new LightTower();gameObject.GetComponent<SpriteRenderer>().sprite = GameManager.instance.tower;break;
            default:break;
        }
        Artifact.artPosition = gameObject.transform.position;
        for (int i = 0; i < BoardManager.row;i++)
            for (int j = 0; j < BoardManager.col;j++)
                if(BoardManager.Grounds[i][j]!=null && Vector3.Distance(Artifact.artPosition,BoardManager.Grounds[i][j].transform.position)<BoardManager.distance/2)
                    Artifact.artGroundPosition = BoardManager.Grounds[i][j].transform.position;
        aiAbleToUse = true;
        Artifact.OnArtCreate();
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            string team = "Team" + (GameManager.instance.MovingTeam + 1).ToString();
            if (GameManager.OccupiedGround[i].Moved == false && GameManager.OccupiedGround[i].PlayerOnGround.tag == team)
            {
                BoardManager.Grounds[GameManager.OccupiedGround[i].i][GameManager.OccupiedGround[i].j].GetComponent<SpriteRenderer>().color = GameManager.instance.MovablePlayerHighlight;
            }
        }
    }
    int ChooseArtifact()
    {
        Dictionary<MotionArtifact,int> spriteDic = new Dictionary<MotionArtifact,int>();
        List<MotionArtifact> randomList = new List<MotionArtifact>();
        MotionArtifact artifact = new AngerCrystal();
        MotionArtifact currentPlus = null;//当前最新解锁的物品出现率加倍
        spriteDic.Add(artifact,1);
        randomList.Add(artifact);
        if(GameManager.Mode>=2)
        {
            MotionArtifact artifact1 = new HopeSpring();
            spriteDic.Add(artifact1,2);
            randomList.Add(artifact1);
        }
        if(GameManager.Mode>=3)
        {
            MotionArtifact artifact2 = new LightTower();
            spriteDic.Add(artifact2, 3);
            randomList.Add(artifact2);
        }
        currentPlus = randomList[randomList.Count-1];
        randomList.Add(currentPlus);
        int rand = Random.Range(0, randomList.Count);
        Artifact = randomList[rand];
        Artifact = randomList[randomList.Count - 1];//for test
        switch (spriteDic[Artifact])
        {
            case 1:gameObject.GetComponent<SpriteRenderer>().sprite=GameManager.instance.crystal;break;
            case 2:gameObject.GetComponent<SpriteRenderer>().sprite=GameManager.instance.spring;break;
            case 3:gameObject.GetComponent<SpriteRenderer>().sprite = GameManager.instance.tower;break;
            default:break;
        }
        return spriteDic[Artifact];
    }


    /// <summary>
    /// OnMouseDown is called when the user has pressed the mouse button while
    /// over the GUIElement or Collider.
    /// </summary>
    public void OnMouseDown()
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
            if (GameManager.RealPlayerTeam.Count < 2 && (!GameManager.UseAI))
            {
                ProtocolBytes protocol = new ProtocolBytes();
                protocol.AddString("UpdateAttack");
                protocol.AddFloat(this.transform.position.x);
                protocol.AddFloat(this.transform.position.y);
                protocol.AddFloat(this.transform.position.z);
                protocol.AddInt(0);
                NetMgr.srvConn.Send(protocol);
            }
            Artifact.ArtOnHit();
            
        }
    }

}
