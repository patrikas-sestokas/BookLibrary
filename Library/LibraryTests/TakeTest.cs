using System;
using System.Linq;
using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class TakeTests
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
        public void TakeForTooLong(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            var books = repo.Filter(ArraySegment<string>.Empty).ToArray();
            var reservation = new Issue("Patrick", DateTime.Now, DateTime.Now + TimeSpan.FromDays(61),
                books[0].ISBN);
            Assert.Throws<ConsoleException>(() => repo.Take(reservation));
        }
        [Test]
        [TestCase("./TestFiles/Nonempty.json")]
        public void TakeNonExisting(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            var reservation = new Issue("Patrick", DateTime.Now, DateTime.Now + TimeSpan.FromDays(30),
                Guid.NewGuid().ToString());
            Assert.Throws<ConsoleException>(() => repo.Take(reservation));
        }
        [Test]
        [TestCase("./TestFiles/Nonempty.json")]
        public void TakeWhenEmpty(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            var book = repo.Filter(ArraySegment<string>.Empty).First();
            var reservation = new Issue("Patrick", DateTime.Now, DateTime.Now + TimeSpan.FromDays(30),
                book.ISBN);
            Assert.DoesNotThrow(() => repo.Delete(book.ISBN, book.Amount - 1));
            Assert.DoesNotThrow(() => repo.Take(reservation));
            Assert.Throws<ConsoleException>(() => repo.Take(reservation));
        }
        [Test]
        [TestCase("./TestFiles/Nonempty.json")]
        public void TakeTooManyPerPerson(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            var books = repo.Filter(ArraySegment<string>.Empty).Take(4);
            var reservations = books
                .Select(b => new Issue("Patrick", DateTime.Now, DateTime.Now + TimeSpan.FromDays(30), b.ISBN))
                .ToArray();
            foreach (var reservation in reservations.Take(3))
                Assert.DoesNotThrow(() => repo.Take(reservation));
            Assert.Throws<ConsoleException>(() => repo.Take(reservations.Last()));
        }
    }
}