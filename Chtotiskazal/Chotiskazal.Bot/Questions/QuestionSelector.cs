using System;
using System.Collections.Generic;
using System.Linq;
using Chotiskazal.Bot.ConcreteQuestions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class QuestionSelector
    {
        public static QuestionSelector Singletone { get; set; }

        private readonly ExamAndPreferredScore _engChoose = new ExamAndPreferredScore(
            question: new EngChooseQuestion(),
            expectedScore: 0.6,
            frequency: 7);

        private readonly ExamAndPreferredScore _ruChoose = new ExamAndPreferredScore(
            question: new RuChooseQuestion(),
            expectedScore: 0.6,
            frequency: 7);

        private readonly ExamAndPreferredScore _engTrust = new ExamAndPreferredScore(
            question: new EnTrustQuestion(),
            expectedScore: 2,
            frequency: 10);

        private readonly ExamAndPreferredScore _ruTrust = new ExamAndPreferredScore(
            question: new RuTrustQuestion(),
            expectedScore: 2,
            frequency: 10);

        private readonly ExamAndPreferredScore _ruTrustSingle = new ExamAndPreferredScore(
            question: new RuTrustSingleTranslationQuestion(), 
            expectedScore: 2,
            frequency: 10);
        
        private readonly ExamAndPreferredScore _ruTrustSingleHideous = new ExamAndPreferredScore(
            question: new ClearScreenQuestionDecorator(new RuTrustSingleTranslationQuestion()), 
            expectedScore: 2,
            frequency: 10);
        
        private readonly ExamAndPreferredScore _engPhraseChoose = new ExamAndPreferredScore(
            question: new EngChoosePhraseQuestion(),
            expectedScore: 1.3,
            frequency: 4);

        private readonly ExamAndPreferredScore _ruPhraseChoose = new ExamAndPreferredScore(
            question: new RuChoosePhraseQuestion(),
            expectedScore: 1.3,
            frequency: 4);

        
        private ExamAndPreferredScore _engEasyWriteMissingLetter =>
            new ExamAndPreferredScore(
                question: new EngWriteMissingLettersQuestion(StarredHardness.Easy),
                expectedScore: 2.1,
                frequency: 7);
        private ExamAndPreferredScore _ruEasyWriteMissingLetter =>
            new ExamAndPreferredScore(
                question: new RuWriteMissingLettersQuestion(StarredHardness.Easy),
                expectedScore: 2.1,
                frequency: 7);
        private ExamAndPreferredScore _engEasyWriteMissingLetterHideous =>
            new ExamAndPreferredScore(
                question: new ClearScreenQuestionDecorator(new EngWriteMissingLettersQuestion(StarredHardness.Easy)),
                expectedScore: 2.3,
                frequency: 7);
        private ExamAndPreferredScore _ruEasyWriteMissingLetterHideous =>
            new ExamAndPreferredScore(
                question: new ClearScreenQuestionDecorator(new RuWriteMissingLettersQuestion(StarredHardness.Easy)),
                expectedScore: 2.3,
                frequency: 7);
        
        private ExamAndPreferredScore _engHardWriteMissingLetter =>
            new ExamAndPreferredScore(
                question: new EngWriteMissingLettersQuestion(StarredHardness.Hard),
                expectedScore: 2.6,
                frequency: 7);
        private ExamAndPreferredScore _ruHardWriteMissingLetter =>
            new ExamAndPreferredScore(
                question: new RuWriteMissingLettersQuestion(StarredHardness.Hard),
                expectedScore: 2.6,
                frequency: 7);
        private ExamAndPreferredScore _engHardWriteMissingLetterHideous =>
            new ExamAndPreferredScore(
                question: new ClearScreenQuestionDecorator(new EngWriteMissingLettersQuestion(StarredHardness.Hard)),
                expectedScore: 2.8,
                frequency: 7);
        private ExamAndPreferredScore _ruHardWriteMissingLetterHideous =>
            new ExamAndPreferredScore(
                question: new ClearScreenQuestionDecorator(new RuWriteMissingLettersQuestion(StarredHardness.Hard)),
                expectedScore: 2.8,
                frequency: 7);
        
        private readonly ExamAndPreferredScore _engChooseWordInPhrase = new ExamAndPreferredScore(
            new EngChooseWordInPhraseQuestion(), 2, 20);

        private readonly ExamAndPreferredScore _clearEngChooseWordInPhrase = new ExamAndPreferredScore(
            new ClearScreenQuestionDecorator(new EngChooseWordInPhraseQuestion()), 2.3, 20);

        private readonly ExamAndPreferredScore _engPhraseSubstitute = new ExamAndPreferredScore(
            question: new EngPhraseSubstituteQuestion(),
            expectedScore: 2,
            frequency: 12);

        private readonly ExamAndPreferredScore _ruPhraseSubstitute = new ExamAndPreferredScore(
            question: new RuPhraseSubstituteQuestion(),
            expectedScore: 2,
            frequency: 12);

        private readonly ExamAndPreferredScore _assemblePhraseExam = new ExamAndPreferredScore(
            new AssemblePhraseQuestion(), 2.3, 7);

        private readonly ExamAndPreferredScore _clearEngPhraseSubstitute = new ExamAndPreferredScore(
            question: new ClearScreenQuestionDecorator(new EngPhraseSubstituteQuestion()),
            expectedScore: 2.6,
            frequency: 12);

        private readonly ExamAndPreferredScore _clearRuPhraseSubstitute = new ExamAndPreferredScore(
            question: new ClearScreenQuestionDecorator(new RuPhraseSubstituteQuestion()),
            expectedScore: 2.6,
            frequency: 12);

        
        private ExamAndPreferredScore EngWrite(LocalDictionaryService service) =>
            new ExamAndPreferredScore(
                question: new EngWriteQuestion(service),
                expectedScore: 2.6,
                frequency: 14);

        private ExamAndPreferredScore RuWrite(LocalDictionaryService service) => new ExamAndPreferredScore(
            question: new RuWriteQuestion(service),
            expectedScore: 2.6,
            frequency: 14);

        private readonly ExamAndPreferredScore _hideousEngPhraseChoose = new ExamAndPreferredScore(
            question: new ClearScreenQuestionDecorator(new EngChoosePhraseQuestion()),
            expectedScore: 2.3,
            frequency: 10);

        private readonly ExamAndPreferredScore _hideousRuPhraseChoose = new ExamAndPreferredScore(
            question: new ClearScreenQuestionDecorator(new RuChoosePhraseQuestion()),
            expectedScore: 2.3,
            frequency: 10);

        private readonly ExamAndPreferredScore _hideousEngTrust = new ExamAndPreferredScore(
            question: new ClearScreenQuestionDecorator(new EnTrustQuestion()),
            expectedScore: 3.3,
            frequency: 2);

        private readonly ExamAndPreferredScore _hideousRuTrust =
            new ExamAndPreferredScore(
                question: new ClearScreenQuestionDecorator(new RuTrustQuestion()),
                expectedScore: 3.3,
                frequency: 3);

        private ExamAndPreferredScore HideousEngWriteExam(LocalDictionaryService service) =>
            new ExamAndPreferredScore(
                question: new ClearScreenQuestionDecorator(new EngWriteQuestion(service)),
                expectedScore: 4,
                frequency: 14);

        private ExamAndPreferredScore HideousRuWriteExam(LocalDictionaryService service) =>
            new ExamAndPreferredScore(
                question: new ClearScreenQuestionDecorator(new RuWriteQuestion(service)),
                expectedScore: 4,
                frequency: 14);

        private readonly ExamAndPreferredScore _transcriptionExam = new ExamAndPreferredScore(
            question: new TranscriptionChooseQuestion(),
            expectedScore: 1.6,
            frequency:10);
        
        private readonly ExamAndPreferredScore _hideousTranscriptionExam = new ExamAndPreferredScore(
            question: new ClearScreenQuestionDecorator(new TranscriptionChooseQuestion()),
            expectedScore: 2.6,
            frequency:10);

        private readonly ExamAndPreferredScore _engChooseByTranscriptionExam = new ExamAndPreferredScore(
            question: new TranscriptionChooseEngQuestion(),
            expectedScore: 2.7,
            frequency: 7);

        private readonly ExamAndPreferredScore _ruChooseByTranscriptionExam = new ExamAndPreferredScore(
            question: new TranscriptionChooseRuQuestion(),
            expectedScore: 3.3,
            frequency: 7);
        
        private readonly ExamAndPreferredScore _hideousRuChooseByTranscriptionExam = new ExamAndPreferredScore(
            question: new ClearScreenQuestionDecorator(new TranscriptionChooseRuQuestion()),
            expectedScore: 4,
            frequency: 10);
        
        private readonly ExamAndPreferredScore _isItRightTranslationExam = new ExamAndPreferredScore(
            question:new IsItRightTranslationQuestion(), 
            expectedScore:1.6,
            frequency:7);
        
        private readonly ExamAndPreferredScore _hideousIsItRightTranslationExam = new ExamAndPreferredScore(
            question: new ClearScreenQuestionDecorator(new IsItRightTranslationQuestion()), 
            expectedScore:2.3,
            frequency:7);
        
        private readonly ExamAndPreferredScore _engChooseMultipleTranslationExam = new ExamAndPreferredScore(
            question:new EngChooseMultipleTranslationsQuestion(),  
            expectedScore:1.6,
            frequency:10);

        private readonly ExamAndPreferredScore _hideousEngChooseMultipleTranslationExam = new ExamAndPreferredScore(
            question: new ClearScreenQuestionDecorator(new EngChooseMultipleTranslationsQuestion()),
            expectedScore: 2.3,
            frequency: 10);

        private ExamAndPreferredScore RuWriteSingleTranslationExam(LocalDictionaryService service) => new ExamAndPreferredScore(
            question:new RuWriteSingleTarnslationQuestion(service),  
            expectedScore:1.6,
            frequency:10);
       
        private ExamAndPreferredScore HideousRuWriteSingleTranslationExam(LocalDictionaryService service) => new ExamAndPreferredScore(
            question:new RuWriteSingleTarnslationQuestion(service),  
            expectedScore:1.6,
            frequency:10);

        public QuestionSelector(LocalDictionaryService localDictionaryService)
        {
            _simpleExamsList = new[]
            {
                _engChoose,
                _ruChoose,
                _ruPhraseChoose,
                _engPhraseChoose,
                _engChooseWordInPhrase,
                _transcriptionExam,
                _isItRightTranslationExam,
                _engChooseMultipleTranslationExam,
            };
            _intermidiateExamsList = new[]
            {
                _engEasyWriteMissingLetter,
                _ruEasyWriteMissingLetter,
                _engHardWriteMissingLetter,
                _ruHardWriteMissingLetter,
                _engChoose,
                _ruChoose,
                _ruPhraseChoose,
                _engPhraseChoose,
                _engTrust,
                _ruTrust,
                _hideousRuTrust,
                _ruTrustSingle,
                _ruTrustSingleHideous,
                _hideousEngTrust,
                _engChooseByTranscriptionExam,
                _ruChooseByTranscriptionExam,
                _transcriptionExam,
                _isItRightTranslationExam,
                _engChooseMultipleTranslationExam,
                _hideousEngChooseMultipleTranslationExam,
            };
            _advancedExamsList = new[]
            {
                _engEasyWriteMissingLetter,
                _ruEasyWriteMissingLetter,
                _engEasyWriteMissingLetterHideous,
                _ruEasyWriteMissingLetterHideous,
                _engHardWriteMissingLetter,
                _ruHardWriteMissingLetter,
                _engHardWriteMissingLetterHideous,
                _ruHardWriteMissingLetterHideous,
                _engChoose,
                _ruChoose,
                _engPhraseChoose,
                _ruPhraseChoose,
                _engTrust,
                _ruTrust,
                _ruTrustSingle,
                EngWrite(localDictionaryService),
                RuWrite(localDictionaryService),
                RuWriteSingleTranslationExam(localDictionaryService),
                _hideousRuPhraseChoose,
                _hideousEngPhraseChoose,
                _hideousEngTrust,
                _hideousRuTrust,
                _ruTrustSingleHideous,
                HideousEngWriteExam(localDictionaryService),
                HideousRuWriteExam(localDictionaryService),
                HideousRuWriteSingleTranslationExam(localDictionaryService),
                _clearEngPhraseSubstitute,
                _clearRuPhraseSubstitute,
                _engPhraseSubstitute,
                _ruPhraseSubstitute,
                _engChooseWordInPhrase,
                _clearEngChooseWordInPhrase,
                _assemblePhraseExam,
                _engChooseByTranscriptionExam,
               _ruChooseByTranscriptionExam,
               _hideousRuChooseByTranscriptionExam,
               _hideousTranscriptionExam,
               _hideousIsItRightTranslationExam,
                _transcriptionExam,
                _isItRightTranslationExam,
                _engChooseMultipleTranslationExam,
                _hideousEngChooseMultipleTranslationExam,
            };
        }

        private readonly ExamAndPreferredScore[] _simpleExamsList;
        private readonly ExamAndPreferredScore[] _intermidiateExamsList;
        private readonly ExamAndPreferredScore[] _advancedExamsList;

        public HashSet<ExamAndPreferredScore> AllQuestions =>
            _simpleExamsList.Concat(_intermidiateExamsList).Concat(_advancedExamsList).ToHashSet();
        public IQuestion GetNextQuestionFor(bool isFirstExam, UserWordModel model)
        {
            if (isFirstExam && model.AbsoluteScore < WordLeaningGlobalSettings.IncompleteWordMinScore)
                return _simpleExamsList.GetRandomItemOrNull().Question;

            var score = model.AbsoluteScore - (isFirstExam ? (WordLeaningGlobalSettings.LearnedWordMinScore/2) : 0);

            if (model.AbsoluteScore < WordLeaningGlobalSettings.FamiliarWordMinScore)
                return ChooseExam(score, _intermidiateExamsList);
            else
                return ChooseExam(score, _advancedExamsList);
        }

        private static IQuestion ChooseExam(double score, ExamAndPreferredScore[] exams)
        {
            score = Math.Min(score, 4.5);
            var probability = new Dictionary<double, IQuestion>(exams.Length);
            double accumulator = 0;
            foreach (var e in exams)
            {
                var delta = e.Frequency / (Math.Abs(e.ExpectedScore - score) + 0.3);
                accumulator += delta;
                probability.Add(accumulator, e.Question);
            }

            var randomValue = Rand.NextDouble() * accumulator;
            var choice = (probability.FirstOrDefault(p => p.Key >= randomValue).Value);
            return choice ?? probability.Last().Value;
        }

        public class ExamAndPreferredScore
        {
            public ExamAndPreferredScore(IQuestion question, double expectedScore, int frequency)
            {
                Question = question;
                ExpectedScore = expectedScore;
                Frequency = frequency;
            }

            public IQuestion Question { get; }
            public double ExpectedScore { get; }
            public int Frequency { get; }
        }
    }
}
