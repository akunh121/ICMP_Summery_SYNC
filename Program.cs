using System.Diagnostics;
using System.Net.NetworkInformation;

class Program
{
    #region Fields & Properties
    static int _PingCount = 2;
    static int _PingInterval = 500;
    static Stopwatch _StopWatch;
    static List<string> _HostsNames = new List<string>()
    {
            "cnn.com",
            "sbs.com.au",
            "bbc.co.uk",
            "maariv.co.il",
            "brazilian.report"
     };
    static string _Menu = @"Choose async method invokation that you would like to compare to sync invokation:
                        t = Thread
                        tp = ThreadPool
                        ta = Task
                        pf = Parallel for
                        pfe = Parallel for each
                        pi = Parallel invoke
                        OR ctrl+C to break...";

    #endregion
    public static void Main()
    {
        Console.WriteLine(_Menu);
        string userInput = Console.ReadLine().ToLower().Trim();
        Console.Clear();
        //
        PrintStars();
        PrintReport(GetHostsReplies);
        //        
        PrintStars();
        if (userInput == "t")
            PrintReport(GetHostsRepliesWithThreads);
        else if (userInput == "tp")
            PrintReport(GetHostsRepliesWithThreadPool);
        else if (userInput == "ta")
            PrintReport(GetHostsRepliesWithTasks);
        else if (userInput == "pf")
            PrintReport(GetHostsRepliesWithParallelFor);
        else if (userInput == "pfe")
            PrintReport(GetHostsRepliesWithParallelForEach);
        else if (userInput == "pi")
            PrintReport(GetHostsRepliesWithParallelInvoke);
        else Console.WriteLine("invalid input...");
    }

    #region  GetHostsReplies
    static Dictionary<string, List<PingReply>> GetHostsReplies()
    {

        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        foreach (var hostName in _HostsNames)
        {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            for (int i = 0; i < _PingCount; i++)
            {
                pingReplies.Add(ping.Send(hostName));
                Thread.Sleep(_PingInterval);
            }
            hostsReplies.Add(hostName, pingReplies);
        }
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithThreads()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        List<Thread> threads = new List<Thread>();
        foreach (var hostName in _HostsNames)
        {
            threads.Add(new Thread(() =>
            {
                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();

                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    Thread.Sleep(_PingInterval);
                }


                hostsReplies.Add(hostName, pingReplies);
            }));
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }




        //foreach (var hostName in _HostsNames)
        //{

        //    Ping ping = new Ping();
        //    List<PingReply> pingReplies = new List<PingReply>();

        //        pingReplies.Add(ping.Send(hostName));


        //    hostsReplies.Add(hostName, pingReplies);
        //}
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithThreadPool()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        List<ManualResetEvent> mres = new List<ManualResetEvent>();
        ManualResetEvent mre2add;
        WaitCallback wc2add;
        foreach (var hostName in _HostsNames)
        {
            mre2add = new ManualResetEvent(false);
            wc2add = new WaitCallback((O) =>
              {
                  hostsReplies.Add(hostName, GetPingReplies(hostName));
                  (O as ManualResetEvent).Set();
              });
            mres.Add(mre2add);
            ThreadPool.QueueUserWorkItem(wc2add, mre2add);
        }
        foreach (var mreItem in mres)
        {
            mreItem.WaitOne();
        }

        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithTasks()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        List<Task> task = new List<Task>();
        foreach (var hostName in _HostsNames)
        {
            task.Add(new Task(() =>
            {
                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();

                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    Thread.Sleep(_PingInterval);
                }


                hostsReplies.Add(hostName, pingReplies);
            }));
        }

        foreach (var taskS in task)
        {
            taskS.Start();
        }
        foreach (var taskS in task)
        {
            taskS.Wait();
        }
        //Task.WaitAll(task);




        //foreach (var hostName in _HostsNames)
        //{

        //    Ping ping = new Ping();
        //    List<PingReply> pingReplies = new List<PingReply>();

        //        pingReplies.Add(ping.Send(hostName));


        //    hostsReplies.Add(hostName, pingReplies);
        //}
        return hostsReplies;
    }


    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelInvoke()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();

        List<Action> actionsArray = new List<Action>();

        foreach (var hostName in _HostsNames)
        {
            actionsArray.Add(() =>
            {
                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();

                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    Thread.Sleep(_PingInterval);
                }


                hostsReplies.Add(hostName, pingReplies);
            });
        }


        Parallel.Invoke(
           actionsArray.ToArray()


        );






        return hostsReplies;
    }



    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelForEach()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();

        List<Action> actionsArray = new List<Action>();

        foreach (var hostName in _HostsNames)
        {
            actionsArray.Add(() =>
            {
                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();

                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    Thread.Sleep(_PingInterval);
                }


                hostsReplies.Add(hostName, pingReplies);
            });
        }

        Parallel.ForEach(actionsArray, (action) => action());

        //Parallel.Invoke(
        //   actionsArray.ToArray()


        //);






        return hostsReplies;
    }

    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelFor()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();

        List<Action> actionsArray = new List<Action>();

        foreach (var hostName in _HostsNames)
        {
            actionsArray.Add(() =>
            {
                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();

                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    Thread.Sleep(_PingInterval);
                }


                hostsReplies.Add(hostName, pingReplies);
            });
        }

        Parallel.For(0, actionsArray.Count, (index) => actionsArray[index]());

        //Parallel.Invoke(
        //   actionsArray.ToArray()


        //);






        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithTPL()
    {
        return null;
    }
    #endregion

    #region Print
    static List<PingReply> GetPingReplies(string hostName)
    {
        return GetPingReplies(hostName, _PingCount, _PingInterval);
    }
    static List<PingReply> GetPingReplies(string hostName, int pingCount = 1, int pingInterval = 1)
    {
        Ping ping = new Ping();
        List<PingReply> pingReplies = new List<PingReply>();
        for (int i = 0; i < pingCount; i++)
        {
            pingReplies.Add(ping.Send(hostName));
            if (pingCount == 1)
            {
                Thread.Sleep(pingInterval);
            }
        }
        return pingReplies;
    }
    static void PrintLine() => Console.WriteLine("---------------------------");
    static void PrintStars() => Console.WriteLine("***************************");
    static void PrintReport(Func<Dictionary<string, List<PingReply>>> getHostsReplies)
    {
        Console.WriteLine($"Started {getHostsReplies.Method.Name}");
        _StopWatch = Stopwatch.StartNew();
        Dictionary<string, List<PingReply>> hostsReplies = getHostsReplies();
        _StopWatch.Stop();
        Console.WriteLine($"Finished {getHostsReplies.Method.Name}");
        PrintLine();
        Console.WriteLine($"Printing {getHostsReplies.Method.Name} report:");
        if (hostsReplies != null)
            PrintHostsRepliesReports(hostsReplies);
        PrintLine();
    }
    static void PrintHostsRepliesReports(Dictionary<string, List<PingReply>> hostsReplies)
    {
        long hostsTotalRoundtripTime = 0;
        Dictionary<string, PingReplyStatistics> hrs = GetHostsRepliesStatistics(hostsReplies);
        PrintTotalRoundtripTime(hrs);
        PrintLine();
        hostsTotalRoundtripTime = hrs.Sum(hr => hr.Value.TotalRoundtripTime);
        Console.WriteLine($"Report took {_StopWatch.ElapsedMilliseconds} ms to generate,{_PingCount * _HostsNames.Count} total pings took total {hostsTotalRoundtripTime} ms hosts roundtrip time");
    }
    static void PrintTotalRoundtripTime(Dictionary<string, PingReplyStatistics> hrs, bool ascendingOrder = true)
    {
        string orderDescription = ascendingOrder ? "ascending" : "descending";
        Console.WriteLine($"Hosts total roundtrip time in {orderDescription} order: (HostName:X,Replies statistics:Y)");
        var orderedHrs = ascendingOrder ? hrs.OrderBy(hr => hr.Value.TotalRoundtripTime) : hrs.OrderByDescending(hr => hr.Value.TotalRoundtripTime);
        foreach (var hr in orderedHrs)
        {
            Console.WriteLine($"{hr.Key},{hr.Value}");
        }
    }
    static void PrintHostsRepliesStatistics(Dictionary<string, PingReplyStatistics> hrs)
    {
        Console.WriteLine("Hosts replies statistics: (HostName:X,Replies statistics:Y)");
        foreach (var hr in hrs)
        {
            Console.WriteLine($"{hr.Key},{hr.Value}");
        }
    }

    #endregion

    static Dictionary<string, PingReplyStatistics> GetHostsRepliesStatistics(Dictionary<string, List<PingReply>> hostsReplies)
    {
        Dictionary<string, PingReplyStatistics> hrs = new Dictionary<string, PingReplyStatistics>();
        foreach (var hr in hostsReplies)
            hrs.Add(hr.Key, new PingReplyStatistics(hr.Value));
        return hrs;
    }
}
