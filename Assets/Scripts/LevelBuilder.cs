using UnityEngine;
using UnityEngine.Windows;
using System.IO;
using File = UnityEngine.Windows.File;

public class LevelBuilder : MonoBehaviour
{
    public static LevelBuilder instance;

    private int currentLevel = 1;
    private GameObject level;

    public int CurrentLevel
    {
        get
        {
            return currentLevel;
        }
        set
        {
            currentLevel = value;
            LoadLevel();
        }
    }

    private string FILE_PATH;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        FILE_PATH = Application.dataPath + "/Levels/LevelNum.txt";

        LoadLevel();
    }

    void LoadLevel()
    {
        Destroy(level);

        level = new GameObject("Level Objects");
        
        //get the lines from the file
        string[] lines = System.IO.File.ReadAllLines(
            FILE_PATH.Replace("Num", currentLevel + ""));

        for (int zLevelPos = 0; zLevelPos < lines.Length; zLevelPos++)
        {
            //get a single line
            string line = lines[zLevelPos].ToUpper();
            
            //turn line into a char array
            char[] characters = line.ToCharArray();

            for (int xLevelPos = 0; xLevelPos < characters.Length; xLevelPos++)
            {
                 //get the first character
                 char c = characters[xLevelPos];

                 GameObject newObject = null;

                 switch (c)
                 {
                     case '1':
                         newObject =
                             Instantiate(Resources.Load<GameObject>("Prefabs/Building1"));
                         break;
                     case '2':
                         newObject =
                             Instantiate(Resources.Load<GameObject>("Prefabs/Building2"));
                         break;
                     case '3':
                         newObject =
                             Instantiate(Resources.Load<GameObject>("Prefabs/Building3"));
                         break;
                     case '4':
                         newObject =
                             Instantiate(Resources.Load<GameObject>("Prefabs/Building4"));
                         break;
                     case '5':
                         newObject =
                             Instantiate(Resources.Load<GameObject>("Prefabs/Building5"));
                         break;
                     default:
                         break;
                 }

                 if (newObject != null)
                 {
                     newObject.transform.parent = level.transform;
                     newObject.transform.position = new Vector3(400 + 60 * -xLevelPos, 0, -400 + 60 * zLevelPos);
                 }
            }
        }
    }
}
