using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class AddTests
    {
        [SetUp]
        public void Setup()
        {
        }
        [Test]
        [TestCase("./TestFiles/Empty.json", "./TestFiles/NewBooks.json")]
        [TestCase("./TestFiles/Nonempty.json", "./TestFiles/NewBooks.json")]
        public void AddNew(string sourceFile, string bookfile)
        {
            var repo = new BookRepository(sourceFile);
            var books = JsonSerializer.Deserialize<List<Book>>(File.ReadAllText(bookfile));
            foreach (var book in books)
                Assert.DoesNotThrow(() => repo.Add(book));
            var booksInRepo = new HashSet<Book>(repo.Filter(ArraySegment<string>.Empty));
            Assert.IsNotEmpty(booksInRepo);
            foreach (var book in books)
                Assert.True(booksInRepo.Contains(book));
            Assert.Pass("Succeeded adding new books");
        }
        [Test]
        [TestCase("./TestFiles/Nonempty.json")]
        public void AddExisting(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            var books = repo.Filter(ArraySegment<string>.Empty).ToArray();
            foreach (var book in books)
                Assert.DoesNotThrow(() => repo.Add(book));
            var updatedBooks = repo.Filter(ArraySegment<string>.Empty).ToArray();
            Assert.AreEqual(books.Length, updatedBooks.Length);
            foreach (var (first, second) in books.Zip(updatedBooks))
                Assert.AreEqual(first.Amount * 2, second.Amount);
        }
    }
}