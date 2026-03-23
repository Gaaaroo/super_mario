public enum PowerUpType
{
    None,
    Invisibility, // Tàng hình
    Gun,          // Súng
    ExtraLife     // Thêm mạng
}

public static class GameData
{
    public static int lives = 5;
    public static int coins = 0;
    // Khi thoát hẳn game (.exe tắt) thì số này mới mất. 
    // Nếu chỉ chuyển Map 1 -> Map 2 thì nó vẫn còn nguyên.
    // Biến này để nhớ số tiền lúc bắt đầu vào màn chơi
    public static int coinsAtLevelStart = 0;

    public static bool isInvincible = false;
}