using UnityEngine;

public class GameAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource uiAudioSource;

    private AudioClip rollClip;
    private AudioClip purchaseClip;
    private AudioClip passClip;
    private AudioClip rentClip;
    private AudioClip positiveEventClip;
    private AudioClip warningEventClip;
    private AudioClip gameOverClip;

    /// <summary>
    /// Gera clips simples em runtime para dar vida ao jogo sem depender de assets externos.
    /// </summary>
    private void Awake()
    {
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.GetComponent<AudioSource>();
        }

        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        }

        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f;
        uiAudioSource.volume = 0.35f;

        rollClip = CreateToneSequence(new[] { 540f, 680f }, 0.08f);
        purchaseClip = CreateToneSequence(new[] { 620f, 820f, 980f }, 0.07f);
        passClip = CreateToneSequence(new[] { 320f, 250f }, 0.09f);
        rentClip = CreateToneSequence(new[] { 420f, 360f, 300f }, 0.07f);
        positiveEventClip = CreateToneSequence(new[] { 700f, 860f, 1020f }, 0.08f);
        warningEventClip = CreateToneSequence(new[] { 480f, 390f, 310f }, 0.09f);
        gameOverClip = CreateToneSequence(new[] { 760f, 640f, 840f, 520f }, 0.10f);
    }

    /// <summary>
    /// Toca o som de inicio da rolagem.
    /// </summary>
    public void PlayRoll()
    {
        PlayClip(rollClip);
    }

    /// <summary>
    /// Toca um som de compra bem-sucedida.
    /// </summary>
    public void PlayPurchase()
    {
        PlayClip(purchaseClip);
    }

    /// <summary>
    /// Toca um som discreto para quando o jogador passa a compra.
    /// </summary>
    public void PlayPass()
    {
        PlayClip(passClip);
    }

    /// <summary>
    /// Toca um som de aluguel pago.
    /// </summary>
    public void PlayRent()
    {
        PlayClip(rentClip);
    }

    /// <summary>
    /// Toca o som correspondente ao tom de um evento de cidade.
    /// </summary>
    public void PlayEventTone(CityEventTone tone)
    {
        switch (tone)
        {
            case CityEventTone.Positive:
                PlayClip(positiveEventClip);
                break;
            case CityEventTone.Warning:
                PlayClip(warningEventClip);
                break;
            default:
                PlayClip(rollClip);
                break;
        }
    }

    /// <summary>
    /// Toca um som de encerramento da partida.
    /// </summary>
    public void PlayGameOver()
    {
        PlayClip(gameOverClip);
    }

    /// <summary>
    /// Toca um clip se o AudioSource estiver disponivel.
    /// </summary>
    private void PlayClip(AudioClip clip)
    {
        if (uiAudioSource == null || clip == null)
        {
            return;
        }

        uiAudioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Gera um pequeno clip formado por multiplas notas em sequencia.
    /// </summary>
    private AudioClip CreateToneSequence(float[] frequencies, float noteDuration)
    {
        const int sampleRate = 44100;
        int samplesPerNote = Mathf.CeilToInt(sampleRate * noteDuration);
        int totalSamples = samplesPerNote * frequencies.Length;
        float[] data = new float[totalSamples];

        for (int noteIndex = 0; noteIndex < frequencies.Length; noteIndex++)
        {
            float frequency = frequencies[noteIndex];

            for (int sampleIndex = 0; sampleIndex < samplesPerNote; sampleIndex++)
            {
                int globalIndex = (noteIndex * samplesPerNote) + sampleIndex;
                float time = (float)sampleIndex / sampleRate;
                float envelope = Mathf.Clamp01(1f - ((float)sampleIndex / samplesPerNote));
                data[globalIndex] = Mathf.Sin(2f * Mathf.PI * frequency * time) * 0.18f * envelope;
            }
        }

        AudioClip clip = AudioClip.Create("GeneratedSfx", totalSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
