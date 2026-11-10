using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBuilder : MonoBehaviour
{
    public static EnemyBuilder instance;

    private int enemyLevel = 0;
    private GameObject e_level;

    public int EnemyLevel
    {
        get
        {
            return enemyLevel;
        }
        set
        {
            enemyLevel = value;
            LoadEnemyLevel();
        }
    }

    private string FILE_PATH;
    
    // Start is called before the first frame update
    void Start()
    {
        FILE_PATH = Application.dataPath + "/EnemyLevels/LevelNum.txt";
        
        LoadEnemyLevel();
    }

    
    void LoadEnemyLevel()
    {
        Destroy(e_level);

        e_level = new GameObject("Enemy Objects");

        string[] lines = System.IO.File.ReadAllLines(
            FILE_PATH.Replace("Num", enemyLevel + ""));

        for (int zPos = 0; zPos < lines.Length; zPos++)
        {
            string line = lines[zPos].ToUpper();

            char[] characters = line.ToCharArray();

            for (int xPos = 0; xPos < characters.Length; xPos++)
            {
                char c = characters[xPos];

                GameObject newObject = null;

                switch (c)
                {
                    case '1':
                        newObject =
                            Instantiate(Resources.Load<GameObject>("Prefabs/Enemy"));
                        break;
                }

                if (newObject != null)
                {
                    newObject.transform.parent = e_level.transform;
                    newObject.transform.position = new Vector3(400 + 60 * -xPos, Random.Range(500, 700), -400 + 60 * zPos);
                }
            }
        }
    }

    private void Update()
    {
        if (e_level.transform.childCount == 0)
        {
            EnemyLevel++;
        }
    }
}
