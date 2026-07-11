namespace Deathmatch;

public static class ModeSelection
{
    public static int GetNextModeId(IEnumerable<int> availableModeIds, int currentModeId, bool randomSelection)
    {
        var modeIds = availableModeIds.Distinct().OrderBy(id => id).ToArray();
        if (modeIds.Length == 0)
            throw new InvalidOperationException("At least one custom mode is required.");
        if (modeIds.Length == 1)
            return modeIds[0];

        if (randomSelection)
        {
            var candidates = modeIds.Where(id => id != currentModeId).ToArray();
            return candidates[Random.Shared.Next(candidates.Length)];
        }

        int currentIndex = Array.IndexOf(modeIds, currentModeId);
        return currentIndex < 0 || currentIndex == modeIds.Length - 1
            ? modeIds[0]
            : modeIds[currentIndex + 1];
    }
}
