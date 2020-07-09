namespace IntegrityChecker.Scheduler
{
    // Json specific type: can be used to send a message on what to do for clients
    public class Tasks
    {
        public enum Task
        {
            Original = 0,
            Backup = 1,
            CompareFileList = 2,
            FileList = 3
        }

        public Task Current { get; set; }

        public string OriginName { get; set; }
    }
}