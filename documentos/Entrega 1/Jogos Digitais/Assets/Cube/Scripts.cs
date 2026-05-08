using UnityEngine;

public class Dado : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
            rb = GetComponent<Rigidbody>();
            rb.angularDamping = 2f; // padrão é 0.05, quanto maior, mais rápido freia
            rb.AddForce(Vector3.down * Random.Range(1f, 2f), ForceMode.Impulse);
            rb.AddTorque(Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f), ForceMode.Impulse);
        
    }

    void Update()
    {
        //LançarDado();
    }

    void LançarDado()
    {
        rb.AddForce(Vector3.down * Random.Range(1f, 2f), ForceMode.Impulse);
        rb.AddTorque(Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f), ForceMode.Impulse);
    }
}