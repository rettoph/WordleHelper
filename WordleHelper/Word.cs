using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordleHelper
{
    public class Word
    {
        private Dictionary _dictionary;

        public String String { get; private set; }
        public Letter[] Letters { get; private set; }
        public Int32 Score { get; private set; }

        public Word(string @string, Dictionary dictionary)
        {
            _dictionary = dictionary;

            this.String = @string;

            var chars = this.String.GroupBy(c => c);
            var letters = new List<Letter>();
            foreach(var group in chars)
            {
                for(int i=0; i<group.Count(); i++)
                {
                    var letter = _dictionary.GetLetter(group.Key, i);
                    letter.AddWord(this);
                    letters.Add(letter);
                }
            }

            this.Letters = letters.ToArray();
        }

        public void SetScore()
        {
            this.Score = 0;
            foreach(Letter letter in this.Letters)
            {
                this.Score += letter.Frequency;
            }
        }

        public override string ToString()
        {
            return $"{this.String}, Score: {this.Score.ToString("#,##0")}";
        }
    }
}
