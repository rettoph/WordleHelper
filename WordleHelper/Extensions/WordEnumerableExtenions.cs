using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WordleHelper
{
    public static class WordEnumerableExtenions
    {
        public static IEnumerable<Word> WithLetters(this IEnumerable<Word> words, params Letter[] letters)
        {
            if (letters is null || letters.Length == 0)
            {
                return words;
            }

            return words.Where(word =>
            {
                foreach (Letter withLetter in letters)
                {
                    if (!word.Letters.Contains(withLetter))
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        public static IEnumerable<Word> WithoutLetters(this IEnumerable<Word> words, params Letter[] letters)
        {
            if (letters is null || letters.Length == 0)
            {
                return words;
            }

            return words.Where(word =>
            {
                foreach (Letter withoutLetter in letters)
                {
                    if (word.Letters.Contains(withoutLetter))
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        public static IEnumerable<Word> Matching(this IEnumerable<Word> words, string pattern)
        {
            if(pattern is null || pattern == String.Empty)
            {
                return words;
            }

            return words.Where(word => Regex.Match(word.String, pattern, RegexOptions.IgnoreCase).Success);
        }

        public static IEnumerable<Word> NotMatching(this IEnumerable<Word> words, params string[] patterns)
        {
            if(patterns is null || patterns.Length == 0)
            {
                return words;
            }

            foreach (string pattern in patterns)
            {
                words = words.Where(word => !Regex.Match(word.String, pattern, RegexOptions.IgnoreCase).Success);
            }

            return words;
        }

        public static IEnumerable<Word> TryWithLetters(this IEnumerable<Word> words, params Letter[] letters)
        {
            if(letters is null || letters.Length == 0)
            {
                return words;
            }


            return words.OrderByDescending(word =>
            {
                var tryWithScore = 0f;
                foreach (Letter letter in letters)
                {
                    if (word.Letters.Contains(letter))
                    {
                        var weight = 1f;
                        tryWithScore += weight;
                    }
                }
                return tryWithScore;
            }).ThenBy(word => word.Score);
        }

        public static IEnumerable<Word> TryWithLetters(this IEnumerable<Word> words, params (Letter letter, int words)[] letters)
        {
            if (letters is null || letters.Length == 0)
            {
                return words;
            }


            return words.OrderByDescending(word =>
            {
                var tryWithScore = 0f;
                foreach ((Letter letter, int words) in letters)
                {
                    if(word.Letters.Contains(letter))
                    {
                        var weight = words * letter.Ratio;
                        tryWithScore += weight;
                    }
                }
                return tryWithScore;
            });
        }

        public static IEnumerable<Word> TryWithoutLetters(this IEnumerable<Word> words, params Letter[] letters)
        {
            if (letters is null || letters.Length == 0)
            {
                return words;
            }


            return words.OrderBy(word =>
            {
                var tryWithScore = 0;
                foreach (Letter letter in letters)
                {
                    tryWithScore += word.Letters.Contains(letter) ? 1 : 0;
                }
                return tryWithScore;
            }).ThenBy(word => word.Score);
        }

        public static IEnumerable<(Letter letter, int words)> NewLetters(this IEnumerable<Word> words, Letter[] letters)
        {
            if (letters is null || letters.Length == 0)
            {
                return default;
            }

            List<Letter> newLetters = new List<Letter>();

            foreach(Word word in words)
            {
                foreach(Letter letter in word.Letters)
                {
                    if(!letters.Contains(letter))
                    {
                        newLetters.Add(letter);
                    }
                }
            }

            return newLetters.GroupBy(letter => letter).Select(group => (letter: group.Key, words: group.Count())).OrderByDescending(result => result.words);
        }
    }
}
