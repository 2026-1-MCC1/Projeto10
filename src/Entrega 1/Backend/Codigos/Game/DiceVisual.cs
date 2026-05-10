using System.Collections;
using UnityEngine;

public class DiceVisual : MonoBehaviour
{
    [SerializeField] private Transform diceTransform;
    [SerializeField] private float rollDuration = 1.2f;
    [SerializeField] private float rotationSpeed = 720f;

    public IEnumerator PlayRollAnimation(int result)
    {
        if (diceTransform == null)
        {
            Debug.LogWarning("DiceVisual nao possui diceTransform configurado.", this);
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < rollDuration)
        {
            float rotationStep = rotationSpeed * Time.deltaTime;

            diceTransform.Rotate(rotationStep, rotationStep * 1.2f, rotationStep * 0.8f, Space.Self);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        diceTransform.rotation = GetRotationForResult(result);
    }

    private Quaternion GetRotationForResult(int result)
    {
        switch (result)
        {
            case 1:
                return Quaternion.Euler(0f, 0f, 0f);
            case 2:
                return Quaternion.Euler(0f, 0f, 90f);
            case 3:
                return Quaternion.Euler(90f, 0f, 0f);
            case 4:
                return Quaternion.Euler(-90f, 0f, 0f);
            case 5:
                return Quaternion.Euler(0f, 0f, -90f);
            case 6:
                return Quaternion.Euler(180f, 0f, 0f);
            default:
                return Quaternion.identity;
        }
    }
}
