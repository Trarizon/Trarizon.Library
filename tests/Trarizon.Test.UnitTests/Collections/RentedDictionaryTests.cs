using FluentAssertions;
using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Test.UnitTests.Collections;

public class RentedDictionaryTests
{
    [Fact]
    public void BasicAddAndRetrieve_ShouldWork()
    {
        // Arrange
        var dict = new RentedDictionary<string, int>(10);

        // Act
        dict.Add("Apple", 1);
        dict.TryAdd("Banana", 2).Should().BeTrue();
        dict.Set("Cherry", 3);

        // Assert
        dict.Count.Should().Be(3);
        dict["Apple"].Should().Be(1);
        dict["Banana"].Should().Be(2);
        dict["Cherry"].Should().Be(3);

        dict.Dispose();
        }

    [Fact]
    public void DuplicateKey_Add_ShouldThrow()
    {
        // Arrange
        using var dict = new RentedDictionary<string, int>();
        dict.Add("Key", 1);

        // Act & Assert
        var act = () => dict.Add("Key", 2);
        act.Should().Throw<Exception>(); // 假设 Throws.KeyAlreadyExists 抛出特定异常
    }

    [Fact]
    public void TryGetValue_ShouldReturnCorrectValue()
    {
        // Arrange
        using var dict = new RentedDictionary<int, string>();
        dict.Add(1, "One");

        // Act
        bool found = dict.TryGetValue(1, out var value);
        bool notFound = dict.TryGetValue(2, out var missing);

        // Assert
        found.Should().BeTrue();
        value.Should().Be("One");
        notFound.Should().BeFalse();
        missing.Should().BeNull();
    }

    [Fact]
    public void Remove_ShouldDecreaseCountAndAllowReAdd()
    {
        // Arrange
        using var dict = new RentedDictionary<int, int>();
        dict.Add(1, 100);
        dict.Add(2, 200);

        // Act
        bool removed = dict.Remove(1, out var value);

        // Assert
        removed.Should().BeTrue();
        value.Should().Be(100);
        dict.Count.Should().Be(1);
        dict.ContainsKey(1).Should().BeFalse();

        // Re-add
        dict.Add(1, 300);
        dict[1].Should().Be(300);
        dict.Count.Should().Be(2);
    }

    [Fact]
    public void Clear_ShouldResetDictionary()
    {
        // Arrange
        using var dict = new RentedDictionary<int, int>();
        for (int i = 0; i < 5; i++) dict.Add(i, i);

        // Act
        dict.Clear();

        // Assert
        dict.Count.Should().Be(0);
        dict.ContainsKey(0).Should().BeFalse();
    }

    [Fact]
    public void Resize_TriggeredByAdd_ShouldKeepDataIntegrity()
    {
        // Arrange - 初始化一个小容量触发扩容
        using var dict = new RentedDictionary<int, int>(2);
        int testSize = 100;

        // Act
        for (int i = 0; i < testSize; i++) {
            dict.Add(i, i * 10);
        }

        // Assert
        dict.Count.Should().Be(testSize);
        for (int i = 0; i < testSize; i++) {
            dict[i].Should().Be(i * 10);
        }
    }

    [Fact]
    public void ContainsValue_ShouldFindValues()
    {
        // Arrange
        using var dict = new RentedDictionary<int, string>();
        dict.Add(1, "Value1");
        dict.Add(2, "Value2");

        // Assert
        dict.ContainsValue("Value1").Should().BeTrue();
        dict.ContainsValue("Value3").Should().BeFalse();
    }

    [Fact]
    public void EnsureCapacity_ShouldIncreaseCapacity()
    {
        // Arrange
        using var dict = new RentedDictionary<int, int>(4);
        int initialCapacity = dict.Capacity;

        // Act
        int newCapacity = dict.EnsureCapacity(20);

        // Assert
        newCapacity.Should().BeGreaterThanOrEqualTo(20);
        dict.Capacity.Should().Be(newCapacity);
    }

    [Fact]
    public void Dispose_ShouldResetInternalState()
    {
        // Arrange
        var dict = new RentedDictionary<int, int>(10);
        dict.Add(1, 1);

        // Act
        dict.Dispose();

        // Assert
        dict.Count.Should().Be(0);
        dict.Capacity.Should().Be(0);
        // 注意：由于数组已交还池，不应再访问 dict 的索引器
    }

    [Fact]
    public void ValueType_EqualityComparer_SpecialCase()
    {
        // 测试值类型不传入 Comparer 时是否走默认逻辑
        using var dict = new RentedDictionary<int, int>();
        dict.Add(10, 100);

        dict.ContainsKey(10).Should().BeTrue();
        dict[10].Should().Be(100);
    }

    [Fact]
    public void FreeList_Reuse_ShouldWork()
    {
        // Arrange
        using var dict = new RentedDictionary<int, int>();
        for (int i = 0; i < 10; i++) dict.Add(i, i);

        // Act - 移除一部分，再添加一部分，触发空闲链表重用
        for (int i = 0; i < 5; i++) dict.Remove(i);
        for (int i = 10; i < 15; i++) dict.Add(i, i);

        // Assert
        dict.Count.Should().Be(10);
        for (int i = 5; i < 15; i++) {
            dict.ContainsKey(i).Should().BeTrue();
        }
    }
}