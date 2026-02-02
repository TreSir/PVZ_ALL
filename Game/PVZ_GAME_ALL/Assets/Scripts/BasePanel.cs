using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour
{
    public bool isRemove = false;
    public new string name;//这个UI面板的文件名
    //继承的时候只需要加入各种UI组件的引用，比如Button、Text等等
    protected virtual void Awake()
    {
        //继承的时候通常会获取一下名字
        //继承的时候还需要把这个UI面板具体的功能实现注册好
        //比如注册按钮的点击事件等等
    }
    public virtual void OpenPanel(string name)
    {
        this.name = name;
        SetActive(true);
    }
    public virtual void ClosePanel() 
    { 
        isRemove = true;
        SetActive(false);
        Destroy(gameObject);
        if (BaseUIManager.Instance.panelDict.ContainsKey(name))
        {
            BaseUIManager.Instance.panelDict.Remove(name);
        }
    }
    public virtual void SetActive(bool _bool)
    {
        gameObject.SetActive(_bool);
    }
}
