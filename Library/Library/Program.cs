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
            "   -take [holder] [time from] [time to] [isbn]",
            "   -return [holder] [time from] [time to] [isbn]",
            "   -filter (with optional arguments specified below)",
            "       author=[author]",
            "       category=[category]",
            "       language=[language]",
            "       isbn=[isbn]",
            "       name=[name]",
            "       reserved=[true/false]",
            "   -delete [isbn] [amount]",
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
                        Reservation reservation = new(parameters[0], DateTime.Parse(parameters[1]),
                            DateTime.Parse(parameters[2]), parameters[3]);
                        book = Repository.Take(reservation);
                        Console.Out.WriteLine();
                        break;

                    case "-return":
                        if (parameters.Count < 1)
                            throw new ConsoleException($"Expected: {Arguments[3]}.", ErrorCode.TooFewArguments);
                        reservation = new(parameters[0], DateTime.Parse(parameters[1]),
                            DateTime.Parse(parameters[2]), parameters[3]);
                        if (reservation.To < DateTime.Now)
                            Console.Out.WriteLine("Aren't we cheeky?");
                        book = Repository.Return(reservation);
                        Console.Out.WriteLine($"{book} successfully returned and {reservation} is removed.");
                        break;

                    case "-filter":
                        Console.Out.WriteLine(string.Join(Environment.NewLine, Repository.Filter(parameters)));
                        break;

                    case "-delete":
                        if (parameters.Count < 2)
                            throw new ConsoleException($"Expected: {Arguments[5]}.", ErrorCode.TooFewArguments);
                        if (!int.TryParse(parameters[1], out var amount))
                            throw new ConsoleException($"Can't parse \"{parameters[1]}\" into integer.",
                                ErrorCode.UserError);
                        book = Repository.Delete(parameters[0], amount);
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