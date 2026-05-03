using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private PlayerPiece playerPiece;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameHUD gameHUD;
    [SerializeField] private PhysicalDice physicalDice;
    [SerializeField] private PurchasePanel purchasePanel;

    private bool isBusy;

    private void Start()
    {
        ResolveReferences();

        if (boardManager == null || playerPiece == null || playerStats == null)
        {
            Debug.LogError("GameController nao conseguiu encontrar todas as referencias necessarias.", this);
            enabled = false;
            return;
        }

        if (boardManager.Tiles.Count == 0)
        {
            boardManager.GenerateBoard();
        }

        playerPiece.Initialize(boardManager);
        RefreshHUD();
        ClearHUDDiceResult();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !playerPiece.IsMoving && !isBusy)
        {
            StartCoroutine(PlayTurn());
        }
    }

    private IEnumerator PlayTurn()
    {
        if (physicalDice == null)
        {
            Debug.LogError("PhysicalDice nao esta configurado no GameController.", this);
            ShowHUDMessage("Configure o dado fisico para continuar.");
            yield break;
        }

        isBusy = true;
        ShowHUDMessage("Rolando dado...");
        ClearHUDDiceResult();

        int diceResult = 0;
        bool hasDiceResult = false;

        yield return StartCoroutine(physicalDice.Roll(result =>
        {
            diceResult = result;
            hasDiceResult = true;
        }));

        if (!hasDiceResult)
        {
            Debug.LogError("O dado fisico nao retornou um resultado valido.", this);
            ShowHUDMessage("Falha ao obter resultado do dado.");
            isBusy = false;
            yield break;
        }

        Debug.Log($"Dado fisico resultou em: {diceResult}");

        yield return StartCoroutine(playerPiece.MoveSteps(diceResult));

        Tile currentTile = boardManager.GetTileByIndex(playerPiece.CurrentTileIndex);

        if (currentTile == null)
        {
            Debug.LogWarning("O jogador terminou o movimento, mas o tile atual nao foi encontrado.", this);
            isBusy = false;
            yield break;
        }

        Debug.Log($"Jogador parou no tile {playerPiece.CurrentTileIndex} - {currentTile.Data.Name}");
        string actionMessage = string.Empty;
        yield return StartCoroutine(ProcessCurrentTile(currentTile, message => actionMessage = message));
        ShowHUDMessage(actionMessage);
        playerStats.PrintStats();
        Debug.Log(ScoreCalculator.GetScoreExplanation(playerStats));
        RefreshHUD();
        isBusy = false;
    }

    private void ResolveReferences()
    {
        if (boardManager == null)
        {
            boardManager = FindObjectOfType<BoardManager>();
        }

        if (playerPiece == null)
        {
            playerPiece = FindObjectOfType<PlayerPiece>();
        }

        if (playerStats == null && playerPiece != null)
        {
            playerStats = playerPiece.GetComponent<PlayerStats>();
        }

        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        if (gameHUD == null)
        {
            gameHUD = FindObjectOfType<GameHUD>();

            if (gameHUD == null)
            {
                Debug.LogWarning("GameHUD nao foi encontrado. O jogo continuara sem interface visual.", this);
            }
        }

        if (physicalDice == null)
        {
            physicalDice = FindObjectOfType<PhysicalDice>();

            if (physicalDice == null)
            {
                Debug.LogWarning("PhysicalDice nao foi encontrado. O jogo ficara sem rolagem fisica do dado.", this);
            }
        }

        if (purchasePanel == null)
        {
            purchasePanel = FindObjectOfType<PurchasePanel>();
        }
    }

    private IEnumerator ProcessCurrentTile(Tile currentTile, System.Action<string> onMessageReady)
    {
        TileData data = currentTile.Data;

        if (data == null)
        {
            Debug.LogWarning("A tile atual nao possui dados configurados.", this);
            onMessageReady?.Invoke("A tile atual nao possui dados configurados.");
            yield break;
        }

        bool canBePurchased = data.Price > 0 &&
                              data.Type != TileType.Start &&
                              data.Type != TileType.Empty;

        if (!canBePurchased)
        {
            onMessageReady?.Invoke(ApplyNonPurchasableTileImpact(data));
            yield break;
        }

        if (currentTile.IsPurchased)
        {
            Debug.Log("Esta propriedade ja foi comprada.");
            onMessageReady?.Invoke("Esta propriedade ja foi comprada.");
            yield break;
        }

        if (purchasePanel == null)
        {
            Debug.LogWarning("PurchasePanel nao esta configurado. A compra sera ignorada.", this);
            onMessageReady?.Invoke($"Painel de compra indisponivel para {data.Name}.");
            yield break;
        }

        bool decisionMade = false;
        bool playerWantsToBuy = false;

        purchasePanel.Show(currentTile, playerStats, decision =>
        {
            playerWantsToBuy = decision;
            decisionMade = true;
        });

        while (!decisionMade)
        {
            yield return null;
        }

        if (!playerWantsToBuy)
        {
            onMessageReady?.Invoke($"Voce decidiu nao comprar {data.Name}.");
            yield break;
        }

        if (!playerStats.CanAfford(data.Price))
        {
            Debug.Log($"Dinheiro insuficiente para comprar {data.Name}");
            onMessageReady?.Invoke($"Dinheiro insuficiente para comprar {data.Name}");
            yield break;
        }

        playerStats.SpendMoney(data.Price);
        currentTile.Purchase();
        playerStats.ApplyTileImpact(data);

        Debug.Log($"Comprou {data.Name} por {data.Price}");
        Debug.Log(
            $"Impactos: Financeiro {data.FinanceImpact} | " +
            $"Bem-estar {data.WellBeingImpact} | " +
            $"Poluicao {data.PollutionImpact}");

        onMessageReady?.Invoke($"Voce comprou {data.Name}.");
    }

    private string ApplyNonPurchasableTileImpact(TileData data)
    {
        if (data.FinanceImpact == 0 && data.WellBeingImpact == 0 && data.PollutionImpact == 0)
        {
            return $"Parou em {data.Name}. Nenhum impacto aplicado.";
        }

        playerStats.ApplyTileImpact(data);

        Debug.Log(
            $"Impactos da tile {data.Name}: Financeiro {data.FinanceImpact} | " +
            $"Bem-estar {data.WellBeingImpact} | " +
            $"Poluicao {data.PollutionImpact}");

        return $"Impactos da tile {data.Name} aplicados.";
    }

    private void RefreshHUD()
    {
        if (gameHUD == null)
        {
            return;
        }

        gameHUD.UpdateStats(playerStats);
    }

    private void ClearHUDDiceResult()
    {
        if (gameHUD == null)
        {
            return;
        }

        gameHUD.ClearDiceResult();
    }

    private void ShowHUDMessage(string message)
    {
        if (gameHUD == null)
        {
            return;
        }

        gameHUD.ShowActionMessage(message);
    }
}
