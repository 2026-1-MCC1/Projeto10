using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    public int Roll()
    {
        return Random.Range(1, 7);
    }
}
