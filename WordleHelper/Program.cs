using WordleHelper;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.RegularExpressions;

var dict = new Dictionary("words.txt");
bool running = true;

var charArg = new Argument<String>("char", "Specify the letter you with to investigate. Formats: 'a', 'a1', 'A', 'A2'");
var letter = new Command("letter", "Learn more about letters.")
{
    charArg
};
letter.SetHandler<String>(c =>
{
    var @char = c.First();
    var countString = c.ElementAtOrDefault(1).ToString() ?? "1";
    if(!int.TryParse(countString, out var count))
    {
        count = 1;
    }

    var letter = dict.GetLetter(@char, count - 1);
    Console.WriteLine(letter);

    Console.WriteLine($"Words ({letter.Words.Count().ToString("#,###")}): ");
    var words = letter.Words.OrderByDescending(word => word.Score).ToArray();
    for (var i=0; i<10; i++)
    {
        Console.WriteLine($"   {words[i]}");
    }

    Console.WriteLine();
}, charArg);

var letterRank = new Command("rank", "View all letter ranks.");
letterRank.SetHandler(() =>
{
    foreach(Letter letter in dict.GetLetters().OrderByDescending(l => l.Frequency))
    {
        Console.WriteLine(letter);
    }

    Console.WriteLine();
});
letter.Add(letterRank);

var withOpt = new Option<String>("--with", "Letters your word must include somewhere.");
var withoutOpt = new Option<String>("--without", "Letters your wors must not include somewhere.");
var matchingOpt = new Option<String>("--matching", "A regex pattern your word must match. Example: 'X.A..'");
var notMatchingOpt = new Option<String>("--not-matching", "Comma seperated regex patterns your word must NOT match. Example: 'A....,...K.'");
var tryWithOpt = new Option<String>("--try-with", "Find words that have the most of these letters, but not necessarily all.");
var tryWithoutOpt = new Option<String>("--try-without", "Find words that have do not have most of these letters, but not necessarily none.");


var word = new Command("word", "Find a word.")
{
    withOpt,
    withoutOpt,
    matchingOpt,
    notMatchingOpt,
    tryWithOpt,
    tryWithoutOpt
};
word.SetHandler<String, String, String, String, String, String>((with, without, matching, notMatching, tryWith, tryWithout) =>
{
    Letter[] withLetters = dict.GetLetters(with).ToArray();
    Letter[] withoutLetters = dict.GetLetters(without).ToArray();
    Letter[] tryWithLetters = dict.GetLetters(tryWith).ToArray();
    Letter[] tryWithoutLetters = dict.GetLetters(tryWithout).ToArray();

    IEnumerable<Word> words = dict.GetWords().OrderByDescending(word => word.Score)
        .WithLetters(withLetters)
        .WithoutLetters(withoutLetters)
        .Matching(matching)
        .NotMatching(notMatching?.Split(","))
        .TryWithLetters(tryWithLetters)
        .TryWithoutLetters(withoutLetters);


    if (!word.Any() || word.Count() == 0)
    {
        Console.WriteLine("No valid Words found.");
        return;
    }

    Console.WriteLine($"Words ({words.Count()}): ");
    foreach (Word word in words.Take(10))
    {
        Console.WriteLine(word);
    }

    Console.WriteLine();
}, withOpt, withoutOpt, matchingOpt, notMatchingOpt, tryWithOpt, tryWithoutOpt);

var exit = new Command("exit");
exit.SetHandler(() => running = false);

var game = new Command("game", "Start a more advanced wordle game helper.");
game.SetHandler(() => new Game(dict));

var root = new RootCommand();
root.Add(letter);
root.Add(word);
root.Add(game);
root.Add(exit);


root.Invoke("--help");
while (running)
{
    root.Invoke(Console.ReadLine().Split());
}