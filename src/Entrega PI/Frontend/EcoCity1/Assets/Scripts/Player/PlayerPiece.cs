using System.Collections;
using UnityEngine;

public class PlayerPiece : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float heightOffset = 0.75f;

    private BoardManager boardManager;

    public int CurrentTileIndex { get; private set; }
    public bool IsMoving { get; private set; }

    public void Initialize(BoardManager manager)
    {
        boardManager = manager;

        if (boardManager == null)
        {
            Debug.LogError("PlayerPiece nao recebeu uma referencia para o BoardManager.", this);
            return;
        }

        CurrentTileIndex = 0;
        transform.position = GetTileCenterPosition(CurrentTileIndex);
    }

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

        if (boardManager.Tiles.Count == 0)
        {
            Debug.LogError("O tabuleiro nao possui tiles para o jogador percorrer.", this);
            yield break;
        }

        IsMoving = true;

        for (int movedSteps = 0; movedSteps < steps; movedSteps++)
        {
            CurrentTileIndex = (CurrentTileIndex + 1) % boardManager.Tiles.Count;
            Vector3 targetPosition = GetTileCenterPosition(CurrentTileIndex);

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime);

                yield return null;
            }

            transform.position = targetPosition;
        }

        IsMoving = false;
    }

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
}
