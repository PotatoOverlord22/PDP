namespace PDP_lab_8.Messages
{
    [Serializable]
    public class UpdateMessage : Message
    {
        public string Var { get; set; }
        public int Value { get; set; }

        public UpdateMessage(string var, int value)
        {
            Var = var;
            Value = value;
        }
    }
}