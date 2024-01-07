using System.Linq;
using NUnit.Framework;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;

namespace SayWhat.UnitTests;

public class FreqWordsTest
{
    [Test]
    public void CentralSection1() =>
        // in empty collection the whole collection has to be a section 
        AssertSection(0, null);

    [Test]
    public void CentralSection2() =>
        // --------g[------------]
        AssertSection(42, null, Green(42));
    
    [Test]
    public void CentralSection2_1() =>
        // -----g---g[------------]
        AssertSection(42, null, Green(40), Green(42));

    [Test]
    public void CentralSection3() =>
        // [--------]r------------
        AssertSection(0, 42, Red(42));
    
    [Test]
    public void CentralSection3_1() =>
        // [--------]r-r-----------
        AssertSection(0, 42, Red(42), Red(44));

    [Test]
    public void CentralSection4() =>
        // --------g[--]r----------
        AssertSection(20, 40, Green(20), Red(40));

    [Test]
    public void CentralSection5() =>
        // --------r[--]g----------
        AssertSection(20, 40, Red(20), Green(40));

    [Test]
    public void CentralSection6() =>
        // ----ggg----r[--]g----------
        AssertSection(20, 40, Green(1), Green(2), Green(3), Red(20), Green(40));


    [Test]
    public void CentralSection7() =>
        // ----ggg----r[--]g---rrr-------
        AssertSection(20, 40, Green(1), Green(2), Green(3), Red(20), Green(40), Red(50), Red(51), Red(52));

    [Test]
    public void CentralSection8() =>
        // ----g-r-gg----r[--]g---rr-g-r-r------
        AssertSection(20, 40, Green(1), Green(2), Red(3), Green(4),
            Red(20), Green(40),
            Red(50), Red(51), Green(52), Red(53), Red(54));


    [Test]
    public void CentralSection9() =>
        // ----g-r-gg[----]r--g---------
        AssertSection(5, 6, Green(1), Red(3), Green(4),
            Green(5), Red(6),
            Green(7));

    [Test]
    public void CentralSection10() =>
        // ----g-r-gg[-]r--g-r--------
        AssertSection(5, 6, Green(1), Red(3), Green(4),
            Green(5), Red(6),
            Green(7), Red(8));
    
    [Test]
    public void CentralSection11() =>
        // -g-g-g[-]
        AssertSection(40, null, Green(10), Green(20), Green(40));
    
    [Test]
    public void CentralSection12() =>
        // -r-g[-]g-
        AssertSection(20, 40, Red(10), Green(20), Green(40));

    [Test]
    public void CentralSection13() =>
        // -g-r[---]g-
        AssertSection(20, 40, Green(10), Red(20), Green(40));
    
    [Test]
    public void CentralSection14() =>
        // -r[---]r-g-
        AssertSection(10, 20, Red(10), Red(20), Green(40));
    
    [Test]
    public void CentralSection15() =>
        // -g-g[---]r-
        AssertSection(20, 40, Green(10), Green(20), Red(40));
    
    [Test]
    public void CentralSection16() =>
        // -g-r[---]g-
        AssertSection(20, 40, Green(10), Red(20), Green(40));
    
    [Test]
    public void CentralSection17() =>
        // -r[-]g-r-
        AssertSection(10, 20, Red(10), Green(20), Red(40));
    
    [Test]
    public void CentralSection18() =>
        // -g[-]r-r-
        AssertSection(10, 20, Green(10), Red(20), Red(40));
    
    [Test]
    public void CentralSection19() =>
        // [-]r-r-r-
        AssertSection(0, 10, Red(10), Red(20), Red(40));

    [Test]
    public void CentralSection20() =>
        // --g--g[--]
        AssertSection(50, null, Green(10), Green(50));
    

    private void AssertSection(int left, int? right, params UserFreqWord[] words)
    {
        int size = 100;
        var selector = new FreqWordsSelector(words.ToList(), size);
        var section = selector.CalcCentralSection();
        Assert.AreEqual(new CentralKnowledgeSection(left, right ?? size), section);
    }

    /// <summary>
    /// Immitate unknown word (word that is learning)
    /// </summary>
    private UserFreqWord Red(int number) => new(number, FreqWordResult.UserSelectToLearn);

    /// <summary>
    /// Immitate known word (word that user already knows)
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    private UserFreqWord Green(int number) => new(number, FreqWordResult.UserSelectThatItIsKnown);
}