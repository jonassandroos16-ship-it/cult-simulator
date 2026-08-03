namespace CultSimulator.Game;

public static class WorldMapSystem
{
    public static MapNodeState? GetNode(OccultState o, string nodeId) => o.MapNodes.FirstOrDefault(n => n.NodeId == nodeId);
    public static bool IsConquered(OccultState o, string nodeId) => GetNode(o, nodeId)?.Conquered ?? false;

    public static bool CanConquer(GameState state, MapNodeDef def) =>
        def.CovenId == state.ActiveCovenId &&
        !IsConquered(state.Occult, def.Id) && state.ActiveCoven.Faith >= def.FaithCost;

    public static bool Conquer(GameState state, MapNodeDef def)
    {
        if (!CanConquer(state, def)) return false;
        state.ActiveCoven.Faith -= def.FaithCost;
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

    public static int ConqueredNodeCount(GameState state)
    {
        var covenNodes = OccultData.NodesForActiveCoven(state);
        return state.Occult.MapNodes.Count(n => n.Conquered && covenNodes.Any(cn => cn.Id == n.NodeId));
    }

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

    public static double GreatSealMultiplier(OccultState o) => 1.0;
}
