using UnityEngine;
using Audio;
using SourceLoad;

public class TestUI : MonoBehaviour
{
    [SerializeField] private string _testClipPath = "Audio/TEST001";

    void Start()
    {

    }

    void Update()
    {

    }

    [ContextMenu("TestUI001")]
    public void OPenMainPanel()
    {
        BaseUIManager.Instance.OpenPanel("MainMenuPanel");
    }

    [ContextMenu("TestAudio_PlayBGM")]
    public void TestPlayBGM()
    {
        var clip = ResourceManager.Load<AudioClip>(_testClipPath);
        if (clip != null)
        {
            AudioManager.Instance.PlayBGM(clip);
            Debug.Log($"[TestAudio] Playing BGM: {_testClipPath}");
        }
        else
        {
            Debug.LogWarning($"[TestAudio] Clip not found: {_testClipPath}");
        }
    }

    [ContextMenu("TestAudio_StopBGM")]
    public void TestStopBGM()
    {
        AudioManager.Instance.StopBGM();
        Debug.Log("[TestAudio] BGM Stopped");
    }

    [ContextMenu("TestAudio_PlaySFX")]
    public void TestPlaySFX()
    {
        var clip = ResourceManager.Load<AudioClip>(_testClipPath);
        if (clip != null)
        {
            AudioManager.Instance.PlaySFX(clip);
            Debug.Log($"[TestAudio] Playing SFX: {_testClipPath}");
        }
        else
        {
            Debug.LogWarning($"[TestAudio] Clip not found: {_testClipPath}");
        }
    }

    [ContextMenu("TestAudio_SetVolumeHalf")]
    public void TestSetVolumeHalf()
    {
        AudioManager.Instance.SetVolumeSettings(0.5f, 0.5f, 0.5f);
        Debug.Log("[TestAudio] Volume set to 50%");
    }

    [ContextMenu("TestAudio_SetVolumeFull")]
    public void TestSetVolumeFull()
    {
        AudioManager.Instance.SetVolumeSettings(1f, 1f, 1f);
        Debug.Log("[TestAudio] Volume set to 100%");
    }
}
