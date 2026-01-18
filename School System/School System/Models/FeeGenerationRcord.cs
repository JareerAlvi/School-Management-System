using System;
using System.Collections.Generic;
namespace School_System.Models
{
    public class FeeGenerationRecord
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime GeneratedOn { get; set; }
    }

}
