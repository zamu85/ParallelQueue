
namespace ParallelQueue;

public class QueueHandler
{
    private IList<JobToRun> _waitQueue;
    private IList<JobToRun> _runningQueue;
    private readonly object _lock;

    public QueueHandler()
    {
        _waitQueue = new List<JobToRun>();
        _runningQueue = new List<JobToRun>(5);
        _lock = new();
    }

    public void Init()
    {
        EnqueueOnWait();
    }

    public void Start()
    {
        Task.Factory.StartNew(() =>
        {
            DequeueFromWait();
        });

        Task.Factory.StartNew(() =>
        {
            DequeueFromRunning();
        });

    }

    private void EnqueueOnWait()
    {
        for (int i = 0; i < 13; i++)
        {
            lock (_lock)
            {
                _waitQueue.Add(new JobToRun() { Id = i });
            }

            Thread.Sleep(100);
        }
    }

    public void ChangeStatus()
    {
        for (int i = 0; i < 13; i++)
        {
            lock (_lock)
            {
                _waitQueue.ElementAt(i).Status = Status.READ;
            }

            Thread.Sleep(100);
        }
    }

    private void DequeueFromWait()
    {
        while (true)
        {
            lock (_lock)
            {
                var toBeRunned = new List<JobToRun>(_waitQueue.Where(q => q.Status == Status.READ).Take(5));
                
                for (int i = 0; i < toBeRunned.Count(); i++)
                {
                    var item = toBeRunned.ElementAt(i);
                    if (!_runningQueue.Contains(item) && _runningQueue.Count() < 5)
                    {
                        System.Console.WriteLine($"Add {item} to running queue");
                        _runningQueue.Add(item);
                        System.Console.WriteLine($"Remove {item} to from wait queue");

                        _waitQueue.Remove(item);
                    }
                }
            }
            Thread.Sleep(200);
        }
    }

    private void DequeueFromRunning()
    {
        IList<JobToRun> list = null!;
        while (true)
        {
            lock (_lock)
            {
                list = _runningQueue.Where(q => q.Status == Status.READ).ToList();
            }

            for (int i = 0; i < list.Count(); i++)
            {
                JobToRun element = list.ElementAt(i);

                ParameterizedThreadStart starter = new ParameterizedThreadStart(LongRunningTask);
                starter += (object? e) =>
                {
                    if (e is null)
                    {
                        return;
                    }

                    lock (_lock)
                    {
                        var index = _runningQueue.IndexOf((JobToRun)e);
                        if (index != -1)
                        {
                            System.Console.WriteLine($"Remove element {element} from running queue");
                            _runningQueue.RemoveAt(index);
                        }
                        else
                        {
                            System.Console.WriteLine($"Element {element} not found");
                        }
                    }
                };
                Thread thread = new Thread(starter) { IsBackground = true };
                thread.Start(element);

            }
            Thread.Sleep(200);
        }
    }

    private void LongRunningTask(object? element)
    {
        Random rndRepeat = new Random();
        int repeatTime = rndRepeat.Next(10);

        Random rndWait = new Random();
        int waitTime = rndWait.Next(200,500);

        ((JobToRun)element).Status = Status.RUNNING;

        for (int i = 0; i < repeatTime; i++)
        {
            System.Console.WriteLine($"Consuming element {element}/{i}, then sleep {waitTime}");
            Thread.Sleep(waitTime);
        }

        ((JobToRun)element).Status = Status.COMPLETE;
    }
}
