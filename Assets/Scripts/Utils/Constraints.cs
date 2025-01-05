using UnityEngine;

public static class Constraints
{
    public static class PlayerPref
    {
        public readonly static string BGMVolume = "BGMVolume";
        public readonly static string SFXVolume = "SFXVolume";

        public readonly static string Coins = "Coins";
    }

    public static class Tag
    {
        public readonly static string Player = "Player";
        public readonly static string BGMSource = "BGMSource";
    }

    public static class Path
    {
        public readonly static string Streaming = Application.streamingAssetsPath + "/Photos";

        // Resources Path
        public readonly static string BGM = "Audio/BGM/";
        public readonly static string SFX = "Audio/SFX/";
        public readonly static string explosionSFX = SFX + "ExplosionSFX";
        public readonly static string policeCarSFX = SFX + "policeSFX";
        public readonly static string missionComplete = SFX + "MissionComplete";

        public readonly static string EndingImage = "Ending/";
        public readonly static string DoNothing = EndingImage + "DoNothing";
        public readonly static string innocentEnd = EndingImage + "OnlyShootInnocent";
        public readonly static string TrueEnd = EndingImage + "OnlyShootTarget";
        public readonly static string KillEveryone = EndingImage + "TheUnscrupulousKiller";
    }
}
