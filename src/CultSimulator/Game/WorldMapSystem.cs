namespace CultSimulator.Game;

public static class WorldMapSystem
{
    public static MapNodeState? GetNode(OccultState o, string nodeId) => o.MapNodes.FirstOrDefault(n => n.NodeId == nodeId);
    public static bool IsConquered(OccultState o, string nodeId) => GetNode(o, nodeId)?.Conquered ?? false;

    public static bool CanConquer(GameState state, MapNodeDef def) =>
        !IsConquered(state.Occult, def.Id) && state.ActiveCoven.Faith >= def.FaithCost && state.Occult.ArmyPower >= def.ArmyPowerRequired;

    public static bool Conquer(GameState state, MapNodeDef def)
    {
        if (!CanConquer(state, def)) return false;
        state.ActiveCoven.Faith -= def.FaithCost;
        state.Occult.ArmyPower -= def.ArmyPowerRequired;
        state.Occult.MapNodes.Add(new MapNodeState { NodeId = def.Id, Conquered = true, Stance = NodeStance.Harvest });
        return true;
    }

    public static void SetStance(OccultState o, string nodeId, NodeStance stance) { var node = GetNode(o, nodeId); if (node?.Conquered == true) node.Stance = stance; }

    public static double TotalFaithPerSec(OccultState o)
    {
        double total = 0;
        foreach (var ns in o.MapNodes) { if (!ns.Conquered || ns.Stance == NodeStance.Veil) continue; var def = OccultData.MapNode(ns.NodeId); if (def != null) total += def.FaithPerSec; }
        return total;
    }

    public static double TotalSuspicionPerSec(OccultState o)
    {
        double total = 0;
        foreach (var ns in o.MapNodes) { if (!ns.Conquered || ns.Stance == NodeStance.Veil) continue; var def = OccultData.MapNode(ns.NodeId); if (def != null) total += def.SuspicionPerSec; }
        return total;
    }

    public static void TickSuspicion(OccultState o, double deltaSec)
    {
        double gen = TotalSuspicionPerSec(o) * CultistHierarchy.SuspicionMult(o) * TechTree.SuspicionReductionMult(o) * Grimoire.SuspicionReductionBonus(o) * o.ElixirSuspicionMult;
        if (o.IsDarkVigilActive) gen *= 0.5;
        if (o.IsCovenBlessingActive) gen *= 0.5;
        o.Suspicion = Math.Clamp(o.Suspicion + (gen - OccultBalance.SuspicionDecayPerSec) * deltaSec, 0, OccultBalance.SuspicionMax);
    }

    public static bool IsRaidTriggered(OccultState o) => o.Suspicion >= OccultBalance.SuspicionRaidThreshold;

    public static void ApplyRaid(OccultState o)
    {
        o.Suspicion = 0;
        if (o.Acolytes > 10) o.Acolytes = (int)(o.Acolytes * 0.8);
        if (o.Minions.Count > 0 && Random.Shared.NextDouble() < 0.3) o.Minions.RemoveAt(Random.Shared.Next(o.Minions.Count));
    }

    public static bool CanConnectLeyLine(OccultState o, string nodeA, string nodeB) =>
        IsConquered(o, nodeA) && IsConquered(o, nodeB) && nodeA != nodeB;

    public static bool ConnectLeyLine(OccultState o, string nodeA, string nodeB)
    {
        if (!CanConnectLeyLine(o, nodeA, nodeB) || o.LeyLines.Any(l => l.Contains(nodeA) && l.Contains(nodeB))) return false;
        o.LeyLines.Add(new[] { nodeA, nodeB });
        return true;
    }

    public static int GreatSealCount(OccultState o)
    {
        int count = 0;
        for (int i = 0; i < o.LeyLines.Count; i++) for (int j = i + 1; j < o.LeyLines.Count; j++) for (int k = j + 1; k < o.LeyLines.Count; k++)
            if (o.LeyLines[i].Concat(o.LeyLines[j]).Concat(o.LeyLines[k]).Distinct().Count() == 3) count++;
        return count;
    }

    public static double GreatSealMultiplier(OccultState o) { int seals = GreatSealCount(o); return seals == 0 ? 1.0 : 1.0 + seals * (Grimoire.GreatSealMult(o) - 1.0); }
    public static int ConqueredNodeCount(OccultState o) => o.MapNodes.Count(n => n.Conquered);

    public static void TickMaterials(OccultState o, double deltaSec)
    {
        foreach (var ns in o.MapNodes)
        {
            if (!ns.Conquered) continue;
            var def = OccultData.MapNode(ns.NodeId);
            if (def?.Materials == null) continue;
            double rate = ns.Stance == NodeStance.Harvest ? 0.1 : 0.05;
            foreach (var (material, amount) in def.Materials) o.Materials[material] = o.Materials.GetValueOrDefault(material) + (int)(amount * rate * deltaSec);
        }
    }
}