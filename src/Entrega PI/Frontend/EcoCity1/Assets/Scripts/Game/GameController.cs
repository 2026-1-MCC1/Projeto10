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
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private TileVisualManager visualManager;

    private bool isBusy;
    private int pendingDiceResult;
    private bool hasPendingDiceResult;

    /// <summary>
    /// Garante a inscricao nos eventos ao habilitar o controlador.
    /// </summary>
    private void OnEnable()
    {
        SubscribeToRoundManager();
        SubscribeToPlayerPiece();
    }

    /// <summary>
    /// Inicializa as referencias necessarias e prepara o estado inicial do jogo.
    /// </summary>
    private void Start()
    {
        ResolveReferences();

        if (boardManager == null || playerPiece == null || playerStats == null || roundManager == null)
        {
            Debug.LogError("GameController nao conseguiu encontrar todas as referencias necessarias.", this);
            enabled = false;
            return;
        }

        SubscribeToRoundManager();
        SubscribeToPlayerPiece();

        if (boardManager.Tiles.Count == 0)
        {
            boardManager.GenerateBoard();
        }

        playerPiece.Initialize(boardManager);
        cameraController?.SetState(CameraState.Idle);
        visualManager?.SetCurrentTile(playerPiece.CurrentTileIndex);
        RefreshHUD();
        ClearHUDDiceResult();
    }

    /// <summary>
    /// Remove inscricoes em eventos quando o objeto e desativado.
    /// </summary>
    private void OnDisable()
    {
        if (roundManager != null)
        {
            roundManager.OnGameOver -= HandleGameOver;
        }

        if (playerPiece != null)
        {
            playerPiece.OnMoveStep -= HandlePlayerMoveStep;
            playerPiece.OnMoveComplete -= HandlePlayerMoveComplete;
        }
    }

    /// <summary>
    /// Monitora a entrada do jogador para iniciar uma nova jogada.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !playerPiece.IsMoving && !isBusy)
        {
            StartCoroutine(PlayTurn());
        }
    }

    /// <summary>
    /// Executa o fluxo completo de uma jogada do jogador.
    /// </summary>
    private IEnumerator PlayTurn()
    {
        if (roundManager != null && !roundManager.CanRoll)
        {
            yield break;
        }

        if (physicalDice == null)
        {
            Debug.LogError("PhysicalDice nao esta configurado no GameController.", this);
            ShowHUDMessage("Configure o dado fisico para continuar.");
            yield break;
        }

        isBusy = true;
        cameraController?.SetState(CameraState.Idle);
        ShowHUDMessage("Rolando dado...");
        ClearHUDDiceResult();

        hasPendingDiceResult = false;
        pendingDiceResult = 0;
        physicalDice.RollDice(OnDiceResult);

        yield return new WaitUntil(() => hasPendingDiceResult);

        if (pendingDiceResult <= 0)
        {
            Debug.LogError("O dado fisico nao retornou um resultado valido.", this);
            ShowHUDMessage("Falha ao obter resultado do dado.");
            isBusy = false;
            yield break;
        }

        yield return StartCoroutine(MoveAndRegister(pendingDiceResult));

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

    /// <summary>
    /// Localiza automaticamente referencias importantes quando necessario.
    /// </summary>
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

        if (roundManager == null)
        {
            roundManager = FindObjectOfType<RoundManager>();
        }

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }

        if (visualManager == null)
        {
            visualManager = FindObjectOfType<TileVisualManager>();
        }
    }

    /// <summary>
    /// Recebe o resultado do dado fisico e prepara a continuidade do turno.
    /// </summary>
    private void OnDiceResult(int result)
    {
        pendingDiceResult = result;
        hasPendingDiceResult = true;
        Debug.Log($"Dado fisico resultou em: {result}");
    }

    /// <summary>
    /// Move o jogador e registra a rodada depois que o deslocamento termina.
    /// </summary>
    private IEnumerator MoveAndRegister(int result)
    {
        yield return StartCoroutine(playerPiece.MoveSteps(result));

        if (roundManager != null)
        {
            roundManager.RegisterRoll();
        }
    }

    /// <summary>
    /// Inscreve o controlador nos eventos do gerenciador de rodadas.
    /// </summary>
    private void SubscribeToRoundManager()
    {
        if (roundManager == null)
        {
            return;
        }

        roundManager.OnGameOver -= HandleGameOver;
        roundManager.OnGameOver += HandleGameOver;
    }

    /// <summary>
    /// Inscreve o controlador nos eventos de movimento da peca.
    /// </summary>
    private void SubscribeToPlayerPiece()
    {
        if (playerPiece == null)
        {
            return;
        }

        playerPiece.OnMoveStep -= HandlePlayerMoveStep;
        playerPiece.OnMoveStep += HandlePlayerMoveStep;
        playerPiece.OnMoveComplete -= HandlePlayerMoveComplete;
        playerPiece.OnMoveComplete += HandlePlayerMoveComplete;
    }

    /// <summary>
    /// Coloca a camera em seguimento enquanto o jogador avanca pelo tabuleiro.
    /// </summary>
    private void HandlePlayerMoveStep(Vector3 stepPosition)
    {
        cameraController?.SetState(CameraState.Follow);
    }

    /// <summary>
    /// Foca a camera na casa final onde o jogador parou.
    /// </summary>
    private void HandlePlayerMoveComplete(Vector3 finalPosition)
    {
        if (boardManager == null || playerPiece == null)
        {
            return;
        }

        Tile currentTile = boardManager.GetTileByIndex(playerPiece.CurrentTileIndex);
        Vector3 focusPosition = currentTile != null ? currentTile.transform.position : finalPosition;
        cameraController?.SetState(CameraState.Focus, focusPosition);
        visualManager?.SetCurrentTile(playerPiece.CurrentTileIndex);
    }

    /// <summary>
    /// Processa os efeitos da casa atual apos o termino do movimento.
    /// </summary>
    private IEnumerator ProcessCurrentTile(Tile currentTile, System.Action<string> onMessageReady)
    {
        TileData data = currentTile.Data;

        if (data == null)
        {
            Debug.LogWarning("A tile atual nao possui dados configurados.", this);
            onMessageReady?.Invoke("A tile atual nao possui dados configurados.");
            yield break;
        }

        if (currentTile.CanBePurchased())
        {
            if (purchasePanel == null)
            {
                Debug.LogWarning("PurchasePanel nao esta configurado. A compra sera ignorada.", this);
                onMessageReady?.Invoke($"Painel de compra indisponivel para {data.Name}.");
                OnPurchaseDecision();
                yield break;
            }

            bool decisionMade = false;
            int previousMoney = playerStats.Money;
            bool alreadyOwnedAfterDecision = false;

            roundManager?.SetCanRoll(false);
            cameraController?.SetState(CameraState.Focus, currentTile.transform.position);
            purchasePanel.ShowPurchase(currentTile, playerStats, () =>
            {
                alreadyOwnedAfterDecision = currentTile.IsPurchased && currentTile.owner == playerStats;
                decisionMade = true;
            });

            while (!decisionMade)
            {
                yield return null;
            }

            if (alreadyOwnedAfterDecision && playerStats.Money < previousMoney)
            {
                visualManager?.MarkAsPurchased(currentTile.TileIndex);
                Debug.Log($"Comprou {data.Name} por {data.purchasePrice}");
                Debug.Log(
                    $"Impactos: Financeiro {data.moneyImpact} | " +
                    $"Bem-estar {data.wellBeingImpact} | " +
                    $"Poluicao {data.pollutionImpact}");
                onMessageReady?.Invoke($"Voce comprou {data.Name}.");
            }
            else
            {
                onMessageReady?.Invoke($"Voce decidiu nao comprar {data.Name}.");
            }

            OnPurchaseDecision();
            yield break;
        }

        if (currentTile.IsPurchased && currentTile.owner != null && currentTile.owner != playerStats)
        {
            if (purchasePanel == null)
            {
                playerStats.SpendMoney(data.rentPrice);
                onMessageReady?.Invoke($"Voce pagou aluguel: ${data.rentPrice}");
                OnPurchaseDecision();
                yield break;
            }

            bool decisionMade = false;
            roundManager?.SetCanRoll(false);
            purchasePanel.ShowRentMessage(currentTile, playerStats, () => decisionMade = true);

            while (!decisionMade)
            {
                yield return null;
            }

            onMessageReady?.Invoke($"Voce pagou aluguel: ${data.rentPrice}");
            OnPurchaseDecision();
            yield break;
        }

        onMessageReady?.Invoke(ApplyNonPurchasableTileImpact(data));
        OnPurchaseDecision();
    }

    /// <summary>
    /// Aplica os efeitos de uma casa que nao pode ser comprada.
    /// </summary>
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

    /// <summary>
    /// Atualiza os valores exibidos na interface do jogador.
    /// </summary>
    private void RefreshHUD()
    {
        if (gameHUD == null)
        {
            return;
        }

        gameHUD.UpdateStats(playerStats);
    }

    /// <summary>
    /// Limpa o resultado anterior do dado na interface.
    /// </summary>
    private void ClearHUDDiceResult()
    {
        if (gameHUD == null)
        {
            return;
        }

        gameHUD.ClearDiceResult();
    }

    /// <summary>
    /// Exibe uma mensagem de acao na interface do jogo.
    /// </summary>
    private void ShowHUDMessage(string message)
    {
        if (gameHUD == null)
        {
            return;
        }

        gameHUD.ShowActionMessage(message);
    }

    /// <summary>
    /// Reage ao encerramento da partida quando o limite de rodadas e atingido.
    /// </summary>
    private void HandleGameOver()
    {
        Debug.Log("Jogo encerrado! " + roundManager.CurrentRound + " rodadas jogadas.");
    }

    /// <summary>
    /// Finaliza a interacao com a casa atual e libera a proxima jogada.
    /// </summary>
    private void OnPurchaseDecision()
    {
        cameraController?.SetState(CameraState.Idle);
        roundManager?.SetCanRoll(true);
        RefreshHUD();
    }
}
