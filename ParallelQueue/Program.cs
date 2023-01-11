// See https://aka.ms/new-console-template for more information
using ParallelQueue;

Console.WriteLine("Hello, World!");

QueueHandler q = new();

q.Init();
q.ChangeStatus();
q.Start();

while (true) {
    Thread.Sleep(1000);
}
