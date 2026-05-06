using System.Collections;
using UnityEngine;

public class PlayerPiece : MonoBehaviour
{
    [SerializeField] private float heightOffset = 0.75f;
    [SerializeField] private float hopHeight = 0.32f;
    [SerializeField] private float hopDuration = 0.58f;
    [SerializeField] private float landingPause = 0.08f;

    private BoardManager boardManager;

    public int CurrentTileIndex { get; private set; }
    public bool IsMoving { get; private set; }
    public event System.Action<Vector3> OnMoveStep;
    public event System.Action<Vector3> OnMoveComplete;

    /// <summary>
    /// Garante que o peao nunca seja tratado como objeto estatico.
    /// </summary>
    private void Awake()
    {
        if (gameObject.isStatic)
        {
            gameObject.isStatic = false;
        }
    }

    /// <summary>
    /// Inicializa a peca do jogador com a referencia do tabuleiro.
    /// </summary>
    public void Initialize(BoardManager manager)
    {
        boardManager = manager;

        if (boardManager == null)
        {
            Debug.LogError("PlayerPiece nao recebeu uma referencia para o BoardManager.", this);
            return;
        }

        if (!boardManager.EnsureTilesReady())
        {
            Debug.LogError("PlayerPiece nao encontrou tiles prontos no BoardManager.", this);
            return;
        }

        CurrentTileIndex = 0;
        transform.position = GetTileCenterPosition(CurrentTileIndex);
        OnMoveStep?.Invoke(transform.position);
    }

    /// <summary>
    /// Move a peca a quantidade de casas indicada pelo resultado do dado.
    /// </summary>
    public IEnumerator MoveSteps(int steps)
    {
        if (IsMoving)
        {
            yield break;
        }

        if (boardManager == null)
        {
            Debug.LogError("PlayerPiece precisa ser inicializado antes de se mover.", this);
            yield break;
        }

        if (!boardManager.EnsureTilesReady())
        {
            Debug.LogError("O tabuleiro nao possui tiles para o jogador percorrer.", this);
            yield break;
        }

        IsMoving = true;

        for (int movedSteps = 0; movedSteps < steps; movedSteps++)
        {
            CurrentTileIndex = (CurrentTileIndex + 1) % boardManager.Tiles.Count;
            Vector3 targetPosition = GetTileCenterPosition(CurrentTileIndex);
            Vector3 startPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < hopDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / hopDuration);
                float forwardT = EaseInOutCubic(t);
                float hopOffset = Mathf.Sin(t * Mathf.PI) * hopHeight;

                Vector3 flatPosition = Vector3.Lerp(startPosition, targetPosition, forwardT);
                transform.position = flatPosition + Vector3.up * hopOffset;
                OnMoveStep?.Invoke(transform.position);

                yield return null;
            }

            transform.position = targetPosition;
            OnMoveStep?.Invoke(transform.position);
            yield return new WaitForSeconds(landingPause);
        }

        IsMoving = false;
        transform.position = GetTileCenterPosition(CurrentTileIndex);
        OnMoveComplete?.Invoke(transform.position);
    }

    /// <summary>
    /// Calcula a posicao central da casa informada.
    /// </summary>
    private Vector3 GetTileCenterPosition(int tileIndex)
    {
        Tile tile = boardManager.GetTileByIndex(tileIndex);

        if (tile == null)
        {
            Debug.LogError($"Nao foi possivel encontrar o tile de indice {tileIndex}.", this);
            return transform.position;
        }

        return tile.transform.position + Vector3.up * heightOffset;
    }

    /// <summary>
    /// Suaviza o deslocamento horizontal para o peao parecer avancar com mais peso.
    /// </summary>
    private float EaseInOutCubic(float t)
    {
        if (t < 0.5f)
        {
            return 4f * t * t * t;
        }

        float f = (-2f * t) + 2f;
        return 1f - ((f * f * f) * 0.5f);
    }
}
