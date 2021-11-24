using System;
using System.Collections.Generic;

namespace Library
{
    //ISBN is often confused to be unique identifier of a particular book. It isn't.
    //It identifies specific editions of the book, not individual copies.
    //With no way to differentiate copies - property "Amount" was introduced.
    public sealed record Book(string Name, string Author, string Category, string Language, DateTime PublicationDate,
        string ISBN, int Amount = 1)
    {
        public static readonly Book Default = new(default, default, default, default, default, default);
        public bool Equals(Book other) => other is not null && other.ISBN == ISBN;
        public override int GetHashCode() => ISBN.GetHashCode();
    }
    public sealed record Issue(string Holder, DateTime From, DateTime To, string ISBN);
    sealed record Repository(HashSet<Book> Books, List<Issue> Reservations);
}