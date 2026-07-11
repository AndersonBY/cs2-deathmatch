using Xunit;

namespace Deathmatch.Tests;

public class SafetyTests
{
    [Fact]
    public void ModeSelection_HandlesNonContiguousModeIds()
    {
        int[] modeIds = [0, 5, 9];

        Assert.Equal(5, ModeSelection.GetNextModeId(modeIds, 0, false));
        Assert.Equal(9, ModeSelection.GetNextModeId(modeIds, 5, false));
        Assert.Equal(0, ModeSelection.GetNextModeId(modeIds, 9, false));
    }

    [Theory]
    [InlineData("mp_free_armor 2", true)]
    [InlineData("sv_infinite_ammo 1", true)]
    [InlineData("exec server.cfg", false)]
    [InlineData("mp_free_armor 2; quit", false)]
    [InlineData("sv_password secret", false)]
    public void IsAllowedModeCommand_UsesExplicitAllowlist(string command, bool expected)
    {
        Assert.Equal(expected, DeathmatchConfigSafety.IsAllowedModeCommand(command));
    }

    [Fact]
    public void PlayerIdentity_RequiresSlotAndSteamId()
    {
        Assert.True(PlayerIdentity.Matches(4, 76561198000000000, 4, 76561198000000000));
        Assert.False(PlayerIdentity.Matches(4, 76561198000000000, 4, 76561198000000001));
        Assert.False(PlayerIdentity.Matches(4, 76561198000000000, 5, 76561198000000000));
    }
}
