using System.Collections;
using System.Data;

/// <summary>
/// Class responsible for printing data from an <see cref="IDataReader"/>.
/// </summary>
public class DataPrinter
{
    /// <summary>
    /// Prints the data from the specified <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="IDataReader"/> containing the data to be printed.</param>
    /// <returns>A string representation of the data.</returns>
    public string PrintData(IDataReader reader)
    {
        string result;
        do
        {
            result = "Fields: ";

            for (int i = 0; i < reader.FieldCount; ++i)
                result += string.Join(", ", Enumerable.Range(0, reader.FieldCount).Select(reader.GetName));

            result += "\n";

            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; ++i)
                {
                    var val = reader.GetValue(i);

                    if (val.GetType().IsArray)
                        result += '[' + string.Join(", ", ((IEnumerable)val).Cast<object>()) + ']';
                    else
                        result += val;
                    result += ", ";
                }
                result += "\n";
            }
            result += "\n";
        } while (reader.NextResult());
        return result;
    }
}