using System.Runtime.CompilerServices;

namespace ParallelQueue;

public enum Status {
    READY = 0,
    READ = 1,
    RUNNING = 2,
    COMPLETE = 3
}

public class JobToRun
{
    public int Id {get; set;} = -1;

    public Status Status {get; set;} = Status.READY;

    public override string ToString() => $"{Id} {Status}";
}
