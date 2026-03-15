using UnityEngine;
using Audio;
using SourceLoad;
using UI;
using EventBus;
using ObjectPool;
using DataManager;

public class TestUI : MonoBehaviour
{
    [SerializeField] private string _testClipPath = "Audio/BGM/B_001";

    #region UI Tests

    [ContextMenu("TestUI_OpenMainPanel")]
    public void TestOpenMainPanel()
    {
        BaseUIManager.Instance.OpenPanel("MainMenuPanel");
        Debug.Log("[TestUI] Opened MainMenuPanel");
    }

    [ContextMenu("TestUI_CloseMainPanel")]
    public void TestCloseMainPanel()
    {
        BaseUIManager.Instance.ClosePanel("MainMenuPanel");
        Debug.Log("[TestUI] Closed MainMenuPanel");
    }

    [ContextMenu("TestUI_CloseAllPanels")]
    public void TestCloseAllPanels()
    {
        BaseUIManager.Instance.CloseAllPanel();
        Debug.Log("[TestUI] Closed all panels");
    }

    #endregion

    #region Audio Tests

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

    [ContextMenu("TestAudio_PauseBGM")]
    public void TestPauseBGM()
    {
        AudioManager.Instance.PauseBGM();
        Debug.Log("[TestAudio] BGM Paused");
    }

    [ContextMenu("TestAudio_ResumeBGM")]
    public void TestResumeBGM()
    {
        AudioManager.Instance.ResumeBGM();
        Debug.Log("[TestAudio] BGM Resumed");
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

    #endregion

    #region EventBus Tests

    [ContextMenu("TestEventBus_Subscribe")]
    public void TestEventBusSubscribe()
    {
        EventBus.EventBus.Subscribe<TestGameEvent>(OnTestGameEvent);
        Debug.Log("[TestEventBus] Subscribed to TestGameEvent");
    }

    [ContextMenu("TestEventBus_Unsubscribe")]
    public void TestEventBusUnsubscribe()
    {
        EventBus.EventBus.Unsubscribe<TestGameEvent>(OnTestGameEvent);
        Debug.Log("[TestEventBus] Unsubscribed from TestGameEvent");
    }

    [ContextMenu("TestEventBus_Publish")]
    public void TestEventBusPublish()
    {
        EventBus.EventBus.Publish(new TestGameEvent
        {
            Message = "Hello from EventBus!",
            Value = 42
        });
        Debug.Log("[TestEventBus] Published TestGameEvent");
    }

    [ContextMenu("TestEventBus_CheckHandler")]
    public void TestEventBusCheckHandler()
    {
        int count = EventBus.EventBus.GetHandlerCount<TestGameEvent>();
        bool hasHandler = EventBus.EventBus.HasHandler<TestGameEvent>();
        Debug.Log($"[TestEventBus] Handler count: {count}, Has handler: {hasHandler}");
    }

    private void OnTestGameEvent(TestGameEvent e)
    {
        Debug.Log($"[TestEventBus] Received event: {e.Message}, Value: {e.Value}");
    }

    #endregion

    #region ObjectPool Tests

    [ContextMenu("TestObjectPool_SpawnEnemy")]
    public void TestObjectPoolSpawn()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Enemy");
        if (prefab != null)
        {
            GameObject enemy = ObjectPoolManager.Instance.Spawn("TestEnemy", prefab, Vector3.zero, Quaternion.identity);
            Debug.Log($"[TestObjectPool] Spawned enemy: {enemy.name}");
        }
        else
        {
            Debug.LogWarning("[TestObjectPool] Enemy prefab not found, using cube instead");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ObjectPoolManager.Instance.GetOrCreatePool("TestCube", cube, PoolConfig.Small);
            Debug.Log("[TestObjectPool] Created test cube pool");
            Destroy(cube);
        }
    }

    [ContextMenu("TestObjectPool_GetPoolInfo")]
    public void TestObjectPoolGetInfo()
    {
        int totalPools = ObjectPoolManager.Instance.TotalPoolCount;
        int activeObjects = ObjectPoolManager.Instance.TotalActiveObjectCount;
        Debug.Log($"[TestObjectPool] Total pools: {totalPools}, Active objects: {activeObjects}");
    }

    [ContextMenu("TestObjectPool_ClearAll")]
    public void TestObjectPoolClearAll()
    {
        ObjectPoolManager.Instance.ClearAllPools();
        Debug.Log("[TestObjectPool] Cleared all pools");
    }

    #endregion

    #region DataManager Tests

    [ContextMenu("TestDataManager_RegisterData")]
    public void TestRegisterData()
    {
        var testData = new TestRuntimeData
        {
            PlayerName = "TestPlayer",
            Score = 100,
            Level = 1
        };
        DataManager.DataManager.Instance.Register("TestData", testData);
        Debug.Log($"[TestDataManager] Registered test data: {testData.PlayerName}");
    }

    [ContextMenu("TestDataManager_GetData")]
    public void TestGetData()
    {
        if (DataManager.DataManager.Instance.TryGet<TestRuntimeData>("TestData", out var data))
        {
            Debug.Log($"[TestDataManager] Got data: {data.PlayerName}, Score: {data.Score}");
        }
        else
        {
            Debug.LogWarning("[TestDataManager] Test data not found, register it first");
        }
    }

    [ContextMenu("TestDataManager_UpdateData")]
    public void TestUpdateData()
    {
        if (DataManager.DataManager.Instance.TryGet<TestRuntimeData>("TestData", out var data))
        {
            data.Score += 10;
            Debug.Log($"[TestDataManager] Updated score: {data.Score}");
        }
    }

    [ContextMenu("TestDataManager_UnregisterData")]
    public void TestUnregisterData()
    {
        DataManager.DataManager.Instance.Unregister("TestData");
        Debug.Log("[TestDataManager] Unregistered test data");
    }

    #endregion

    #region SaveManager Tests (实际使用方式)

    [ContextMenu("TestSave_SelectSlot0")]
    public void TestSelectSlot0()
    {
        SaveManager.Instance.SelectSlot(0);
        Debug.Log($"[TestSave] Selected slot 0, Current slot: {SaveManager.Instance.CurrentSlotIndex}");
    }

    [ContextMenu("TestSave_SelectSlot1")]
    public void TestSelectSlot1()
    {
        SaveManager.Instance.SelectSlot(1);
        Debug.Log($"[TestSave] Selected slot 1, Current slot: {SaveManager.Instance.CurrentSlotIndex}");
    }

    [ContextMenu("TestSave_ShowAllSlots")]
    public void TestShowAllSlots()
    {
        var slots = SaveManager.Instance.GetAllSaveSlotInfos();
        foreach (var slot in slots)
        {
            string info = slot.isEmpty ? "Empty" : $"Last save: {slot.lastSaveTime}, Play time: {slot.playTime}";
            Debug.Log($"[TestSave] Slot {slot.slotIndex}: {info}");
        }
    }

    [ContextMenu("TestSave_ShowDataPath")]
    public void TestShowDataPath()
    {
        Debug.Log($"[TestSave] Data path: {SaveManager.Instance.PersistentDataPath}");
    }

    [ContextMenu("TestSave_SaveGame")]
    public void TestSaveGame()
    {
        var saveData = SaveManager.Instance.GetCurrentSaveData<GameSaveData>();
        saveData.playTime += 10f;

        bool success = SaveManager.Instance.SaveCurrentData(saveData);
        Debug.Log($"[TestSave] Save result: {(success ? "Success" : "Failed")}");
        Debug.Log($"[TestSave] Play time: {saveData.playTime}, Last save: {saveData.lastSaveTime}");
    }

    [ContextMenu("TestSave_LoadGame")]
    public void TestLoadGame()
    {
        var saveData = SaveManager.Instance.GetCurrentSaveData<GameSaveData>();
        Debug.Log($"[TestSave] Loaded - Play time: {saveData.playTime}, Last save: {saveData.lastSaveTime}");
    }

    [ContextMenu("TestSave_MarkDirty")]
    public void TestMarkDirty()
    {
        SaveManager.Instance.MarkDirty();
        Debug.Log($"[TestSave] Marked dirty, Has unsaved changes: {SaveManager.Instance.HasUnsavedChanges}");
    }

    [ContextMenu("TestSave_DeleteCurrentSlot")]
    public void TestDeleteCurrentSlot()
    {
        int slot = SaveManager.Instance.CurrentSlotIndex;
        if (slot >= 0)
        {
            SaveManager.Instance.DeleteSlot(slot);
            Debug.Log($"[TestSave] Deleted slot {slot}");
        }
        else
        {
            Debug.LogWarning("[TestSave] No slot selected");
        }
    }

    #endregion
}

#region Test Event & Data Classes

public struct TestGameEvent : IEvent
{
    public string Message;
    public int Value;
}

[System.Serializable]
public class TestRuntimeData
{
    public string PlayerName;
    public int Score;
    public int Level;
}

#endregion
