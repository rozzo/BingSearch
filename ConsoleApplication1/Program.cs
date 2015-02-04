using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        /// <summary>
        /// User Defined Parameters
        /// Gold: 90 searches (1 point for 3 searches, 30 points max)
        /// Silver: 30 Searches (1 point for 2 searches, 15 points max)
        /// Blue: ?
        /// </summary>
        private static int Searches = 90;
        private static int MaxSleep = 60000;
        private static TimeSpan startTime;
        private static TimeSpan endTime;

        /// <summary>
        /// Program to run bing Searches
        /// </summary>
        /// <param name="args">ForeverMode (bool) startTime(int) endTime(int)</param>
        static void Main(string[] args)
        {
            // There is an issue when IE is already open and since it will not be usable kill all windows
            KillIE();

            // If forever mode
            if (args.Length > 0 && args[0].Equals("true"))
            {
                var start = int.Parse(args[1]);
                var end = int.Parse(args[2]);

                startTime = new TimeSpan(start, 0, 0);
                endTime = new TimeSpan(end, 0, 0);

                System.Console.WriteLine("In Forever Mode.");
                // Run between time frame to look more natural
                MaxSleep = (int)(endTime.TotalMilliseconds - startTime.TotalMilliseconds) / Searches;

                //In forever mode run FOREVER!!!! MUHAHA
                while (true)
                {
                    var currentTime = System.DateTime.Now.TimeOfDay;

                    System.Console.WriteLine("Current Time: " + currentTime);

                    if (currentTime >= startTime && endTime > currentTime)
                    {
                        System.Console.WriteLine("Running Bing Search.");
                        // Get me my points
                        BingSearch();

                        // There is an issue killing all processes so do house cleaning.
                        KillIE();

                        //sleep until end time (otherwise we may run again)
                        currentTime = System.DateTime.Now.TimeOfDay;
                        if (currentTime < endTime)
                        {
                            System.Console.WriteLine("Waiting for time frame to end.");
                            var waitTime = endTime.Subtract(currentTime);

                            //wait time may be inaccurate and waking up early (fraction of a second)
                            waitTime = waitTime.Add(new TimeSpan(0, 10, 0));
                            Thread.Sleep(waitTime);
                        }
                    }
                    else
                    {
                        var waitTime = startTime.Subtract(currentTime);

                        //if waitTime is negative then you have to wait until tomorrow to run
                        if (waitTime < new TimeSpan(0, 0, 0))
                            waitTime = waitTime.Add(new TimeSpan(24, 1, 0));

                        System.Console.WriteLine("Waiting for Next run time. " + waitTime);
                        Thread.Sleep(waitTime);
                    }
                }
            }
            else
            {
                System.Console.WriteLine("Running Big Search");
                // Get me my points
                BingSearch();

                // There is an issue killing all processes so do house cleaning.
                KillIE();
            }
        }

        static void Main2(string[] args)
        {
            BingWebRequest bing = new BingWebRequest("userName", "Password");
            bing.SendBingRequest();
            bing.SendBingRequest();
        }

        static void BingSearch()
        {
            var startTime = System.DateTime.Now;
            var url = "http://www.bing.com/search?q={term}&form=MOZSBR&pc=MOZI";
            var rand = new Random();

            // Words from http://wordlist.sourceforge.net/
            var searchTerms = new List<string>(File.ReadAllLines("english-words.70"));

            searchTerms.Add("google is better");
            searchTerms.Add("bing is terrible");
            searchTerms.Add("bing sucks");

            int i = 0;
            var processesToKill = new List<Process>();

            while (i < Searches)
            {
                var term = searchTerms[rand.Next(searchTerms.Count)];
                var tempUrl = url.Replace("{term}", term);
                
                // Make window open up minimized
                var psi = new ProcessStartInfo("c:/program files/internet explorer/iexplore.exe", tempUrl)
                {
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                processesToKill.Add(Process.Start(psi));

                // Sleep for random amount of time
                Thread.Sleep(rand.Next(MaxSleep));
                i++;

                if (i > 0 && i % 10 == 0)
                    KillIE(processesToKill);
            }

            KillIE(processesToKill);
        }

        private static void KillIE()
        {
            KillIE(new List<Process>(Process.GetProcessesByName("iexplore")));
        }

        private static void KillIE(List<Process> processesToKill)
        {
            foreach (var process in processesToKill)
            {
                if (!process.HasExited)
                {
                    process.CloseMainWindow();
                    Thread.Sleep(500);
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception)
                    {
                        //ignore error
                    }
                }
            }

            processesToKill.Clear();
        }
    }
}
