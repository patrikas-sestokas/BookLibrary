using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Library
{
    public class BookRepository
    {
        readonly string _file;
        readonly Repository _repository;
        public BookRepository(string file) =>
            (_file, _repository) = (file, JsonSerializer.Deserialize<Repository>(File.ReadAllText(file)));
        /// <summary>
        ///     Either adds a new book or updates the amount of existing book copies.
        /// </summary>
        /// <param name="copy"></param>
        public void Add(Book copy)
        {
            if (_repository.Books.TryGetValue(copy, out var book))
            {
                _repository.Books.Remove(book);
                _repository.Books.Add(book with { Amount = book.Amount + copy.Amount });
            }
            else
            {
                _repository.Books.Add(copy);
            }
        }
        /// <summary>
        ///     Creates issue and updates amount of reserved book accordingly.
        /// </summary>
        /// <param name="issue"></param>
        /// <returns>The book with specified ISBN.</returns>
        /// <exception cref="ConsoleException">
        ///     throws exception if:
        ///     <para>Duration between [From] and [To] is longer than 60 days or shorter than 0.</para>
        ///     <para>Book with ISBN specified in issue model doesn't exist.</para>
        ///     <para>There are no more free copies of said book.</para>
        ///     <para>Holder has more than 3 books reserved.</para>
        /// </exception>
        public Book Take(Issue issue)
        {
            if(issue.To - issue.From > TimeSpan.FromDays(60))
                throw new ConsoleException("Cannot take a book for longer than 2 months.",
                    ErrorCode.UserError);
            if(issue.To - issue.From < TimeSpan.Zero)
                throw new ConsoleException("[From] and [To] are mixed up, resulting in negative duration.",
                    ErrorCode.UserError);
            if (!_repository.Books.TryGetValue(Book.Default with { ISBN = issue.ISBN }, out var book))
                throw new ConsoleException($"Book with ISBN: {issue.ISBN} doesn't exist.",
                    ErrorCode.ItemNotFound);
            if (book.Amount == 0)
                throw new ConsoleException($"All copies of book with ISBN: {issue.ISBN} are already reserved!",
                    ErrorCode.InsufficientAmount);
            if (_repository.Reservations.Count(r => r.Holder == issue.Holder) >= 3)
                throw new ConsoleException(
                    "A single person can have no more than 3 books at once! Please return one of them.",
                    ErrorCode.UserError);
            _repository.Books.Remove(book);
            _repository.Books.Add(book with { Amount = book.Amount - 1 });
            _repository.Reservations.Add(issue);
            return book with { Amount = 1 };
        }
        /// <summary>
        ///     Returns the book with specified ISBN and removes issue.
        /// </summary>
        /// <param name="issue"></param>
        /// <returns>The book with specified ISBN.</returns>
        /// <exception cref="ConsoleException">
        ///     throws exception if:
        ///     <para>Specified issue is not found in records.</para>
        ///     <para>The book with specified ISBN doesn't exist.</para>
        ///     >
        /// </exception>
        public Book Return(Issue issue)
        {
            var index = _repository.Reservations.IndexOf(issue);
            if (index == -1)
                throw new ConsoleException($"{issue} not found!", ErrorCode.ItemNotFound);
            if (!_repository.Books.TryGetValue(Book.Default with { ISBN = issue.ISBN }, out var book))
                throw new ConsoleException($"Book with ISBN: {issue.ISBN} doesn't exist!",
                    ErrorCode.ItemNotFound);
            _repository.Books.Remove(book);
            _repository.Books.Add(book with { Amount = book.Amount + 1 });
            _repository.Reservations.RemoveAt(index);
            return book with { Amount = 1 };
        }
        /// <summary>
        ///     Returns book collection filtered by specified predicates.
        /// </summary>
        /// <param name="predicates">User specified filters</param>
        /// <returns>Filtered collection</returns>
        /// <exception cref="ConsoleException">
        ///     throws exception if:
        ///     <para>A predicate doesn't match [property]=[value] convention.</para>
        ///     <para>Said [property] doesn't exist.</para>
        ///     <para>[value] cannot be converted into necessary type.</para>
        /// </exception>
        public IEnumerable<Book> Filter(IEnumerable<string> predicates)
        {
            IEnumerable<Book> books = _repository.Books;
            foreach (var predicate in predicates)
            {
                var values = predicate.Split('=');
                if (values.Length < 2)
                    throw new ConsoleException($"Failed to interpret \"{predicate}\", consult -help for proper format.",
                        ErrorCode.UserError);
                var (property, value) = (values[0], values[1]);
                books = property switch
                {
                    "author" => books.Where(b => b.Author == value),
                    "category" => books.Where(b => b.Category == value),
                    "language" => books.Where(b => b.Language == value),
                    "isbn" => new HashSet<Book>(books).TryGetValue(Book.Default with { ISBN = value }, out var book)
                        ? new[] { book }
                        : Array.Empty<Book>(),
                    "name" => books.Where(b => b.Name == value),
                    "issued" => bool.TryParse(value, out var reserved)
                        ? (reserved
                            ? books.Select(b =>
                                b with { Amount = _repository.Reservations.Count(r => r.ISBN == b.ISBN) })
                            : books)
                        .Where(b => b.Amount > 0)
                        : throw new ConsoleException(
                            $"Failed to convert \"{value}\" in parameter \"{predicate}\" into bool.",
                            ErrorCode.UserError),
                    _ => throw new ConsoleException($"Unknown parameter \"{property}\" in \"{predicate}\".",
                        ErrorCode.UserError)
                };
            }

            return books;
        }
        /// <summary>
        /// Deletes the specified amount of books with specified ISBN.
        /// If there are no reservations and amount of said book reaches 0 - it's deleted entirely.
        /// </summary>
        /// <param name="ISBN"></param>
        /// <param name="amount"></param>
        /// <returns>The deleted book.</returns>
        /// <exception cref="ConsoleException">
        ///     throws exception if:
        ///     <para>Book with specified ISBN doesn't exist.</para>
        ///     <para>The remaining amount of said book is lower than specified.</para>
        /// </exception>
        public Book Delete(string ISBN, int amount)
        {
            if (!_repository.Books.TryGetValue(Book.Default with { ISBN = ISBN }, out var book))
                throw new ConsoleException($"Book with ISBN: {ISBN} doesn't exist!", ErrorCode.ItemNotFound);
            if (book.Amount < amount)
                throw new ConsoleException($"Not enough books with ISBN: {ISBN} remaining to satisfy the request.",
                    ErrorCode.InsufficientAmount);
            _repository.Books.Remove(book);
            if (book.Amount > amount || _repository.Reservations.Any(r => r.ISBN == ISBN))
                _repository.Books.Add(book with { Amount = book.Amount - amount });
            return book with { Amount = amount };
        }
        public void Clear()
        {
            _repository.Books.Clear();
            _repository.Reservations.Clear();
        }
        public void WriteToFile() => File.WriteAllText(_file, JsonSerializer.Serialize(_repository, new()
        {
            WriteIndented = true
        }));
    }
}