using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Property
{
    public string name;

    public int price;

    public int rentPerRound;

    public int ownerId = -1; //-1 = sem dono
}
public class Player
{
    public string name;

    public int money;
}

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        properties = new List<Property>
    {
        new Property { name = "Fábrica",  price = 200, rentPerRound = 20 },
        new Property { name = "Parque",   price = 150, rentPerRound = 15 },
        new Property { name = "Shopping", price = 300, rentPerRound = 30 }
    };

        players = new List<Player>
    {
        new Player { name = "Jogador 1", money = 1500 },
        new Player { name = "Jogador 2", money = 1500 }
    };

        PrintStatus(); // mostra estado inicial no console
    }

    public void PrintStatus()
    {
        Debug.Log("========== STATUS DO JOGO ==========");
        Debug.Log($"Rodada: {round} | Vez de: {players[currentPlayerIndex].name}");

        Debug.Log("-- Jogadores --");
        foreach (Player p in players)
            Debug.Log($"  {p.name}: R${p.money}");

        Debug.Log("-- Propriedades --");
        foreach (Property prop in properties)
        {
            string owner = prop.ownerId == -1 ? "Ninguém" : players[prop.ownerId].name;
            Debug.Log($"  {prop.name} | Preço: R${prop.price} | Aluguel: R${prop.rentPerRound}/rodada | Dono: {owner}");
        }

        Debug.Log("=====================================");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) BuyProperty(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) BuyProperty(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) BuyProperty(2);
        if (Input.GetKeyDown(KeyCode.N)) NextRound();
        if (Input.GetKeyDown(KeyCode.S)) PrintStatus();
    }
    public List<Property> properties;

    public List<Player> players;

    public int currentPlayerIndex = 0;

    public int round = 1;
    public void BuyProperty(int propertyIndex)
    {
        if (properties[propertyIndex].ownerId != -1) { Debug.Log("Já tem dono!"); return; }
        if (players[currentPlayerIndex].money < properties[propertyIndex].price) { Debug.Log("Sem dinheiro!"); return; }

        players[currentPlayerIndex].money -= properties[propertyIndex].price;
        properties[propertyIndex].ownerId = currentPlayerIndex;

        Debug.Log($"{players[currentPlayerIndex].name} comprou {properties[propertyIndex].name}!");
    }

    public void NextRound()
    {
        foreach (Property prop in properties)
        {
            if (prop.ownerId == -1) continue; // sem dono, pula

            for (int i = 0; i < players.Count; i++)
            {
                if (i == prop.ownerId)
                    players[i].money += prop.rentPerRound; // dono recebe
                else
                    players[i].money -= prop.rentPerRound; // outros pagam
            }
        }

        round++;
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
    }
}
