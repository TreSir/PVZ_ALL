using System;

namespace DataManager
{
    [Serializable]
    public class GameSaveData : ISaveData
    {
        public string lastSaveTime { get; set; }
        public float playTime { get; set; }

        // TODO: 游戏数据字段待后续补充
    }
}
