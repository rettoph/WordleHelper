using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordleHelper
{
    public class Letter
    {
        private Dictionary _dictionary;
        private HashSet<Word> _words;

        public Char Char { get; private set; }
        public Int32 Count { get; private set; }
        public Int32 Frequency { get; private set; }
        public Single Ratio { get; private set; }
        public Int32 Rank { get; private set; }

        public IEnumerable<Word> Words => _words;

        public Letter(char @char, int count, Dictionary dictionary)
        {
            _dictionary = dictionary;
            _words = new HashSet<Word>();

            this.Char = @char;
            this.Count = count;
        }

        public void AddWord(Word word)
        {
            this.Frequency++;

            _words.Add(word);
        }

        public void SetRatio(Int32 totalLettersAcrossAllWords)
        {
            this.Ratio = (Single)this.Frequency / totalLettersAcrossAllWords;
        }

        public void SetRank(Int32 rank)
        {
            this.Rank = rank;
        }

        public override string ToString()
        {
            return $"{this.Rank.ToString("00")}. {this.Char}:{this.Count+1}, Percent: {(this.Ratio * 100).ToString("00.000")}%, Freqency: {this.Frequency.ToString("0,000")}, Words: {_words.Count.ToString("0,000")}";
        }
    }
}
