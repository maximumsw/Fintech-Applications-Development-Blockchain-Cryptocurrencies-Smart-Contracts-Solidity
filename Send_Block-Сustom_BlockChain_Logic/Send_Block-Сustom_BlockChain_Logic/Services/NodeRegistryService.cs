namespace Send_Block_Сustom_BlockChain_Logic.Services;

public class NodeRegistryService
{
    private readonly Dictionary<string, BlockChainService> _nodes = new();

    public NodeRegistryService()
    {
        // Ініціалізувати три вузли
        _nodes["A"] = new BlockChainService("A");
        _nodes["B"] = new BlockChainService("B");
        _nodes["C"] = new BlockChainService("C");
    }

    public BlockChainService GetNode(string nodeId)
    {
        if (!_nodes.ContainsKey(nodeId))
            throw new ArgumentException($"Node '{nodeId}' not found");

        return _nodes[nodeId];
    }

    public IEnumerable<string> GetAllNodeIds()
    {
        return _nodes.Keys;
    }

    public Dictionary<string, bool> BroadcastBlock(string fromNodeId)
    {
        var sourceNode = GetNode(fromNodeId);
        var blockToBroadcast = sourceNode.GetLatestBlock();

        var results = new Dictionary<string, bool>();

        foreach (var nodeId in _nodes.Keys)
        {
            if (nodeId == fromNodeId)
                continue; // Не транслювати собі

            var targetNode = _nodes[nodeId];
            var accepted = targetNode.TryAddExternalBlock(blockToBroadcast);
            results[nodeId] = accepted;
        }

        return results;
    }

    public int GetNodeCount()
    {
        return _nodes.Count;
    }
}
