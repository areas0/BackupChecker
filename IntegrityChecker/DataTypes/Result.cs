namespace IntegrityChecker.DataTypes
{
    // Class to be able to serialize and send results
    public class Result
    {
        public string ErrorMessage { get; set; }
        public int ErrorCount { get; set; }
    }
}