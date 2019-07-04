using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ElementsLoader : MonoBehaviour
{
    static List<string> tags = new List<string>();
    static List<Vector2> positions = new List<Vector2>();

    public static void StartElementLoader(SceneAsset scene, string currentScenePath)
    {
        tags = new List<string>();
        positions = new List<Vector2>();

        FindElements(scene, currentScenePath);
        LoadElements();
    }

    public static void FindElements(SceneAsset scene, string currentScenePath)
    {
        EditorSceneManager.OpenScene("Assets/LevelGenerator/ElementsToLoad/" + scene.name + ".unity");

        int x = 0, y = 0;
        int lastX;
        RaycastHit hit;
        Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
        while(hit.collider != null)
        {
            x--;
            y--;
            Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
        }
        x++;
        y++;
        lastX = x;

        Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
        while (hit.collider != null)
        {
            while (hit.collider != null)
            {
                if (hit.collider.gameObject.tag != "Placeholder")
                {
                    tags.Add(hit.collider.gameObject.tag);
                    positions.Add(hit.collider.gameObject.transform.position);
                }

                x++;
                Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
            }

            x = lastX;
            y++;
            Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
        }

        EditorSceneManager.OpenScene(currentScenePath);
    }

    public static void LoadElements()
    {
        for (int i = 0; i < tags.Count; i++)
        {
            GameObject tile = AssetDatabase.LoadAssetAtPath("Assets/LevelGenerator/Tiles/" + tags[i] + ".prefab", typeof(GameObject)) as GameObject;
            tile = PrefabUtility.InstantiatePrefab(tile) as GameObject;
            tile.transform.position = new Vector2(positions[i].x, positions[i].y);
        }
    }

    public static void DestroyElements(SceneAsset scene, string currentScenePath)
    {
        FindElements(scene, currentScenePath);
        for (int i = 0; i < positions.Count; i++)
        {
            RaycastHit hit;
            Physics.Raycast(new Vector3(positions[i].x, positions[i].y, 1), new Vector3(0, 0, -1), out hit, 1);
            if (hit.collider != null)
            {
                DestroyImmediate(hit.collider.gameObject);
            }
        }

        tags = new List<string>();
        positions = new List<Vector2>();
    }
}