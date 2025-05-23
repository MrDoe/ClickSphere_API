namespace ClickSphere_API.Tools;
/// <summary>
/// Represents the result of an operation.
/// </summary>
public class Result(bool isSuccessful, string output)
{
    /// <summary>
    /// Gets or sets the output of the operation.
    /// </summary>
    public string Output { get; set; } = output;
    
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccessful { get; set; } = isSuccessful;

    /// <summary>
    /// Creates a new instance of the <see cref="Result"/> class representing a successful operation.
    /// </summary>
    /// <returns>A new instance of the <see cref="Result"/> class with <see cref="IsSuccessful"/> set to true and <see cref="Output"/> set to "Success".</returns>
    public static Result Ok(string? output = "Success")
    {
        if (output == null)
            return new Result(true, "Success");
        else
            return new Result(true, output);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Result"/> class representing a failed operation.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new instance of the <see cref="Result"/> class with <see cref="IsSuccessful"/> set to false and <see cref="Output"/> set to the specified error message.</returns>
    public static Result BadRequest(string message)
    {
        return new Result(false, message);
    }
}