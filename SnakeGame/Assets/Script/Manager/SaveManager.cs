using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    public SaveState SaveData;

    public bool shouldDoTutorial = true;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;

        LoadSave();
        setTutorialState();
    }

    public void Save()
    {
        PlayerPrefs.SetString("save", Util.Serialize(SaveData));
        setTutorialState();
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey("save");
    }

    public SaveState LoadSave()
    {
        if (PlayerPrefs.HasKey("save"))
        {
            SaveData = Util.Deserialize<SaveState>(PlayerPrefs.GetString("save"));
        }
        else
        {
            SaveData = new SaveState();
        }

        return SaveData;
    }

    void setTutorialState()
    {
        long currTime = Util.GetCurrWorldTime();
        shouldDoTutorial = SaveData.TimeLastTutorial - currTime > GENERAL_CONFIG.TIME_BEFORE_TUTORIAL_AGAIN || SaveData.TimeLastTutorial == 0;
    }
}
