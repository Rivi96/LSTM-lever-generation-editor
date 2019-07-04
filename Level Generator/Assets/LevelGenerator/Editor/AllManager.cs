using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AllManager : EditorWindow {

    // Save
    private int levelReadMethod = 0;
    private Object createdLevelsFolder;
    private string savedLevelsFileName = "SavedLevels";
    private string tileContainerName = "TileContainer";
    // Train
    private int sequenceLength = 25;
    private int LSTMunits = 256;
    private int epochs = 200;
    private TextAsset savedLevelsFile;
    private string lstmSettingsFileName = "NetworkSettings";
    // Generate
    private int seed = 1;
    private TileContainer tileContainer;
    private Object lstmSettingsFile;
    private List<float> tilesSlider = new List<float>(new float[20]);
    // Level elements load
    private SceneAsset scene;
    // Path find
    private Color32 pathColor = new Color32(92, 255, 25, 255);
    private GameObject startingGameobject;
    private GameObject targetGameobject;

    #region UI description
    // Save
    private GUIContent[] readingMethodDesc = new GUIContent[] { new GUIContent("LRLR", "Read level from left to right"),
                                                                new GUIContent("BTBT", "Read level from bottom to top"),
                                                                new GUIContent("BTTB", "Read level from bottom to top, then next column from top to bottom") };
    private string createdLevelsFolderDesc = "Drag and drop folder with created levels as unity scenes from project assets";
    private string savedLevelsFileNameDesc = "File that contains coded levels as string will be saved under this name in Files folder";
    private string tileContainerNameDesc = "Prefab that contains tiles names and their codes will be saved under this name in Files folder";
    // Train
    private string sequenceLengthDesc = "Determines how many tiles network will store in sequence. Sequence should be smaller than all number of tiles in one level";
    private string LSTMunitsDesc = "Determines how many LSTM units network will use";
    private string epochsDesc = "Epochs determines how long network will be learning";
    private string savedLevelsFileDesc = "Drag and drop text file with saved levels created in first step";
    private string lstmSettingsFileNameDesc = "Name of the file that contains network settings. File will be saved in Files folder";
    // Generate
    private string seedDesc = "Number used to generate unique levels";
    private string lstmSettingsFileDesc = "Drag and drop network settings that was saved in second step";
    private string tileContainerDesc = "Drag and drop tile container prefab that was saved in first step. Allows to change tiles probabilities";
    // Level elements load
    private string elementsLoadDesc = "Drag and drop scene with additional tiles to load them in current scene";
    // Path find
    private string pathColorDesc = "Specify color of the path";
    private string startingGameobjectDesc = "Drag and drop tile from scene that will be starting point";
    private string targetGameobjectDesc = "Drag and drop tile from scene that will be ending point";
    #endregion

    [MenuItem("Tools/LevelManager")]
    public static void ShowWindow()
    {
        GetWindow<AllManager>("Level Manager");
    }

    void OnGUI()
    {
        OnGUISave();
        OnGUITrain();
        OnGUIGenerate();
        OnGUILevelElementsLoad();
        OnGUIFindPath();
    }

    private void OnGUISave()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("SAVE LEVELS", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Levels read method", EditorStyles.label);
        levelReadMethod = GUILayout.SelectionGrid(levelReadMethod, readingMethodDesc, 3);
        createdLevelsFolder = EditorGUILayout.ObjectField(new GUIContent("Created levels folder ", createdLevelsFolderDesc), createdLevelsFolder, typeof(Object), true);
        savedLevelsFileName = EditorGUILayout.TextField(new GUIContent("Levels file name", savedLevelsFileNameDesc), savedLevelsFileName);
        tileContainerName = EditorGUILayout.TextField(new GUIContent("Tile container name", tileContainerNameDesc), tileContainerName);

        GUI.enabled = createdLevelsFolder != null && savedLevelsFileName != "" && tileContainerName != "";
        if (GUILayout.Button("SAVE"))
        {
            string currentScenePath = SceneManager.GetActiveScene().path;
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            LevelWriter.SaveLevelsToFile(createdLevelsFolder, savedLevelsFileName, tileContainerName, levelReadMethod, currentScenePath);
        }
        GUI.enabled = true;
    }

    private void OnGUITrain()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("TRAIN", EditorStyles.boldLabel);

        sequenceLength = EditorGUILayout.IntField(new GUIContent("Sequence length", sequenceLengthDesc), sequenceLength);
        LSTMunits = EditorGUILayout.IntField(new GUIContent("LSTM units", LSTMunitsDesc), LSTMunits);
        epochs = EditorGUILayout.IntField(new GUIContent("Epochs", epochsDesc), epochs);
        savedLevelsFile = (TextAsset)EditorGUILayout.ObjectField(new GUIContent("Levels file", savedLevelsFileDesc), savedLevelsFile, typeof(TextAsset), true);
        lstmSettingsFileName = EditorGUILayout.TextField(new GUIContent("LSTM settings file name", lstmSettingsFileNameDesc), lstmSettingsFileName);

        sequenceLength = Mathf.Clamp(sequenceLength, 1, 500);
        LSTMunits = Mathf.Clamp(LSTMunits, 1, 1024);
        epochs = Mathf.Clamp(epochs, 1, 5000);

        GUI.enabled = savedLevelsFile != null && lstmSettingsFileName != "";
        if (GUILayout.Button("TRAIN"))
        {
            if (sequenceLength != 0 && epochs != 0 && lstmSettingsFileName != null && savedLevelsFile != null)
            {
                File.WriteAllText("Assets/LevelGenerator/Files/NetworkFiles/TrainFile.txt", lstmSettingsFileName);

                string lstmParameters = sequenceLength.ToString() + "\n" +
                                        LSTMunits.ToString() + "\n" +
                                        epochs.ToString() + "\n" +
                                        "Weights" + lstmSettingsFileName + "\n" +
                                        savedLevelsFile.name;

                File.WriteAllText("Assets/LevelGenerator/Files/" + lstmSettingsFileName + ".txt", lstmParameters);

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = Directory.GetCurrentDirectory() + @"\Assets\LevelGenerator\LSTM\";
                startInfo.FileName = "Training.exe";

                Process.Start(startInfo);//.WaitForExit();
            }
        }
        GUI.enabled = true;
    }

    private void OnGUIGenerate()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("GENERATE", EditorStyles.boldLabel);
        seed = EditorGUILayout.IntField(new GUIContent("Seed", seedDesc), seed);
        lstmSettingsFile = EditorGUILayout.ObjectField(new GUIContent("LSTM settings file", lstmSettingsFileDesc), lstmSettingsFile, typeof(Object), true);
        tileContainer = (TileContainer)EditorGUILayout.ObjectField(new GUIContent("Tile container prefab", tileContainerDesc), tileContainer, typeof(TileContainer), true);

        seed = Mathf.Clamp(seed, 1, 25);

        if (tileContainer != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tile probability coefficient", EditorStyles.miniLabel);
            for (int i = 0; i < tileContainer.tileTag.Count; i++)
            {
                tilesSlider[i] = EditorGUILayout.Slider(new GUIContent(tileContainer.tileTag[i]), tilesSlider[i], -20f, 20f);
            }
        }

        GUI.enabled = lstmSettingsFile != null && tileContainer != null;
        if (GUILayout.Button("GENERATE"))
        {
            string generateParameters = lstmSettingsFile.name + '\n';
            generateParameters += seed.ToString() + '\n';
            generateParameters += tileContainer.tileTag.Count.ToString() + "\n";
            for (int i = 0; i < tileContainer.tileTag.Count; i++)
            {
                generateParameters += tilesSlider[i].ToString() + "\n";
            }

            File.WriteAllText("Assets/LevelGenerator/Files/NetworkFiles/GenerateFile.txt", generateParameters);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = Directory.GetCurrentDirectory() + @"\Assets\LevelGenerator\LSTM\";
            startInfo.FileName = "Generating.exe";

            Process.Start(startInfo).WaitForExit();

            LevelReader.ReadLevelFromFile("Assets/LevelGenerator/Files/NetworkFiles/OutputFile.txt", tileContainer);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        GUI.enabled = true;
    }

    private void OnGUILevelElementsLoad()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("LOAD LEVEL ELEMENTS", EditorStyles.boldLabel);
        scene = (SceneAsset)EditorGUILayout.ObjectField(new GUIContent("Elements scene", elementsLoadDesc), scene, typeof(SceneAsset), true);

        GUI.enabled = scene != null;
        if (GUILayout.Button("LOAD ELEMENTS"))
        {
            string currentScenePath = SceneManager.GetActiveScene().path;

            ElementsLoader.DestroyElements(scene, currentScenePath);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            ElementsLoader.StartElementLoader(scene, currentScenePath);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        if (GUILayout.Button("CLEAR ELEMENTS"))
        {
            string currentScenePath = SceneManager.GetActiveScene().path;

            ElementsLoader.DestroyElements(scene, currentScenePath);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        GUI.enabled = true;
    }

    private void OnGUIFindPath()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("FIND PATH", EditorStyles.boldLabel);
        pathColor = EditorGUILayout.ColorField(new GUIContent("Path color", pathColorDesc), pathColor);
        startingGameobject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Start tile", startingGameobjectDesc), startingGameobject, typeof(GameObject), true);
        targetGameobject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Target tile", targetGameobjectDesc), targetGameobject, typeof(GameObject), true);

        GUI.enabled = startingGameobject != null && targetGameobject != null;
        if (GUILayout.Button("FIND PATH"))
        {
            PathFinder.DestroyPathDots();
            bool result = PathFinder.StartPathFinding(startingGameobject, targetGameobject, pathColor);

            if (!result)
            {
                EditorUtility.DisplayDialog("Path finder", "Path has not been found." + "\n" + "Path is unreachable or obstacle gameobject was selected as a start or target tile.", "OK");
            }
        }
        GUI.enabled = true;
    }
}