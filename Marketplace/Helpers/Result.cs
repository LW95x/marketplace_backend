namespace Marketplace.Helpers
{
    public class Result
    {
        public bool Succeeded { get; private set; }
        public string? Error { get; private set; }
        public static Result Success()
        {
            return new Result { Succeeded = true };
        }

        public static Result Fail(string? errorMessage)
        {
            return new Result { Succeeded = false, Error = errorMessage };
        }
    }
}
