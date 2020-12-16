﻿using System;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class AssemblePhraseExam :IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Assemble phrase";

        public async Task<ExamResult> Pass(ChatIO chatIo, UserWordModel word, UserWordModel[] examList) 
        {
            if (!word.HasAnyExamples)
                return ExamResult.Impossible;

            var targetPhrase = word.GetRandomExample();

            string shuffled;
            while (true)
            {
                var wordsInExample = targetPhrase.SplitWordsOfPhrase;
                
                if (wordsInExample.Length < 2)
                    return ExamResult.Impossible;

                shuffled = string.Join(" ", wordsInExample.Randomize());
                if(shuffled!= targetPhrase.OriginPhrase)
                    break;
            }

            await chatIo.SendMessageAsync("Words in phrase are shuffled. Write them in correct order:\r\n'" +  shuffled+ "'");
            var entry = await chatIo.WaitUserTextInputAsync();
            entry = entry.Trim();

            if (targetPhrase.OriginPhrase.AreEqualIgnoreCase(entry.Trim()))
                return ExamResult.Passed;

            await chatIo.SendMessageAsync($"Original phrase was: '{targetPhrase.OriginPhrase}'");
            return ExamResult.Failed;
        }
    }
}