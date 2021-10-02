using Bot.Core.Enums;
using Bot.Core.Interfaces;
using Bot.Core.Interfaces.BotFSM;
using Bot.Core.Models;
using Common;
using Common.Services;
using Common.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Bot.Core.Services.Bot;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Core.Services
{

    public partial class Bot
    {
        public partial class FSM<TBot> where TBot:IConfig, new()
        {
            #region Properties and fields
            public static IServiceProvider serviceProvider;

            public readonly long chatId;


            internal readonly TBot config;
            private ISubFSM<TBot> subFSM;
            internal readonly IFSMFactory<TBot> subFSMFactory;
            internal readonly IReadyProcessor<TBot> readyProcessor;
            internal readonly IBusyProcessor busyProcessor;
            private readonly IRightChecker rightChecker;

            #endregion
            public FSM(long chatId, TBot settings)
            {
                if (serviceProvider == null)
                {
                    throw new ArgumentNullException("IServiceProvider serviceProvider static field is null! Work impossible!");
                }
                this.chatId = chatId;

                readyProcessor = (IReadyProcessor<TBot>)serviceProvider.GetService(typeof(IReadyProcessor<TBot>));
                busyProcessor = (IBusyProcessor)serviceProvider.GetService(typeof(IBusyProcessor));
                rightChecker = (IRightChecker)serviceProvider.GetService(typeof(IRightChecker));
                subFSMFactory = (IFSMFactory<TBot>)serviceProvider.GetService(typeof(IFSMFactory<TBot>));
                config = settings;
            }
            public async Task Process(Update update, CancellationToken token)
            {
                if (await rightChecker.Check(update, this))
                {
                    try
                    {
                        switch (config.BotState)
                        {
                            case PrivateChatState.SubFSMWorking:
                                {
                                    await subFSM.ProcessUpdate(update, config, token);
                                    break;
                                }
                            case PrivateChatState.Ready:
                                {
                                    await readyProcessor.ProcessUpdate(update, this);
                                    break;
                                }
                            case PrivateChatState.Busy:
                                {
                                    await busyProcessor.ProcessUpdate<TBot>(update, this);
                                    break;
                                }
                        }
                    }
                    catch { }
                }
                
            }

            #region private

            internal async Task<bool> TryStartSubFSM(Update update, bool force = false)
            {
                var fsm = await subFSMFactory.Get(update);
                if (!fsm.IsEmpty)
                {
                    subFSM = fsm;
                    config.BotState = PrivateChatState.SubFSMWorking;
                    await subFSM.ProcessUpdate(update, config, CancellationToken.None);
                    return true;
                }
                return false;
            }



            #endregion
        }
    }
}
