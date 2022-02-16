using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WordleHelper
{
    public class Dictionary
    {
        private Dictionary<(char, int), Letter> _letters;
        private Dictionary<string, Word> _words;

        public Dictionary(String source)
        {
            _letters = new Dictionary<(char, int), Letter>();
            _words = new Dictionary<string, Word>();

            if(File.Exists(source))
            {
                using (FileStream stream = File.OpenRead(source))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var word = reader.ReadLine();

                            if (word.Length == 5)
                            {
                                _words[word] = new Word(word.ToUpper(), this);
                            }
                        }
                    }
                }

                Console.WriteLine($"Loaded {_words.Values.Count().ToString("#,##0")} words from custom dictionary.");
            }
            else
            {
                var ass = Assembly.GetExecutingAssembly();
                using (Stream stream = ass.GetManifestResourceStream("WordleHelper.words.txt"))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var word = reader.ReadLine();

                            if (word.Length == 5)
                            {
                                _words[word] = new Word(word.ToUpper(), this);
                            }
                        }
                    }
                }
                Console.WriteLine($"Loaded {_words.Values.Count().ToString("#,##0")} words from internal dictionary.");
            }                

            var totalLettersAcrossAllWords = _letters.Values.Sum(l => l.Frequency);
            foreach (Letter letter in _letters.Values)
            {
                letter.SetRatio(totalLettersAcrossAllWords);
            }

            var rank = 1;
            foreach(Letter letter in _letters.Values.OrderByDescending(letter => letter.Frequency))
            {
                letter.SetRank(rank++);
            }

            foreach(Word word in _words.Values)
            {
                word.SetScore();
            }

            Console.WriteLine();
        }

        public Letter GetLetter(Char @char, int count = 0)
        {
            @char = @char.ToString().ToUpper()[0];

            if (!_letters.TryGetValue((@char, count), out Letter letter))
            {
                _letters[(@char, count)] = letter = new Letter(@char, count, this);
            }

            return letter;
        }

        public IEnumerable<Letter> GetLetters(String chars)
        {
            if(chars is not null)
            {
                foreach(var group in chars.GroupBy(c => c))
                {
                    for(int i=0; i<group.Count(); i++)
                    {
                        yield return this.GetLetter(group.Key, i);
                    }
                }
            }
        }

        public IEnumerable<Letter> GetLetters()
        {
            return _letters.Values;
        }

        public IEnumerable<Word> GetWords()
        {
            return _words.Values.OrderByDescending(w => w.Score);
        }

        public Word GetWord(String @string)
        {
            _words.TryGetValue(@string.ToUpper(), out Word word);
            return word;
        }
    }
}
