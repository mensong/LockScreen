using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class tbl_QuestionBank
    {
        public int id { get; set; }

        public string question { get; set; }

        public string answer { get; set; }

        public bool caseSensitive { get; set; }

        public int level { get; set; }
    }
}