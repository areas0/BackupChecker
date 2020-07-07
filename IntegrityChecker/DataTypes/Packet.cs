namespace IntegrityChecker.DataTypes
{
    // Tcp packet class with an id to identify the packets, message in string, and an owner (sending the message)
    public class Packet
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public Owner OwnerT { get; set; }
        public enum Owner
        {
            Server,
            Client
        }
    }
}