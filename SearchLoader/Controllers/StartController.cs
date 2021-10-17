using Common;
using Common.Services.gRPC;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using SearchLoader.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SearchLoader.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StartController
    {
        static List<string> WordsPool = new List<string>() { "какой-то", "запрос", "для", "теста", "условный","срок", "депутат","онотоле","вассерман",
        "космос", "РКН", "роскомпозор","моторолла","хуснуллин","стройкомплекс","людоед","фавшист","коронавирус","ковид","шприцеваться","вакцина",
        "вантиваксер","луна","марс","несквик","молоко","криветки","криль","кит","синий","парашют","десант","авианосец","мистраль","рис"};
        static ConcurrentQueue<SearchRequest> searchRequests = new ConcurrentQueue<SearchRequest>();
        private readonly IServiceProvider serviceProvider;
        public StartController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        [HttpPost("load")]
        public async Task<string> Run(RunLoadModel runModel, CancellationToken token)
        {
            Random rnd = new Random();

            Task t = Task.Factory.StartNew((_token) => 
            { 
                if (_token is CancellationToken tok)
                {
                    while (!tok.IsCancellationRequested && searchRequests.Count<1000)
                    {
                        HashSet<string> words = new HashSet<string>();
                        words.Add(WordsPool[rnd.Next(0, WordsPool.Count)]);
                        while (rnd.NextDouble() > 0.7)
                        {
                            words.Add(WordsPool[rnd.Next(0, WordsPool.Count)]);
                        }
                        string req = "";
                        foreach (string w in words)
                        {
                            req += w + '&';
                        }
                        req = req.Substring(0, req.Length - 2);
                        SearchRequest sr = new SearchRequest()
                        {
                            IsChannel = true,
                            IsGroup = true,
                            Request = req,
                            StartTime = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-rnd.Next(1, 1000))),
                            EndTime = Timestamp.FromDateTime(DateTime.UtcNow)
                        };

                        searchRequests.Enqueue(sr);
                        
                    }
                    Task.Delay(100);
                }
            }, token);
            List<Task> tasks = new List<Task>() {t };
            while (!token.IsCancellationRequested)
            {
                tasks.RemoveAll(item => item.IsCompleted);
                int starti = tasks.Count;
                for (int i = starti; i < runModel.Threads; i++)
                {
                    if (searchRequests.TryDequeue(out var sr))
                    {
                        SearchClient searchClient = (SearchClient)serviceProvider.GetService(typeof(SearchClient));
                        tasks.Add(searchClient.Search(sr, token));
                    }
                }
                await Task.WhenAny(tasks);
            }
            return "ok";
        }

        [HttpPost("sample")]
        public async Task<string> Run(RunSampleModel runSampleModel ,CancellationToken token)
        {
            Queue<SearchRequest> requests = new Queue<SearchRequest>();
            while (searchRequests.TryDequeue(out var sr)&&requests.Count< runSampleModel.Count)
            {
                requests.Enqueue(sr); 
            }
            DateTime dateTime = DateTime.UtcNow;
            while(requests.TryDequeue(out var req))
            {
                SearchClient searchClient = (SearchClient)serviceProvider.GetService(typeof(SearchClient));
                await searchClient.Search(req, token);
            }
            return Math.Round(DateTime.UtcNow.Subtract(dateTime).TotalSeconds / runSampleModel.Count,3).ToString();

        }
    }
}
