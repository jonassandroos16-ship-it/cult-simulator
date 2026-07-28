namespace CultSimulator.Game;

/// <summary>
/// World map logic: conquering nodes, managing stances, suspicion, and
/// Ley Line connections. Pure functions over <see cref="OccultState"/>.
/// </summary>
public static class WorldMapSystem
{
    public static MapNodeState? GetNode(OccultState o, string nodeId) =>
        o.MapNodes.FirstOrDefault(n => n.NodeId == nodeId);

    public static bool IsConquered(OccultState o, string nodeId)
    {
        var node = GetNode(o, nodeId);
        return node != null && node.Conquered;
    }

    public static bool CanConquer(OccultState o, MapNodeDef def)
    {
        if (IsConquered(o, def.Id)) return false;
        if (o.Devotion < def.DevotionCost) return false;
        if (o.ArmyPower < def.ArmyPowerRequired) return false;
        return true;
    }

    public static bool Conquer(OccultState o, MapNodeDef def)
    {
        if (!CanConquer(o, def)) return false;
        o.Devotion -= def.DevotionCost;
        o.ArmyPower -= def.ArmyPowerRequired;
        o.MapNodes.Add(new MapNodeState
        {
            NodeId = def.Id,
            Conquered = true,
            Stance = NodeStance.Harvest
        });
        return true;
    }

    public static void SetStance(OccultState o, string nodeId, NodeStance stance)
    {
        var node = GetNode(o, nodeId);
        if (node != null && node.Conquered) node.Stance = stance;
    }

    public static double TotalFkPerSec(OccultState o)
    {
        double total = 0;
        foreach (var nodeState in o.MapNodes)
        {
            if (!nodeState.Conquered) continue;
            var def = OccultData.MapNode(nodeState.NodeId);
            if (def == null) continue;
            if (nodeState.Stance == NodeStance.Veil) continue;
            total += def.FkPerSec;
        }
        return total;
    }

    public static double TotalSuspicionPerSec(OccultState o)
    {
        double total = 0;
        foreach (var nodeState in o.MapNodes)
        {
            if (!nodeState.Conquered) continue;
            var def = OccultData.MapNode(nodeState.NodeId);
            if (def == null) continue;
            if (nodeState.Stance == NodeStance.Veil) continue;
            total += def.SuspicionPerSec;
        }
        return total;
    }

    public static void TickSuspicion(OccultState o, double deltaSec)
    {
        double baseGen = TotalSuspicionPerSec(o);
        double mult = CultistHierarchy.SuspicionMult(o)
            * TechTree.SuspicionReductionMult(o)
            * Grimoire.SuspicionReductionBonus(o)
            * o.ElixirSuspicionMult;
        double gen = baseGen * mult;
        double decay = OccultBalance.SuspicionDecayPerSec;
        o.Suspicion = Math.Clamp(o.Suspicion + (gen - decay) * deltaSec, 0, OccultBalance.SuspicionMax);
    }

    public static bool IsRaidTriggered(OccultState o) =>
        o.Suspicion >= OccultBalance.SuspicionRaidThreshold;

    public static void ApplyRaid(OccultState o)
    {
        o.Suspicion = 0;
        if (o.Acolytes > 10) o.Acolytes = (int)(o.Acolytes * 0.8);
        if (o.Minions.Count > 0 && Random.Shared.NextDouble() < 0.3)
        {
            var idx = Random.Shared.Next(o.Minions.Count);
            o.Minions.RemoveAt(idx);
        }
    }

    public static bool CanConnectLeyLine(OccultState o, string nodeA, string nodeB)
    {
        if (!TechTree.HasTech(o, TechId.LeyLineWeaving)) return false;
        if (!IsConquered(o, nodeA) || !IsConquered(o, nodeB)) return false;
        return true;
    }

    public static bool ConnectLeyLine(OccultState o, string nodeA, string nodeB)
    {
        if (!CanConnectLeyLine(o, nodeA, nodeB)) return false;
        var key = new[] { nodeA, nodeB };
        if (o.LeyLines.Any(l => l.Contains(nodeA) && l.Contains(nodeB))) return false;
        o.LeyLines.Add(key);
        return true;
    }

    public static int GreatSealCount(OccultState o)
    {
        int count = 0;
        for (int i = 0; i < o.LeyLines.Count; i++)
        {
            for (int j = i + 1; j < o.LeyLines.Count; j++)
            {
                for (int k = j + 1; k < o.LeyLines.Count; k++)
                {
                    var nodes = o.LeyLines[i]
                        .Concat(o.LeyLines[j])
                        .Concat(o.LeyLines[k])
                        .Distinct()
                        .ToList();
                    if (nodes.Count == 3) count++;
                }
            }
        }
        return count;
    }

    public static double GreatSealMultiplier(OccultState o)
    {
        int seals = GreatSealCount(o);
        if (seals == 0) return 1.0;
        return 1.0 + seals * (Grimoire.GreatSealMult(o) - 1.0);
    }

    public static int ConqueredNodeCount(OccultState o) =>
        o.MapNodes.Count(n => n.Conquered);

    public static void TickMaterials(OccultState o, double deltaSec)
    {
        foreach (var nodeState in o.MapNodes)
        {
            if (!nodeState.Conquered) continue;
            var def = OccultData.MapNode(nodeState.NodeId);
            if (def?.Materials == null) continue;
            double rate = nodeState.Stance == NodeStance.Harvest ? 0.1 : 0.05;
            foreach (var (material, amount) in def.Materials)
            {
                o.Materials[material] = o.Materials.GetValueOrDefault(material) + (int)(amount * rate * deltaSec);
            }
        }
    }
}