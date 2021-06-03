using System.Collections;
using System.Windows.Forms;

namespace TaskManager
{
    public class ListViewItemComparer : IComparer
    {
        public int ColumnIndex { get; set; }
        public SortOrder SortOrder { get; set; }

        public ListViewItemComparer()
        {
            SortOrder = SortOrder.None;
        }

        public int Compare(object x, object y)
        {
            ListViewItem listViewItemX = x as ListViewItem;
            ListViewItem listViewItemY = y as ListViewItem;

            int result;

            switch (ColumnIndex)
            {
                case 0:
                    result = string.Compare(listViewItemX.SubItems[ColumnIndex].Text, listViewItemY.SubItems[ColumnIndex].Text, false);
                    break;

                case 1:
                    result = GetIntResult(listViewItemX, listViewItemY);
                    break;

                default:
                    result = GetIntResult(listViewItemX, listViewItemY);
                    break;
            }

            return (SortOrder == SortOrder.Descending) ? -result : result;
        }

        private int GetIntResult(ListViewItem listViewItemX, ListViewItem listViewItemY)
        {
            double valueX = double.Parse(listViewItemX.SubItems[ColumnIndex].Text);
            double valueY = double.Parse(listViewItemY.SubItems[ColumnIndex].Text);
            return valueX.CompareTo(valueY);
        }
    }
}
