using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    //几种玩家状态，临时替代一种game object多状态
    public GameObject LongGround;
    public GameObject ShortGround;
    public GameObject DragGround;
    public GameObject NothingGround;
    //用于生成地图
    List<GameObject> RandomGround = new List<GameObject>();
    public static float distance;//格子间距离
    // Start is called before the first frame update
    void Start()
    {
        distance=1.44f;
        RandomGround.Add(LongGround);
        RandomGround.Add(ShortGround);
        RandomGround.Add(DragGround);
        RandomGround.Add(NothingGround);
        SetBoard();
    }

    void SetBoard()//生成地图
    {
        int row = 7;
        int col = 7;
        int random;
        Vector3 position;
        int[][] randomlist=new int[7][];
        for (int i = 0; i < row; i++)
        {
            randomlist[i] = new int[7];
            for (int j = 0; j < col; j++)
            {
                randomlist[i][j] = 3;
            }
        }
        randomlist[1][1] = randomlist[3][0] = randomlist[3][3] = randomlist[4][3] = randomlist[3][6] = 0;
        randomlist[1][2] = randomlist[4][2] = randomlist[0][3] = randomlist[5][3] = randomlist[4][4] = randomlist[0][4] = 1;
        randomlist[3][2] = randomlist[5][4] = randomlist[2][5] = 2;
        //固定地图
        for (int i = 0; i < row; i++)
        {

            for (int j = 0; j < col; j++)
            {
                //控制形状
                switch (j)
                {
                    case 0: if (i == 0 || i > 4) continue; break;
                    case 1: if (i == 0 || i > 5) continue; break;
                    case 2: if (i > 5) continue; break;
                    case 3: if (i > 6) continue; break;
                    case 4: if (i > 5) continue; break;
                    case 6: if (i == 0 || i > 4) continue; break;
                    case 5: if (i == 0 || i > 5) continue; break;
                }
                //随机地图
                //random = Random.Range(0, 3);
                //固定地图
                random = randomlist[i][j];
                position = new Vector3(distance * i - 3,1.732f * 0.5f * distance * j - 4, 0);
                if (j % 2 != 0)
                    position.x -= distance / 2;
                Instantiate(RandomGround[random], position, Quaternion.identity, GameObject.Find("Grounds").transform);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
