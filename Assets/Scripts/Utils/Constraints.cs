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

    public static class InputKey
    {
        public readonly static string Move = "Move";
        public readonly static string Jump = "Jump";
        public readonly static string Defense = "Defense";
        public readonly static string Attack = "Attack";
        public readonly static string Record = "Record";
        public readonly static string Roll = "Roll";
        public readonly static string Aim = "Aim";
        public readonly static string FarAttack = "FarAttack";
        public readonly static string Skill1 = "Skill1";
        public readonly static string Skill2 = "Skill2";
        public readonly static string Tab = "Tab";
        public readonly static string T = "T";
        public readonly static string ESC = "Escape";
        public readonly static string Purchase = "Purchase";
    }

    public static class CheatKey
    {
        public readonly static string AddCoins = "AddCoins";
        public readonly static string AddSkillPoints = "AddSkillPoints";
        public readonly static string GetSP = "GetSP";
        public readonly static string GetMP = "GetMP";
        public readonly static string Heal = "Heal";
    }

    public static class ControlScheme
    {
        public readonly static string GamePad = "Gamepad";
        public readonly static string Keyboard = "Keyboard";
    }
}
