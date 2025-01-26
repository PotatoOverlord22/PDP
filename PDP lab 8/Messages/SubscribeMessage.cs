namespace PDP_lab_8.Messages
{
    [Serializable]
    public class SubscribeMessage : Message
    {
        public string Var { get; set; }
        public int Rank { get; set; }

        public SubscribeMessage(string var, int rank)
        {
            Var = var;
            Rank = rank;
        }
    }
}