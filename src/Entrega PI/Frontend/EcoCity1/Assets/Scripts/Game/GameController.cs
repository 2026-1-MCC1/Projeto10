using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    [SerializeField] private PropertySpawner propertySpawner;
    [SerializeField] private FinalResultScreen finalResultScreen;
    [SerializeField] private CityEventSystem cityEventSystem;
    [SerializeField] private GameAudioController gameAudioController;
    [Header("Debug de Finais")]
    [SerializeField] private bool enableEndingDebugPreview = true;

    private bool isBusy;
    private int pendingDiceResult;
    private bool hasPendingDiceResult;
    private bool pendingGameOver;
    private bool finalScreenShown;
    private bool isPreviewingEnding;
    private bool isPaused;
    private GameObject pausePanelRoot;

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

        roundManager?.ResetRounds();
        isBusy = false;
        pendingDiceResult = 0;
        hasPendingDiceResult = false;
        pendingGameOver = false;
        finalScreenShown = false;
        isPreviewingEnding = false;

        if (!boardManager.EnsureTilesReady())
        {
            boardManager.GenerateBoard();
        }

        playerPiece.Initialize(boardManager);
        cameraController?.SetState(CameraState.Idle);
        visualManager?.SetCurrentTile(playerPiece.CurrentTileIndex);
        RefreshHUD();
        UpdateRoundHUD();
        ClearHUDDiceResult();
        gameHUD?.ShowTutorialSequence();
        BuildPauseMenu();
        SetGameplayCursorState();
    }

    /// <summary>
    /// Remove inscricoes em eventos quando o objeto e desativado.
    /// </summary>
    private void OnDisable()
    {
        if (roundManager != null)
        {
            roundManager.OnGameOver -= HandleGameOver;
            roundManager.OnRoundChanged -= HandleRoundChanged;
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
        HandleDebugEndingPreviewInput();

        if (finalScreenShown || isPreviewingEnding)
        {
            UpdateCursorState();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        UpdateCursorState();

        if (isPaused)
        {
            return;
        }

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
        gameAudioController?.PlayRoll();

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

        if (pendingGameOver && !finalScreenShown)
        {
            ShowFinalScreen();
            yield break;
        }

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

        if (propertySpawner == null)
        {
            propertySpawner = FindObjectOfType<PropertySpawner>();
        }

        if (cityEventSystem == null)
        {
            cityEventSystem = FindObjectOfType<CityEventSystem>();

            if (cityEventSystem == null)
            {
                GameObject eventObject = new GameObject("CityEventSystem");
                cityEventSystem = eventObject.AddComponent<CityEventSystem>();
            }
        }

        if (gameAudioController == null)
        {
            gameAudioController = FindObjectOfType<GameAudioController>();

            if (gameAudioController == null)
            {
                GameObject audioObject = new GameObject("GameAudioController");
                gameAudioController = audioObject.AddComponent<GameAudioController>();
            }
        }

        if (finalResultScreen == null)
        {
            finalResultScreen = FindObjectOfType<FinalResultScreen>();

            if (finalResultScreen == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();

                if (canvas != null)
                {
                    GameObject screenObject = new GameObject("FinalResultScreen", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(FinalResultScreen));
                    screenObject.transform.SetParent(canvas.transform, false);
                    finalResultScreen = screenObject.GetComponent<FinalResultScreen>();
                }
            }
        }
    }

    /// <summary>
    /// Recebe o resultado do dado fisico e prepara a continuidade do turno.
    /// </summary>
    private void OnDiceResult(int result)
    {
        pendingDiceResult = result;
        hasPendingDiceResult = true;
        gameHUD?.UpdateDiceResult(result);
        Debug.Log($"Dado fisico resultou em: {result}");
    }

    /// <summary>
    /// Move o jogador e registra a rodada depois que o deslocamento termina.
    /// </summary>
    private IEnumerator MoveAndRegister(int result)
    {
        cameraController?.SetState(CameraState.Follow);
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
        roundManager.OnRoundChanged -= HandleRoundChanged;
        roundManager.OnRoundChanged += HandleRoundChanged;
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

        if (!boardManager.EnsureTilesReady())
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

        yield return new WaitForSeconds(0.2f);

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
            propertySpawner?.ShowProperty(currentTile);
            cameraController?.SetCinematic();
            yield return new WaitForSeconds(0.8f);
            SetInteractiveCursorState();
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
                propertySpawner?.ConfirmPurchase();
                visualManager?.MarkAsPurchased(currentTile.TileIndex);
                gameAudioController?.PlayPurchase();
                Debug.Log($"Comprou {data.Name} por {data.purchasePrice}");
                Debug.Log(
                    $"Impactos: Financeiro {data.moneyImpact} | " +
                    $"Bem-estar {data.wellBeingImpact} | " +
                    $"Poluicao {data.pollutionImpact}");
                onMessageReady?.Invoke($"Você comprou {data.Name}.");
                gameHUD?.ShowTemporaryActionMessage(
                    $"Compra concluída: {data.Name} | {FormatSignedMoney(data.moneyImpact)}/rodada | Bem-estar {FormatSignedValue(data.wellBeingImpact)} | Poluição {FormatSignedValue(data.pollutionImpact)}",
                    3.4f);
            }
            else
            {
                propertySpawner?.CancelPurchase();
                gameAudioController?.PlayPass();
                onMessageReady?.Invoke($"Você decidiu não comprar {data.Name}.");
                gameHUD?.ShowTemporaryActionMessage($"Você passou a compra de {data.Name}.", 2.2f);
            }

            yield return new WaitForSeconds(0.6f);
            OnPurchaseDecision();
            yield break;
        }

        if (currentTile.IsPurchased && currentTile.owner != null && currentTile.owner != playerStats)
        {
            if (purchasePanel == null)
            {
                playerStats.SpendMoney(data.rentPrice);
                gameAudioController?.PlayRent();
                onMessageReady?.Invoke($"Você pagou aluguel: ${data.rentPrice}");
                gameHUD?.ShowTemporaryActionMessage($"Aluguel pago: -${data.rentPrice}", 2.2f);
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

            gameAudioController?.PlayRent();
            onMessageReady?.Invoke($"Você pagou aluguel: ${data.rentPrice}");
            gameHUD?.ShowTemporaryActionMessage($"Aluguel pago: -${data.rentPrice}", 2.2f);
            OnPurchaseDecision();
            yield break;
        }

        if (data.Type == TileType.Start)
        {
            CityEventResult startEvent = cityEventSystem != null
                ? cityEventSystem.ApplyStartBonus(playerStats)
                : null;

            if (startEvent != null && startEvent.Triggered)
            {
                gameAudioController?.PlayEventTone(startEvent.Tone);
                onMessageReady?.Invoke(startEvent.Message);
                gameHUD?.ShowTemporaryActionMessage(cityEventSystem.BuildEventSummary(startEvent), 3.4f);
                RefreshHUD();
                OnPurchaseDecision();
                yield break;
            }
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
        pendingGameOver = true;
        roundManager?.SetCanRoll(false);
        Debug.Log("Jogo encerrado! " + roundManager.CurrentRound + " rodadas jogadas.");

        if (!isBusy && !finalScreenShown)
        {
            ShowFinalScreen();
        }
    }

    /// <summary>
    /// Aplica os ganhos e custos recorrentes das propriedades compradas ao fim de cada rodada.
    /// </summary>
    private void HandleRoundChanged(int roundNumber)
    {
        if (boardManager == null || playerStats == null)
        {
            return;
        }

        int totalRecurringMoney = 0;

        foreach (Tile tile in boardManager.Tiles)
        {
            if (tile == null || !tile.IsPurchased || tile.owner != playerStats || tile.Data == null)
            {
                continue;
            }

            totalRecurringMoney += tile.Data.moneyImpact;
        }

        if (totalRecurringMoney != 0)
        {
            playerStats.ApplyImpacts(totalRecurringMoney, 0, 0);
            Debug.Log($"Renda recorrente da rodada {roundNumber}: {totalRecurringMoney}");
            gameHUD?.ShowTemporaryActionMessage($"Renda das propriedades: {FormatSignedMoney(totalRecurringMoney)}", 2.8f);
        }

        if (cityEventSystem != null)
        {
            CityEventResult roundEvent = cityEventSystem.TryApplyRoundEvent(roundNumber, playerStats);

            if (roundEvent != null && roundEvent.Triggered)
            {
                gameAudioController?.PlayEventTone(roundEvent.Tone);
                Debug.Log($"Evento de cidade: {roundEvent.Title} | {roundEvent.Message}");
                gameHUD?.ShowTemporaryActionMessage(cityEventSystem.BuildEventSummary(roundEvent), 3.8f);
            }
        }

        RefreshHUD();
        UpdateRoundHUD();
    }

    /// <summary>
    /// Finaliza a interacao com a casa atual e libera a proxima jogada.
    /// </summary>
    private void OnPurchaseDecision()
    {
        cameraController?.SetState(CameraState.Idle);
        RefreshHUD();
        UpdateRoundHUD();
        SetGameplayCursorState();

        if (!pendingGameOver)
        {
            roundManager?.SetCanRoll(true);
        }
    }

    /// <summary>
    /// Exibe a tela final com a classificacao da cidade ao termino da partida.
    /// </summary>
    private void ShowFinalScreen()
    {
        pendingGameOver = false;
        finalScreenShown = true;
        isBusy = true;
        cameraController?.SetState(CameraState.Idle);
        gameHUD?.SetGameplayUIVisible(false);
        ShowHUDMessage(string.Empty);
        ClearHUDDiceResult();
        SetInteractiveCursorState();

        FinalEvaluationResult result = EndingEvaluator.Evaluate(playerStats);
        finalResultScreen?.Show(result, playerStats);
        gameAudioController?.PlayGameOver();

        Debug.Log(
            $"Final da partida: {result.Title} | " +
            $"Slot de imagem sugerido: {result.ImageSlotName} | " +
            $"{EndingEvaluator.BuildStatsSummary(playerStats).Replace('\n', ' ')}");
    }

    /// <summary>
    /// Permite abrir rapidamente qualquer tela final no editor sem jogar 12 rodadas.
    /// </summary>
    private void HandleDebugEndingPreviewInput()
    {
        if (!enableEndingDebugPreview)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isPreviewingEnding)
        {
            HideDebugEndingPreview();
            return;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowDebugEndingPreview(CityEndingType.GestorExemplar);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            ShowDebugEndingPreview(CityEndingType.BomGestor);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            ShowDebugEndingPreview(CityEndingType.GestaoDesequilibrada);
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            ShowDebugEndingPreview(CityEndingType.MagnataSemEscrupulos);
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            ShowDebugEndingPreview(CityEndingType.CidadeEmColapso);
        }
    }

    /// <summary>
    /// Abre uma tela final de teste e pausa temporariamente a jogabilidade.
    /// </summary>
    private void ShowDebugEndingPreview(CityEndingType endingType)
    {
        ResolveReferences();

        FinalEvaluationResult previewResult = EndingEvaluator.CreatePreviewResult(endingType);
        finalResultScreen?.Show(previewResult, playerStats);
        gameHUD?.SetGameplayUIVisible(false);
        isPreviewingEnding = true;
        isBusy = true;
        SetInteractiveCursorState();
    }

    /// <summary>
    /// Fecha a tela final de teste e devolve a interface normal do jogo.
    /// </summary>
    private void HideDebugEndingPreview()
    {
        finalResultScreen?.HideImmediate();
        gameHUD?.SetGameplayUIVisible(true);
        RefreshHUD();
        UpdateRoundHUD();
        isPreviewingEnding = false;
        isBusy = false;
        SetGameplayCursorState();
    }

    /// <summary>
    /// Cria um menu de pausa simples em runtime.
    /// </summary>
    private void BuildPauseMenu()
    {
        if (pausePanelRoot != null)
        {
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            return;
        }

        pausePanelRoot = new GameObject("PauseMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pausePanelRoot.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = pausePanelRoot.GetComponent<RectTransform>();
        StretchRect(rootRect);
        Image overlay = pausePanelRoot.GetComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.78f);

        GameObject cardObject = new GameObject("PauseCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cardObject.transform.SetParent(pausePanelRoot.transform, false);
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.35f, 0.20f);
        cardRect.anchorMax = new Vector2(0.65f, 0.80f);
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;
        Image cardImage = cardObject.GetComponent<Image>();
        cardImage.color = new Color(0.08f, 0.12f, 0.18f, 0.96f);

        TextMeshProUGUI title = CreatePauseText("PauseTitle", cardObject.transform, "JOGO PAUSADO", 34f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.10f, 0.82f);
        titleRect.anchorMax = new Vector2(0.90f, 0.94f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        PositionPauseButton(CreatePauseButton(cardObject.transform, "CONTINUAR", new Color(0.24f, 0.67f, 0.30f, 1f), ResumeGameplay), 0.58f);
        PositionPauseButton(CreatePauseButton(cardObject.transform, "REINICIAR", new Color(0.22f, 0.52f, 0.86f, 1f), RestartCurrentGame), 0.42f);
        PositionPauseButton(CreatePauseButton(cardObject.transform, "MENU INICIAL", new Color(0.25f, 0.35f, 0.47f, 1f), ReturnToMainMenuFromPause), 0.26f);
        PositionPauseButton(CreatePauseButton(cardObject.transform, "SAIR DO JOGO", new Color(0.72f, 0.31f, 0.31f, 1f), QuitGameFromPause), 0.10f);

        pausePanelRoot.SetActive(false);
    }

    /// <summary>
    /// Alterna o estado do menu de pausa.
    /// </summary>
    private void TogglePauseMenu()
    {
        if (finalScreenShown || isPreviewingEnding)
        {
            return;
        }

        if (purchasePanel != null && purchasePanel.IsVisible)
        {
            return;
        }

        BuildPauseMenu();

        if (pausePanelRoot == null)
        {
            return;
        }

        isPaused = !isPaused;
        pausePanelRoot.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        roundManager?.SetCanRoll(!isPaused && !pendingGameOver);
        UpdateCursorState();
    }

    /// <summary>
    /// Retoma a partida a partir do menu de pausa.
    /// </summary>
    private void ResumeGameplay()
    {
        isPaused = false;

        if (pausePanelRoot != null)
        {
            pausePanelRoot.SetActive(false);
        }

        Time.timeScale = 1f;
        roundManager?.SetCanRoll(!pendingGameOver);
        SetGameplayCursorState();
    }

    /// <summary>
    /// Reinicia a cena atual.
    /// </summary>
    private void RestartCurrentGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Volta ao menu principal.
    /// </summary>
    private void ReturnToMainMenuFromPause()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Encerra o jogo ou o Play Mode.
    /// </summary>
    private void QuitGameFromPause()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Atualiza visibilidade do cursor conforme o contexto atual.
    /// </summary>
    private void UpdateCursorState()
    {
        bool shouldShowCursor =
            isPaused ||
            finalScreenShown ||
            isPreviewingEnding ||
            (purchasePanel != null && purchasePanel.IsVisible);

        if (shouldShowCursor)
        {
            SetInteractiveCursorState();
        }
        else
        {
            SetGameplayCursorState();
        }
    }

    /// <summary>
    /// Esconde o cursor durante a jogabilidade.
    /// </summary>
    private void SetGameplayCursorState()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Mostra o cursor quando a interface precisa de clique.
    /// </summary>
    private void SetInteractiveCursorState()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Cria um botao do menu de pausa.
    /// </summary>
    private RectTransform CreatePauseButton(Transform parent, string label, Color color, UnityEngine.Events.UnityAction callback)
    {
        GameObject buttonObject = new GameObject(label.Replace(" ", string.Empty) + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = color;
        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.08f;
        colors.pressedColor = color * 0.92f;
        colors.selectedColor = color;
        button.colors = colors;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);

        TextMeshProUGUI labelText = CreatePauseText("Label", buttonObject.transform, label, 20f, FontStyles.Bold, TextAlignmentOptions.Center);
        StretchRect(labelText.rectTransform);
        return buttonObject.GetComponent<RectTransform>();
    }

    /// <summary>
    /// Cria um texto TMP do menu de pausa.
    /// </summary>
    private TextMeshProUGUI CreatePauseText(string objectName, Transform parent, string value, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.enableWordWrapping = true;
        return text;
    }

    /// <summary>
    /// Posiciona um botao no cartao de pausa.
    /// </summary>
    private void PositionPauseButton(RectTransform rectTransform, float yMin)
    {
        rectTransform.anchorMin = new Vector2(0.12f, yMin);
        rectTransform.anchorMax = new Vector2(0.88f, yMin + 0.10f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Estica um rect para preencher seu pai.
    /// </summary>
    private void StretchRect(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// Atualiza o texto de rodada na HUD usando os dados atuais do RoundManager.
    /// </summary>
    private void UpdateRoundHUD()
    {
        if (gameHUD == null || roundManager == null)
        {
            return;
        }

        gameHUD.UpdateRoundInfo(roundManager.CurrentRound, roundManager.MaxRounds);
    }

    /// <summary>
    /// Formata um valor financeiro com sinal para os resumos da interface.
    /// </summary>
    private string FormatSignedMoney(int value)
    {
        return value >= 0 ? $"+${value}" : $"-${Mathf.Abs(value)}";
    }

    /// <summary>
    /// Formata qualquer valor inteiro com sinal explicito.
    /// </summary>
    private string FormatSignedValue(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }
}
