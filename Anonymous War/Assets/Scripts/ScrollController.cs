using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour
{
    public GameObject hint;
    public Scrollbar m_Scrollbar;
    public ScrollRect m_ScrollRect;

    private float mTargetValue;

    private bool mNeedMove = false;

    private const float MOVE_SPEED = 1F;

    private const float SMOOTH_TIME = 0.2F;

    private float mMoveSpeed = 0f;
    public int total = 5;
    Vector3 pointerPosition;
    float CurrentValue;
    public Image ClickedButton;
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        ClickedButton.color = new Color(0,340,20);
    }
    public void OnPointerDown()
    {
        mNeedMove = false;
        pointerPosition = Input.mousePosition;
    }

    public void OnPointerUp()
    {
        if(Input.mousePosition.x>pointerPosition.x+50&&CurrentValue>=1/(float)(total-1)-0.01f)
            mTargetValue = CurrentValue - 1 / (float)(total - 1);
        if(Input.mousePosition.x<pointerPosition.x-50&&CurrentValue<=1-1/(float)(total-1)+0.01f)
            mTargetValue = CurrentValue + 1 / (float)(total - 1);
            /*
        // 判断当前位于哪个区间，设置自动滑动至的位置
        if (m_Scrollbar.value <= 0.125f)
        {
            mTargetValue = 0;
        }
        else if (m_Scrollbar.value <= 0.375f)
        {
            mTargetValue = 0.25f;
        }
        else if (m_Scrollbar.value <= 0.625f)
        {
            mTargetValue = 0.5f;
        }
        else if (m_Scrollbar.value <= 0.875f)
        {
            mTargetValue = 0.75f;
        }
        else
        {
            mTargetValue = 1f;
        }
*/
        mNeedMove = true;
        mMoveSpeed = 0;
    }
    //左滑右滑按钮
    public void ToLeftPage()
    {
        if(CurrentValue>=1/(float)(total-1)-0.01f)
            mTargetValue = CurrentValue - 1 / (float)(total - 1);
        mNeedMove = true;
    }
    public void ToRightPage()
    {
        if(CurrentValue<=1-1/(float)(total-1)+0.01f)
            mTargetValue = CurrentValue + 1 / (float)(total - 1);
        mNeedMove = true;
    }

    public void OnCloseClick()
    {
        hint.SetActive(false);
    }

    public void OnButtonClick(int value)
    {
        mTargetValue = (float)(value-1) / (total-1);
        if(mTargetValue<0||mTargetValue>1)
            Debug.Log("ScrollError");
        mNeedMove = true;
    }

    void Update()
    {
        if (mNeedMove)
        {
            if (Mathf.Abs(m_Scrollbar.value - mTargetValue) < 0.01f)
            {
                m_Scrollbar.value = mTargetValue;
                mNeedMove = false;
                CurrentValue = mTargetValue;
                ClickedButton.color = new Color(255, 255, 255);
                Debug.Log(((int)(m_Scrollbar.value * (total - 1) + 1.1f)).ToString());
                ClickedButton = GameObject.Find("button" + ((int)(m_Scrollbar.value * (total - 1) + 1.1f)).ToString()).GetComponent<Image>();
                ClickedButton.color = new Color(0,340,20);
                return;
            }
            m_Scrollbar.value = Mathf.SmoothDamp(m_Scrollbar.value, mTargetValue, ref mMoveSpeed, SMOOTH_TIME);
        }
    }

}
