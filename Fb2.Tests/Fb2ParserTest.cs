using System.Linq;
using Fb2.Specification;
using NUnit.Framework;
using Shouldly;

namespace Fb2.Tests
{
    [TestFixture]
    public class Fb2ParserTest
    {
        [Test]
        public void Load_WithSimpleBookContent_ShouldParseIt()
        {
            // Arrange
            // Act
            var book = Fb2Parser.Load("<FictionBook><body><p>Some text</p></body></FictionBook>");

            // Assert
            book.ShouldSatisfyAllConditions(
                () => book.ShouldBeOfType<FictionBook>(),
                () => book.ToStringArray().ShouldBe(new []
                {
                    nameof(Body),
                    nameof(Paragraph),
                    nameof(Text),
                    nameof(Paragraph),
                    nameof(Body)
                }));

            var text = book.Items.OfType<Text>().Single();
            text.Value.ShouldBe("Some text");
        }

        [Test]
        public void Load_WithSelfClosedTag_ShouldAddOnlyOne()
        {
            // Arrange
            // Act
            var book = Fb2Parser.Load("<FictionBook><body><empty-line /></body></FictionBook>");

            // Assert
            book.ShouldSatisfyAllConditions(
                () => book.ShouldBeOfType<FictionBook>(),
                () => book.ToStringArray().ShouldBe(new[]
                {
                    nameof(Body),
                    nameof(EmptyLine),
                    nameof(Body)
                }));
        }
    }

    public static class FictionBookExtension
    {
        public static string[] ToStringArray(this FictionBook book)
        {
            return book.Items.Select(i => i.GetType().Name).ToArray();
        }
    }
}
