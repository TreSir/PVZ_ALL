using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class BasePanel : MonoBehaviour
{
    public bool isRemove = false;
    public abstract string UIName {get;}//这个UI面板的文件名
                           //继承的时候只需要加入各种UI组件的引用，比如Button、Text等等

    /// <summary>
    //继承的时候通常会获取一下名字
    //继承的时候还需要把这个UI面板具体的功能实现注册好
    //比如注册按钮的点击事件等等
    /// </summary>
    public abstract void InitUI();
    protected virtual void Awake()
    {
        InitUI();  
    }
    public virtual void OpenPanel()
    {
        SetActive(true);
    }
    public virtual void ClosePanel() 
    { 
        isRemove = true;
        SetActive(false);
        Destroy(gameObject);
        if (BaseUIManager.Instance.panelDict.ContainsKey(UIName))
        {
            BaseUIManager.Instance.panelDict.Remove(UIName);
        }
    }
    public virtual void SetActive(bool _bool)
    {
        gameObject.SetActive(_bool);
    }
}
