using FluentAssertions;
using Trarizon.Library.Text;

namespace Trarizon.Test.UnitTests;

public class Text_NumberTextTests
{
    [Fact]
    public void NumberToChineseTests()
    {
        NumberText.NumberToChinese(1).Should().Be("一");
        NumberText.NumberToChinese(10).Should().Be("十");
        NumberText.NumberToChinese(100).Should().Be("一百");
        NumberText.NumberToChinese(1000).Should().Be("一千");
        NumberText.NumberToChinese(1_0000).Should().Be("一万");
        NumberText.NumberToChinese(10_0000).Should().Be("十万");
        NumberText.NumberToChinese(100_0000).Should().Be("一百万");
        NumberText.NumberToChinese(1000_0000).Should().Be("一千万");
        NumberText.NumberToChinese(1_0000_0000).Should().Be("一亿");
        NumberText.NumberToChinese(10_0000_0000).Should().Be("十亿");

        NumberText.NumberToChinese(101).Should().Be("一百零一");
        NumberText.NumberToChinese(1001).Should().Be("一千零一");
        NumberText.NumberToChinese(1_0001).Should().Be("一万零一");
        NumberText.NumberToChinese(10_0001).Should().Be("十万零一");
        NumberText.NumberToChinese(1_0000_0001).Should().Be("一亿零一");

        NumberText.NumberToChinese(1_1000).Should().Be("一万一千");
        NumberText.NumberToChinese(10_1000).Should().Be("十万一千");
        NumberText.NumberToChinese(10_0100).Should().Be("十万零一百");
        NumberText.NumberToChinese(1_0001_0000).Should().Be("一亿零一万");

        NumberText.NumberToChinese(1011).Should().Be("一千零一十一");
        NumberText.NumberToChinese(1101).Should().Be("一千一百零一");
        NumberText.NumberToChinese(10101).Should().Be("一万零一百零一");
        NumberText.NumberToChinese(1_0001_0001).Should().Be("一亿零一万零一");
    }

    [Fact]
    public void NumberToChinese_UseLiangForTwoTests()
    {
        var opt = NumberText.ToChineseOptions.UseLiangForTwo;

        // 个位数2使用"两"
        NumberText.NumberToChinese(2, opt).Should().Be("两");
        NumberText.NumberToChinese(12, opt).Should().Be("十二"); // 十位仍用"二"
        NumberText.NumberToChinese(102, opt).Should().Be("一百零二");
        NumberText.NumberToChinese(1002, opt).Should().Be("一千零二");
        NumberText.NumberToChinese(1_0002, opt).Should().Be("一万零二");

        // 十位上的2不使用"两"
        NumberText.NumberToChinese(20, opt).Should().Be("二十");
        NumberText.NumberToChinese(21, opt).Should().Be("二十一");
        NumberText.NumberToChinese(120, opt).Should().Be("一百二十");

        // 其他位不受影响
        NumberText.NumberToChinese(200, opt).Should().Be("二百");
        NumberText.NumberToChinese(2000, opt).Should().Be("二千");
        NumberText.NumberToChinese(2_0000, opt).Should().Be("二万");

        opt = NumberText.ToChineseOptions.UseLiangForTwoHundred;

        // 百位2使用"两百"
        NumberText.NumberToChinese(200, opt).Should().Be("两百");
        NumberText.NumberToChinese(221, opt).Should().Be("两百二十一");
        NumberText.NumberToChinese(2200, opt).Should().Be("二千两百");
        NumberText.NumberToChinese(2_0200, opt).Should().Be("二万零两百");

        // 其他位不受影响
        NumberText.NumberToChinese(2, opt).Should().Be("二");
        NumberText.NumberToChinese(20, opt).Should().Be("二十");
        NumberText.NumberToChinese(2000, opt).Should().Be("二千");
        NumberText.NumberToChinese(2_0000, opt).Should().Be("二万");

        opt = NumberText.ToChineseOptions.UseLiangForTwoThousand;

        // 千位2使用"两千"
        NumberText.NumberToChinese(2000, opt).Should().Be("两千");
        NumberText.NumberToChinese(2001, opt).Should().Be("两千零一");
        NumberText.NumberToChinese(12_2000, opt).Should().Be("十二万两千");

        // 其他位不受影响
        NumberText.NumberToChinese(2, opt).Should().Be("二");
        NumberText.NumberToChinese(200, opt).Should().Be("二百");
        NumberText.NumberToChinese(2_0000, opt).Should().Be("二万");

        opt = NumberText.ToChineseOptions.UseLiangForTwoWanUnits;

        // 万级单位2使用"两万"
        NumberText.NumberToChinese(2_0000, opt).Should().Be("两万");
        NumberText.NumberToChinese(2_0001, opt).Should().Be("两万零一");
        NumberText.NumberToChinese(2_0000_0000, opt).Should().Be("两亿");
        NumberText.NumberToChinese(2_0002_0001, opt).Should().Be("两亿零两万零一");

        // 其他位不受影响
        NumberText.NumberToChinese(2, opt).Should().Be("二");
        NumberText.NumberToChinese(200, opt).Should().Be("二百");
        NumberText.NumberToChinese(2000, opt).Should().Be("二千");

        opt = NumberText.ToChineseOptions.UseLiangForTwoAlways;

        // 所有位都使用"两"
        NumberText.NumberToChinese(2, opt).Should().Be("两");
        NumberText.NumberToChinese(200, opt).Should().Be("两百");
        NumberText.NumberToChinese(2000, opt).Should().Be("两千");
        NumberText.NumberToChinese(2_0000, opt).Should().Be("两万");
        NumberText.NumberToChinese(2_0000_0000, opt).Should().Be("两亿");

        // 十位仍不使用"两"
        NumberText.NumberToChinese(20, opt).Should().Be("二十");
        NumberText.NumberToChinese(12, opt).Should().Be("十二");

        // 复杂组合
        NumberText.NumberToChinese(2_2202, opt).Should().Be("两万两千两百零二");
        NumberText.NumberToChinese(2_2222_2222, opt).Should().Be("两亿两千两百二十二万两千两百二十二");

        // 只使用部分组合
        var opt1 = NumberText.ToChineseOptions.UseLiangForTwo | NumberText.ToChineseOptions.UseLiangForTwoHundred;
        NumberText.NumberToChinese(202, opt1).Should().Be("两百零二");
        NumberText.NumberToChinese(2000, opt1).Should().Be("二千"); // 千位不受影响

        var opt2 = NumberText.ToChineseOptions.UseLiangForTwoThousand | NumberText.ToChineseOptions.UseLiangForTwoWanUnits;
        NumberText.NumberToChinese(2_2000, opt2).Should().Be("两万两千");
        NumberText.NumberToChinese(2, opt2).Should().Be("二"); // 个位不受影响
    }
}