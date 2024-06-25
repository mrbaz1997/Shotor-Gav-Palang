using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelBuilderWindow : EditorWindow
{
    private uint width = 12;
    private uint height = 7;
    private uint maxWidth = 12;
    private uint maxHeight = 7;
    private bool hasDirectionalLight = true;
    private List<GameObject> prefabPool;
    private int selectedPrefabIndex = -1;
    private Dictionary<int, PrefabData[,]> gridLayers;
    private GameObject parentObject;
    private VisualElement gridContainer;
    private VisualElement prefabPoolContainer;
    private ScrollView selectedPrefabContainer;
    private static readonly List<Type> validComponents = new() { typeof(OperationBase) };
    private readonly string _localPath = "/Prefabs/Resources/Levels";
    private int levelId;

    [MenuItem("Tools/Level Builder")]
    public static void ShowWindow()
    {
        GetWindow<LevelBuilderWindow>("Level Builder");
    }

    private void OnEnable()
    {
        prefabPool ??= new List<GameObject>();
        gridLayers ??= new Dictionary<int, PrefabData[,]>();

        //InitializeGridLayers();
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        VisualElement mainContainer = new();
        mainContainer.style.flexDirection = FlexDirection.Row;
        mainContainer.style.flexGrow = 1;
        root.Add(mainContainer);

        VisualElement leftContainer = new();
        leftContainer.style.flexDirection = FlexDirection.Column;
        leftContainer.style.flexGrow = 1;
        leftContainer.style.width = 500;
        mainContainer.Add(leftContainer);

        SetupPrefabListContainer(leftContainer);            // top-left container
        SetupPrefabComponentsContainer(leftContainer);      // middle-left container
        SetupGeneralParametersContainer(leftContainer);     // bottom-left container
        SetupGridContainer(mainContainer);                  // right container

        RefreshPrefabPoolUI();
        DrawGridVisualization();
    }

    private void SetupPrefabListContainer(VisualElement leftContainer)
    {
        ScrollView prefabListContainer = new();
        prefabListContainer.style.flexBasis = Length.Percent(100);
        prefabListContainer.style.flexGrow = 1;
        prefabListContainer.AddBorder();

        VisualElement addPrefabOptions = new();
        addPrefabOptions.style.flexDirection = FlexDirection.Row;
        addPrefabOptions.style.flexGrow = 1;
        Button loadAllFromFolder = new(() =>
        {
            prefabPool = new List<GameObject>();
            LoadAllPrefabsFromFolder("Assets/Prefabs/LevelObjects");
            RefreshPrefabPoolUI();
        })
        { text = "Clear Exists & Reload Prefabs", tooltip = "Load From Prefabs/LevelObjects" };
        addPrefabOptions.Add(loadAllFromFolder);
        Button addPrefabButton = new(() =>
        {
            prefabPool.Add(null);
            RefreshPrefabPoolUI();
        })
        { text = "Add Prefab" };
        addPrefabOptions.Add(addPrefabButton);
        prefabListContainer.Add(addPrefabOptions);
        Label prefabPoolLabel = new("Prefab Pool:");
        prefabListContainer.Add(prefabPoolLabel);

        prefabPoolContainer = new VisualElement();
        prefabPoolContainer.style.flexDirection = FlexDirection.Column;
        prefabListContainer.Add(prefabPoolContainer);
        leftContainer.Add(prefabListContainer);
    }

    private void LoadAllPrefabsFromFolder(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            prefabPool.Add(prefab);
        }
    }

    private void SetupPrefabComponentsContainer(VisualElement mainContainer)
    {
        selectedPrefabContainer = new ScrollView();
        selectedPrefabContainer.style.height = 400;
        selectedPrefabContainer.style.flexGrow = 1;
        selectedPrefabContainer.AddBorder();

        ShowSerializableFields();
        mainContainer.Add(selectedPrefabContainer);
    }

    private void SetupGeneralParametersContainer(VisualElement leftContainer)
    {
        VisualElement GeneralParameters = new();
        GeneralParameters.style.height = 240;
        GeneralParameters.style.flexGrow = 1;
        GeneralParameters.AddBorder();

        IntegerField widthField = new("Width") { value = (int)width };
        widthField.RegisterValueChangedCallback(evt =>
        {
            widthField.value = evt.newValue.Clamp(1, 50);
            width = (uint)widthField.value;
            maxWidth = math.max(maxWidth, width);
            InitializeGridLayers();

        });
        GeneralParameters.Add(widthField);

        IntegerField heightField = new("Height") { value = (int)height };
        heightField.RegisterValueChangedCallback(evt =>
        {
            heightField.value = evt.newValue.Clamp(1, 50);
            height = (uint)heightField.value;
            maxHeight = math.max(maxHeight, height);
            InitializeGridLayers();
        });
        GeneralParameters.Add(heightField);

        Toggle directionalLight = new("Has Directional Light") { value = hasDirectionalLight };
        directionalLight.RegisterValueChangedCallback((evt) => hasDirectionalLight = evt.newValue);
        GeneralParameters.Add(directionalLight);

        IntegerField path = new("Level ID: ") { value = levelId };
        path.RegisterValueChangedCallback((evt) => levelId = evt.newValue);
        GeneralParameters.Add(path);

        Button generateButton = new(GenerateGrid)
        { text = "Generate" };
        GeneralParameters.Add(generateButton);



        Button saveButton = new(SaveLevel)
        { text = "Save Level" };
        GeneralParameters.Add(saveButton);
        leftContainer.Add(GeneralParameters);
    }
    private void SetupGridContainer(VisualElement mainContainer)
    {
        ScrollView rightContainer = new();
        rightContainer.style.flexBasis = Length.Percent(100);
        rightContainer.style.flexGrow = 1;
        rightContainer.style.backgroundColor = new Color(0.6f, 0.6f, 0.6f);
        mainContainer.Add(rightContainer);

        VisualElement buttonsArea = new();

        //Button Preset1

        Button clearButton = new(ClearGrid)
        { text = "Clear Grid" };
        buttonsArea.Add(clearButton);
        rightContainer.Add(buttonsArea);

        gridContainer = new VisualElement();
        gridContainer.style.flexDirection = FlexDirection.Column;
        rightContainer.Add(gridContainer);
    }

    private void RefreshPrefabPoolUI()
    {
        prefabPoolContainer.Clear();
        for (int i = 0; i < prefabPool.Count; i++)
        {
            VisualElement prefabElement = new();
            prefabElement.style.flexDirection = FlexDirection.Row;
            int index = i;

            Button fillAll = new(() =>
            {
                selectedPrefabIndex = index;
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        AddPrefabToGrid(x, y);

                DrawGridVisualization();
            })
            { text = "Fill All" };
            prefabElement.Add(fillAll);

            Button removeButton = new(() =>
            {
                prefabPool.RemoveAt(index);
                RefreshPrefabPoolUI();
            })
            { text = "Remove" };
            prefabElement.Add(removeButton);

            Button selectButton = new(() => OnSelectPrefab(index)) { text = "Select" };
            prefabElement.Add(selectButton);

            ObjectField prefabField = new() { objectType = typeof(GameObject), value = prefabPool[i] };
            prefabField.RegisterValueChangedCallback(evt => prefabPool[index] = (GameObject)evt.newValue);
            prefabElement.Add(prefabField);

            prefabPoolContainer.Add(prefabElement);
        }
    }

    private void OnSelectPrefab(int index)
    {
        if (prefabPool[index] == null) return;
        selectedPrefabIndex = index;
        int layer = prefabPool[index].layer;
        if (!gridLayers.ContainsKey(layer))
        {
            PrefabData[,] grid = new PrefabData[maxWidth, maxHeight];
            for (int x = 0; x < maxWidth; x++)
            {
                for (int y = 0; y < maxHeight; y++)
                    grid[x, y] = new PrefabData();
            }
            gridLayers[layer] = grid;
        }

        ShowSerializableFields();
    }

    private void InitializeGridLayers()
    {
        var layers = new List<int>(gridLayers.Keys);
        foreach (var layer in layers)
        {
            var grid = new PrefabData[maxWidth, maxHeight];
            for (int x = 0; x < maxWidth; x++)
            {
                for (int y = 0; y < maxHeight; y++)
                {
                    if (gridLayers[layer].GetLength(0) <= x || gridLayers[layer].GetLength(1) <= y)
                        grid[x, y] = new PrefabData();
                    else
                        grid[x, y] = gridLayers[layer][x, y];
                }
            }
            gridLayers[layer] = grid;
        }
        DrawGridVisualization();
    }

    private void DrawGridVisualization()
    {
        if (gridContainer is null)
            return;
        gridContainer.Clear();
        VisualElement gridElement = new()
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                flexWrap = Wrap.Wrap,
                justifyContent = Justify.Center,
                alignItems = Align.Center,
                alignContent = Align.Center
            }
        };

        for (int y = 0; y < height; y++)
        {
            VisualElement row = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            for (int x = 0; x < width; x++)
            {
                int _y = y;
                int _x = x;
                VisualElement cellContainer = new()
                {
                    style =
                    {
                        width = 40,
                        height = 40,
                        marginBottom = 2,
                        marginRight = 2,
                        position = Position.Relative
                    }
                };
                Button cellButton = new(() => AddPrefabToGrid(_x, _y))
                {
                    style =
                    {
                        width = 40,
                        height = 40,
                        position = Position.Absolute,
                        backgroundColor = Color.white
                    }

                };

                cellContainer.Add(cellButton);

                var layers = gridLayers.Keys.OrderBy(x => x);
                foreach (var layer in layers)
                {
                    if (GetGridTileOf(layer, _x, _y).prefabIndex != -1 && prefabPool[GetGridTileOf(layer, _x, _y).prefabIndex] != null)
                    {
                        SpriteRenderer sr = prefabPool[GetGridTileOf(layer, _x, _y).prefabIndex].GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            VisualElement spriteElement = new()
                            {
                                style =
                                {
                                    width = 40,
                                    height = 40,
                                    backgroundImage = new StyleBackground(sr.sprite),
                                    position = Position.Absolute
                                }
                            };
                            spriteElement.RegisterCallback<PointerDownEvent>((evt) =>
                            {
                                if (evt.button == 0)
                                    AddPrefabToGrid(_x, _y);
                                if (evt.button == 1)
                                    RemovePrefabFromGrid(_x, _y);
                            });
                            cellContainer.Add(spriteElement);
                        }
                    }
                }

                row.Add(cellContainer);
            }
            gridElement.Add(row);
        }

        gridContainer.Add(gridElement);
    }

    private PrefabData GetGridTileOf(int layer, int x, int y)
    {
        return gridLayers[layer][x, y];
    }

    private void RemovePrefabFromGrid(int x, int y)
    {
        if (selectedPrefabIndex >= 0 && selectedPrefabIndex < prefabPool.Count && prefabPool[selectedPrefabIndex] != null)
        {
            foreach (var item in gridLayers)
                item.Value[x, y] = new PrefabData();
            DrawGridVisualization();
        }
    }

    private void AddPrefabToGrid(int x, int y)
    {
        if (selectedPrefabIndex >= 0 && selectedPrefabIndex < prefabPool.Count && prefabPool[selectedPrefabIndex] != null)
        {
            int layer = prefabPool[selectedPrefabIndex].layer;

            if (!gridLayers.ContainsKey(layer))
                OnSelectPrefab(selectedPrefabIndex);

            var prefabData = new PrefabData
            {
                prefabIndex = selectedPrefabIndex,
                components = new List<ComponentData>()
            };

            foreach (var component in prefabPool[selectedPrefabIndex].GetComponentsInChildren<Component>())
            {
                if (component != null && !IsInValidComponent(component.GetType()))
                    prefabData.components.Add(ComponentUtility.SerializeComponent(component));
            }

            gridLayers[layer][x, y] = prefabData;
            DrawGridVisualization();
        }
    }

    private void GenerateGrid()
    {
        if (prefabPool.Count == 0)
        {
            Debug.LogError("Prefab pool is empty!");
            return;
        }
        parentObject = new GameObject("LevelGrid", typeof(LevelController));
        parentObject.GetComponent<LevelController>().SetData(levelId, width, height);

        if (hasDirectionalLight)
        {
            parentObject.AddComponent<Light>();
            Light light = parentObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.5f, 0.5f, 0.5f);
        }

        foreach (var layer in gridLayers.Keys)
        {
            GameObject layerObject = new($"Layer_{layer}");
            layerObject.transform.parent = parentObject.transform;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var prefabData = gridLayers[layer][x, y];
                    if (gridLayers[layer][x, y].prefabIndex != -1)
                    {
                        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabPool[gridLayers[layer][x, y].prefabIndex]);
                        obj.transform.position = new Vector3(x, -y, -layer / 10f);
                        obj.transform.parent = layerObject.transform;

                        foreach (var componentData in prefabData.components)
                        {
                            if (componentData.type != null && !IsInValidComponent(componentData.type))
                            {
                                var component = obj.GetComponentInChildren(componentData.type);
                                if (component != null)
                                    ComponentUtility.DeserializeComponent(component, componentData);
                            }
                        }
                    }
                }
            }
        }
    }

    private void ClearGrid()
    {
        gridLayers.Clear();
        if (prefabPool.Count > 0 && prefabPool[0] != null)
            OnSelectPrefab(0);
        DrawGridVisualization();
    }

    private void SaveLevel()
    {
        if (parentObject == null)
        {
            EditorUtility.DisplayDialog(
               "No Generated Game Object",
               $"Please First Generate Its Gameobject",
               "Confirm!"
           );
            return;
        }

        string localPath = "Assets" + _localPath + "/Level " + levelId + ".prefab";


        if (System.IO.File.Exists(localPath))
        {
            bool confirmDelete = EditorUtility.DisplayDialog(
               "Confirm Replace",
               $"Level " + levelId + " already exists! Do you want to replace it?",
               "Replace",
               "Cancel"
           );

            if (confirmDelete)
            {
                System.IO.File.Delete(localPath);
                AssetDatabase.Refresh();
                SaveLevel();
            }
        }
        else
        {
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            PrefabUtility.SaveAsPrefabAssetAndConnect(parentObject, localPath, InteractionMode.UserAction);
            Debug.Log("Level saved as: " + localPath);
            DestroyImmediate(parentObject);
            parentObject = null;
        }
    }

    private void ShowSerializableFields()
    {
        selectedPrefabContainer.Clear();
        if (selectedPrefabIndex < 0 || selectedPrefabIndex >= prefabPool.Count || prefabPool[selectedPrefabIndex] == null)
        {
            Label _label = new("Selected Prefab: ");
            selectedPrefabContainer.Add(_label);
            selectedPrefabIndex = -1;
            return;
        }

        Label label = new("Selected Prefab: " + prefabPool[selectedPrefabIndex].name);
        selectedPrefabContainer.Add(label);
        GameObject prefabRoot = prefabPool[selectedPrefabIndex];
        if (prefabRoot == null) return;

        Component[] components = prefabRoot.GetComponentsInChildren<Component>();

        foreach (Component component in components)
        {
            if (component == null || IsInValidComponent(component.GetType())) continue;

            SerializedObject serializedObject = new(component);
            Label componentLabel = new(component.GetType().Name);
            componentLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            selectedPrefabContainer.Add(componentLabel);

            SerializedProperty property = serializedObject.GetIterator();
            if (property.NextVisible(true))
            {
                do
                {
                    PropertyField propertyField = new(property);
                    propertyField.Bind(serializedObject);
                    selectedPrefabContainer.Add(propertyField);
                } while (property.NextVisible(false));
            }

            selectedPrefabContainer.Add(new VisualElement { style = { height = 10 } });
        }
    }

    private static bool IsInValidComponent(Type componentType)
    {
        foreach (var validComponent in validComponents)
        {
            if (validComponent.IsAssignableFrom(componentType))
                return false;
        }

        return true;
    }
}

public static class BuilderExtentions
{
    public static int Clamp(this int value, int min, int max)
    {
        return (value < min) ? min : (value > max) ? max : value;
    }

    public static void AddBorder(this VisualElement element, int width = 1, Color color = default)
    {
        if (color == default) color = Color.black;
        element.style.borderLeftWidth = width;
        element.style.borderRightWidth = width;
        element.style.borderTopWidth = width;
        element.style.borderBottomWidth = width;
        element.style.borderLeftColor = color;
        element.style.borderRightColor = color;
        element.style.borderTopColor = color;
        element.style.borderBottomColor = color;
    }
}

[Serializable]
public class ComponentData
{
    public Type type;
    public string jsonData;
}

[Serializable]
public class PrefabData
{
    public int prefabIndex = -1;
    public List<ComponentData> components;
}

public static class ComponentUtility
{
    public static ComponentData SerializeComponent(Component component)
    {
        ComponentData data = new()
        {
            type = component.GetType(),
            jsonData = JsonUtility.ToJson(component)
        };
        return data;
    }

    public static void DeserializeComponent(Component component, ComponentData data)
    {
        JsonUtility.FromJsonOverwrite(data.jsonData, component);
    }
}
