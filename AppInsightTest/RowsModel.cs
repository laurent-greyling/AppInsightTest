using System;
using System.Collections.Generic;
using System.Text;

namespace AppInsightTest
{
    public class RowsModel
    {
        public string Name { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }

        public double Average { get; set; }
    }
}
