using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.Bll.Dto;
using SayWhat.MongoDAL.Examples;

namespace PureVocabBuilder;

class AllExamplesDictionary {
    private Dictionary<string, List<EssentialPhrase>> _dictionary = new();
    public int WordsCount => _dictionary.Count;

    public IReadOnlyList<EssentialPhrase> GetFor(string word) {
        if (!_dictionary.ContainsKey(word.ToLower()))
            return Array.Empty<EssentialPhrase>();
        return _dictionary[word.ToLower()] ?? new List<EssentialPhrase>();
    }

    public void AddExample(string en, string ru) {
        if (en.StartsWith("<"))
        {
            en = en.Replace("<", "").Replace(">", "");
        }

        var words = en.ToLower().Split(new[] { " ", "-", "'s" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string word in words)
        {
            var resultWord = word;
            //
            // if (word.EndsWith("'s"))
            //     resultWord = word.Remove(word.Length - 2);
            // if (word.Length > 3)
            // {
            //     if (word.EndsWith("s"))
            //         resultWord = word.Remove(word.Length - 1);
            // }

            AddExample(resultWord, new EssentialPhrase(en, ru));
        }
    }

    public List<EssentialPhrase> GetPhrases(string en, string ru) {
        var phrases = GetFor(en);
        var result = phrases
                     .Where(p => p.Fits(en, ru))
                     .ToList();
        return result;
    }

    public void AddExample(Example example) {
        var (en, ru) = example.Deconstruct();
        AddExample(en, ru);
    }

    private void AddExample(string word, EssentialPhrase example) {
        var key = word.ToLower();

        if (!_dictionary.ContainsKey(key))
            _dictionary.Add(key, new List<EssentialPhrase>());
        else
        {
            if (_dictionary[key].Any(i => i.En == example.En))
                return;
        }

        _dictionary[key].Add(example);
    }
}