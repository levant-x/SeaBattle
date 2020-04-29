using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameField;

public static class Settings 
{
    public enum CompexityLevel
    {
        Easy, Normal, Hard
    }

    public static event Action<PlayerGameField> enemyInitialized;
    public static CellState[,] playerField { get; set; }
    public static CellState[,] enemyField { get; set; }
    public static CompexityLevel compexityLevel { get; set; }
    public static bool isMultiplayerMode { get; set; }
    public static IAttacker attacker { get; private set; }

    static List<PlayerGameField> gameFields = new List<PlayerGameField>(2);

    static Settings()
    {
        compexityLevel = CompexityLevel.Normal;
    }

    public static void ChangeScene(string sceneName)
    {

        Debug.Log(sceneName + " requested");
        if (sceneName == "Battle") SetComplexityLevel();
        SceneManager.LoadScene(sceneName);
    }

    public static void ScaleSpriteByY(SpriteRenderer sr, float yScale, out float spriteSize)
    {
        var currentSize = sr.bounds.size.x;
        var desiredSize = Camera.main.orthographicSize * 2 * yScale;
        var scaleFactor = desiredSize / currentSize;
        sr.transform.localScale *= scaleFactor;
        spriteSize = sr.bounds.size.x;
    }

    public static void RegisterGameField(PlayerGameField gameField)
    {
        gameFields.Add(gameField);
        foreach (var field in gameFields) enemyInitialized(field);
    }

    public static Vector2 ConvertLinearCoordinateToDecart(int i, int width, int height)
    {
        return new Vector2(i % width, i / width);
    }
         
    public static int ConvertDecartCoordinatesToLinear(int x, int y, int width)
    {
        return y * width + x;
    }

    public static bool IsPointWithinMatrix<T>(int x, int y, T[,] matrix)
    {
        int width = matrix.GetLength(0), height = matrix.GetLength(1);
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    static void SetComplexityLevel()
    {
        if (compexityLevel == CompexityLevel.Easy) attacker = new EnemyAI1();
        else if (compexityLevel == CompexityLevel.Normal) attacker = new EnemyAI2();
    }
}
