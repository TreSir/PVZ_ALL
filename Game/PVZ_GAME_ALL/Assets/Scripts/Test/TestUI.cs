using UnityEngine;

public class TestUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("TestUI001")]
    public void OPenMainPanel()
    {
        BaseUIManager.Instance.OpenPanel("MainMenuPanel");
    }
}
