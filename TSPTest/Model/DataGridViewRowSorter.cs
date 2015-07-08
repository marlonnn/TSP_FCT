using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;

namespace TSPTest.Model
{
    public class DataGridViewRowSorter : IComparer
    {
        private int columnToSort;
        private SortOrder orderOfSort;
        private CaseInsensitiveComparer objectCompare;
        public DataGridViewRowSorter()
        {
            columnToSort = 0;
            orderOfSort = SortOrder.None;
            objectCompare = new CaseInsensitiveComparer();
        }
        public int Compare(object x, object y)
        {
            int compareResult;
            DataGridViewRow dataGridViewRowX, dataGridViewRowY;
            dataGridViewRowX = (DataGridViewRow)x;
            dataGridViewRowY = (DataGridViewRow)y;
            //比较
            compareResult = objectCompare.Compare(dataGridViewRowX.Cells[columnToSort].Value,
                dataGridViewRowY.Cells[columnToSort].Value);
            if (orderOfSort == SortOrder.Ascending)
            {
                return compareResult;
            }
            else if (orderOfSort == SortOrder.Descending)
            {
                return (-compareResult);
            }
            else
            {
                return 0;
            }
        }

        public int SortColumn
        {
            get { return columnToSort; }
            set { columnToSort = value; }
        }

        public SortOrder OrderOfSort
        {
            get { return orderOfSort; }
            set { orderOfSort = value; }
        }
    }
}
