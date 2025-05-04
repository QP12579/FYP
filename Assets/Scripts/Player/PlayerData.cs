public static class PlayerData
{
  
    public static float HP { get; set; } = 100;
    public static float MP { get; set; } = 50;
    public static int Level { get; set; } = 1;

  
    public static void SavePlayerState(Player player)
    {
        HP = player.HP;
        MP = player.MP;
        Level = player.level;
    }

   
    public static void LoadPlayerState(Player player)
    {
        player.HP = HP;
        player.MP = MP;
        player.level = Level;
        player.UpdatePlayerUIInfo();
    }
}