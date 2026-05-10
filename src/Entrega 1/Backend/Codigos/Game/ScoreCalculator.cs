public static class ScoreCalculator
{
    public static float CalculateFinalScore(PlayerStats stats)
    {
        if (stats == null)
        {
            return 0f;
        }

        return stats.Money + (stats.WellBeing * 10f) - (stats.Pollution * 8f);
    }

    public static string GetScoreExplanation(PlayerStats stats)
    {
        if (stats == null)
        {
            return "Pontuação indisponível: PlayerStats não encontrado.";
        }

        float finalScore = CalculateFinalScore(stats);

        return
            $"Dinheiro: {stats.Money} | " +
            $"Bem-estar: {stats.WellBeing} | " +
            $"Poluição: {stats.Pollution} | " +
            $"Pontuação final: {finalScore}";
    }
}
