using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace NetFabric.Hyperlinq.UnitTests
{
    public class AsValueEnumerableEnumerableTests
    {
        [Theory]
        [MemberData(nameof(TestData.Empty), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.Single), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.Multiple), MemberType = typeof(TestData))]
        public void AsValueEnumerable_With_ValidData_Should_Succeed(int[] source)
        {
            // Arrange
            var wrapped = Wrap.AsEnumerable(source);

            // Act
            var result = Enumerable.AsValueEnumerable(wrapped);

            // Assert
            result.Should().BeOfType<Enumerable.ValueEnumerableWrapper<int>>();
            result.Must().BeEnumerable(wrapped);
        }

        [Theory]
        [MemberData(nameof(TestData.ElementAt), MemberType = typeof(TestData))]
        public void AsValueEnumerable_With_ElementAt_Should_Succeed(int[] source, int index)
        {
            // Arrange
            var wrapped = Wrap.AsEnumerable(source);
            var expected = System.Linq.Enumerable.ElementAt(wrapped, index);

            // Act
            var result = Enumerable.AsValueEnumerable(wrapped).ElementAt(index);

            // Assert
            result.Should().Be(expected);
        }
    }
}