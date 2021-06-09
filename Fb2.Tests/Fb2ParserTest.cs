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
                () => book.Items.Single().ShouldBeOfType<Body>(),
                () => book.Items.Single().Items.Single().ShouldBeOfType<Paragraph>(),
                () => book.Items.Single().Items.Single().Items.Single().ShouldBeOfType<Text>());

            var text = book.Items.Single().Items.Single().Items.OfType<Text>().Single();
            text.Value.ShouldBe("Some text");
        }
    }
}
