namespace ClickSphere_API.Tools;
public class Result(bool isSuccessful, string output)
{
    public string Output { get; set; } = output;
    public bool IsSuccessful { get; set; } = isSuccessful;

    public static Result Ok()
    {
        return new Result(true, "Success");
    }

    public static Result BadRequest(string message)
    {
        return new Result(false, message);
    }
}