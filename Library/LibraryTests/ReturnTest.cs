using System;
using System.Linq;
using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class ReturnTests
    {
        [SetUp]
        public void Setup()
        {
        }
        [Test]
        [TestCase("./TestFiles/Nonempty.json")]
        public void Take(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            var books = repo.Filter(ArraySegment<string>.Empty).ToArray();
            var reservation = new Issue("Patrick", DateTime.Now, DateTime.Now + TimeSpan.FromDays(30),
                books[0].ISBN);
            Assert.DoesNotThrow(() => repo.Take(reservation));
        }
        [Test]
        [TestCase("./TestFiles/Nonempty.json")]
        public void Return(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            var books = repo.Filter(ArraySegment<string>.Empty).ToArray();
            var reservation = new Issue("Patrick", DateTime.Now, DateTime.Now + TimeSpan.FromDays(30),
                books[0].ISBN);
            Assert.DoesNotThrow(() => repo.Take(reservation));
            Assert.DoesNotThrow(() => repo.Return(reservation));
        }
        [Test]
        [TestCase("./TestFiles/Nonempty.json")]
        public void ReturnNonExisting(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            var books = repo.Filter(ArraySegment<string>.Empty).ToArray();
            var reservation = new Issue("Patrick", DateTime.Now, DateTime.Now + TimeSpan.FromDays(30),
                books[0].ISBN);
            Assert.DoesNotThrow(() => repo.Take(reservation));
            Assert.Throws<ConsoleException>(() => repo.Return(reservation with { Holder = Guid.NewGuid().ToString() }));
            Assert.Throws<ConsoleException>(() => repo.Return(reservation with { ISBN = Guid.NewGuid().ToString() }));
        }
    }
}