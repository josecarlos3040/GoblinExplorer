using UnityEngine;
using TMPro;

public class GoblinSaveSystem : MonoBehaviour
{
    private const string RecordKey = "GoblinExplorer_Record";
    private const string CoinsKey = "GoblinExplorer_Coins";

    [Header("UI")]
    [SerializeField] private TMP_Text recordText;

    public int Record { get; private set; }
    public int Coins { get; private set; }

    private void Awake()
    {
        LoadData();
    }

    public void LoadData()
    {
        Record = PlayerPrefs.GetInt(RecordKey, 0);
        Coins = PlayerPrefs.GetInt(CoinsKey, 0);

        UpdateUI();

        Debug.Log("Recorde carregado: " + Record);
        Debug.Log("Moedas carregadas: " + Coins);
    }

    public void SaveGameResult(int distance)
    {
        if (distance > Record)
        {
            Record = distance;

            PlayerPrefs.SetInt(RecordKey, Record);
            PlayerPrefs.Save();

            Debug.Log("Novo recorde: " + Record + "m");
        }

        UpdateUI();
    }

    public void SaveCoins(int amount)
    {
        Coins = Mathf.Max(0, amount);

        PlayerPrefs.SetInt(CoinsKey, Coins);
        PlayerPrefs.Save();

        Debug.Log("Moedas salvas: " + Coins);
    }

    public void UpdateUI()
    {
        if (recordText != null)
            recordText.text = "Recorde: " + Record + "m";
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveGameResult(GameUI.distanceGoblin);
            PlayerPrefs.Save();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGameResult(GameUI.distanceGoblin);
        PlayerPrefs.Save();
    }
}