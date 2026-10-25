using System;
using UnityEngine;

public sealed class ApiError
{
    public const string CODE_NETWORK = "NETWORK_FAILURE";
    public const string CODE_BAD_REPLY = "BAD_REPLY";
    public const string CODE_UNKNOWN = "UNKNOWN";

    const string VALIDATION_FAILED_MESSAGE = "Please check what you entered and try again.";
    const string UNAUTHENTICATED_MESSAGE = "Please sign in to continue.";
    const string TOKEN_EXPIRED_MESSAGE = "Your session has expired. Please sign in again.";
    const string FORBIDDEN_MESSAGE = "You are not allowed to do that.";
    const string NOT_FOUND_MESSAGE = "We could not find what you asked for.";
    const string CONFLICT_MESSAGE = "That action conflicts with the current state. Please try again.";
    const string RATE_LIMITED_MESSAGE = "Too many attempts. Please wait a moment and try again.";
    const string NETWORK_MESSAGE = "Could not reach the server. Check your connection and try again.";
    const string GENERIC_MESSAGE = "Something went wrong. Please try again.";

    [Serializable]
    public class ErrorInfo
    {
        public string code;
    }

    [Serializable]
    public class ErrorReply
    {
        public ErrorInfo error;
    }

    public string code { get; private set; }
    public long status { get; private set; }
    public string message { get; private set; }
    public bool session_ended { get; internal set; }

    public bool is_network_failure
    {
        get { return code == CODE_NETWORK; }
    }

    ApiError(string code, long status)
    {
        this.code = string.IsNullOrEmpty(code) ? CODE_UNKNOWN : code;
        this.status = status;
        message = friendly(this.code);
    }

    public static ApiError network()
    {
        return new ApiError(CODE_NETWORK, 0);
    }

    public static ApiError bad_reply()
    {
        return new ApiError(CODE_BAD_REPLY, 0);
    }

    public static ApiError from_reply(long status, string body)
    {
        return new ApiError(read_code(body), status);
    }

    public static string friendly(string code)
    {
        switch (code)
        {
            case "VALIDATION_FAILED": return VALIDATION_FAILED_MESSAGE;
            case "UNAUTHENTICATED":
            case "INVALID_TOKEN": return UNAUTHENTICATED_MESSAGE;
            case "TOKEN_EXPIRED": return TOKEN_EXPIRED_MESSAGE;
            case "FORBIDDEN": return FORBIDDEN_MESSAGE;
            case "NOT_FOUND": return NOT_FOUND_MESSAGE;
            case "CONFLICT": return CONFLICT_MESSAGE;
            case "RATE_LIMITED": return RATE_LIMITED_MESSAGE;
            case CODE_NETWORK: return NETWORK_MESSAGE;
            default: return GENERIC_MESSAGE;
        }
    }

    internal static bool is_session_code(string code)
    {
        return code == "UNAUTHENTICATED" || code == "TOKEN_EXPIRED" || code == "INVALID_TOKEN";
    }

    internal static string read_code(string body)
    {
        if (string.IsNullOrEmpty(body)) return string.Empty;

        try
        {
            var parsed = JsonUtility.FromJson<ErrorReply>(body);
            if (parsed == null || parsed.error == null || parsed.error.code == null) return string.Empty;
            return parsed.error.code;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
