using NSubstitute;
using WordCounter.Core.Interfaces;
using WordCounter.Core.Services;

namespace WordCounter_Test.UnitTest.Services
{
    [TestClass]
    public class WordCounterServiceTests
    {
        private static (INormalizer norm, IWordExtractor tok) Mocks() =>
            (Substitute.For<INormalizer>(), Substitute.For<IWordExtractor>());

        [TestMethod]
        public void Count_AggregatesAcrossInputs_And_AppliesNormalization()
        {
            // Arrange
            var (norm, tok) = Mocks();
            var sut = new WordCounterService(norm, tok);

            tok.Tokenize("Hello world").Returns(["Hello", "world"]);
            tok.Tokenize("hello HELLO").Returns(["hello", "HELLO"]);

            norm.NormalizeToken("Hello").Returns("HELLO");
            norm.NormalizeToken("world").Returns("WORLD");
            norm.NormalizeToken("hello").Returns("HELLO");
            norm.NormalizeToken("HELLO").Returns("HELLO");

            var texts = new[] { "Hello world", "hello HELLO" };
            var excluded = new HashSet<string>(StringComparer.Ordinal);

            // Act
            var (counts, excludedTotal) = sut.Count(texts, excluded);

            // Assert
            Assert.AreEqual(0L, excludedTotal);
            Assert.AreEqual(3, counts["HELLO"]);
            Assert.AreEqual(1, counts["WORLD"]);
            Assert.HasCount(2, counts);
        }

        [TestMethod]
        public void Count_ExcludedWords_AreNotCounted_And_ExcludedTotalIsAccurate()
        {
            // Arrange
            var (norm, tok) = Mocks();
            var sut = new WordCounterService(norm, tok);

            tok.Tokenize("a skip skip keep").Returns(["a", "skip", "skip", "keep"]);
            tok.Tokenize("skip A").Returns(["skip", "A"]);

            norm.NormalizeToken(Arg.Any<string>()).Returns(ci => (string)ci[0] switch
            {
                "a" => "A",
                "A" => "A",
                "skip" => "SKIP",
                "keep" => "KEEP",
                _ => (string)ci[0]
            });

            var texts = new[] { "a skip skip keep", "skip A" };
            var excluded = new HashSet<string>(StringComparer.Ordinal) { "SKIP" };

            // Act
            var (counts, excludedTotal) = sut.Count(texts, excluded);

            // Assert
            Assert.AreEqual(3L, excludedTotal);
            Assert.IsFalse(counts.ContainsKey("SKIP"));
            Assert.AreEqual(2, counts["A"]);
            Assert.AreEqual(1, counts["KEEP"]);
            Assert.HasCount(2, counts);
        }

        [TestMethod]
        public void Count_Skips_Empty_Normalized_Tokens()
        {
            // Arrange
            var (norm, tok) = Mocks();
            var sut = new WordCounterService(norm, tok);

            tok.Tokenize("x  y  z").Returns(["x", "y", "z"]);

            norm.NormalizeToken("x").Returns("X");
            norm.NormalizeToken("y").Returns(string.Empty);
            norm.NormalizeToken("z").Returns("Z");

            var texts = new[] { "x  y  z" };
            var excluded = new HashSet<string>(StringComparer.Ordinal);

            // Act
            var (counts, excludedTotal) = sut.Count(texts, excluded);

            // Assert
            Assert.AreEqual(0L, excludedTotal);
            CollectionAssert.AreEquivalent(new[] { "X", "Z" }, counts.Keys.ToArray());
            Assert.AreEqual(1, counts["X"]);
            Assert.AreEqual(1, counts["Z"]);
        }

        [TestMethod]
        public void Count_Uses_Ordinal_Comparer_Distinguishes_Casing_If_Normalizer_Returns_Different_Case()
        {
            // Arrange
            var (norm, tok) = Mocks();
            var sut = new WordCounterService(norm, tok);

            tok.Tokenize("A a").Returns(["A", "a"]);

            norm.NormalizeToken("A").Returns("A");
            norm.NormalizeToken("a").Returns("a");

            var texts = new[] { "A a" };
            var excluded = new HashSet<string>(StringComparer.Ordinal);

            // Act
            var (counts, excludedTotal) = sut.Count(texts, excluded);

            // Assert
            Assert.AreEqual(0L, excludedTotal);
            Assert.AreEqual(1, counts["A"]);
            Assert.AreEqual(1, counts["a"]);
            Assert.HasCount(2, counts);
        }

        [TestMethod]
        public void Count_Is_ThreadSafe_When_Many_Inputs_Same_Token()
        {
            // Arrange
            var (norm, tok) = Mocks();
            var sut = new WordCounterService(norm, tok);
            
            var inputs = Enumerable.Range(0, 200).Select(i => $"line-{i}").ToArray();

            tok.Tokenize(Arg.Any<string>()).Returns(Enumerable.Repeat("x", 20).ToArray());
            norm.NormalizeToken("x").Returns("X");

            var excluded = new HashSet<string>(StringComparer.Ordinal);

            // Act
            var (counts, excludedTotal) = sut.Count(inputs, excluded);

            // Assert
            Assert.AreEqual(0L, excludedTotal);
            Assert.AreEqual(200 * 20, counts["X"]);
            Assert.HasCount(1, counts);
            
            norm.Received(200 * 20).NormalizeToken("x");
        }

        [TestMethod]
        public void Count_Normalizer_Is_Called_Before_ExclusionCheck()
        {            
            var (norm, tok) = Mocks();
            var sut = new WordCounterService(norm, tok);

            tok.Tokenize("skip skip").Returns(["skip", "skip"]);
            norm.NormalizeToken("skip").Returns("SKIP");

            var excluded = new HashSet<string>(StringComparer.Ordinal) { "SKIP" };

            var (counts, excludedTotal) = sut.Count(["skip skip"], excluded);

            Assert.AreEqual(2L, excludedTotal);
            Assert.IsEmpty(counts);
            norm.Received(2).NormalizeToken("skip");
        }

        [TestMethod]
        public void Count_With_Empty_Texts_Returns_Empty_Counts_And_Zero_Excluded()
        {
            var (norm, tok) = Mocks();
            var sut = new WordCounterService(norm, tok);

            var (counts, excludedTotal) = sut.Count([], new HashSet<string>(StringComparer.Ordinal));

            Assert.AreEqual(0L, excludedTotal);
            Assert.IsEmpty(counts);
            tok.DidNotReceiveWithAnyArgs().Tokenize(default!);
            norm.DidNotReceiveWithAnyArgs().NormalizeToken(default!);
        }
    }
}
