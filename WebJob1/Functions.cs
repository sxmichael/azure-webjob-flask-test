using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ProcessQueueRegular
{
    public class Functions
    {
        public static void GenerateList(
            [QueueTrigger("queue-init")] string size,
            [Queue("queue-list")] ICollector<string> outputQueueMessage,
            TextWriter logger)
        {
            logger.LogWithTime("Starting the init job...");
            for (var i = 0; i < Convert.ToInt32(size); i++)
            {
                outputQueueMessage.Add((4567 + i).ToString());
            }
            logger.LogWithTime("Done the init job...");
        }

        public static void GenerateMessages(
            [QueueTrigger("queue-list")] string size,
            [Queue("queue")] ICollector<string> outputQueueMessage,
            TextWriter logger)
        {
            logger.LogWithTime("Starting the list job for {0}...", size);
            var count = 0;
            var chunkSize = 1000;
            foreach (var chunk in StringGenerator().Take(Convert.ToInt32(size)).Chunk(chunkSize))
            {
                outputQueueMessage.Add(string.Join("|", chunk));
                logger.WriteLine("Created {0} names", (++count) * chunkSize);
            }
            logger.LogWithTime("Finished the list job for {0}...", size);
        }

        private static IEnumerable<string> StringGenerator()
        {
            while (true)
                yield return Guid.NewGuid().ToString();
        }

        public static void ProcessQueueMessage([QueueTrigger("queue")] string message, TextWriter log)
        {
            var url = "http://sxwebjobtest.azurewebsites.net/do";
            var guid = Guid.NewGuid();
            log.LogWithTime("Starting web job");
            log.LogWithTime(guid.ToString());
            using (var wb = new WebClientEx())
            {
                //message = guid.ToString() + "$$" + message;
                try
                {
                    var response = wb.UploadString(url + "?id=" + guid.ToString(), message);
                    log.LogWithTime(response);
                }
                catch (WebException e)
                {
                    log.LogWithTime(e.ToString());
                    throw e;
                }

            }
            log.LogWithTime("finish log");
            Console.WriteLine("finish console");

        }
    }

    public static class Ext
    {
        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<List<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            var chunk = new List<T>();
            foreach (var e in source)
            {
                chunk.Add(e);
                if (chunk.Count >= chunksize)
                {
                    yield return chunk;
                    chunk.Clear();
                }
            }
        }

        public static void LogWithTime(this TextWriter log, string message, params object[] prms)
        {
            log.LogWithTime(string.Format(message, prms));
        }
        public static void LogWithTime(this TextWriter log, string message)
        {
            log.WriteLine("[" + DateTime.UtcNow + "] " + message);
        }

    }

    public class WebClientEx : WebClient
    {
        public int Timeout { get; set; }

        public WebClientEx() : this(600000) { }

        public WebClientEx(int timeout)
        {
            this.Timeout = timeout;
        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            request.Timeout = Timeout;
            return request;
        }
    }

}
