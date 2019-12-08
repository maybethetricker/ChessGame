﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ArtifactController : PlayerController
{
    public MotionArtifact Artifact;
    
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
        ChooseArtifact();
        Artifact.artPosition = gameObject.transform.position;
        aiAbleToUse = true;
        Artifact.OnArtCreate();
    }
    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        CheckAttack();
        //扩毒
        if (GameManager.instance.Turn > 2 && !GameManager.ArtActFinished)
        {
            Artifact.ArtPower();
            //SetMug((GameManager.Turn) / 2);
        }
    }
    void ChooseArtifact()
    {
        Dictionary<MotionArtifact,Sprite> spriteDic = new Dictionary<MotionArtifact,Sprite>();
        List<MotionArtifact> randomList = new List<MotionArtifact>();
        MotionArtifact artifact = new AngerCrystal();
        MotionArtifact currentPlus = null;//当前最新解锁的物品出现率加倍
        spriteDic.Add(artifact,GameManager.instance.crystal);
        randomList.Add(artifact);
        if(GameManager.Mode>=2)
        {
            MotionArtifact artifact1 = new HopeSpring();
            spriteDic.Add(artifact1,GameManager.instance.spring);
            randomList.Add(artifact1);
        }
        currentPlus = randomList[randomList.Count-1];
        randomList.Add(currentPlus);
        int rand = Random.Range(0, randomList.Count);
        Artifact = randomList[rand];
        gameObject.GetComponent<SpriteRenderer>().sprite = spriteDic[Artifact];
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
