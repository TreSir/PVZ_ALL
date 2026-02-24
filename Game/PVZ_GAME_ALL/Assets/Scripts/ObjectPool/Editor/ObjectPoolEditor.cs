#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ObjectPool.Editor
{
    [CustomPropertyDrawer(typeof(PoolConfig))]
    public class PoolConfigDrawer : PropertyDrawer
    {
        private static readonly GUIContent InitialSizeContent = new GUIContent("初始大小", "池初始化时预创建的对象数量");
        private static readonly GUIContent MaxSizeContent = new GUIContent("最大容量", "池的最大容量，0表示无限制");
        private static readonly GUIContent AutoExpandContent = new GUIContent("自动扩容", "当池为空时是否自动创建新对象");
        private static readonly GUIContent AutoReleaseContent = new GUIContent("场景切换释放", "场景切换时是否自动释放所有对象");
        private static readonly GUIContent AutoReleaseIntervalContent = new GUIContent("自动释放间隔", "自动释放的间隔时间(秒)，-1表示禁用");
        private static readonly GUIContent CollectionCheckContent = new GUIContent("重复释放检查", "是否检查重复释放对象");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                
                var y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                var height = EditorGUIUtility.singleLineHeight;
                var spacing = EditorGUIUtility.standardVerticalSpacing;

                var initialSizeProp = property.FindPropertyRelative("initialSize");
                var maxSizeProp = property.FindPropertyRelative("maxSize");
                var autoExpandProp = property.FindPropertyRelative("autoExpand");
                var autoReleaseProp = property.FindPropertyRelative("autoReleaseOnSceneChange");
                var autoReleaseIntervalProp = property.FindPropertyRelative("autoReleaseInterval");
                var collectionCheckProp = property.FindPropertyRelative("collectionCheck");

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), initialSizeProp, InitialSizeContent);
                y += height + spacing;
                
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), maxSizeProp, MaxSizeContent);
                y += height + spacing;
                
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), autoExpandProp, AutoExpandContent);
                y += height + spacing;
                
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), autoReleaseProp, AutoReleaseContent);
                y += height + spacing;
                
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), autoReleaseIntervalProp, AutoReleaseIntervalContent);
                y += height + spacing;
                
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), collectionCheckProp, CollectionCheckContent);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight * 7 + EditorGUIUtility.standardVerticalSpacing * 6;
        }
    }

    [CustomEditor(typeof(PoolConfig), true)]
    public class PoolConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _initialSizeProp;
        private SerializedProperty _maxSizeProp;
        private SerializedProperty _autoExpandProp;
        private SerializedProperty _autoReleaseProp;
        private SerializedProperty _autoReleaseIntervalProp;
        private SerializedProperty _collectionCheckProp;

        private void OnEnable()
        {
            _initialSizeProp = serializedObject.FindProperty("initialSize");
            _maxSizeProp = serializedObject.FindProperty("maxSize");
            _autoExpandProp = serializedObject.FindProperty("autoExpand");
            _autoReleaseProp = serializedObject.FindProperty("autoReleaseOnSceneChange");
            _autoReleaseIntervalProp = serializedObject.FindProperty("autoReleaseInterval");
            _collectionCheckProp = serializedObject.FindProperty("collectionCheck");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("池配置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_initialSizeProp, new GUIContent("初始大小"));
            EditorGUILayout.PropertyField(_maxSizeProp, new GUIContent("最大容量"));
            EditorGUILayout.PropertyField(_autoExpandProp, new GUIContent("自动扩容"));
            EditorGUILayout.PropertyField(_autoReleaseProp, new GUIContent("场景切换释放"));
            EditorGUILayout.PropertyField(_autoReleaseIntervalProp, new GUIContent("自动释放间隔"));
            EditorGUILayout.PropertyField(_collectionCheckProp, new GUIContent("重复释放检查"));

            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox("预设配置", MessageType.Info);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("小型池"))
            {
                _initialSizeProp.intValue = 5;
                _maxSizeProp.intValue = 20;
            }
            if (GUILayout.Button("中型池"))
            {
                _initialSizeProp.intValue = 20;
                _maxSizeProp.intValue = 100;
            }
            if (GUILayout.Button("大型池"))
            {
                _initialSizeProp.intValue = 50;
                _maxSizeProp.intValue = 500;
            }
            if (GUILayout.Button("无限制"))
            {
                _initialSizeProp.intValue = 10;
                _maxSizeProp.intValue = 0;
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
