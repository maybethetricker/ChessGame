using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    //几种玩家状态，临时替代一种game object多状态
    public GameObject LongGround;
    public GameObject ShortGround;
    public GameObject DragGround;
    public GameObject NothingGround;
    public GameObject AxGround;
    public GameObject ShieldGround;
    public GameObject BumbGround;
    List<string> weaponList;
    //用于生成地图
    List<GameObject> setBoardRandomGround = new List<GameObject>();
    public static float distance;//格子间距离
    public static GameObject[][] Grounds;
    public static int row;
    public static int col;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
        distance = 21.6f;
        setBoardRandomGround.Add(LongGround);
        setBoardRandomGround.Add(ShortGround);
        setBoardRandomGround.Add(DragGround);
        setBoardRandomGround.Add(NothingGround);
        setBoardRandomGround.Add(ShieldGround);
        setBoardRandomGround.Add(AxGround);
        setBoardRandomGround.Add(BumbGround);
        int weaponkind=0;
        if (GameManager.Mode < 2)
            {
                weaponkind = 5;
            }
            else if(GameManager.Mode<3)
            {
                weaponkind = 6;
            }
            else
            {
                weaponkind = 7;
            }
        if (GameManager.UseAI || GameManager.RealPlayerTeam.Contains("Team1"))
        {
            if (GameManager.Mode < 2)
            {
                weaponkind = 5;
                SetBoard1(weaponkind, 15, 13);
            }
            else if(GameManager.Mode<3)
            {
                weaponkind = 6;
                SetBoard1(weaponkind, 19, 17);
            }
            else if(GameManager.Mode==8)
            {
                weaponkind = 7;
                SetBoard1(weaponkind, 30, 27);
            }
            else if(GameManager.Mode==9)
            {
                SetBoard1(0, 0, 0);
            }
            else
            {
                weaponkind = 7;
                SetBoard1(weaponkind, 23, 20);
            }
        }
        weaponList = new List<string>();
        for (int i = 0; i < weaponkind;i++)
            weaponList.Add(setBoardRandomGround[i].tag);
        foreach (Transform t in GameObject.Find("PlayerWeaponCard").GetComponentInChildren<Transform>())
        {
            if (!weaponList.Contains(t.tag) || GameManager.Mode==9)
                t.gameObject.SetActive(false);
        }
    }

    public virtual void SetBoard1(int weaponKind,int weaponNumMax,int weaponNumMin)//生成地图
    {
        row = 7;
        col = 7;
        int[][] randomlist = new int[row][];
        Grounds = new GameObject[row][];
        for (int i = 0; i < row; i++)
        {
            Grounds[i] = new GameObject[col];
            randomlist[i] = new int[col];
            for (int j = 0; j < col; j++)
            {
                randomlist[i][j] = 3;
            }
        }
        if (GameManager.Guide >= 1)
        {
            randomlist[2][1] = randomlist[5][0] = randomlist[3][3] = randomlist[4][3] = randomlist[2][6] = 0;
            randomlist[2][2] = randomlist[5][2] = randomlist[0][3] = randomlist[5][3] = randomlist[4][4] = randomlist[0][4] = 1;
            randomlist[4][2] = randomlist[5][4] = randomlist[1][5] = 2;
            randomlist[4][0] = randomlist[3][1] = 4;
        }
        else
        {
            int k = 0;
            List<int> eachWeaponNum = new List<int>();//Guarentee that each kind of weapon has more than 2
            for (int i = 0; i < weaponKind; i++)
            {
                eachWeaponNum.Add(2);
            }
            int dragCount = 0;
            int WeaponNum = Random.Range(weaponNumMin, weaponNumMax + 1) - 2 * weaponKind;
            Debug.Log(WeaponNum);
            while (k < WeaponNum)
            {
                int i = Random.Range(0, 7);
                int j = Random.Range(0, 7);
                if(randomlist[i][j]!=3)
                    continue;
                switch (j)
                {
                    case 0: if (i < 3 || i > 6) continue; break;
                    case 1: if (i < 2 || i > 6) continue; break;
                    case 2: if (i == 0 || i > 6) continue; break;
                    case 3: if (i > 6) continue; break;
                    case 4: if (i > 5) continue; break;
                    case 5: if (i > 4) continue; break;
                    case 6: if (i > 3) continue; break;
                }
                int rand = Random.Range(0,weaponKind);
                while(rand==3)
                    rand=Random.Range(0, weaponKind);
                if (eachWeaponNum[rand] > 0)
                {
                    eachWeaponNum[rand] = eachWeaponNum[rand] - 1;
                    WeaponNum++;
                }
                while (dragCount > 3 && rand == 2)
                    rand = Random.Range(0, weaponKind);
                if (rand == 2)
                    dragCount++;
                randomlist[i][j] = rand;
                k++;
            }
            Debug.Log("DragCount" + dragCount);
            for (int i = 0; i < weaponKind; i++)
            {
                if(i==3)
                    continue;
                while(eachWeaponNum[i]>0)
                {
                    int randx = Random.Range(0, 7);
                    int randy = Random.Range(0, 7);
                    if(randomlist[randx][randy]!=3)
                        continue;
                    switch (randy)
                    {
                        case 0: if (randx < 3 || randx > 6) continue; break;
                        case 1: if (randx < 2 || randx > 6) continue; break;
                        case 2: if (randx == 0 || randx > 6) continue; break;
                        case 3: if (randx > 6) continue; break;
                        case 4: if (randx > 5) continue; break;
                        case 5: if (randx > 4) continue; break;
                        case 6: if (randx > 3) continue; break;
                    }
                    randomlist[randx][randy] = i;
                    eachWeaponNum[i]--;
                }
            }
            if (!GameManager.UseAI)
            {
                ProtocolBytes protocol = new ProtocolBytes();
                protocol.AddString("SetBoard");
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        protocol.AddInt(randomlist[i][j]);
                    }
                }
                //StartCoroutine(WaitToSent(protocol));
                NetMgr.srvConn.Send(protocol);
            }
            //固定地图
        }
        InstantiateBoard(randomlist);
    }
    void SetBoard2()
    {
        
    }
    IEnumerator WaitToSent(ProtocolBytes protocol)
    {
        for (int i = 0; i < 100;i++)
            yield return 0;
        NetMgr.srvConn.Send(protocol);
    }
    public void InstantiateBoard(int[][] randomlist)
    {
        for (int i = 0; i < row; i++)
        {

            for (int j = 0; j < col; j++)
            {
                Grounds[i][j] = null;
                //控制形状
                switch (j)
                {
                    case 0: if (i < 3 || i > 6) continue; break;
                    case 1: if (i < 2 || i > 6) continue; break;
                    case 2: if (i == 0 || i > 6) continue; break;
                    case 3: if (i > 6) continue; break;
                    case 4: if (i > 5) continue; break;
                    case 5: if (i > 4) continue; break;
                    case 6: if (i > 3) continue; break;
                }
                int random = randomlist[i][j];
                Vector3 position = new Vector3(distance * i - 3 - 55, 1.732f * 0.5f * distance * j - 3 + 56, 78);
                position.x += (j - 3) * distance / 2;
                Grounds[i][j] = Instantiate(setBoardRandomGround[random], position, Quaternion.identity, GameObject.Find("Grounds").transform);
                Grounds[i][j].GetComponent<SpriteRenderer>().color = new Color(255,255,255);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
