using CounterStrikeSharp.API.Core;

namespace Deathmatch;

public static class PlayerIdentity
{
    public static bool Matches(int expectedSlot, ulong expectedSteamId, CCSPlayerController player)
    {
        return player.IsValid && Matches(expectedSlot, expectedSteamId, player.Slot, player.SteamID);
    }

    public static bool Matches(int expectedSlot, ulong expectedSteamId, int actualSlot, ulong actualSteamId)
    {
        return expectedSlot == actualSlot && expectedSteamId != 0 && expectedSteamId == actualSteamId;
    }
}
