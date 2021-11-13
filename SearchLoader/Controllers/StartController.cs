using Common;
using Common.Services.gRPC;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using SearchLoader.Models;
using SearchLoader.Services;
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
        "вантиваксер","луна","марс","несквик","молоко","криветки","криль","кит","синий","парашют","десант","авианосец","мистраль","рис","обрати",
        "неубиваемые","машины","пробега","тойота","оптика","препод"
        ,"люди","директор","школа","учитель"
        ,"человечество","термоядерный","синтез","гелий"
        ,"батон","ядрен","уран","уравнение"
        ,"сомбрерро","булка","виноград","хурма"
        ,"потокобезопасность","утечка","память","потоки"
        ,"продакшн","хуяк","деплой","докер"
        ,"тестировщик","разработчик","датасатанист","ученый"
        ,"технических","доктор","микробиолог","исследователь"};
        static ConcurrentQueue<SearchRequest> searchRequests = new ConcurrentQueue<SearchRequest>();
        private readonly IServiceProvider serviceProvider;
        public StartController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [HttpPost("exec")]
        public async Task<string> exec(RunLoadModel runModel, CancellationToken token)
        {
            ConcurrentQueue<SearchRequest> searchRequests = new ConcurrentQueue<SearchRequest>();
            Random rnd = new Random();
            while (searchRequests.Count < runModel.RequestsNumber)
            {
                HashSet<string> words = new HashSet<string>();
                words.Add(WordsPool[rnd.Next(0, WordsPool.Count)]);
                while (rnd.NextDouble() > 0.5)
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
                    StartTime = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-rnd.Next(1, 30))),
                    EndTime = Timestamp.FromDateTime(DateTime.UtcNow),
                    SearchType = SearchType.SearchNamePeriod,
                    Limit = 100
                };

                searchRequests.Enqueue(sr);
            }


            DateTime dateTime = DateTime.UtcNow;
            List<Task> tasks = new List<Task>() {};
            List<SearchResultsTestReciever> srtrs = new List<SearchResultsTestReciever>() {};
            while (!token.IsCancellationRequested)
            {
                tasks.RemoveAll(item => item.IsCompleted);
                //tasks2.RemoveAll(item => item.IsCompleted);
                int starti = tasks.Count;
                for (int i = starti; i < runModel.Threads; i++)
                {
                    if (searchRequests.TryDequeue(out var sr))
                    {
                        SearchClient searchClient = (SearchClient)serviceProvider.GetService(typeof(SearchClient));
                        if (searchClient.searchResultReciever is SearchResultsTestReciever srtr)
                        {
                            srtr.CreationTime = DateTime.UtcNow;
                            srtrs.Add(srtr);
                            tasks.Add(searchClient.Search(sr, token));
                            //tasks2.Add();
                        }
                        else
                        {
                            tasks.Add(searchClient.Search(sr, token));
                        }
                    }
                }
                //tasks2.AddRange(tasks);
                //tasks2 = tasks2.Distinct().ToList();
                if (tasks.Count==0) break;
                await Task.WhenAny(tasks);
            }
            var data = srtrs.Where(item => item.FirstRecieved.HasValue).Select(item => item.FirstRecieved.Value.Subtract(item.CreationTime).TotalSeconds);
            double sum = data.Sum();
            double count = data.Count();
            double mean = sum / count;
            var data2 = data.Select(item =>(Math.Pow(item - mean,2)/ count));
            double std = Math.Pow(data2.Sum(), 0.5);
            var data2sigma = data.Where(item => Math.Abs(item - mean) < 2 * std);
            var data3sigma = data.Where(item => Math.Abs(item - mean) < 3 * std);
            string message = string.Format("ForFirstResult. Count: {0}; Mean: {1}; std: {2}; min: {3}; max: {4}; 2sigmaMean: {5}; 2sigmaCount: {6}; 3sigmaMean: {7}; 3sigma Count: {8};", Math.Round(count,3), Math.Round(mean, 3), Math.Round(std, 3), data.Min(), data.Max(), data2sigma.Average(), data2sigma.Count(), data3sigma.Average(), data3sigma.Count()) ; 
            return message+=" "+ "ForFullResult: " + DateTime.UtcNow.Subtract(dateTime).TotalSeconds.ToString();
        }


        [HttpPost("load")]
        public async Task<string> Run(RunLoadModel runModel, CancellationToken token)
        {
            Random rnd = new Random();

            Task t = Task.Factory.StartNew((_token) => 
            { 
                if (_token is CancellationToken tok)
                {
                    while (!tok.IsCancellationRequested)
                    {
                        while (searchRequests.Count < 1000)
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
                                StartTime = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-rnd.Next(1, 30))),
                                EndTime = Timestamp.FromDateTime(DateTime.UtcNow),
                                SearchType = SearchType.SearchNamePeriod,
                                Limit = 100
                            };

                            searchRequests.Enqueue(sr);
                        }
                        Task.Delay(100);

                    }

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
            if (runSampleModel.Request == null)
            {
                while (searchRequests.TryDequeue(out var sr) && requests.Count < runSampleModel.Count)
                {
                    requests.Enqueue(sr);
                }
            }
            else
            {
                requests.Enqueue(new SearchRequest() 
                {
                    IsChannel=true,
                    IsGroup=true,
                    Request=runSampleModel.Request,
                    StartTime=Timestamp.FromDateTime( DateTime.UtcNow.AddDays(-365)),
                    EndTime=Timestamp.FromDateTime(DateTime.UtcNow),
                    SearchType=SearchType.SearchNamePeriod,
                    Limit=100
                });
            }

            DateTime dateTime = DateTime.UtcNow;
            SearchClient searchClient = (SearchClient)serviceProvider.GetService(typeof(SearchClient));
            while (requests.TryDequeue(out var req))
            {
                await searchClient.Search(req, token);
            }
            string result = "";
            if (searchClient.searchResultReciever is SearchResultsTestReciever srr && srr.FirstRecieved.HasValue)
            {
                result += "Results: "+srr.count;
                result+="First recieved: "+Math.Round(DateTime.UtcNow.Subtract(srr.FirstRecieved.Value).TotalSeconds / runSampleModel.Count, 3).ToString();
            }
            
            return result+="Total: " +Math.Round(DateTime.UtcNow.Subtract(dateTime).TotalSeconds / runSampleModel.Count,3).ToString();

        }
    }
}
