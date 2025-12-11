using System;
using System.Collections.Generic;
using System.IO;
using _Data.LevelConfig;
using _Scripts.Grid;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    public class LevelDesign : EditorWindow
    {
        private Dictionary<EGridPositionType, Color> _dictTargetColor = new();
        private const string CONFIG_FOLDER_PATH = "level_config_folder_path";
        private string _folderPath;

        private const int CELL_HEIGHT = 50;
        private const int CELL_WIDTH = 50;

        private LevelConfigModel _selectedLevelConfig;
        private readonly List<LevelConfigModel> _availableLevelConfig = new();
        private GridConfigModel[,] _cacheGridPosition;
        private int _selectedIndex;
        public Vector2 _scrollPos;

        private LevelConfigModel this[int levelID] => _availableLevelConfig.Find(x => x.levelID == levelID);
        
        [MenuItem("Tools/Level design")]
        public static void OpenWindow()
        {
            var w = GetWindow<LevelDesign>("Level design");
            w.minSize = new Vector2(400, 200);
        }

        private void OnEnable()
        {
            _folderPath = EditorPrefs.GetString(CONFIG_FOLDER_PATH, "");
            foreach (EGridPositionType t in Enum.GetValues(typeof(EGridPositionType)))
            {
                if (!_dictTargetColor.ContainsKey(t))
                {
                    _dictTargetColor.Add(t, t switch
                    {
                        EGridPositionType.None => Color.black,
                        EGridPositionType.Gem => Color.white,
                        _ => throw new ArgumentOutOfRangeException()
                    });
                }
            }

            _cacheGridPosition = new GridConfigModel[Definition.BOARD_HEIGHT, Definition.BOARD_WIDTH];
        }

        private void OnGUI()
        {
            _DrawController();
            _DrawGrid();
        }

        #region ----- Controller -----

        private void _DrawController()
        {
            _DrawFolderPath();
            _DrawLevelConfigDropDown();
            
            if (_selectedLevelConfig == null && _availableLevelConfig.Count > 0)
            {
                _LoadConfigToMap(_availableLevelConfig[_selectedIndex]);
            }

            // if (GUILayout.Button("Play"))
            // {
            //     if (_selectedLevelConfig == null)
            //     {
            //         return;
            //     }
            //     PlayerPrefs.SetInt("TestLevel", _selectedIndex);
            //     _PlayFromScene("TestScene");
            // }
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("New level", GUILayout.Width(150)))
                {
                    int newLevel = _availableLevelConfig.Count == 0 ? 1 : _availableLevelConfig[^1].levelID + 1;
                    _selectedLevelConfig = CreateInstance<LevelConfigModel>();
                    _selectedLevelConfig.name = $"Level {newLevel}";
                    _selectedLevelConfig.CreateDefaultData(newLevel);
                    _availableLevelConfig.Add(_selectedLevelConfig);

                    _selectedLevelConfig = _availableLevelConfig[^1];
                    _LoadConfigToMap(_selectedLevelConfig);

                    AssetDatabase.CreateAsset(_selectedLevelConfig,
                        Path.Combine(_folderPath, $"{_selectedLevelConfig.name}.asset"));
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                if (GUILayout.Button("Save", GUILayout.Width(150)))
                {
                    _Save();
                }

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
        }

        private void _DrawFolderPath()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginChangeCheck();
                _folderPath = EditorGUILayout.TextField("Folder Path", _folderPath, GUILayout.Width(400));
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetString(CONFIG_FOLDER_PATH, _folderPath);
                }

                if (GUILayout.Button("...", GUILayout.Width(35)))
                {
                    string selected = EditorUtility.OpenFolderPanel("Chọn folder", _folderPath, "");
                    if (!string.IsNullOrEmpty(selected))
                    {
                        _folderPath = selected;
                        EditorPrefs.SetString(CONFIG_FOLDER_PATH, _folderPath); // save
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Load config..."))
            {
                _GetAvailableConfig();
                _DrawLevelConfigDropDown();
            }
        }

        private void _GetAvailableConfig()
        {
            _availableLevelConfig.Clear();
            string[] guids = AssetDatabase.FindAssets("t:LevelConfigModel", new[] { _folderPath });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LevelConfigModel so = (LevelConfigModel)AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so != null)
                {
                    _availableLevelConfig.Add(so);
                }
            }
        }

        private void _DrawLevelConfigDropDown()
        {
            if (_availableLevelConfig.Count > 0)
            {
                string[] options = new string[_availableLevelConfig.Count];
                for (int i = 0; i < _availableLevelConfig.Count; i++)
                {
                    options[i] = _availableLevelConfig[i].name;
                }
                
                int newIndex = EditorGUILayout.Popup("Chọn ScriptableObject", _selectedIndex, options);
                if (newIndex != _selectedIndex)
                {
                    if (_NeedToSave())
                    {
                        _Save();
                    }
                    else
                    {
                        _selectedIndex = newIndex;
                        _LoadConfigToMap(_availableLevelConfig[_selectedIndex]);
                    }
                }
            }
            else
            {
                GUILayout.Label("Chưa có ScriptableObject nào", EditorStyles.helpBox);
            }
        }

        private void _LoadConfigToMap(LevelConfigModel config)
        {
            if (_selectedLevelConfig != null && _selectedLevelConfig == config)
            {

            }
            else
            {
                // tạo instance trống trước
                _selectedLevelConfig = ScriptableObject.CreateInstance<LevelConfigModel>();

                // rồi đổ dữ liệu
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(config), _selectedLevelConfig);
            }

            foreach (var gridConfig in _selectedLevelConfig.gridConfigs)
            {
                _cacheGridPosition[gridConfig.coordinates.x, gridConfig.coordinates.y] = gridConfig;
            }
        }

        private bool _NeedToSave()
        {
            if (_selectedLevelConfig == null)
            {
                return false;
            }
        
            string currentData = JsonUtility.ToJson(_selectedLevelConfig);
            string localData = JsonUtility.ToJson(_availableLevelConfig[_selectedIndex]);
        
            return currentData != localData;
        }

        private void _Save()
        {
            if (_selectedLevelConfig == null)
            {
                return;
            }

            bool confirm = EditorUtility.DisplayDialog(
                "Save",
                "Are you sure?",
                "Yes",
                "No"
            );

            if (confirm)
            {
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(_selectedLevelConfig), _availableLevelConfig[_selectedIndex]);
                EditorUtility.SetDirty(_availableLevelConfig[_selectedIndex]);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }


        #endregion
        
        private void _DrawGrid()
        {
            if (_selectedLevelConfig == null)
            {
                return;
            }
            EditorGUILayout.BeginVertical();
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                {
                    int midX = Definition.BOARD_WIDTH / 2;
                    int midY = Definition.BOARD_HEIGHT / 2;
                    
                    for (int x = midX - 1; x >= -midX; x--)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            for (int y = midY - 1; y >= -midY; y--)
                            {
                                _DrawGridPositionButton(x + midX, y + midY);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
            }        
            EditorGUILayout.EndVertical();
        }

        private void _DrawGridPositionButton(int x, int y)
        {
            Color originColor = GUI.color;
            GUI.color = _dictTargetColor[_cacheGridPosition[x, y].type];
            {
                if (GUILayout.Button($"[{x} _ {y}]", GUILayout.Width(CELL_WIDTH), GUILayout.Height(CELL_HEIGHT)))
                {
                    int next = (int)_cacheGridPosition[x, y].type + 1;

                    // Nếu vượt max thì quay về 0 (tùy bạn muốn kiểu gì)
                    if (next > Enum.GetValues(typeof(EGridPositionType)).Length - 1)
                    {
                        next = 0;
                    }

                    _cacheGridPosition[x, y].type = (EGridPositionType)next;
                }
            }
            GUI.color = originColor;
        }
    }
}