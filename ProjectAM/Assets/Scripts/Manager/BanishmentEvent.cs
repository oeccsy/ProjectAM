using System.Collections.Generic;

// 해질녘 추방 판정: 의심치가 임계를 넘은 최고 의심 개체를 고른다. 오판일 수 있다.
public class BanishmentEvent
{
    private readonly SuspicionEvaluator evaluator = new SuspicionEvaluator();

    public NPC SelectAccused(int suspicionThreshold)
    {
        Dictionary<NpcColor, int> suspicion = evaluator.Evaluate();

        NPC accused = null;
        int highestScore = 0;

        foreach (KeyValuePair<NpcColor, int> pair in suspicion)
        {
            if (pair.Value <= highestScore) continue;

            NPC candidate = World.Instance.NPCs.Get(pair.Key);
            if (candidate == null) continue;
            if (!candidate.IsAlive) continue;

            highestScore = pair.Value;
            accused = candidate;
        }

        if (highestScore < suspicionThreshold) return null;

        return accused;
    }
}
