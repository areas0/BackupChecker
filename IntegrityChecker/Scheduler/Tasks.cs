namespace IntegrityChecker.Scheduler
{
    public class Tasks
    {
        public enum Task
        {
            Original = 0,
            Backup = 1,
            Compare = 2,
        }

        public Task Current { get; set; }

        public string OriginName { get; set; }
    }
}