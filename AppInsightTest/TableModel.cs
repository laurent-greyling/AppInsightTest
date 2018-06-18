using System;
using System.Collections.Generic;
using System.Text;

namespace AppInsightTest
{
    public class TableModel
    {
        public string Name { get; set; }
        public List<ColumnsModel> Columns { get; set; }

        public List<RowsModel> Rows { get; set; }
    }
}
