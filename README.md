# LibraryTerminal Instruction Manual

## Description

A command-line based library management utility.

## Prerequisites

- [.NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0) (preferably the latest revision).
- An IDE of your choice.

## Commands

A list of commands and their parameters.

⚠ Parameters between `[]` brackets are required.

**Commands can be chained** like so: `-add file.json -list isbn=xxxxxxx`
* **`-add`** `[path to json file containing book model]`

  Adds book from .json file to the library, if said book exists - updates it's amount.
* **`-take`** `[holder] [time from : yyyy-mm-dd] [time to : yyyy-mm-dd] [isbn]` 

  Creates issue record and takes book by `[isbn]` from library.
  
  Taking more than 3 books per person is not allowed, neither is leasing a book for longer than 2 months.
* **`-return`** `[time from : yyyy-mm-dd] [time to : yyyy-mm-dd] [isbn]`
  
  Returns the book when provided with the same parameters as in **`-take`** command.
  
  And yes, the application is aware of late returns.
* **`-list`** lists books filtered using any number optional parameters below:
  * `author=[author]`
  * `category=[category]`
  * `language=[language]`
  * `isbn=[isbn]`
  * `name=[name]`
  * `issued=[true/false]` 

    Both `true` and `false` will return a list of books, however the `Amount` property will show:
    
    * if `true` - the amount of issues of said book.
    * if `false` - the amount of free remaining copies in the library
* **`-delete`** `[isbn] [amount : int]`

  Updates the amount of books with `[isbn]` accordingly, if there are no issues of said book and it's amount reaches 0 - it's deleted entirely and can only be reintroduced using **`-add`** command.
* **`-help`**

## Examples

* **`-add`**

  `-add file.json` when `file.json` looks like:
  ```json
  {
     "Name": "Random Book",
     "Author": "Randy",
     "Category": "Parody",
     "Language": "French",
     "PublicationDate": "2008-06-26",
     "ISBN": "471658197-X"
  }
  ```
  Property `Amount=[amount : int]` can also be added to specify how many copies said edition of book has. If it isn't provided - the default is 1.

  Output:
  ```
  Book { Name = Random Book, Author = Randy, Category = Parody, Language = French, PublicationDate = 6/26/2008 12:00:00 AM, ISBN = 471658197-X, Amount = 1 } added successfully!
  ```

* **`-take`**
  
  `-take "John smith" 2022-11-23 2022-12-20 471658197-X`:
  ```
  Book { Name = Random Book, Author = Randy, Category = Parody, Language = French, PublicationDate = 6/26/2008 12:00:00 AM, ISBN = 471658197-X, Amount = 1 } issued successfully.
  ```
  
  In case the same person has leased 3 books and is trying to take 4th one:
  ```
  Library.ConsoleException: A single person can have no more than 3 books at once! Please return one of them.
  ...
  ```

* **`-return`**

  `-return "John smith" 2022-11-23 2022-12-20 471658197-X`

  Output:
  ```
  Book { Name = Random Book, Author = Randy, Category = Parody, Language = French, PublicationDate = 6/26/2008 12:00:00 AM, ISBN = 471658197-X, Amount = 1 } successfully returned and Issue { Holder = John smith, From = 11/23/2022 12:00:00 AM, To = 12/20/2022 12:00:00 AM, ISBN = 471658197-X } is removed.
  ```
  
  In case the return is late the normal output is prepended with:
  ```
  Aren't we cheeky?
  ...
  ```

* **`-list`**
  
  `-list issued=true` after using **`-take`** example:
  ```
  Book { Name = Random Book, Author = Randy, Category = Parody, Language = French, PublicationDate = 6/26/2008 12:00:00 AM, ISBN = 471658197-X, Amount = 1 }
  ```
  `-list issued=false` after using **`-return`** example:
  ```
  ...
  Book { Name = Random Book, Author = Randy, Category = Parody, Language = French, PublicationDate = 6/26/2008 12:00:00 AM, ISBN = 471658197-X, Amount = 1 }
  ```

* **`-delete`**

  `-delete 471658197-X 1`:
  ```
  Successfully removed Book { Name = Random Book, Author = Randy, Category = Parody, Language = French, PublicationDate = 6/26/2008 12:00:00 AM, ISBN = 471658197-X, Amount = 1 }.
  ```
  
## License
[MIT](https://choosealicense.com/licenses/mit/)
