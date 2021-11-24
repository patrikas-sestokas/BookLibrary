using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Library
{
    public enum ErrorCode
    {
        Success = 0,
        TooFewArguments = 1,
        FileNotFound = 2,
        ItemNotFound = 3,
        InsufficientAmount = 4,
        UserError = 5
    }
    class Program
    {
        const string MakeshiftDatabase = @".\Books.json";
        static readonly BookRepository Repository = new(MakeshiftDatabase);
        static readonly string[] Arguments =
        {
            "Expected arguments:",
            "   -add [path to json file containing book model]",
            "   -take [holder] [time from : yyyy-mm-dd] [time to : yyyy-mm-dd] [isbn]",
            "   -return [holder] [time from : yyyy-mm-dd] [time to : yyyy-mm-dd] [isbn]",
            "   -list (with optional arguments specified below)",
            "       author=[author]",
            "       category=[category]",
            "       language=[language]",
            "       isbn=[isbn]",
            "       name=[name]",
            "       issued=[true/false]",
            "   -delete [isbn] [amount : int]",
            "   -help"
        };
        static IEnumerable<(string command, List<string> parameters)> ProcessArguments(string[] args)
        {
            (string command, List<string> parameters) current = default;
            foreach (var arg in args)
                if (arg.StartsWith('-'))
                {
                    if (current != default) yield return current;
                    current = (arg, new());
                }
                else
                {
                    current.parameters.Add(arg);
                }

            yield return current;
        }
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += ConsoleExceptionHandler;
            if (args.Length < 1)
                throw new ConsoleException("No arguments provided, use -help for instructions",
                    ErrorCode.TooFewArguments);
            foreach (var (command, parameters) in ProcessArguments(args))
                switch (command)
                {
                    case "-add":
                        if (parameters.Count < 1)
                            throw new ConsoleException($"Expected: {Arguments[1]}.", ErrorCode.TooFewArguments);
                        if (!File.Exists(parameters[0]))
                            throw new ConsoleException($"File \"{parameters[0]}\" does not exist!",
                                ErrorCode.FileNotFound);
                        var book = JsonSerializer.Deserialize<Book>(File.ReadAllText(parameters[0]));
                        Repository.Add(book);
                        Console.Out.WriteLine($"{book} added successfully!");
                        break;

                    case "-take":
                        if (parameters.Count < 4)
                            throw new ConsoleException($"Expected: {Arguments[2]}.", ErrorCode.TooFewArguments);
                        Issue issue = new(parameters[0], DateTime.Parse(parameters[1]),
                            DateTime.Parse(parameters[2]), parameters[3]);
                        book = Repository.Take(issue);
                        Console.Out.WriteLine($"{book} successfully reserved.");
                        break;

                    case "-return":
                        if (parameters.Count < 4)
                            throw new ConsoleException($"Expected: {Arguments[3]}.", ErrorCode.TooFewArguments);
                        issue = new(parameters[0], DateTime.Parse(parameters[1]),
                            DateTime.Parse(parameters[2]), parameters[3]);
                        book = Repository.Return(issue);
                        if (issue.To < DateTime.Now)
                            Console.Out.WriteLine("Aren't we cheeky?");
                        Console.Out.WriteLine($"{book} successfully returned and {issue} is removed.");
                        break;

                    case "-list":
                        Console.Out.WriteLine(string.Join(Environment.NewLine, Repository.Filter(parameters)));
                        break;

                    case "-delete":
                        if (parameters.Count < 2)
                            throw new ConsoleException($"Expected: {Arguments[5]}.", ErrorCode.TooFewArguments);
                        book = Repository.Delete(parameters[0], int.Parse(parameters[1]));
                        Console.Out.WriteLine($"Successfully removed {book}.");
                        break;

                    case "-help":
                        Console.Out.WriteLine(string.Join(Environment.NewLine, Arguments));
                        break;

                    default:
                        throw new ConsoleException($"Unknown command \"{command}\".", ErrorCode.UserError);
                }

            Repository.WriteToFile();
            return (int)ErrorCode.Success;
        }
        static void ConsoleExceptionHandler(object sender, UnhandledExceptionEventArgs eventArgs)
        {
            Console.Error.WriteLine(eventArgs.ExceptionObject.ToString());
            if (eventArgs.ExceptionObject is ConsoleException)
                Repository.WriteToFile();
            Environment.Exit(((Exception)eventArgs.ExceptionObject).HResult);
        }
    }
    public class ConsoleException : Exception
    {
        public ConsoleException(string message, ErrorCode code) : base(message) => HResult = (int)code;
    }
}