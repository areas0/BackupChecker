﻿namespace IntegrityChecker.DataTypes
{
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