using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class ListTests
    {
        [SetUp]
        public void Setup()
        {
        }
        [Test]
        [TestCase("./TestFiles/Empty.json", "./TestFiles/NewBooks.json")]
        [TestCase("./TestFiles/Nonempty.json", "./TestFiles/NewBooks.json")]
        public void List(string sourceFile, string bookFile)
        {
            var repo = new BookRepository(sourceFile);
            var books = JsonSerializer.Deserialize<List<Book>>(File.ReadAllText(bookFile));
            foreach (var book in books)
                Assert.DoesNotThrow(() => repo.Add(book));
            var booksInRepo = repo.Filter(ArraySegment<string>.Empty).ToArray();
            foreach (var book in books)
                Assert.Contains(book, booksInRepo);
            var rng = new Random();
            var (author, category, language, isbn, name) =
                (books[rng.Next(0, books.Count)].Author,
                    books[rng.Next(0, books.Count)].Category,
                    books[rng.Next(0, books.Count)].Language,
                    books[rng.Next(0, books.Count)].ISBN,
                    books[rng.Next(0, books.Count)].Name);
            var tests = new[]
            {
                ((List<Book> list, Action<Book> test))
                (repo.Filter(new[] { $"author={author}" }).ToList(), book => Assert.AreEqual(book.Author, author)),
                (repo.Filter(new[] { $"language={language}" }).ToList(),
                    book => Assert.AreEqual(book.Language, language)),
                (repo.Filter(new[] { $"category={category}" }).ToList(),
                    book => Assert.AreEqual(book.Category, category)),
                (repo.Filter(new[] { $"isbn={isbn}" }).ToList(), book => Assert.AreEqual(book.ISBN, isbn)),
                (repo.Filter(new[] { $"name={name}" }).ToList(), book => Assert.AreEqual(book.Name, name))
            };
            foreach (var (list, test) in tests)
            {
                Assert.IsNotEmpty(list);
                list.ForEach(b => test(b));
            }
        }
        [Test]
        [TestCase("./TestFiles/Nonempty.json")]
        public void ListNonExistent(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            Assert.IsEmpty(repo.Filter(new[] { $"isbn={Guid.NewGuid().ToString()}" }));
        }
        [Test]
        [TestCase("./TestFiles/Nonempty.json")]
        public void ListWrongFormat(string sourceFile)
        {
            var repo = new BookRepository(sourceFile);
            Assert.Throws<ConsoleException>(() => repo.Filter(new[] { $"{Guid.NewGuid().ToString()}" }));
            Assert.Throws<ConsoleException>(
                () => repo.Filter(new[] { $"{Guid.NewGuid().ToString()}={Guid.NewGuid()}" }));
            Assert.Throws<ConsoleException>(() => repo.Filter(new[] { $"issued={Guid.NewGuid()}" }));
        }
    }
}