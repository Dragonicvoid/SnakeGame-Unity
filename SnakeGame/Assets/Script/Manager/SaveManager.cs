using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public SaveState SaveData;

    public bool shouldDoTutorial = true;

    public static SaveManager _instance;

    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SaveManager();
            }
            return _instance;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _instance = this;

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
