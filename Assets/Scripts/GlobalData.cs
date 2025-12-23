public static class GlobalData
{
    // Variabel yang bisa berubah
    public static float gauge = 0f;

    // Konstanta (Tetap/Settings)
    public const float SMALL_FAIL = 0.10f;
    public const float BIG_FAIL = 0.33f;

    // Fungsi tambahan (Opsional)
    public static void ResetGame()
    {
        gauge = 0f;
    }
}