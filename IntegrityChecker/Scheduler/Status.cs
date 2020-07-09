namespace IntegrityChecker.Scheduler
{
    // Statuses to synchronize clients over network
    public enum Status
    {
        Ok = 0,
        Error = 1,
        Waiting = 2,
    }
}