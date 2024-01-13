using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trarizon.Library.GeneratorToolkit.Extensions;

namespace Trarizon.Test.UnitTest;
[TestClass]
public class GeneratorToolkitTest
{
    [TestMethod]
    public void MatchMetadataStringTest()
    {
        Assert.IsTrue(SymbolExtensions.IsMatchMetadataString("Tra.Get<string>", "Tra.Get`1"));
        Assert.IsTrue(SymbolExtensions.IsMatchMetadataString("Tra.Get<string, int>", "Tra.Get`2"));
        Assert.IsTrue(SymbolExtensions.IsMatchMetadataString("Tra.Get<string, List<int,str>>", "Tra.Get`2"));
        Assert.IsTrue(SymbolExtensions.IsMatchMetadataString("Tra.Get<string, List<int,str>>.Get<b>()", "Tra.Get`2.Get`1()"));
    }
}
