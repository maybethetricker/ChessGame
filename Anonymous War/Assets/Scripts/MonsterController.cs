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
}
