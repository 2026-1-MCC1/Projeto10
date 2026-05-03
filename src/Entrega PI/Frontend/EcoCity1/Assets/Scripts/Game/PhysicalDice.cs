using System;
using System.Collections;
using UnityEngine;

public class PhysicalDice : MonoBehaviour
{
    [SerializeField] private Rigidbody diceRigidbody;
    [SerializeField] private Transform diceTransform;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 8f, 0f);
    [SerializeField] private float throwForce = 6f;
    [SerializeField] private float torqueForce = 12f;
    [SerializeField] private float stopVelocityThreshold = 0.05f;
    [SerializeField] private float stopAngularVelocityThreshold = 0.05f;
    [SerializeField] private float settleTime = 0.8f;

    public IEnumerator Roll(Action<int> onResult)
    {
        if (diceRigidbody == null || diceTransform == null)
        {
            Debug.LogError("PhysicalDice precisa de Rigidbody e Transform configurados.", this);
            yield break;
        }

        PrepareDiceForRoll();

        Vector3 throwDirection = new Vector3(
            UnityEngine.Random.Range(-0.8f, 0.8f),
            -1f,
            UnityEngine.Random.Range(-0.8f, 0.8f)).normalized;

        Vector3 randomTorque = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)) * torqueForce;

        diceRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        diceRigidbody.AddTorque(randomTorque, ForceMode.Impulse);

        yield return StartCoroutine(WaitUntilDiceStops());

        int result = GetTopFaceValue();
        onResult?.Invoke(result);
    }

    private void PrepareDiceForRoll()
    {
        diceRigidbody.isKinematic = false;
        diceRigidbody.useGravity = true;

        diceTransform.position = spawnPosition;
        diceTransform.rotation = UnityEngine.Random.rotation;

        diceRigidbody.linearVelocity = Vector3.zero;
        diceRigidbody.angularVelocity = Vector3.zero;
        diceRigidbody.Sleep();
        diceRigidbody.WakeUp();
    }

    private IEnumerator WaitUntilDiceStops()
    {
        float stableTime = 0f;

        while (stableTime < settleTime)
        {
            bool isSlowEnough =
                diceRigidbody.linearVelocity.magnitude <= stopVelocityThreshold &&
                diceRigidbody.angularVelocity.magnitude <= stopAngularVelocityThreshold;

            if (isSlowEnough)
            {
                stableTime += Time.deltaTime;
            }
            else
            {
                stableTime = 0f;
            }

            yield return null;
        }

        diceRigidbody.linearVelocity = Vector3.zero;
        diceRigidbody.angularVelocity = Vector3.zero;
    }

    private int GetTopFaceValue()
    {
        // Este mapeamento assume um dado padrao onde:
        // +Y = face 1, -Y = face 6, +Z = face 2,
        // -Z = face 5, +X = face 3 e -X = face 4.
        // Se voce colocar textura ou numeracao diferente no cubo,
        // ajuste os valores retornados abaixo.
        float upDot = Vector3.Dot(diceTransform.up, Vector3.up);
        float downDot = Vector3.Dot(-diceTransform.up, Vector3.up);
        float forwardDot = Vector3.Dot(diceTransform.forward, Vector3.up);
        float backDot = Vector3.Dot(-diceTransform.forward, Vector3.up);
        float rightDot = Vector3.Dot(diceTransform.right, Vector3.up);
        float leftDot = Vector3.Dot(-diceTransform.right, Vector3.up);

        float highestDot = upDot;
        int result = 1;

        if (downDot > highestDot)
        {
            highestDot = downDot;
            result = 6;
        }

        if (forwardDot > highestDot)
        {
            highestDot = forwardDot;
            result = 2;
        }

        if (backDot > highestDot)
        {
            highestDot = backDot;
            result = 5;
        }

        if (rightDot > highestDot)
        {
            highestDot = rightDot;
            result = 3;
        }

        if (leftDot > highestDot)
        {
            result = 4;
        }

        return result;
    }
}
