using UnityEngine;
using Unity.Services.LevelPlay;

public class AdInitializer : MonoBehaviour
{
    [Header("LevelPlay")]
    [SerializeField] private string appKey = "281060c35";
    [SerializeField] private string rewardedAdUnitId = "posbsvfdf2ww1x7g";

    private LevelPlayRewardedAd rewardedAd;

    private void Awake()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
        Debug.Log("Inicializando LevelPlay...");

        LevelPlay.OnInitSuccess += OnInitializationComplete;
        LevelPlay.OnInitFailed += OnInitializationFailed;

        LevelPlay.Init(appKey);
    }

    private void OnInitializationComplete(LevelPlayConfiguration configuration)
    {
        Debug.Log("LevelPlay inicializado com sucesso!");

        CreateRewardedAd();
    }

    private void OnInitializationFailed(LevelPlayInitError error)
    {
        Debug.LogError("Falha ao inicializar LevelPlay: " + error);
    }

    private void CreateRewardedAd()
    {
        rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);

        rewardedAd.OnAdLoaded += OnRewardedAdLoaded;
        rewardedAd.OnAdLoadFailed += OnRewardedAdLoadFailed;
        rewardedAd.OnAdDisplayed += OnRewardedAdDisplayed;
        rewardedAd.OnAdDisplayFailed += OnRewardedAdDisplayFailed;
        rewardedAd.OnAdRewarded += OnRewardedAdRewarded;
        rewardedAd.OnAdClosed += OnRewardedAdClosed;
        rewardedAd.OnAdClicked += OnRewardedAdClicked;

        LoadRewardedAd();
    }

    public void LoadRewardedAd()
    {
        if (rewardedAd == null)
        {
            Debug.LogWarning("Rewarded ainda não foi inicializado.");
            return;
        }

        Debug.Log("Carregando Rewarded...");
        rewardedAd.LoadAd();
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd == null)
        {
            Debug.LogWarning("Rewarded ainda não foi inicializado.");
            return;
        }

        if (rewardedAd.IsAdReady())
        {
            Debug.Log("Mostrando anúncio de revive...");
            rewardedAd.ShowAd();
        }
        else
        {
            Debug.LogWarning("Rewarded ainda não está carregado.");
            LoadRewardedAd();
        }
    }

    private void OnRewardedAdLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded carregado!");
    }

    private void OnRewardedAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError("Erro ao carregar Rewarded: " + error);
    }

    private void OnRewardedAdDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded começou a ser exibido.");
    }

    private void OnRewardedAdDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        Debug.LogError("Erro ao mostrar Rewarded: " + error);
    }

    private void OnRewardedAdRewarded(
        LevelPlayAdInfo adInfo,
        LevelPlayReward reward)
    {
        Debug.Log("Jogador recebeu a recompensa: " + reward.Name);

        if (Goblin.instance != null)
        {
            Goblin.instance.Revive();
        }
    }

    private void OnRewardedAdClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded fechado.");

        LoadRewardedAd();
    }

    private void OnRewardedAdClicked(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Jogador clicou no anúncio.");
    }

    private void RevivePlayer()
    {
        Debug.Log("REVIVE! Dar vida ao jogador aqui.");

        // Coloque aqui o código que já existe no seu jogo
        // para reviver o jogador.
    }
}