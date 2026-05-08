using UnityEngine;

public enum CameraState
{
    Idle,
    Follow,
    Focus,
    Cinematic
}

public class CameraController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform boardCenter;
    [SerializeField] private Transform playerPiece;
    [SerializeField] private Transform resultPoint;

    [Header("Estado Idle - Isometrica")]
    [SerializeField] private float idleHeight = 8f;
    [SerializeField] private float idleDistance = 6f;
    [SerializeField] private float idleAngle = 50f;

    [Header("Estado Follow - Segue Jogador")]
    [SerializeField] private float followHeight = 5f;
    [SerializeField] private float followDistance = 4f;
    [SerializeField] private float followAngle = 45f;
    [SerializeField] private float followSpeed = 4f;

    [Header("Estado Focus - Foca na Propriedade")]
    [SerializeField] private float focusHeight = 2.2f;
    [SerializeField] private float focusDistance = 2.6f;
    [SerializeField] private float focusAngle = 30f;
    [SerializeField] private float focusSpeed = 3f;
    [SerializeField] private float focusOffsetX = 1.5f;
    [Header("Estado Cinematic")]
    [SerializeField] private float cinematicHeight = 3.4f;
    [SerializeField] private float cinematicDistance = 5.2f;
    [SerializeField] private float cinematicAngle = 18f;
    [SerializeField] private float cinematicSpeed = 2.5f;
    [SerializeField] private float cinematicSideOffset = 1.15f;
    [Header("Screen Shake")]
    [SerializeField] private float shakeIntensity = 0.015f;
    [SerializeField] private float shakeSpeed = 18f;

    public Transform ResultPoint => resultPoint;

    private CameraState currentState = CameraState.Idle;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isShaking = false;
    private float currentShakePower = 0f;
    private Vector3 basePosition;

    /// <summary>
    /// Posiciona a camera no enquadramento inicial do tabuleiro.
    /// </summary>
    private void Start()
    {
        ResolveReferences();
        targetPosition = CalculateIdlePosition();
        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    /// <summary>
    /// Atualiza a camera de acordo com o estado atual do jogo.
    /// </summary>
    private void Update()
    {
        if (boardCenter == null)
        {
            return;
        }

        if (currentState == CameraState.Idle)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 3f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
        }
        else if (currentState == CameraState.Follow)
        {
            UpdateFollowState();
        }
        else if (currentState == CameraState.Focus)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * focusSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * focusSpeed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * cinematicSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * cinematicSpeed);
        }

        basePosition = targetPosition;

        if (isShaking)
        {
            float magnitude = shakeIntensity * currentShakePower;
            Vector3 shake = new Vector3(
                Mathf.Sin(Time.time * shakeSpeed) * magnitude,
                Mathf.Sin(Time.time * shakeSpeed * 1.3f) * magnitude * 0.6f,
                0f);

            transform.position = targetPosition + shake;
        }
    }

    /// <summary>
    /// Define o estado atual da camera e recalcula o alvo quando necessario.
    /// </summary>
    public void SetState(CameraState newState, Vector3? focusTarget = null)
    {
        currentState = newState;

        if (currentState == CameraState.Idle)
        {
            targetPosition = CalculateIdlePosition();
            return;
        }

        if (currentState == CameraState.Focus && focusTarget.HasValue)
        {
            CalculateFocusPosition(focusTarget.Value);
            return;
        }

        if (currentState == CameraState.Cinematic)
        {
            CalculateCinematicPosition();
        }
    }

    /// <summary>
    /// Coloca a camera na posicao cinematica para destacar o jogador e a construcao.
    /// </summary>
    public void SetCinematic()
    {
        SetState(CameraState.Cinematic);
    }

    /// <summary>
    /// Retorna o ponto usado para exibir o resultado do dado na frente da camera.
    /// </summary>
    public Transform GetResultPoint()
    {
        return resultPoint;
    }

    /// <summary>
    /// Inicia a vibracao da camera durante o carregamento do lancamento.
    /// </summary>
    public void StartShake(float power)
    {
        isShaking = true;
        currentShakePower = power;
        basePosition = transform.position;
    }

    /// <summary>
    /// Atualiza a intensidade da vibracao enquanto o jogador carrega o dado.
    /// </summary>
    public void UpdateShake(float power)
    {
        currentShakePower = power;
    }

    /// <summary>
    /// Interrompe a vibracao e restaura a camera para a posicao base.
    /// </summary>
    public void StopShake()
    {
        isShaking = false;
        currentShakePower = 0f;
        transform.position = basePosition;
    }

    /// <summary>
    /// Atualiza em tempo real o enquadramento de acompanhamento da peca.
    /// </summary>
    private void UpdateFollowState()
    {
        if (playerPiece == null)
        {
            return;
        }

        Vector3 backwardOffset = Quaternion.Euler(followAngle, 0f, 0f) * Vector3.back * followDistance;
        Vector3 followTarget = playerPiece.position + Vector3.up * followHeight + backwardOffset;
        targetPosition = followTarget;

        transform.position = Vector3.Lerp(transform.position, followTarget, Time.deltaTime * followSpeed);

        Quaternion lookRot = Quaternion.LookRotation(playerPiece.position - transform.position);
        targetRotation = lookRot;
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * followSpeed);
    }

    /// <summary>
    /// Calcula a posicao e a rotacao de foco para destacar a casa atual.
    /// </summary>
    private void CalculateFocusPosition(Vector3 tilePos)
    {
        Vector3 backwardOffset = Quaternion.Euler(focusAngle, 0f, 0f) * Vector3.back * focusDistance;
        Vector3 pos = tilePos + Vector3.up * focusHeight + backwardOffset + Vector3.right * focusOffsetX;
        Quaternion rot = Quaternion.LookRotation(tilePos - pos);

        targetPosition = pos;
        targetRotation = rot;
    }

    /// <summary>
    /// Calcula a posicao da camera cinematica atras do jogador olhando para o centro.
    /// </summary>
    private void CalculateCinematicPosition()
    {
        if (playerPiece == null || boardCenter == null)
        {
            return;
        }

        Vector3 toBoard = (boardCenter.position - playerPiece.position);
        Vector3 horizontalToBoard = new Vector3(toBoard.x, 0f, toBoard.z).normalized;

        if (horizontalToBoard.sqrMagnitude < 0.001f)
        {
            horizontalToBoard = Vector3.forward;
        }

        Vector3 backwardDirection = -horizontalToBoard;
        Vector3 sideDirection = Vector3.Cross(Vector3.up, horizontalToBoard).normalized;
        Vector3 backwardOffset = backwardDirection * cinematicDistance;
        Vector3 sideOffset = sideDirection * cinematicSideOffset;
        Vector3 heightOffset = Vector3.up * cinematicHeight;
        Vector3 tiltOffset = Quaternion.Euler(cinematicAngle, 0f, 0f) * Vector3.back * 0.35f;

        targetPosition = playerPiece.position + backwardOffset + sideOffset + heightOffset + tiltOffset;

        Vector3 lookTarget = boardCenter.position + Vector3.up * 1.15f;
        targetRotation = Quaternion.LookRotation(lookTarget - targetPosition);
    }

    /// <summary>
    /// Calcula a posicao isometrica padrao do tabuleiro.
    /// </summary>
    private Vector3 CalculateIdlePosition()
    {
        Vector3 backwardOffset = Quaternion.Euler(idleAngle, 0f, 0f) * Vector3.back * idleDistance;
        Vector3 pos = boardCenter.position + Vector3.up * idleHeight + backwardOffset;
        targetRotation = Quaternion.Euler(idleAngle, 0f, 0f);
        return pos;
    }

    /// <summary>
    /// Procura automaticamente referencias essenciais quando possivel.
    /// </summary>
    private void ResolveReferences()
    {
        if (playerPiece == null)
        {
            PlayerPiece piece = FindObjectOfType<PlayerPiece>();

            if (piece != null)
            {
                playerPiece = piece.transform;
            }
        }
    }
}
