using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelWriter : MonoBehaviour {

    static string[] allSymbols = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };
    static List<string> tileSymbol = new List<string>();
    static List<string> tileTag = new List<string>();
    static string codedLevels;
    static int width, height;
    static int readMethod;

    public static void SaveLevelsToFile(Object levelsFolder, string fileName, string tileContainer, int readMethod, string currentScenePath)
    {
        LevelWriter.readMethod = readMethod;
        tileSymbol = new List<string>();
        tileTag = new List<string>();
        codedLevels = "";

        FindScenes(levelsFolder, readMethod);
        SaveTilesDictionary(tileContainer);
        SaveCodedLevels(fileName);

        EditorSceneManager.OpenScene(currentScenePath);
    }

    public static void FindScenes(Object levelsFolder, int readMethod)
    {
        if (levelsFolder != null)
        {
            string createLevelsPath = AssetDatabase.GetAssetPath(levelsFolder);
            foreach (string GUID in AssetDatabase.FindAssets(null, new[] { createLevelsPath }))
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(GUID);
                DetectTilesInScene(scenePath, readMethod);
            }
        }
    }

    public static void DetectTilesInScene(string scenePath, int readMethod)
    {
        EditorSceneManager.OpenScene(scenePath);
        int direction = 1;
        int x = 0;
        int y = 0;

        switch (readMethod)
        {
            case 0:
                {
                    RaycastHit hit;
                    Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);

                    while (hit.collider != null)
                    {
                        while (hit.collider != null)
                        {
                            string tag = hit.collider.transform.tag;
                            if (!tileTag.Contains(tag))
                            {
                                tileSymbol.Add(allSymbols[tileTag.Count]);
                                tileTag.Add(tag);
                            }
                            string symbol;
                            symbol = tileSymbol[tileTag.IndexOf(tag)];
                            codedLevels += symbol;

                            x++;
                            width = x;
                            Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
                        }

                        x = 0;
                        y++;
                        height = y;
                        Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
                    }
                    return;
                }
            case 1:
                {
                    RaycastHit hit;
                    Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);

                    while (hit.collider != null)
                    {
                        while (hit.collider != null)
                        {
                            string tag = hit.collider.transform.tag;
                            if (!tileTag.Contains(tag))
                            {
                                tileSymbol.Add(allSymbols[tileTag.Count]);
                                tileTag.Add(tag);
                            }
                            string symbol;
                            symbol = tileSymbol[tileTag.IndexOf(tag)];
                            codedLevels += symbol;

                            y++;
                            height = y;                          
                            Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
                        }

                        y = 0;
                        x++;
                        width = x;
                        Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
                    }
                    return;
                }
            case 2:
                {
                    RaycastHit hit;
                    Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);

                    while (hit.collider != null)
                    {
                        while (hit.collider != null)
                        {
                            string tag = hit.collider.transform.tag;
                            if (!tileTag.Contains(tag))
                            {
                                tileSymbol.Add(allSymbols[tileTag.Count]);
                                tileTag.Add(tag);
                            }
                            string symbol;
                            symbol = tileSymbol[tileTag.IndexOf(tag)];
                            codedLevels += symbol;

                            y += direction;
                            if (y > height)
                            {
                                height = y;
                            }
                            Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
                        }

                        direction = -direction;
                        y += direction;
                        x++;
                        width = x;
                        Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
                    }
                    return;
                }
        }    
    }

    public static void SaveTilesDictionary(string tileContainerName)
    {
        Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/LevelGenerator/Files/" + tileContainerName + ".prefab");

        GameObject tileContainer = new GameObject();
        tileContainer.hideFlags = HideFlags.HideInHierarchy;
        tileContainer.AddComponent<TileContainer>();
        tileContainer.GetComponent<TileContainer>().tileSymbol = tileSymbol;
        tileContainer.GetComponent<TileContainer>().tileTag = tileTag;
        tileContainer.GetComponent<TileContainer>().width = width;
        tileContainer.GetComponent<TileContainer>().height = height;
        tileContainer.GetComponent<TileContainer>().readMethod = readMethod;

        PrefabUtility.ReplacePrefab(tileContainer, prefab, ReplacePrefabOptions.Default);
    }

    public static void SaveCodedLevels(string fileName)
    {
        string levelSize = "\n" + (width * height).ToString();
        File.WriteAllText("Assets/LevelGenerator/Files/" + fileName + ".txt", codedLevels + levelSize);
    }
}