using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUIManager
{
    private static BaseUIManager _instance;
    public static BaseUIManager Instance
    {
        get 
        { 
            if (_instance == null)
            {
                _instance = new BaseUIManager();
            }
            return _instance; 
        }
    }

    private Dictionary<string, string> pathDict;
    private Dictionary<string, GameObject> prefabDict;
    public Dictionary<string, BasePanel> panelDict;
    private Transform _uiRoot;
    public Transform UIRoot
    {
        get
        {
            if(_uiRoot == null)
            {
                if (GameObject.Find("Canvas")) 
                {
                    _uiRoot = GameObject.Find("Canvas").transform;
                }
                else
                {
                    _uiRoot = new GameObject("Canvas").transform;
                }
            }
            return _uiRoot;
        }
    }
    public int sortingOrder = 1;

    private BaseUIManager()
    {
        InitDics();
    }
    private void InitDics()
    {
        prefabDict = new Dictionary<string, GameObject>();
        panelDict = new Dictionary<string, BasePanel>();
        //这个字典里面需要提前写好所有的panel prefab的路径
        pathDict = new Dictionary<string, string>()
        {
            //格式“{UIConst名字,panelPrefab路径}”,如下
            //{UIConst.MainMenuPanel, "Resource/Prefabs/MainMenuPanel"}
        };
    }


/// <summary>
/// 打开一个panel，这个方法只管用就行了
/// </summary>
/// <param name="panelName"></param>
/// <returns></returns>
    public BasePanel OpenPanel(string panelName)
    {
        BasePanel panel = null;
        if(panelDict.TryGetValue(panelName, out panel))
        {
                Debug.Log("已打开");
                return null;
        }
        string path = null;
        if(!pathDict.TryGetValue(panelName, out path))
        {
            Debug.Log("不存在该panel path");
            return null;
        }
        GameObject prefab = null;
        if(!prefabDict.TryGetValue(panelName,out prefab))
        {
            prefab = Resources.Load<GameObject>("Prefab/" + path);
            prefabDict[panelName] = prefab;
        }
        GameObject panelObject = GameObject.Instantiate(prefab, UIRoot, false);
        panel = panelObject.GetComponent<BasePanel>();
        panelDict[panelName] = panel;
        
        panel.OpenPanel(panelName);
        Canvas panelCavas = panelObject.GetComponent<Canvas>();
        panelCavas.sortingOrder = sortingOrder;
        sortingOrder += 1; 
        return panel;
    }
    /// <summary>
    /// 可以关闭一个panel，这个方法只管用就行了
    /// 如果想移除panel并注销，就传true
    /// 如果只是隐藏panel，或者这个panel物体已经没了但是没被注销，就传false
    /// </summary>
    /// <param name="panelName"></param>
    /// <param name="isRemove"></param>
    /// <returns></returns>
    public bool ClosePanel(string panelName, bool wantRemove = true)
    {
        BasePanel panel = null;
        if(!panelDict.TryGetValue(panelName,out panel))
        {
            Debug.Log("未打开");
            return false;
        }
        if (wantRemove)
        {
            panel.ClosePanel();
        }
        panelDict.Remove(panelName);
        return true;
    }
    /// <summary>
    /// 关闭所有panel，只管用就行了，换场景的时候直接用
    /// </summary>
    /// <param name="wantRemove"></param>
    public void CloseAllPanel( bool wantRemove = true)
    {
        List<string> panelNames = new List<string>(panelDict.Keys);
        foreach(var panelName in panelNames)
        {
            ClosePanel(panelName, wantRemove);
        }
    }

}
public class UIConst
{
    //这个里面需要提前配置好所有的panel的名字，如下：
    //public const string MainMenuPanel = "MainMenuPanel";
}
