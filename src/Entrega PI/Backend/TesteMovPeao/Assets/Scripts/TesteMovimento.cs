using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TesteMovimento : MonoBehaviour
{
    private int casaAtual = 0;
    private bool movendo = false;
    public float velocidade = 5f;

    private Vector3[] casas = new Vector3[]
    {
    new Vector3(0, 0, 0),
    new Vector3(2, 0, 0),
    new Vector3(4, 0, 0),
    new Vector3(6, 0, 0),
    new Vector3(8, 0, 0),
    new Vector3(10, 0, 0),
    };
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !movendo)
        {
            int dado = Random.Range(1, 7);
            Debug.Log("Dado: " + dado);
            int destino = Mathf.Min(casaAtual + dado, casas.Length - 1);
            StartCoroutine(Mover(destino));
        }
    }   // <- fecha o Update aqui

    IEnumerator Mover(int destino)   // <- sem o ; aqui
    {
        movendo = true;
        while (casaAtual < destino)
        {
            casaAtual++;
            Vector3 alvo = casas[casaAtual];
            while (Vector3.Distance(transform.position, alvo) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, alvo, velocidade * Time.deltaTime);
                yield return null;
            }
            transform.position = alvo;
        }
        movendo = false;
    }

}

