﻿using Bot.Core.Models;
using Common;
using Common.Services.gRPC;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Services
{
    public class SearchReciever
    {
        private readonly SearchClient searchClient;
        private readonly SearchState searchState;
        private readonly MessagesSender messagesSender;

        public SearchReciever(SearchClient searchClient, SearchState searchState, MessagesSender messagesSender)
        {
            this.searchClient = searchClient;
            this.searchState = searchState;
            this.messagesSender = messagesSender;
        }

        public async Task Search(long user, SearchRequest searchRequest, CancellationToken token)
        {
            Guid guid = Guid.NewGuid();
            Task searchTask = searchClient.Search(searchRequest, token);
            Task processingTask = Task.Factory.StartNew(async () =>
            {
                bool lastExecutionEnable = true;
                int number = 0;
                Page PageForSave = null;
                Page currentPage = new Page(guid, number) { position = Page.Position.First };
                while (!searchTask.IsCompleted || lastExecutionEnable)
                {
                    while (searchClient.searchResultReciever.TryDequeueResult(out var result))
                    {
                        if (!currentPage.TryAddResult(result))
                        {
                            int count = currentPage.count;
                            PageForSave = currentPage;
                            number++;
                            currentPage = new Page(guid, number) { position = Page.Position.Last, count = count };
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

                                await searchState.SavePage(user, PageForSave, token);
                                currentPage.MessageNumber = PageForSave.MessageNumber;
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
                await searchState.SavePage(user, currentPage, token);
                if (number == 0 && string.IsNullOrEmpty(currentPage.Text))
                {
                    messagesSender.AddItem(new TextMessage(null, user, "Ничего не найдено! Попробуйте другой запрос.", null, new ReplyKeyboardRemove()));
                }
            }, TaskCreationOptions.LongRunning);
            await Task.WhenAll(searchTask, processingTask);
        }
    }
}