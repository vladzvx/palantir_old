﻿using Common.Services.DataBase.Interfaces;
using Common.Services.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase
{
    public class SMProcessor : ICommonProcessor<SavedMessage>
    {
        private readonly object locker = new object();

        public SMProcessor()
        {
            IsResultsOk = false;
            ProcessingsCounter = 0;
        }
        public bool IsResultsOk
        {
            get 
            { 
                lock (locker) return IsResultsOk; 
            }
            private set 
            {
                lock (locker) IsResultsOk = value; 
            }
        }

        public int ProcessingsCounter
        {
            get
            {
                lock (locker) return ProcessingsCounter;
            }
            private set
            {
                lock (locker) ProcessingsCounter = value;
            }
        }

        public void Process(SavedMessage data)
        {
            if (!string.IsNullOrEmpty(data.Text))
            {
                foreach (char c in "абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИКЛМНОПРСТУФХЦЧШЩЪЬЭЮЯ")
                {
                    IsResultsOk |= data.Text.Any(item => item == c);
                    ProcessingsCounter++;
                }
            }
        }
    }
}