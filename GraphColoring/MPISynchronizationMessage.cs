namespace GraphColoring
{
    [Serializable]
    public class MPISynchronizationMessage
    {
        public Node[] Nodes { get; set; }

        public int ProcessId { get; set; }
    }
}
