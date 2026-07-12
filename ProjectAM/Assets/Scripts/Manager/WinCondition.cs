// 인원수 승패: 범인이 제거되면 시민 승, 범인 수가 시민 수 이상이 되면 범인 승.
public static class WinCondition
{
    public static GameResult? Evaluate()
    {
        int aliveCitizens = 0;
        int aliveCulprits = 0;

        foreach (NPC npc in World.Instance.NPCs.All)
        {
            if (!npc.IsAlive) continue;

            if (npc.Role == Role.Witch) aliveCulprits++;
            else aliveCitizens++;
        }

        if (aliveCulprits == 0) return GameResult.CitizensWin;
        if (aliveCulprits >= aliveCitizens) return GameResult.CulpritWins;

        return null;
    }
}
