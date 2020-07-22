using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RelaxModeUI : MonoBehaviour
{
    public Button StartGame;
    public Button OpenHint;
    public Button Back;
    public Button ChangeMode;
    public GameObject hint;
    public GameObject ButtonHolder;
    public Scrollbar bar;
    int Mode;
    // Start is called before the first frame update
    void Start()
    {
        StartGame.onClick.RemoveAllListeners();
        StartGame.onClick.AddListener(StartExtraMode);
        OpenHint.onClick.AddListener(openHint);
        Back.onClick.AddListener(delegate { StartCoroutine(DynamicButtonCreate(new Vector3(-284,0,0),120,true)); });
        ChangeMode.onClick.AddListener(changeMode);
        Root.instance.flowchart.SetBooleanVariable("Started", false);
        Root.instance.flowchart.SetBooleanVariable("Finnished", false);
        if(int.Parse(System.DateTime.Now.ToString("yyyy:MM:dd").Split(new char[]{':'})[2])%2==0)
            Mode = 9;
        else
            Mode = 8;
        openHint();
        StartCoroutine(DynamicButtonCreate(new Vector3(0,0,0),100,false));
    }
    void openHint()
    {
       
        hint.SetActive(true);
         if (Mode == 8)
            bar.value = 0;
        else
            bar.value = 0.25f;
        Debug.Log(bar.value);
    }
    void StartExtraMode()
    {
        Root.instance.ShowNotice("匹配中", "取消", delegate ()
        {
            ProtocolBytes protocol2 = new ProtocolBytes();
            protocol2.AddString("StopMatch");
            NetMgr.srvConn.Send(protocol2);
        });
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartMatch");
        protocol.AddInt(Mode);
        NetMgr.srvConn.Send(protocol);
    }
    void changeMode()
    {
        if(Mode==9)
            Mode = 8;
        else
            Mode = 9;
        openHint();
    }
    IEnumerator DynamicButtonCreate(Vector3 Aim,float Speed,bool OpenHint)
    {
        while (Vector3.Distance(ButtonHolder.transform.position,Aim)>0.1f)
        {
            ButtonHolder.transform.position = Vector3.MoveTowards(ButtonHolder.transform.position, Aim, Speed * Time.deltaTime);
            yield return 0;
        }
        ButtonHolder.transform.position = Aim;
        if(OpenHint)
            SceneManager.LoadScene("MainPage");
    }
}
