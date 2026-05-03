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
            return "Score indisponivel: PlayerStats nao encontrado.";
        }

        float finalScore = CalculateFinalScore(stats);

        return
            $"Money: {stats.Money} | " +
            $"WellBeing: {stats.WellBeing} | " +
            $"Pollution: {stats.Pollution} | " +
            $"Score final: {finalScore}";
    }
}
