using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalDice : MonoBehaviour
{
    [SerializeField] private float settleTime = 2.5f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform faceReference;
    [SerializeField] private LayerMask groundLayer;
    [Header("Power Charge")]
    [SerializeField] private float minForce = 3f;
    [SerializeField] private float maxForce = 12f;
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private float minTorque = 4f;
    [SerializeField] private float maxTorque = 14f;
    [Header("Lancamento pela Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform boardCenter;
    [SerializeField] private float spawnDistanceFromCamera = 0.8f;
    [SerializeField] private float launchAngleVariance = 15f;
    [Header("Spin no Lancamento")]
    [SerializeField] private float minSpin = 180f;
    [SerializeField] private float maxSpin = 900f;
    [Header("Camera Result")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float floatHeight = 0.3f;
    [SerializeField] private float moveTocameraDuration = 0.8f;
    [SerializeField] private float showResultDuration = 1.8f;
    [SerializeField] private float returnDuration = 0.6f;
    [Header("Audio")]
    [SerializeField] private AudioSource chargeAudio;
    [SerializeField] private float minPitch = 0.6f;
    [SerializeField] private float maxPitch = 1.6f;
    [Header("Mapeamento visual das faces")]
    [SerializeField] private int upFaceValue = 1;
    [SerializeField] private int downFaceValue = 6;
    [SerializeField] private int forwardFaceValue = 2;
    [SerializeField] private int backFaceValue = 5;
    [SerializeField] private int rightFaceValue = 3;
    [SerializeField] private int leftFaceValue = 4;
    [SerializeField] private CameraController cameraController;

    private Rigidbody rb;
    private Camera cachedCamera;
    private bool isRolling = false;
    private Action<int> onRollComplete;
    private Dictionary<Vector3, int> faceDirections;
    private Renderer[] cachedRenderers;
    private Collider[] cachedColliders;
    private bool waitingForCharge;
    private bool isCharging = false;
    private float chargeStartTime = 0f;
    private float currentChargePower = 0f;
    private Vector3 lastRestPosition;
    private Vector3 defaultScale;

    public event Action<float> OnChargePowerChanged;
    public float CurrentChargePower => waitingForCharge ? Mathf.Max(currentChargePower, isCharging ? 0.01f : 0f) : 0f;
    public bool IsWaitingForCharge => waitingForCharge;
    public bool IsCharging => isCharging;

    /// <summary>
    /// Inicializa o Rigidbody e o mapeamento das faces do dado.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("PhysicalDice precisa de um Rigidbody no mesmo GameObject.", this);
            return;
        }

        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedColliders = GetComponentsInChildren<Collider>(true);
        defaultScale = transform.localScale;
        cachedCamera = cameraTransform != null ? cameraTransform.GetComponent<Camera>() : null;

        rb.mass = 1f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 1.5f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        SetDiceVisible(false);
    }

    /// <summary>
    /// Monitora a entrada do jogador para carregar e soltar o lancamento do dado.
    /// </summary>
    private void Update()
    {
        if (!waitingForCharge)
        {
            return;
        }

        if (!isCharging && (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space)))
        {
            BeginCharge();
        }

        if (Input.GetKey(KeyCode.Space) && isCharging)
        {
            currentChargePower = Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeTime);
            cameraController?.UpdateShake(currentChargePower);
            UpdateChargeAudio();
            OnChargePowerChanged?.Invoke(CurrentChargePower);
        }

        if (Input.GetKeyUp(KeyCode.Space) && isCharging)
        {
            isCharging = false;
            waitingForCharge = false;

            float chargedForce = Mathf.Lerp(minForce, maxForce, currentChargePower);
            float chargedTorque = Mathf.Lerp(minTorque, maxTorque, currentChargePower);
            float chargedPower = currentChargePower;

            transform.localScale = defaultScale;
            cameraController?.StopShake();
            StopChargeAudio();
            currentChargePower = 0f;
            OnChargePowerChanged?.Invoke(0f);
            LaunchDice(chargedForce, chargedTorque, chargedPower);
        }
    }

    /// <summary>
    /// Inicia a rolagem fisica do dado e registra o callback do resultado.
    /// </summary>
    public void RollDice(Action<int> callback)
    {
        if (isRolling)
        {
            return;
        }

        if (rb == null)
        {
            Debug.LogError("PhysicalDice nao possui Rigidbody configurado.", this);
            callback?.Invoke(0);
            return;
        }

        if (faceReference == null)
        {
            Debug.LogError("PhysicalDice precisa de um Face Reference configurado no Inspector.", this);
            callback?.Invoke(0);
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogError("PhysicalDice precisa de uma camera do jogador configurada no Inspector.", this);
            callback?.Invoke(0);
            return;
        }

        if (cameraTransform == null)
        {
            Debug.LogError("PhysicalDice precisa de uma referencia para a camera principal.", this);
            callback?.Invoke(0);
            return;
        }

        if (boardCenter == null)
        {
            Debug.LogError("PhysicalDice precisa de um BoardCenter configurado no Inspector.", this);
            callback?.Invoke(0);
            return;
        }

        onRollComplete = callback;
        isRolling = true;
        waitingForCharge = true;
        isCharging = false;
        currentChargePower = 0f;

        transform.localScale = defaultScale;
        SetDiceVisible(false);
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        OnChargePowerChanged?.Invoke(0f);

        if (Input.GetKey(KeyCode.Space))
        {
            BeginCharge();
        }
    }

    /// <summary>
    /// Inicia o carregamento assim que o jogador pressiona ou ja esta segurando o espaco.
    /// </summary>
    private void BeginCharge()
    {
        if (isCharging)
        {
            return;
        }

        isCharging = true;
        chargeStartTime = Time.time;
        currentChargePower = 0f;
        cameraController?.StartShake(currentChargePower);
        OnChargePowerChanged?.Invoke(CurrentChargePower);
    }

    /// <summary>
    /// Aplica a forca carregada e inicia a espera pelo resultado final.
    /// </summary>
    private void LaunchDice(float force, float torque, float chargePower)
    {
        Vector3 launchOrigin = CalculateLaunchOrigin();
        transform.position = launchOrigin;
        transform.rotation = UnityEngine.Random.rotation;
        SetDiceVisible(true);
        transform.localScale = defaultScale;

        Vector3 targetPoint = boardCenter.position + Vector3.up * 0.2f;
        Vector3 horizontalDir = targetPoint - launchOrigin;
        horizontalDir.y = 0f;
        horizontalDir.Normalize();

        float arcLift = Mathf.Lerp(0.38f, 0.14f, chargePower);
        Vector3 launchDir = horizontalDir + Vector3.up * arcLift;
        launchDir.Normalize();

        launchDir = Quaternion.Euler(
            UnityEngine.Random.Range(-2f, 2f),
            UnityEngine.Random.Range(-6f, 6f),
            0f) * launchDir;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.WakeUp();

        rb.AddForce(launchDir * force, ForceMode.Impulse);

        Vector3 spinAxis = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-0.3f, 0.3f),
            UnityEngine.Random.Range(-1f, 1f)).normalized;

        float spinMagnitude = Mathf.Lerp(minSpin, maxSpin, chargePower) * Mathf.Deg2Rad;
        rb.angularVelocity = spinAxis * spinMagnitude;
        rb.AddTorque(spinAxis * torque, ForceMode.Impulse);
        StartCoroutine(WaitAndDetect());
    }

    /// <summary>
    /// Calcula um ponto de origem mais proximo do tabuleiro, na parte inferior da tela.
    /// </summary>
    private Vector3 CalculateLaunchOrigin()
    {
        if (cachedCamera == null && cameraTransform != null)
        {
            cachedCamera = cameraTransform.GetComponent<Camera>();
        }

        if (cachedCamera != null)
        {
            Vector3 viewportOrigin = new Vector3(
                0.5f + UnityEngine.Random.Range(-0.06f, 0.06f),
                0.18f + UnityEngine.Random.Range(-0.03f, 0.03f),
                2f);

            return cachedCamera.ViewportToWorldPoint(viewportOrigin);
        }

        return cameraTransform.position
               + cameraTransform.forward * 1.5f
               - cameraTransform.up * 1.2f
               + cameraTransform.right * UnityEngine.Random.Range(-0.25f, 0.25f);
    }

    /// <summary>
    /// Aguarda o dado estabilizar, detecta a face de cima e inicia a animacao de resultado.
    /// </summary>
    private IEnumerator WaitAndDetect()
    {
        yield return StartCoroutine(WaitForGroundContact());
        yield return new WaitForSeconds(settleTime);

        int result = DetectTopFace();
        yield return StartCoroutine(ShowResultToCamera(result));
    }

    /// <summary>
    /// Aguarda o dado tocar o chao configurado antes de iniciar a leitura final.
    /// </summary>
    private IEnumerator WaitForGroundContact()
    {
        if (groundLayer.value == 0)
        {
            yield break;
        }

        float elapsedTime = 0f;
        RaycastHit hitInfo;

        while (!Physics.Raycast(transform.position, Vector3.down, out hitInfo, 5f, groundLayer))
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= 3f)
            {
                yield break;
            }

            yield return null;
        }

        while (hitInfo.distance > 0.7f)
        {
            yield return null;
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= 3f)
            {
                yield break;
            }

            Physics.Raycast(transform.position, Vector3.down, out hitInfo, 5f, groundLayer);
        }
    }

    /// <summary>
    /// Detecta qual face do dado esta virada para cima.
    /// </summary>
    private int DetectTopFace()
    {
        RefreshFaceDirections();

        float highestDot = float.MinValue;
        int detectedValue = UnityEngine.Random.Range(1, 7);

        foreach (KeyValuePair<Vector3, int> face in faceDirections)
        {
            Vector3 worldDir = faceReference.TransformDirection(face.Key);
            float dot = Vector3.Dot(worldDir, Vector3.up);

            if (dot > highestDot)
            {
                highestDot = dot;
                detectedValue = face.Value;
            }
        }

        return detectedValue;
    }

    /// <summary>
    /// Move o dado ate a frente da camera, exibe o resultado e retorna ao ponto de descanso.
    /// </summary>
    private IEnumerator ShowResultToCamera(int result)
    {
        lastRestPosition = transform.position;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        Vector3 startPos = transform.position;
        Transform resultAnchor = cameraController != null ? cameraController.GetResultPoint() : null;
        Vector3 targetPos = resultAnchor != null
            ? resultAnchor.position
            : playerCamera.position + playerCamera.forward * 1.2f + Vector3.up * floatHeight;
        Quaternion targetRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < moveTocameraDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveTocameraDuration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        yield return new WaitForSeconds(showResultDuration);

        elapsed = 0f;
        Vector3 returnStart = transform.position;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / returnDuration);
            transform.position = Vector3.Lerp(returnStart, lastRestPosition, t);
            yield return null;
        }

        transform.position = lastRestPosition;
        isRolling = false;
        onRollComplete?.Invoke(result);

        yield return new WaitForSeconds(0.2f);
        SetDiceVisible(false);
    }

    /// <summary>
    /// Interrompe o som de tensao do charge quando o lancamento termina.
    /// </summary>
    private void StopChargeAudio()
    {
        if (chargeAudio != null && chargeAudio.isPlaying)
        {
            chargeAudio.Stop();
        }
    }

    /// <summary>
    /// Atualiza o pitch do audio de tensao enquanto o jogador carrega o lancamento.
    /// </summary>
    private void UpdateChargeAudio()
    {
        if (chargeAudio == null)
        {
            return;
        }

        chargeAudio.pitch = Mathf.Lerp(minPitch, maxPitch, currentChargePower);

        if (!chargeAudio.isPlaying)
        {
            chargeAudio.Play();
        }
    }

    /// <summary>
    /// Atualiza o dicionario de faces usando os valores configurados no Inspector.
    /// </summary>
    private void RefreshFaceDirections()
    {
        faceDirections = new Dictionary<Vector3, int>()
        {
            { Vector3.up, upFaceValue },
            { Vector3.down, downFaceValue },
            { Vector3.forward, forwardFaceValue },
            { Vector3.back, backFaceValue },
            { Vector3.right, rightFaceValue },
            { Vector3.left, leftFaceValue }
        };
    }

    /// <summary>
    /// Exibe ou oculta os elementos visuais e de colisao do dado.
    /// </summary>
    private void SetDiceVisible(bool visible)
    {
        if (cachedRenderers != null)
        {
            foreach (Renderer cachedRenderer in cachedRenderers)
            {
                cachedRenderer.enabled = visible;
            }
        }

        if (cachedColliders != null)
        {
            foreach (Collider cachedCollider in cachedColliders)
            {
                cachedCollider.enabled = visible;
            }
        }

        if (rb != null)
        {
            rb.isKinematic = !visible;
        }

        if (!visible)
        {
            transform.localScale = defaultScale;
        }
    }
}
