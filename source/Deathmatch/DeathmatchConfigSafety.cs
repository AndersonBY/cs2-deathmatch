using System.Text.RegularExpressions;
using DeathmatchAPI.Helpers;

namespace Deathmatch;

public static partial class DeathmatchConfigSafety
{
    private static readonly HashSet<string> AllowedModeCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "mp_free_armor",
        "mp_damage_headshot_only",
        "mp_teammates_are_enemies",
        "mp_buy_anywhere",
        "mp_buytime",
        "mp_buy_during_immunity",
        "mp_respawn_on_death_ct",
        "mp_respawn_on_death_t",
        "mp_solid_teammates",
        "sv_infinite_ammo"
    };

    [GeneratedRegex("^[a-z0-9_]+$", RegexOptions.CultureInvariant)]
    private static partial Regex CommandTokenPattern();

    [GeneratedRegex("^weapon_[a-z0-9_]+$", RegexOptions.CultureInvariant)]
    private static partial Regex WeaponNamePattern();

    [GeneratedRegex("^-?[a-zA-Z0-9._]+$", RegexOptions.CultureInvariant)]
    private static partial Regex CommandArgumentPattern();

    public static void Validate(DeathmatchConfig config)
    {
        if (config.CustomModes.Count == 0)
            throw new InvalidOperationException("At least one custom mode must be configured.");

        var parsedModes = new HashSet<int>();
        foreach (var (modeKey, mode) in config.CustomModes)
        {
            if (!int.TryParse(modeKey, out int modeId) || modeId < 0 || !parsedModes.Add(modeId))
                throw new InvalidOperationException($"Custom mode key '{modeKey}' must be a unique non-negative integer.");

            ValidateMode(modeKey, mode);
        }

        if (!config.CustomModes.ContainsKey(config.Gameplay.MapStartMode.ToString()))
            throw new InvalidOperationException($"Map Start Custom Mode '{config.Gameplay.MapStartMode}' is not defined.");

        ValidateCommandList(config.CustomCommands.DeatmatchMenuCmds, "Deathmatch menu commands");
        ValidateCommandList(config.CustomCommands.WeaponSelectCmds, "weapon selection commands");
        foreach (var (shortcut, weapon) in config.CustomCommands.CustomShortcuts)
        {
            if (!CommandTokenPattern().IsMatch(shortcut))
                throw new InvalidOperationException($"Weapon shortcut '{shortcut}' contains unsupported characters.");
            if (!WeaponNamePattern().IsMatch(weapon))
                throw new InvalidOperationException($"Weapon shortcut '{shortcut}' has invalid weapon name '{weapon}'.");
        }

        config.Gameplay.GameLength = Math.Clamp(config.Gameplay.GameLength, 1, 180);
        config.Gameplay.NewModeCountdown = Math.Clamp(config.Gameplay.NewModeCountdown, 0, 300);
        config.Gameplay.ModeMessageDuration = Math.Clamp(config.Gameplay.ModeMessageDuration, 0, 300);
        config.SpawnSystem.DistanceRespawn = Math.Clamp(config.SpawnSystem.DistanceRespawn, 67, 5000);
    }

    private static void ValidateMode(string modeKey, ModeData mode)
    {
        mode.Interval = Math.Clamp(mode.Interval, 10, 86_400);
        mode.Armor = Math.Clamp(mode.Armor, 0, 2);

        foreach (var weapon in mode.PrimaryWeapons.Concat(mode.SecondaryWeapons).Concat(mode.Utilities))
        {
            if (!WeaponNamePattern().IsMatch(weapon))
                throw new InvalidOperationException($"Custom mode '{modeKey}' has invalid weapon name '{weapon}'.");
        }

        foreach (var command in mode.ExecuteCommands)
        {
            if (!IsAllowedModeCommand(command))
                throw new InvalidOperationException($"Custom mode '{modeKey}' contains unsafe command '{command}'.");
        }
    }

    private static void ValidateCommandList(string commands, string label)
    {
        foreach (var command in commands.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!CommandTokenPattern().IsMatch(command))
                throw new InvalidOperationException($"{label} contains invalid command '{command}'.");
        }
    }

    public static bool IsAllowedModeCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command) || command.IndexOfAny([';', '\r', '\n']) >= 0)
            return false;

        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0 || !AllowedModeCommands.Contains(parts[0]))
            return false;

        return parts.Skip(1).All(argument => CommandArgumentPattern().IsMatch(argument));
    }
}
