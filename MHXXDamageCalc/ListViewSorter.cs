using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

/// <summary>
/// This class is an implementation of the 'IComparer' interface.
/// </summary>
public class ListViewColumnSorter : IComparer
{
    /// <summary>
    /// Specifies the column to be sorted
    /// </summary>
    private int ColumnToSort;
    /// <summary>
    /// Specifies the order in which to sort (i.e. 'Ascending').
    /// </summary>
    private SortOrder OrderOfSort;
    /// <summary>
    /// Case insensitive comparer object
    /// </summary>
    private CaseInsensitiveComparer ObjectCompare;

    private List<string> sharpnesses = new List<string> { "Purple", "White", "Blue", "Green", "Yellow", "Orange", "Red" };
    private List<string> elements = new List<string> { "Fire", "Water", "Thunder", "Ice", "Dragon", "Poison", "Para", "Sleep", "Blast", "(No Element)" };
    private List<string> DBelements = new List<string> { "DB - Fire", "DB - Water", "DB - Thunder", "DB - Ice", "DB - Dragon", "DB - Poison", "DB - Para", "DB - Blast", "(No Element)" };
    private List<string> SAelements = new List<string> { "SA - Dragon", "SA - Poison", "SA - Para", "SA - Exhaust" };

    /// <summary>
    /// Class constructor.  Initializes various elements
    /// </summary>
    public ListViewColumnSorter()
    {
        // Initialize the column to '0'
        ColumnToSort = 0;

        // Initialize the sort order to 'none'
        OrderOfSort = SortOrder.None;

        // Initialize the CaseInsensitiveComparer object
        ObjectCompare = new CaseInsensitiveComparer();
    }

    /// <summary>
    /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
    /// </summary>
    /// <param name="x">First object to be compared</param>
    /// <param name="y">Second object to be compared</param>
    /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
    public int Compare(object x, object y)
    {
        int compareResult;
        ListViewItem listviewX, listviewY;

        // Cast the objects to be compared to ListViewItem objects
        listviewX = (ListViewItem)x;
        listviewY = (ListViewItem)y;

        if (int.TryParse(listviewX.SubItems[ColumnToSort].Text, out int result))
        {
            float leftVal;
            float rightVal;
            if (listviewX.SubItems[ColumnToSort].Text.Contains("/"))
            {
                string[] affinity = listviewX.SubItems[ColumnToSort].Text.Split(new string[] { "/" }, System.StringSplitOptions.None);
                leftVal = float.Parse(affinity[0]);
            }
            else
            {
                leftVal = float.Parse(listviewX.SubItems[ColumnToSort].Text);
            }
            
            if(listviewY.SubItems[ColumnToSort].Text.Contains("/"))
            {
                string[] affinity = listviewY.SubItems[ColumnToSort].Text.Split(new string[] { "/" }, System.StringSplitOptions.None);
                rightVal = float.Parse(affinity[0]);
            }
            else
            {
                rightVal = float.Parse(listviewY.SubItems[ColumnToSort].Text);
            }
            
            compareResult = (int)rightVal - (int)leftVal;
        }
        else if (sharpnesses.Contains(listviewX.SubItems[ColumnToSort].Text))
        {
            int leftVal = sharpnesses.IndexOf(listviewX.SubItems[ColumnToSort].Text);
            int rightVal = sharpnesses.IndexOf(listviewY.SubItems[ColumnToSort].Text);
            compareResult = leftVal - rightVal;
        }
        else if (elements.Contains(listviewX.SubItems[ColumnToSort].Text))
        {
            int leftVal = elements.IndexOf(listviewX.SubItems[ColumnToSort].Text);
            int rightVal = elements.IndexOf(listviewY.SubItems[ColumnToSort].Text);
            compareResult = leftVal - rightVal;
        }
        else if (DBelements.Contains(listviewX.SubItems[ColumnToSort].Text))
        {
            int leftVal = DBelements.IndexOf(listviewX.SubItems[ColumnToSort].Text);
            int rightVal = DBelements.IndexOf(listviewY.SubItems[ColumnToSort].Text);
            compareResult = leftVal - rightVal;
        }
        else if (SAelements.Contains(listviewX.SubItems[ColumnToSort].Text))
        {
            int leftVal = SAelements.IndexOf(listviewX.SubItems[ColumnToSort].Text);
            int rightVal = SAelements.IndexOf(listviewY.SubItems[ColumnToSort].Text);
            compareResult = leftVal - rightVal;
        }
        else
        {
            // Compare the two items
            compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
        }


        // Calculate correct return value based on object comparison
        if (OrderOfSort == SortOrder.Ascending)
        {
            if(ColumnToSort == 0)
            {
                return -compareResult;
            }
            // Ascending sort is selected, return normal result of compare operation
            return compareResult;
        }
        else if (OrderOfSort == SortOrder.Descending)
        {
            if (ColumnToSort == 0)
            {
                return compareResult;
            }
            // Descending sort is selected, return negative result of compare operation
            return (-compareResult);
        }
        else
        {
            // Return '0' to indicate they are equal
            return 0;
        }
    }

    /// <summary>
    /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
    /// </summary>
    public int SortColumn
    {
        set
        {
            ColumnToSort = value;
        }
        get
        {
            return ColumnToSort;
        }
    }


    /// <summary>
    /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
    /// </summary>
    public SortOrder Order
    {
        set
        {
            OrderOfSort = value;
        }
        get
        {
            return OrderOfSort;
        }
    }

}