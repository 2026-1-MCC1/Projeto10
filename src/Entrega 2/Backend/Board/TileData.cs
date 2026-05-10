using System;
using UnityEngine;

[Serializable]
public class TileData
{
    public string Name;
    public TileType Type;

    [Header("Propriedade")]
    public int purchasePrice;
    public int rentPrice;

    [Header("Impactos ao Comprar")]
    public int moneyImpact;
    public int wellBeingImpact;
    public int pollutionImpact;

    [Header("Descrição")]
    public string propertyDescription;

    public int Price => purchasePrice;
    public int FinanceImpact => moneyImpact;
    public int WellBeingImpact => wellBeingImpact;
    public int PollutionImpact => pollutionImpact;
}
