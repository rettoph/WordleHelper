using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WordleHelper
{
    public class Game
    {
        enum Result
        {
            Unknown,
            Green,
            Yellow,
            Black
        };

        struct LetterPositionResult
        {
            public readonly Letter Letter;
            public readonly Result Result;
            public readonly int Position;

            public LetterPositionResult(Letter letter, Result result, int position)
            {
                Letter = letter;
                Result = result;
                Position = position;
            }

            public override bool Equals(object? obj)
            {
                return obj is LetterPositionResult result &&
                       this.Letter.Char == result.Letter.Char &&
                       this.Position == result.Position &&
                       Result == result.Result;
            }

            public string NotMatchingPattern()
            {
                var pattern = new StringBuilder(new string('.', 5));
                pattern[this.Position] = this.Letter.Char;

                return pattern.ToString();
            }
        }

        private Dictionary _dictionary;

        private HashSet<Letter> _hasLetters;
        private HashSet<Letter> _doesNotHaveLetters;
        private HashSet<string> _notMatchingPatterns;
        private StringBuilder _matchingPattern;

        private bool _running;

        public Game(Dictionary dictionary)
        {
            _dictionary = dictionary;
            _hasLetters = new HashSet<Letter>();
            _doesNotHaveLetters = new HashSet<Letter>();
            _notMatchingPatterns = new HashSet<string>();
            _matchingPattern = new StringBuilder(new string('.', 5));
            _running = true;

            Console.WriteLine();

            this.PrintLine("Starting Wordle game!\n", ConsoleColor.Green);

            this.PrintLine("Here are some suggested first guesses:", ConsoleColor.White);

            this.Print(_dictionary.GetWords().First().String, ConsoleColor.Cyan);
            this.PrintLine(" - Highest letter score.", ConsoleColor.Gray);

            this.Print(_dictionary.GetWords().TryWithLetters(_dictionary.GetLetters("aei").ToArray()).WithoutLetters(_dictionary.GetLetters("ou").ToArray()).First().String, ConsoleColor.Cyan);
            this.PrintLine(" - Highest letter score, with 'aei', without 'ou'.", ConsoleColor.Gray);

            this.Print(_dictionary.GetWords().TryWithLetters(_dictionary.GetLetters("aeiou").ToArray()).First().String, ConsoleColor.Cyan);
            this.PrintLine(" - Highest letter score, most vowels, without y.", ConsoleColor.Gray);

            this.Print(_dictionary.GetWords().TryWithLetters(_dictionary.GetLetters("aeiouy").ToArray()).First().String, ConsoleColor.Cyan);
            this.PrintLine(" - Highest letter score, most vowels, with y.", ConsoleColor.Gray);

            while(_running)
            {
                this.Guess();
            }
        }

        public void Guess()
        {
            this.PrintLine("\nInput your guess:\n", ConsoleColor.White);
            string guess = Console.ReadLine();

            if(guess.Length > 5)
            {
                this.Error("Guess is too long.");
                return;
            }

            if (guess.Length < 5)
            {
                this.Error("Guess is too short.");
                return;
            }

            if (!Regex.Match(guess, "[a-z]{5}", RegexOptions.IgnoreCase).Success)
            {
                this.Error("Guess is invalid.");
                return;
            }

            Word word = _dictionary.GetWord(guess);

            if(word is null)
            {
                this.Error("Unknown word.");
                return;
            }

            this.PrintLine("What was the result? Type 'g' for green, 'y' for yellow, and 'b' for black.", ConsoleColor.White);

            Result[] results = Console.ReadLine().ToCharArray().Select(l => GetResultFromChar(l)).ToArray();

            if(results.Length < 5)
            {
                this.Error("Result input is too short.");
                return;
            }

            if (results.Length > 5)
            {
                this.Error("Result input is too long.");
                return;
            }

            if (results.Contains(Result.Unknown))
            {
                this.Error("Invalid result.");
                return;
            }

            LetterPositionResult[] letterPositionResults = new LetterPositionResult[5];
            int greens = 0;

            List<Letter> wordLetters = new List<Letter>(word.Letters);
            for (int i = 0; i < 5; i++)
            {
                var letter = wordLetters.First(l => l.Char == word.String[i]);
                letterPositionResults[i] = new LetterPositionResult(letter, results[i], i);
                wordLetters.Remove(letter);

                if (results[i] == Result.Green)
                {
                    greens++;
                }
            }

            foreach(LetterPositionResult letterPositionResult in letterPositionResults)
            {
                switch (letterPositionResult.Result)
                {
                    case Result.Green:
                        _hasLetters.Add(letterPositionResult.Letter);
                        _matchingPattern[letterPositionResult.Position] = letterPositionResult.Letter.Char;
                        break;
                    case Result.Yellow:
                        _hasLetters.Add(letterPositionResult.Letter);
                        _notMatchingPatterns.Add(letterPositionResult.NotMatchingPattern());
                        break;
                    case Result.Black:
                        _doesNotHaveLetters.Add(letterPositionResult.Letter);
                        break;
                }
            }

            var possibleWords = _dictionary.GetWords()
                .WithLetters(_hasLetters.ToArray())
                .WithoutLetters(_doesNotHaveLetters.ToArray())
                .Matching(_matchingPattern.ToString())
                .NotMatching(_notMatchingPatterns.ToArray());

            this.Print($"Top possible answer(s) ({possibleWords.Count().ToString("#,##0")}): ", ConsoleColor.White);
            this.PrintLine(string.Join(", ", possibleWords.Take(5).Select(w => w.String)), ConsoleColor.Cyan);

            var newLetters = possibleWords.NewLetters(_hasLetters.Concat(_doesNotHaveLetters).ToArray());
            var possibleGuesses = _dictionary.GetWords()
                .TryWithLetters(newLetters.ToArray());

            this.Print($"Most informative guess(es) ({possibleGuesses.Count().ToString("#,##0")}): ", ConsoleColor.White);
            this.PrintLine(string.Join(", ", possibleGuesses.Take(5).Select(w => w.String)), ConsoleColor.Magenta);

            if(greens == 5 || possibleWords.Count() <= 1)
            {
                Console.WriteLine("Game complete! Congrats.");
                _running = false;
            }
        }

        private void Error(string message)
        {
            this.PrintLine(message, ConsoleColor.Red);
        }

        private void Print(string message, ConsoleColor color)
        {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = original;
        }

        private void PrintLine(string message, ConsoleColor color)
        {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = original;
        }

        private Result GetResultFromChar(char c)
        {
            switch(c.ToString().ToLower())
            {
                case "g":
                    return Result.Green;
                case "y":
                    return Result.Yellow;
                case "b":
                    return Result.Black;
                default:
                    return Result.Unknown;
            }
        }
    }
}
