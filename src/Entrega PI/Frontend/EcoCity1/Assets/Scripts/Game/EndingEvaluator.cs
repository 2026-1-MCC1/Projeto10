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
                "Não foi possível avaliar sua gestão.",
                "O jogo terminou sem dados suficientes para montar a classificação final da cidade.",
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
                "A poluição saiu do controle, o bem-estar despencou e a cidade perdeu qualidade de vida. Sua administração falhou em equilibrar crescimento, sustentabilidade e cuidado com a população.",
                "Imagem_CidadeEmColapso",
                new Color(0.85f, 0.26f, 0.23f));
        }

        if (stats.Money >= 700 && (stats.Pollution >= 55 || stats.WellBeing <= 40))
        {
            return CreateResult(
                CityEndingType.MagnataSemEscrupulos,
                "MAGNATA SEM ESCRUPULOS",
                "Você enriqueceu, mas abandonou a cidade.",
                "Os cofres estão cheios, mas a população paga o preço. Poluição, desgaste urbano e perda de bem-estar marcaram sua administração. Sua cidade cresceu no papel, mas não para as pessoas.",
                "Imagem_MagnataSemEscrupulos",
                new Color(0.93f, 0.64f, 0.15f));
        }

        if (stats.WellBeing >= 75 && stats.Pollution <= 30 && stats.Money >= 350 && score >= 1100f)
        {
            return CreateResult(
                CityEndingType.GestorExemplar,
                "GESTOR EXEMPLAR",
                "Sua cidade prosperou com equilibrio.",
                "Você provou que desenvolvimento econômico, qualidade de vida e responsabilidade ambiental podem caminhar juntos. A população confia na sua liderança e a cidade se tornou referência de planejamento urbano.",
                "Imagem_GestorExemplar",
                new Color(0.30f, 0.69f, 0.31f));
        }

        if (score >= 950f && stats.WellBeing >= 55 && stats.Pollution <= 45)
        {
            return CreateResult(
                CityEndingType.BomGestor,
                "BOM GESTOR",
                "Sua cidade cresceu de forma consistente.",
                "Você tomou boas decisões e conduziu a cidade por um caminho seguro. Ainda existem desafios pela frente, mas o saldo da sua gestão foi amplamente positivo.",
                "Imagem_BomGestor",
                new Color(0.13f, 0.59f, 0.95f));
        }

        return CreateResult(
            CityEndingType.GestaoDesequilibrada,
            "GESTAO DESEQUILIBRADA",
            "Você manteve a cidade de pé, mas sem harmonia.",
            "Algumas escolhas trouxeram avanços, mas outras criaram problemas que impediram um desenvolvimento mais saudável. Sua cidade não entrou em colapso, mas também não atingiu seu potencial.",
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
            $"Poluição: {stats.Pollution}";
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
                    "Você provou que desenvolvimento econômico, qualidade de vida e responsabilidade ambiental podem caminhar juntos. A população confia na sua liderança e a cidade se tornou referência de planejamento urbano.",
                    "Imagem_GestorExemplar",
                    new Color(0.30f, 0.69f, 0.31f));

            case CityEndingType.BomGestor:
                return CreateResult(
                    CityEndingType.BomGestor,
                    "BOM GESTOR",
                    "Sua cidade cresceu de forma consistente.",
                    "Você tomou boas decisões e conduziu a cidade por um caminho seguro. Ainda existem desafios pela frente, mas o saldo da sua gestão foi amplamente positivo.",
                    "Imagem_BomGestor",
                    new Color(0.13f, 0.59f, 0.95f));

            case CityEndingType.MagnataSemEscrupulos:
                return CreateResult(
                    CityEndingType.MagnataSemEscrupulos,
                    "MAGNATA SEM ESCRUPULOS",
                    "Você enriqueceu, mas abandonou a cidade.",
                    "Os cofres estão cheios, mas a população paga o preço. Poluição, desgaste urbano e perda de bem-estar marcaram sua administração. Sua cidade cresceu no papel, mas não para as pessoas.",
                    "Imagem_MagnataSemEscrupulos",
                    new Color(0.93f, 0.64f, 0.15f));

            case CityEndingType.CidadeEmColapso:
                return CreateResult(
                    CityEndingType.CidadeEmColapso,
                    "CIDADE EM COLAPSO",
                    "Sua gestao levou a cidade ao limite.",
                    "A poluição saiu do controle, o bem-estar despencou e a cidade perdeu qualidade de vida. Sua administração falhou em equilibrar crescimento, sustentabilidade e cuidado com a população.",
                    "Imagem_CidadeEmColapso",
                    new Color(0.85f, 0.26f, 0.23f));

            default:
                return CreateResult(
                    CityEndingType.GestaoDesequilibrada,
                    "GESTAO DESEQUILIBRADA",
                    "Você manteve a cidade de pé, mas sem harmonia.",
                    "Algumas escolhas trouxeram avanços, mas outras criaram problemas que impediram um desenvolvimento mais saudável. Sua cidade não entrou em colapso, mas também não atingiu seu potencial.",
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
