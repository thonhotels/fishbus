namespace Thon.Hotels.FishBus;

public class HandlerResult
{
    public ResultCode Code { get; }
    public string Message { get; }

    public enum ResultCode { SUCCESS, FAILED, ABORT }
    public static HandlerResult Success() => new HandlerResult(ResultCode.SUCCESS);

    public static HandlerResult Failed() => new HandlerResult(ResultCode.FAILED);

    public static HandlerResult Abort(string message) => new HandlerResult(ResultCode.ABORT, message);

    public static bool IsSuccess(HandlerResult r) => r.Code == ResultCode.SUCCESS;
    public static bool IsUnsuccessful(HandlerResult r) => HandlerResult.IsFailed(r) || HandlerResult.IsAbort(r);
    public static bool IsFailed(HandlerResult r) => r.Code == ResultCode.FAILED;
    public static bool IsAbort(HandlerResult r) => r.Code == ResultCode.ABORT;

    private HandlerResult(ResultCode code, string message = "")
    {
        Code = code;
        Message = message;
    }
}