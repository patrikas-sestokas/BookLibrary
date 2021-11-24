using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class DeleteTests
    {
        [SetUp]
        public void Setup()
        {
        }
        [Test]
        [TestCase("./TestFiles/Empty.json", "./TestFiles/NewBooks.json")]
        public void Delete(string sourceFile, string booksFile)
        {
            var repo = new BookRepository(sourceFile);
            var books = JsonSerializer.Deserialize<List<Book>>(File.ReadAllText(booksFile));
            foreach (var book in books)
                Assert.DoesNotThrow(() => repo.Add(book));
            Assert.DoesNotThrow(() => repo.Delete(books[0].ISBN, books[0].Amount));
            Assert.That(repo.Filter(ArraySegment<string>.Empty), Has.No.Member(books[0]));
            Assert.DoesNotThrow(() => repo.Delete(books[1].ISBN, books[1].Amount - 1));
            var reservation = new Issue("Patrick", DateTime.Now, DateTime.Now + TimeSpan.FromDays(30),
                books[1].ISBN);
            Assert.DoesNotThrow(() => repo.Take(reservation));
            Assert.Contains(books[1], repo.Filter(ArraySegment<string>.Empty).ToArray());
        }
        [Test]
        [TestCase("./TestFiles/Nonempty.json")]
        public void DeleteMoreThanThereIs(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            var books = repo.Filter(ArraySegment<string>.Empty).ToArray();
            Assert.Throws<ConsoleException>(() => repo.Delete(books[0].ISBN, books[0].Amount + 1));
        }
        [Test]
        [TestCase("./TestFiles/Nonempty.json")]
        public void DeleteNonExistent(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            var books = repo.Filter(ArraySegment<string>.Empty).ToArray();
            Assert.Throws<ConsoleException>(() => repo.Delete(Guid.NewGuid().ToString(), books[0].Amount + 1));
        }
    }
}