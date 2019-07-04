using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LevelReader : MonoBehaviour {

	public static void ReadLevelFromFile(string filePath, TileContainer tileContainer)
    {
        ClearScene();
        string level = File.ReadAllText(filePath);

        int width = tileContainer.width;
        int height = tileContainer.height;
        int direction = 1;

        switch (tileContainer.readMethod)
        {
            case 0:
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int index = tileContainer.tileSymbol.IndexOf(level[x + y * width].ToString());
                            string tag = tileContainer.tileTag[index];
                            GameObject tile = AssetDatabase.LoadAssetAtPath("Assets/LevelGenerator/Tiles/" + tag + ".prefab", typeof(GameObject)) as GameObject;
                            tile = PrefabUtility.InstantiatePrefab(tile) as GameObject;
                            tile.transform.position = new Vector2(x, y);

                            LoadBackgroundElement(tile, x, y);
                        }
                    }
                    return;
                }
            case 1:
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            int index = tileContainer.tileSymbol.IndexOf(level[y + x * height].ToString());
                            string tag = tileContainer.tileTag[index];
                            GameObject tile = AssetDatabase.LoadAssetAtPath("Assets/LevelGenerator/Tiles/" + tag + ".prefab", typeof(GameObject)) as GameObject;
                            tile = PrefabUtility.InstantiatePrefab(tile) as GameObject;
                            tile.transform.position = new Vector2(x, y);

                            LoadBackgroundElement(tile, x, y);
                        }
                    }
                    return;
                }
            case 2:
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (direction == 1)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                int index = tileContainer.tileSymbol.IndexOf(level[y + x * height].ToString());
                                string tag = tileContainer.tileTag[index];
                                GameObject tile = AssetDatabase.LoadAssetAtPath("Assets/LevelGenerator/Tiles/" + tag + ".prefab", typeof(GameObject)) as GameObject;
                                tile = PrefabUtility.InstantiatePrefab(tile) as GameObject;
                                tile.transform.position = new Vector2(x, y);

                                LoadBackgroundElement(tile, x, y);
                            }
                        }
                        else
                        {
                            for (int y = 0; y < height; y++)
                            {
                                int index = tileContainer.tileSymbol.IndexOf(level[y + x * height].ToString());
                                string tag = tileContainer.tileTag[index];
                                GameObject tile = AssetDatabase.LoadAssetAtPath("Assets/LevelGenerator/Tiles/" + tag + ".prefab", typeof(GameObject)) as GameObject;
                                tile = PrefabUtility.InstantiatePrefab(tile) as GameObject;
                                tile.transform.position = new Vector2(x, height - 1 - y);

                                LoadBackgroundElement(tile, x, height - 1 - y);
                            }
                        }
                        direction = -direction;
                    }
                    return;
                }
        }      
    }

    private static void LoadBackgroundElement(GameObject tile, int x, int y)
    {
        if (tile.GetComponent<MovableElement>() != null)
        {
            GameObject backgroundTile = tile.GetComponent<MovableElement>().backgroundElement;
            backgroundTile = PrefabUtility.InstantiatePrefab(backgroundTile) as GameObject;
            backgroundTile.transform.position = new Vector2(x, y);
        }
    }

    private static void ClearScene()
    {
        foreach (GameObject gameObject in FindObjectsOfType<GameObject>())
        {
            if (gameObject.tag != "TileManager" && gameObject.tag != "MainCamera")
            {
                DestroyImmediate(gameObject);
            }
        }
    }
}
