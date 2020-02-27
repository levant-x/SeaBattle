using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameField;

public class Settings : MonoBehaviour
{
    public static CellState[,] PlayerField;
    public static CellState[,] EnemyField;

    static GameObject[] sprites;
    static Settings instance;

    static Settings()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private Settings()
    {
        instance = this;
    }

    static void OnActiveSceneChanged(Scene previousScene, Scene currentScene)
    {
        //Debug.Log(previousScene.name);
        //Debug.Log(currentScene.name);
        //Do();
    }

    public static void ScaleSpriteByY(SpriteRenderer sr, float yScale, out float spriteSize)
    {
        var currentSize = sr.bounds.size.x;
        var desiredSize = Camera.main.orthographicSize * 2 * yScale;
        var scaleFactor = desiredSize / currentSize;
        sr.transform.localScale *= scaleFactor;
        spriteSize = sr.bounds.size.x;
    }

    static void Do()
    {
        var path = Path.GetFullPath("Assets/Sprites");
        var dirInfo = new DirectoryInfo(path);
        var files = dirInfo.GetFiles("*.png");
        foreach (var item in files)
        {
            var b = File.ReadAllBytes(item.FullName);
            //Debug.Log(item.FullName);
            //var g = GameObject.Find(item.FullName);
            Debug.Log(b);
            var t = Texture2D.whiteTexture;
            t.LoadImage(b);


            Sprite sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0, 0), 32f);
        }
    }
}
