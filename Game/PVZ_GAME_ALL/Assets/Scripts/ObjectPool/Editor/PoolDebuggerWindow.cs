#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ObjectPool.Editor
{
    public class PoolDebuggerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private List<PoolInfo> _poolInfos = new List<PoolInfo>();
        private int _selectedTab;
        private readonly string[] _tabs = { "池列表", "性能统计", "设置" };
        
        private int _totalGetCount;
        private int _totalReleaseCount;
        private int _totalCreateCount;
        
        private bool _autoRefresh = true;
        private float _refreshInterval = 1f;
        private double _lastRefreshTime;

        [MenuItem("Tools/ObjectPool/调试窗口")]
        public static void ShowWindow()
        {
            var window = GetWindow<PoolDebuggerWindow>("对象池调试");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            PoolEvents.OnPoolCreated += OnPoolCreated;
            PoolEvents.OnPoolRemoved += OnPoolRemoved;
            PoolEvents.OnObjectGet += OnObjectGet;
            PoolEvents.OnObjectRelease += OnObjectRelease;
            
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
        {
            PoolEvents.OnPoolCreated -= OnPoolCreated;
            PoolEvents.OnPoolRemoved -= OnPoolRemoved;
            PoolEvents.OnObjectGet -= OnObjectGet;
            PoolEvents.OnObjectRelease -= OnObjectRelease;
            
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                _totalGetCount = 0;
                _totalReleaseCount = 0;
                _totalCreateCount = 0;
            }
        }

        private void OnPoolCreated(string key, int count)
        {
            _totalCreateCount += count;
        }

        private void OnPoolRemoved(string key)
        {
            Repaint();
        }

        private void OnObjectGet(string key, GameObject obj)
        {
            _totalGetCount++;
        }

        private void OnObjectRelease(string key, GameObject obj)
        {
            _totalReleaseCount++;
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("请在播放模式下使用调试功能", MessageType.Info);
                return;
            }

            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs);

            EditorGUILayout.Space();

            switch (_selectedTab)
            {
                case 0:
                    DrawPoolList();
                    break;
                case 1:
                    DrawStatistics();
                    break;
                case 2:
                    DrawSettings();
                    break;
            }

            if (_autoRefresh && EditorApplication.isPlaying)
            {
                var currentTime = EditorApplication.timeSinceStartup;
                if (currentTime - _lastRefreshTime >= _refreshInterval)
                {
                    _lastRefreshTime = currentTime;
                    Repaint();
                }
            }
        }

        private void DrawPoolList()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("对象池列表", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            _autoRefresh = EditorGUILayout.ToggleLeft("自动刷新", _autoRefresh, GUILayout.Width(80));
            
            if (GUILayout.Button("刷新", GUILayout.Width(60)))
            {
                RefreshPoolInfos();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (!ObjectPoolManager.HasInstance)
            {
                EditorGUILayout.HelpBox("ObjectPoolManager 未初始化", MessageType.Warning);
                return;
            }

            _poolInfos = ObjectPoolManager.Instance.GetAllPoolInfo();

            if (_poolInfos.Count == 0)
            {
                EditorGUILayout.HelpBox("当前没有活动的对象池", MessageType.Info);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var info in _poolInfos)
            {
                DrawPoolInfoItem(info);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("释放所有池"))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要释放所有池中的对象吗？", "确定", "取消"))
                {
                    ObjectPoolManager.Instance.ClearAllPools();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPoolInfoItem(PoolInfo info)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(info.Key, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            var typeLabel = info.IsGameObjectPool ? "GameObject" : info.ObjectType?.Name ?? "Unknown";
            GUILayout.Label($"[{typeLabel}]", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            
            var countColor = info.Count > 0 ? Color.green : Color.gray;
            var activeColor = info.ActiveCount > 0 ? Color.yellow : Color.gray;

            var prevColor = GUI.color;
            
            GUI.color = countColor;
            GUILayout.Label($"缓存: {info.Count}", GUILayout.Width(80));
            
            GUI.color = activeColor;
            GUILayout.Label($"使用中: {info.ActiveCount}", GUILayout.Width(80));
            
            GUI.color = prevColor;
            
            GUILayout.Label($"容量: {(info.Capacity <= 0 ? "∞" : info.Capacity.ToString())}", GUILayout.Width(80));
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("预加载 +10", GUILayout.Width(80)))
            {
                ObjectPoolManager.Instance.Preload(info.Key, 10);
            }
            if (GUILayout.Button("清空", GUILayout.Width(50)))
            {
                ObjectPoolManager.Instance.RemovePool(info.Key);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawStatistics()
        {
            GUILayout.Label("性能统计", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("获取次数:", _totalGetCount.ToString());
            EditorGUILayout.LabelField("释放次数:", _totalReleaseCount.ToString());
            EditorGUILayout.LabelField("创建次数:", _totalCreateCount.ToString());
            
            var hitRate = _totalGetCount > 0 ? (float)(_totalGetCount - _totalCreateCount) / _totalGetCount * 100 : 0;
            EditorGUILayout.LabelField("命中率:", $"{hitRate:F1}%");
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (!ObjectPoolManager.HasInstance)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("总池数量:", ObjectPoolManager.Instance.TotalPoolCount.ToString());
            EditorGUILayout.LabelField("总活跃对象:", ObjectPoolManager.Instance.TotalActiveObjectCount.ToString());
            EditorGUILayout.LabelField("ListPool 缓存:", ListPool<int>.Count.ToString());
            EditorGUILayout.LabelField("ListPool 使用中:", ListPool<int>.ActiveCount.ToString());
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (GUILayout.Button("重置统计"))
            {
                _totalGetCount = 0;
                _totalReleaseCount = 0;
                _totalCreateCount = 0;
            }
        }

        private void DrawSettings()
        {
            GUILayout.Label("调试设置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _autoRefresh = EditorGUILayout.Toggle("自动刷新", _autoRefresh);
            _refreshInterval = EditorGUILayout.Slider("刷新间隔(秒)", _refreshInterval, 0.1f, 5f);

            EditorGUILayout.Space();

            GUILayout.Label("快捷操作", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("预热 ListPool (100)"))
            {
                ListPool<int>.WarmUp(100);
                ListPool<string>.WarmUp(100);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "使用说明:\n" +
                "• 池列表: 查看所有活动池的状态\n" +
                "• 性能统计: 监控对象池使用效率\n" +
                "• 命中率越高，性能优化效果越好",
                MessageType.Info);
        }

        private void RefreshPoolInfos()
        {
            if (ObjectPoolManager.HasInstance)
            {
                _poolInfos = ObjectPoolManager.Instance.GetAllPoolInfo();
            }
        }
    }
}
#endif
