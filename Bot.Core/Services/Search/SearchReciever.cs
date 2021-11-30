using Bot.Core.Interfaces;
using Bot.Core.Models;
using Common;
using Common.Services.gRPC;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Services
{
    public class SearchReciever<TBot> where TBot:IConfig, new()
    {
        private readonly SearchClient searchClient;
        private readonly SearchState<TBot> searchState;
        private readonly IMessagesSender messagesSender;

        public SearchReciever(SearchClient searchClient, SearchState<TBot> searchState, IMessagesSender messagesSender)
        {
            this.searchClient = searchClient;
            this.searchState = searchState;
            this.messagesSender = messagesSender;
        }

        public async Task Search(long user, SearchRequest searchRequest, CancellationToken token)
        {
            Task searchTask = searchClient.Search(searchRequest, token);
            Task processingTask = Task.Factory.StartNew(async (tok) =>
            {
                CancellationToken token1;
                if (tok is CancellationToken token2)
                {
                    token1 = token2;
                }
                else token1 = CancellationToken.None;
                bool lastExecutionEnable = true;
                int number = 1;
                Page PageForSave = null;
                ObjectId currentId = ObjectId.GenerateNewId();
                ObjectId? nextId = ObjectId.GenerateNewId();
                ObjectId? prevId = null;
                List<Page> pages = new List<Page>();
                Page currentPage = new Page(currentId, prevId, nextId) { position = Page.Position.First };
                Channel<int> channel = Channel.CreateBounded<int>(1);
                bool sended = false;
                HashSet<string> texts = new HashSet<string>();
                HashSet<string> texts2 = new HashSet<string>();

                while ((!searchTask.IsCompleted || lastExecutionEnable)&&!token1.IsCancellationRequested)
                {
                    while (searchClient.searchResultReciever.TryDequeueResult(out var result))
                    {
                        if (texts.Contains(result.Text))
                        {
                            continue;
                        }
                        else
                        {
                            texts.Add(result.Text);
                            try
                            {
                                var words = result.Text.Split(' ','\n').Select(item=>item.ToLower()).Distinct().ToList();
                                if (words.Count < 10)
                                {
                                    words.Sort((item1, item2) => item1.Length == item2.Length ? 0 : item1.Length < item2.Length ? -1 : 1);
                                    string str = "";
                                    foreach (string st in words)
                                    {
                                        str += st;
                                    }
                                    if (texts2.Contains(str))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        texts2.Add(str);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }

                        }

                        if (!currentPage.TryAddResult(result))
                        {
                            int count = currentPage.count;
                            PageForSave = currentPage;
                            number++;
                            prevId = currentId;
                            currentId = nextId.Value;
                            nextId = ObjectId.GenerateNewId();
                            currentPage = new Page(currentId, prevId, nextId) { position = Page.Position.Last, count = count, Number=number };
                            currentPage.TryAddResult(result);
                        }
                        else
                        {
                            if (PageForSave != null)
                            {
                                if (PageForSave.position != Page.Position.First)
                                {
                                    PageForSave.position = Page.Position.Middle;
                                }
                                try
                                {
                                    //PageForSave.Text += "\n\n" + PageForSave.Number.ToString();
                                    pages.Add(PageForSave);
                                    if (pages.Count == 1)
                                    {
                                        if (!string.IsNullOrEmpty(pages[0].Text))
                                        {
                                            TextMessage textMessage = pages[0].GetTextMessage(null, user, channel,true);
                                            messagesSender.AddItem(textMessage);
                                        }
                                        sended = true;
                                    }

                                }
                                catch (Exception ex) 
                                { 
                                
                                }
                                //currentPage.MessageNumber = PageForSave.MessageNumber;
                                PageForSave = null;
                            }
                        }
                    }
                    if (searchTask.IsCompleted && lastExecutionEnable)
                    {
                        lastExecutionEnable = false;
                    }
                    await Task.Delay(50);
                }

                //    await searchState.SavePage(user, currentPage, token);

                if (!pages.Contains(currentPage) && string.IsNullOrEmpty(currentPage.Text)) ;
                    pages.Add(currentPage);
                int i = 1;

                foreach (var pag in pages)
                {
                    if (string.IsNullOrEmpty(pag.Text)) continue;
                    int offset = pag.Text.Length;
                    string pageSuff = "\n\nСтраница " + i.ToString() + " из " + pages.Count.ToString();
                    pag.Text += pageSuff;
                    pag.Formatting.Add(new Telegram.Bot.Types.MessageEntity() { Offset = offset, Length = pageSuff.Length, Type = Telegram.Bot.Types.Enums.MessageEntityType.Bold });
                    i++;
                }

                if (sended)
                {
                    int messNumber = await channel.Reader.ReadAsync();
                    messagesSender.AddItem(pages[0].GetEditTextMessage(null, user, messNumber));
                }
                else
                {
                    if (!string.IsNullOrEmpty(pages[0].Text))
                    {
                        TextMessage textMessage = pages[0].GetTextMessage(null, user, channel);
                        messagesSender.AddItem(textMessage);
                    }
                }


                await searchState.SavePages(pages, token);

                if (number == 1 && string.IsNullOrEmpty(currentPage.Text) && !token1.IsCancellationRequested)
                {
                    messagesSender.AddItem(new TextMessage(null, user, "Ничего не найдено! Попробуйте другой запрос.", null, new ReplyKeyboardRemove()));
                }
            }, token, TaskCreationOptions.LongRunning);
            await Task.WhenAll(searchTask, processingTask);
        }
    }
}
