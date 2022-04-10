using System.Text;
using Chotiskazal.Bot;
using Chotiskazal.Bot.Interface;
using NUnit.Framework;
using NUnit.Framework.Internal.Execution;
using SayWhat.Bll;

namespace SayWhat.MongoDAL.Tests {

public class MarkdownTest {
    [TestCase("Word.", "Word\\.")]
    [TestCase(".Word.", "\\.Word\\.")]
    [TestCase("*Word*", "\\*Word\\*")]
    [TestCase("Wo*rd", "Wo\\*rd")]
    [TestCase("*Word", "\\*Word")]
    [TestCase("Word*", "Word\\*")]
    [TestCase("", "")]

    public void GetMarkdownStringWhenEscaped(string origin, string expected) => Assert.AreEqual(expected, Markdown.Escaped(origin).GetMarkdownString());

    [TestCase("Word.", "Word.")]
    [TestCase(".Word.", ".Word.")]
    [TestCase("*Word*", "*Word*")]
    [TestCase("", "")]

    public void GetOrdinaryStringWhenEscaped(string origin, string expected) => Assert.AreEqual(expected, Markdown.Escaped(origin).GetOrdinalString());

    [TestCase("Word.", "Word.")]
    [TestCase(".Word.", ".Word.")]
    [TestCase("*Word*", "*Word*")]
    [TestCase("", "")]
    public void GetMarkdownStringWhenByPasssed(string origin, string expected) => Assert.AreEqual(expected, Markdown.Bypassed(origin).GetMarkdownString());

    [TestCase("Word.", "Word.")]
    [TestCase(".Word.", ".Word.")]
    [TestCase("*Word*", "*Word*")]
    [TestCase("", "")]

    public void GetOrdinaryStringWhenByPasssed(string origin, string expected) => Assert.AreEqual(expected, Markdown.Bypassed(origin).GetOrdinalString());

    [TestCase("markdown1", "markdown2","markdown1markdown2")]
    [TestCase("markdown1", "*markdown2","markdown1\\*markdown2")]
    [TestCase("", "","")]
    public void AddMarkdownTest(string s1, string s2, string expected) {
        var m1 = Markdown.Escaped(s1);
        var m2 = Markdown.Escaped(s2);
        Assert.AreEqual(expected, m1.AddMarkdown(m2).GetMarkdownString());
        Assert.AreEqual(expected,m1.AddEscaped(s2).GetMarkdownString());
        Assert.AreEqual(expected,(m1+m2).GetMarkdownString());
    }
    
    [TestCase("markdown1", "markdown2","markdown1markdown2")]
    [TestCase("markdown1", "*markdown2","markdown1\\*markdown2")]
    [TestCase("", "","")]
    public void AddEscapedTextToMarkdownTest(string s1, string s2, string expected) {
        var m1 = Markdown.Escaped(s1);
        var m2 = Markdown.Escaped(s2);
        Assert.AreEqual(expected,m1.AddEscaped(s2).GetMarkdownString());
        Assert.AreEqual(expected, m1.AddMarkdown(s2.ToEscapedMarkdown()).GetMarkdownString());
    }
    
    [TestCase("markdown1", "markdown2","markdown1markdown2")]
    [TestCase("markdown1", "*markdown2","markdown1*markdown2")]
    [TestCase("markdown1", "*markdown2*","markdown1*markdown2*")]
    [TestCase("", "","")]
    public void AddBypassedTextTest(string s1, string s2, string expected) {
        var m1 = Markdown.Escaped(s1);
        var m2 = Markdown.Escaped(s2);
        Assert.AreEqual(expected,m1.AddBypassed(s2).GetMarkdownString());
        Assert.AreEqual(expected, m1.AddMarkdown(s2.ToBypassedMarkdown()).GetMarkdownString());
    }
    
    [TestCase("markdown1", "markdown2","_markdown1markdown2_")]
    [TestCase("","","")]
    public void ToItalicTest(string s1, string s2, string expected) {
        var m1 = Markdown.Escaped(s1);
        var m2 = Markdown.Escaped(s2);
        Assert.AreEqual(expected, m1.AddMarkdown(m2).ToItalic().GetMarkdownString());
        Assert.AreEqual(expected,m1.AddEscaped(s2).ToItalic().GetMarkdownString());
        Assert.AreEqual(expected,(m1+m2).ToItalic().GetMarkdownString());
    }
    
    [TestCase("markdown1", "markdown2","markdown1_markdown2_")]
    [TestCase("markdown1", "markd*own2","markdown1_markd\\*own2_")]
    [TestCase("markdown1", "*markdown2*","markdown1_\\*markdown2\\*_")]
    [TestCase("","","")]
    public void AddItalicTest(string s1, string s2, string expected) {
        var m1 = Markdown.Escaped(s1);
        var m2 = Markdown.Escaped(s2);
        Assert.AreEqual(expected,m1.AddMarkdown(s2.ToItalicMarkdown()).GetMarkdownString());
        Assert.AreEqual(expected,(m1+m2.ToItalic()).GetMarkdownString());
    }
    
    [TestCase("markdown1", "markdown2","*markdown1markdown2*")]
    [TestCase("","","")]
    public void ToSemiboldTest(string s1, string s2, string expected) {
        var m1 = Markdown.Escaped(s1);
        var m2 = Markdown.Escaped(s2);
        Assert.AreEqual(expected, m1.AddMarkdown(m2).ToSemiBold().GetMarkdownString());
        Assert.AreEqual(expected,m1.AddEscaped(s2).ToSemiBold().GetMarkdownString());
        Assert.AreEqual(expected,(m1+m2).ToSemiBold().GetMarkdownString());
    }
    
    [TestCase("markdown1", "markdown2","markdown1*markdown2*")]
    [TestCase("markdown1", "markd*own2","markdown1*markd\\*own2*")]
    [TestCase("markdown1", "*markdown2*","markdown1*\\*markdown2\\**")]
    [TestCase("","","")]
    public void AddSemiboldTest(string s1, string s2, string expected) {
        var m1 = Markdown.Escaped(s1);
        var m2 = Markdown.Escaped(s2);
        Assert.AreEqual(expected,m1.AddMarkdown(s2.ToSemiBoldMarkdown()).GetMarkdownString());
        Assert.AreEqual(expected,(m1+m2.ToSemiBold()).GetMarkdownString());
    }
    
    [TestCase("markdown1", "markdown2","`\r\nmarkdown1markdown2\r\n`")]
    [TestCase("","","")]
    public void ToMonoTextTest(string s1, string s2, string expected) {
        var m1 = Markdown.Escaped(s1);
        var m2 = Markdown.Escaped(s2);
        Assert.AreEqual(expected, m1.AddMarkdown(m2).ToMono().GetMarkdownString());
        Assert.AreEqual(expected,m1.AddEscaped(s2).ToMono().GetMarkdownString());
        Assert.AreEqual(expected,(m1+m2).ToMono().GetMarkdownString());
    }
    
    [TestCase("markdown1", "markdown2","markdown1`\r\nmarkdown2\r\n`")]
    [TestCase("markdown1", "markd*own2","markdown1`\r\nmarkd\\*own2\r\n`")]
    [TestCase("markdown1", "*markdown2*","markdown1`\r\n\\*markdown2\\*\r\n`")]
    [TestCase("","","")]
    public void AddMonoTextTest(string s1, string s2, string expected) {
        var m1 = Markdown.Escaped(s1);
        var m2 = Markdown.Escaped(s2);
        Assert.AreEqual(expected,m1.AddMarkdown(s2.ToMonoMarkdown()).GetMarkdownString());
        Assert.AreEqual(expected,(m1+m2.ToMono()).GetMarkdownString());
    }
    
    [TestCase("markdown1", "markdown2","```\r\nmarkdown1markdown2\r\n```")]
    [TestCase("","","")]
    public void ToPreFormattedMonoTextTest(string s1, string s2, string expected) {
        var m1 = Markdown.Escaped(s1);
        var m2 = Markdown.Escaped(s2);
        Assert.AreEqual(expected, m1.AddMarkdown(m2).ToPreFormattedMono().GetMarkdownString());
        Assert.AreEqual(expected,m1.AddEscaped(s2).ToPreFormattedMono().GetMarkdownString());
        Assert.AreEqual(expected,(m1+m2).ToPreFormattedMono().GetMarkdownString());
    }

    [TestCase("markdown1", "markdown2", "markdown1```\r\nmarkdown2\r\n```")]
    [TestCase("markdown1", "markd*own2", "markdown1```\r\nmarkd\\*own2\r\n```")]
    [TestCase("markdown1", "*markdown2*", "markdown1```\r\n\\*markdown2\\*\r\n```")]
    [TestCase("", "", "")]
    public void AddPreFormattedMonoTextTest(string s1, string s2, string expected) {
        var m1 = Markdown.Escaped(s1);
        var m2 = Markdown.Escaped(s2);
        Assert.AreEqual(expected,m1.AddMarkdown(s2.ToPreFormattedMonoMarkdown()).GetMarkdownString());
        Assert.AreEqual(expected, (m1 + m2.ToPreFormattedMono()).GetMarkdownString());
    }

    [TestCase("markdown1","```\r\nmarkdown1\r\n```")]
    public void StringBuilderCompositionMethodTest(string s, string expected) {
        var sb = new StringBuilder();
        sb.Append($"```\r\n{Markdown.Escaped(s).GetMarkdownString()}\r\n```");
        Assert.AreEqual(expected,sb.ToString().ToBypassedMarkdown().GetMarkdownString());
    }
    
    public void UseEmojimarkdownTest() {
        foreach (var element in Emojis.Elements) {
            Assert.AreEqual(element, element.ToEscapedMarkdown().GetMarkdownString());
            Assert.AreEqual(element, element.ToBypassedMarkdown().GetMarkdownString());
        }
    }
}
}
