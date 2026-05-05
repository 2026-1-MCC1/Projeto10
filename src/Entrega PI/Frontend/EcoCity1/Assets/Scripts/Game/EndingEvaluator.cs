using UnityEngine;

public enum CityEndingType
{
    GestorExemplar,
    BomGestor,
    GestaoDesequilibrada,
    MagnataSemEscrupulos,
    CidadeEmColapso
}

public sealed class FinalEvaluationResult
{
    public CityEndingType EndingType { get; private set; }
    public string Title { get; private set; }
    public string Subtitle { get; private set; }
    public string Message { get; private set; }
    public string ImageSlotName { get; private set; }
    public Color AccentColor { get; private set; }

    public FinalEvaluationResult(
        CityEndingType endingType,
        string title,
        string subtitle,
        string message,
        string imageSlotName,
        Color accentColor)
    {
        EndingType = endingType;
        Title = title;
        Subtitle = subtitle;
        Message = message;
        ImageSlotName = imageSlotName;
        AccentColor = accentColor;
    }
}

public static class EndingEvaluator
{
    /// <summary>
    /// Avalia o resultado final da cidade com base no equilibrio entre dinheiro, bem-estar e poluicao.
    /// </summary>
    public static FinalEvaluationResult Evaluate(PlayerStats stats)
    {
        if (stats == null)
        {
            return CreateResult(
                CityEndingType.GestaoDesequilibrada,
                "RESULTADO INDISPONIVEL",
                "Nao foi possivel avaliar sua gestao.",
                "O jogo terminou sem dados suficientes para montar a classificacao final da cidade.",
                "Imagem_GestaoDesequilibrada",
                new Color(0.75f, 0.75f, 0.75f));
        }

        float score = ScoreCalculator.CalculateFinalScore(stats);

        if (stats.Pollution >= 75 || stats.WellBeing <= 25 || score < 700f)
        {
            return CreateResult(
                CityEndingType.CidadeEmColapso,
                "CIDADE EM COLAPSO",
                "Sua gestao levou a cidade ao limite.",
                "A poluicao saiu do controle, o bem-estar despencou e a cidade perdeu qualidade de vida. Sua administracao falhou em equilibrar crescimento, sustentabilidade e cuidado com a populacao.",
                "Imagem_CidadeEmColapso",
                new Color(0.85f, 0.26f, 0.23f));
        }

        if (stats.Money >= 700 && (stats.Pollution >= 55 || stats.WellBeing <= 40))
        {
            return CreateResult(
                CityEndingType.MagnataSemEscrupulos,
                "MAGNATA SEM ESCRUPULOS",
                "Voce enriqueceu, mas abandonou a cidade.",
                "Os cofres estao cheios, mas a populacao paga o preco. Poluicao, desgaste urbano e perda de bem-estar marcaram sua administracao. Sua cidade cresceu no papel, mas nao para as pessoas.",
                "Imagem_MagnataSemEscrupulos",
                new Color(0.93f, 0.64f, 0.15f));
        }

        if (stats.WellBeing >= 75 && stats.Pollution <= 30 && stats.Money >= 350 && score >= 1100f)
        {
            return CreateResult(
                CityEndingType.GestorExemplar,
                "GESTOR EXEMPLAR",
                "Sua cidade prosperou com equilibrio.",
                "Voce provou que desenvolvimento economico, qualidade de vida e responsabilidade ambiental podem caminhar juntos. A populacao confia na sua lideranca e a cidade se tornou referencia de planejamento urbano.",
                "Imagem_GestorExemplar",
                new Color(0.30f, 0.69f, 0.31f));
        }

        if (score >= 950f && stats.WellBeing >= 55 && stats.Pollution <= 45)
        {
            return CreateResult(
                CityEndingType.BomGestor,
                "BOM GESTOR",
                "Sua cidade cresceu de forma consistente.",
                "Voce tomou boas decisoes e conduziu a cidade por um caminho seguro. Ainda existem desafios pela frente, mas o saldo da sua gestao foi amplamente positivo.",
                "Imagem_BomGestor",
                new Color(0.13f, 0.59f, 0.95f));
        }

        return CreateResult(
            CityEndingType.GestaoDesequilibrada,
            "GESTAO DESEQUILIBRADA",
            "Voce manteve a cidade de pe, mas sem harmonia.",
            "Algumas escolhas trouxeram avancos, mas outras criaram problemas que impediram um desenvolvimento mais saudavel. Sua cidade nao entrou em colapso, mas tambem nao atingiu seu potencial.",
            "Imagem_GestaoDesequilibrada",
            new Color(0.48f, 0.55f, 0.60f));
    }

    /// <summary>
    /// Monta o resumo numerico exibido na tela final.
    /// </summary>
    public static string BuildStatsSummary(PlayerStats stats)
    {
        if (stats == null)
        {
            return "Pontuacao indisponivel.";
        }

        float score = ScoreCalculator.CalculateFinalScore(stats);

        return
            $"Pontuacao Final: {score:0}\n" +
            $"Dinheiro: {stats.Money}\n" +
            $"Bem-estar: {stats.WellBeing}\n" +
            $"Poluicao: {stats.Pollution}";
    }

    /// <summary>
    /// Cria um resultado de preview para testar rapidamente cada tela final no editor.
    /// </summary>
    public static FinalEvaluationResult CreatePreviewResult(CityEndingType endingType)
    {
        switch (endingType)
        {
            case CityEndingType.GestorExemplar:
                return CreateResult(
                    CityEndingType.GestorExemplar,
                    "GESTOR EXEMPLAR",
                    "Sua cidade prosperou com equilibrio.",
                    "Voce provou que desenvolvimento economico, qualidade de vida e responsabilidade ambiental podem caminhar juntos. A populacao confia na sua lideranca e a cidade se tornou referencia de planejamento urbano.",
                    "Imagem_GestorExemplar",
                    new Color(0.30f, 0.69f, 0.31f));

            case CityEndingType.BomGestor:
                return CreateResult(
                    CityEndingType.BomGestor,
                    "BOM GESTOR",
                    "Sua cidade cresceu de forma consistente.",
                    "Voce tomou boas decisoes e conduziu a cidade por um caminho seguro. Ainda existem desafios pela frente, mas o saldo da sua gestao foi amplamente positivo.",
                    "Imagem_BomGestor",
                    new Color(0.13f, 0.59f, 0.95f));

            case CityEndingType.MagnataSemEscrupulos:
                return CreateResult(
                    CityEndingType.MagnataSemEscrupulos,
                    "MAGNATA SEM ESCRUPULOS",
                    "Voce enriqueceu, mas abandonou a cidade.",
                    "Os cofres estao cheios, mas a populacao paga o preco. Poluicao, desgaste urbano e perda de bem-estar marcaram sua administracao. Sua cidade cresceu no papel, mas nao para as pessoas.",
                    "Imagem_MagnataSemEscrupulos",
                    new Color(0.93f, 0.64f, 0.15f));

            case CityEndingType.CidadeEmColapso:
                return CreateResult(
                    CityEndingType.CidadeEmColapso,
                    "CIDADE EM COLAPSO",
                    "Sua gestao levou a cidade ao limite.",
                    "A poluicao saiu do controle, o bem-estar despencou e a cidade perdeu qualidade de vida. Sua administracao falhou em equilibrar crescimento, sustentabilidade e cuidado com a populacao.",
                    "Imagem_CidadeEmColapso",
                    new Color(0.85f, 0.26f, 0.23f));

            default:
                return CreateResult(
                    CityEndingType.GestaoDesequilibrada,
                    "GESTAO DESEQUILIBRADA",
                    "Voce manteve a cidade de pe, mas sem harmonia.",
                    "Algumas escolhas trouxeram avancos, mas outras criaram problemas que impediram um desenvolvimento mais saudavel. Sua cidade nao entrou em colapso, mas tambem nao atingiu seu potencial.",
                    "Imagem_GestaoDesequilibrada",
                    new Color(0.48f, 0.55f, 0.60f));
        }
    }

    /// <summary>
    /// Cria o objeto de resultado final com todos os dados necessarios para a UI.
    /// </summary>
    private static FinalEvaluationResult CreateResult(
        CityEndingType endingType,
        string title,
        string subtitle,
        string message,
        string imageSlotName,
        Color accentColor)
    {
        return new FinalEvaluationResult(endingType, title, subtitle, message, imageSlotName, accentColor);
    }
}
