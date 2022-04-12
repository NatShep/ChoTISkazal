using NUnit.Framework;
using SayWhat.Bll;
using SayWhat.Bll.Strings;

namespace SayWhat.MongoDAL.Tests {

public class StringHelperTest {
    [TestCase("Something",0, "")]
    [TestCase("Something",1, "g")]
    [TestCase("Something",2, "ng")]
    [TestCase("Something",7, "mething")]
    [TestCase("a",1, "a")]
    [TestCase("",0, "")]
    public void TailTest(string origin, int count, string expected) => Assert.AreEqual(origin.Tail(count), expected);

    [TestCase("S",'S')]
    [TestCase("Something",'S')]
    public void FirstSymbolTest(string origin, char expected) => Assert.AreEqual(origin.FirstSymbol(), expected);
    
    [TestCase("S",'S')]
    [TestCase("Something",'g')]
    public void LastSymbolTest(string origin, char expected) => Assert.AreEqual(origin.LastSymbol(), expected);
    
    [TestCase("S",3,"SSS")]
    [TestCase("Something", 0, "")]
    [TestCase("Something", 1, "Something")]
    [TestCase("Something", 2, "SomethingSomething")]
    public void RepeatString(string origin, int count, string expected) => Assert.AreEqual(origin.Repeat(count), expected);
    
    [TestCase('S',3,"SSS")]
    [TestCase('S',0,"")]
    [TestCase('S',1,"S")]
    public void RepeatChar(char origin, int count, string expected) => Assert.AreEqual(origin.Repeat(count), expected);

    [TestCase("origin", "or**in","ig")]
    [TestCase("an", "a*","n")]
    [TestCase("Namaste", "Na***te", "mas")]
    [TestCase("supermegapuperword", "su**************rd","permegapuperwo")]
    public void GetWithStarredBodyEasy(string origin, string expected, string body) {
        var actual = origin.GetWithStarredBody(StarredHardness.Easy, out var actualBody);
        Assert.AreEqual(expected, actual);
        Assert.AreEqual(body, actualBody);
    }

    [TestCase("origin", "o*****", "rigin")]
    [TestCase("an", "a*", "n")]
    [TestCase("Namaste", "N******", "amaste")]
    [TestCase("supermegapuperword", "su***************d", "permegapuperwor")]
    public void GetWithStarredBodyHard(string origin, string expected, string body) {
        var actual = origin.GetWithStarredBody(StarredHardness.Hard, out var actualBody);
        Assert.AreEqual(expected, actual);
        Assert.AreEqual(body, actualBody);
    }

    [TestCase("origin", "Origin")]
    [TestCase("ВАСЯ", "Вася")]
    [TestCase("петька", "Петька")]
    public void CapitalizeTest(string origin, string expected) => Assert.AreEqual(expected, origin.Capitalize());

    [TestCase("a", "a")]
    [TestCase("", "")]
    [TestCase("abc", "aBc")]
    [TestCase(" abc", " aBc")]
    [TestCase("Иди ты в пень", "Иди ты в пень")]
    public void CheckMistakes_returnsEqual(string wordA, string wordB)
        => Assert.AreEqual(StringsCompareResult.Equal, wordA.CheckCloseness(wordB));


    [TestCase("meaningfull", "meaningful")]
    [TestCase("meaningfull", "meanignfull")]
    [TestCase("meaningfull", "meaningful")]
    [TestCase("meaningfull", "meaninful")]
    [TestCase("безболезненный", "безболезненый")]
    public void CheckMistakes_returnsSmallMistakes(string wordA, string wordB)
        => Assert.AreEqual(StringsCompareResult.SmallMistakes, wordA.CheckCloseness(wordB));


    [TestCase("безболезненный", "безбоелзнненый")]
    public void CheckMistakes_returnsBigMistakes(string wordA, string wordB)
        => Assert.AreEqual(StringsCompareResult.BigMistakes, wordA.CheckCloseness(wordB));

    [TestCase("Chekc", "che")]
    [TestCase("a", "b")]
    [TestCase("вася", "петя")]
    public void CheckMistakes_returnsNotEqual(string wordA, string wordB)
        => Assert.AreEqual(StringsCompareResult.NotEqual, wordA.CheckCloseness(wordB));
}

}