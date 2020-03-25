using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionArtifact
{
    public Vector3 artPosition;
    public Vector3 artGroundPosition;
    public virtual void OnArtCreate()
    {

    }

    public virtual void ArtPower()
    {

    }

    public virtual void ArtOnHit()
    {
        
    }

    public virtual void ArtPerActionPower()
    {

    }

    //same as playercontroller.onhitation
    public IEnumerator OnHitAction(Vector3 attackerposotion,GameObject aim)
    {
        float singleFlameMovement=0.1f;
        if(attackerposotion.x>aim.transform.position.x)
            singleFlameMovement = -0.1f;
        Vector3 nowPosition = aim.transform.position;
        Vector3 OrigPosition = nowPosition;
        for (int i = 0; i < 10;i++)
        {
            if(aim==null)
                break;
            nowPosition.x += singleFlameMovement;
            aim.transform.position = nowPosition;
            yield return 0;
        }
        for (int i = 0; i < 10;i++)
        {
            if(aim==null)
                break;
            nowPosition.y -= singleFlameMovement;
            aim.transform.position = nowPosition;
            yield return 0;
        }
        if(aim!=null)
            aim.transform.position = OrigPosition;
    }

    public GameObject FindMaxHate()
    {
        //int max = 0;
        GameObject MaxHatePlayer = null;
        for (int i = 0; i < GameManager.OccupiedGround.Count; i++)
        {
            //if (GameManager.OccupiedGround[i].Hate > max)
            {
                MaxHatePlayer = GameManager.OccupiedGround[i].PlayerOnGround;
                //max = GameManager.OccupiedGround[i].Hate;
            }
        }
        return MaxHatePlayer;
    }

}
