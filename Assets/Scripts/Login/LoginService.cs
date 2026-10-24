using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class LoginService
{
    const string PATH_LOGIN = "/auth/login";
    const string PATH_GUEST = "/auth/guest";
    const string PATH_REGISTER = "/auth/register";
    const string PATH_REFRESH = "/auth/refresh";
    const string PATH_LOGOUT = "/auth/logout";
    const string JSON_TYPE = "application/json";
    const int TIMEOUT_SECONDS = 15;
    const long STATUS_OK_MIN = 200;
    const long STATUS_OK_MAX = 299;

    const string FORGOT_MESSAGE = "this is not available yet";
    const string NETWORK_MESSAGE = "Could not reach the server. Check your connection and try again.";
    const string GENERIC_MESSAGE = "Something went wrong. Please try again.";
    const string INVALID_CREDENTIALS_MESSAGE = "Wrong email, username or password.";
    const string RATE_LIMITED_MESSAGE = "Too many attempts. Please wait a moment and try again.";
    const string VALIDATION_FAILED_MESSAGE = "Please check what you entered and try again.";
    const string GUEST_NAME_INVALID_MESSAGE = "That guest name is not allowed. Try another one.";
    const string USERNAME_TAKEN_MESSAGE = "That username is already taken.";
    const string EMAIL_TAKEN_MESSAGE = "That email is already registered.";
    const string TOKEN_EXPIRED_MESSAGE = "Your session has expired. Please sign in again.";
    const string INVALID_TOKEN_MESSAGE = "Your session is no longer valid. Please sign in again.";

    enum Flow
    {
        Login,
        Register,
        Guest
    }

    [Serializable]
    public class LoginBody
    {
        public string identifier;
        public string password;
    }

    [Serializable]
    public class RegisterBody
    {
        public string username;
        public string email;
        public string password;
    }

    [Serializable]
    public class GuestBody
    {
        public string username;
    }

    [Serializable]
    public class TokenBody
    {
        public string refresh_token;
    }

    [Serializable]
    public class UserReply
    {
        public string username;
    }

    [Serializable]
    public class SessionReply
    {
        public string access_token;
        public string refresh_token;
        public UserReply user;
    }

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

    sealed class Reply
    {
        public bool network_failure;
        public long status;
        public string body;
    }

    public static void login(string identifier, string password, Action on_ok, Action<string> on_fail)
    {
        var body = JsonUtility.ToJson(new LoginBody { identifier = (identifier ?? string.Empty).Trim(), password = password ?? string.Empty });
        post_session(PATH_LOGIN, body, Flow.Login, string.Empty, on_ok, on_fail);
    }

    public static void guest(string name, Action on_ok, Action<string> on_fail)
    {
        var clean = (name ?? string.Empty).Trim();
        var body = JsonUtility.ToJson(new GuestBody { username = clean });
        post_session(PATH_GUEST, body, Flow.Guest, clean, on_ok, on_fail);
    }

    public static void register(string username, string email, string password, Action on_ok, Action<string> on_fail)
    {
        var clean_name = (username ?? string.Empty).Trim();
        var body = JsonUtility.ToJson(new RegisterBody { username = clean_name, email = (email ?? string.Empty).Trim(), password = password ?? string.Empty });
        post_session(PATH_REGISTER, body, Flow.Register, clean_name, on_ok, on_fail);
    }

    public static void forgot(string email, Action on_ok, Action<string> on_fail)
    {
        report(on_fail, FORGOT_MESSAGE);
    }

    public static void restore(Action on_ok, Action<string> on_fail)
    {
        var token = LoginStore.load();
        if (string.IsNullOrEmpty(token))
        {
            fail_restore(on_fail, GENERIC_MESSAGE);
            return;
        }

        send(PATH_REFRESH, JsonUtility.ToJson(new TokenBody { refresh_token = token }), reply =>
        {
            if (reply.network_failure)
            {
                fail_restore(on_fail, NETWORK_MESSAGE);
                return;
            }

            if (!is_success(reply))
            {
                fail_restore(on_fail, friendly(error_code(reply)));
                return;
            }

            SessionReply session;
            if (!read_session(reply, out session) || string.IsNullOrEmpty(session.refresh_token))
            {
                fail_restore(on_fail, GENERIC_MESSAGE);
                return;
            }

            LoginStore.save(session.refresh_token);
            LoginSession.set(session_name(session, string.Empty), session.access_token, session.refresh_token, false);
            if (on_ok != null) on_ok();
        });
    }

    public static void sign_out()
    {
        sign_out(null);
    }

    public static void sign_out(Action on_done)
    {
        var token = LoginSession.refresh_token;
        if (string.IsNullOrEmpty(token)) token = LoginStore.load();

        if (string.IsNullOrEmpty(token))
        {
            clear_local(on_done);
            return;
        }

        send(PATH_LOGOUT, JsonUtility.ToJson(new TokenBody { refresh_token = token }), reply => clear_local(on_done));
    }

    static void post_session(string path, string body, Flow flow, string fallback_name, Action on_ok, Action<string> on_fail)
    {
        send(path, body, reply =>
        {
            if (reply.network_failure)
            {
                LoginFeedback.toast_retry(NETWORK_MESSAGE, () => retry(path, body, flow, fallback_name, on_ok, on_fail));
                report(on_fail, NETWORK_MESSAGE);
                return;
            }

            if (!is_success(reply))
            {
                report(on_fail, friendly(error_code(reply)));
                return;
            }

            SessionReply session;
            if (!read_session(reply, out session))
            {
                report(on_fail, GENERIC_MESSAGE);
                return;
            }

            var name = session_name(session, fallback_name);
            if (flow == Flow.Guest)
            {
                LoginSession.set(name, session.access_token, string.Empty, true);
            }
            else
            {
                if (string.IsNullOrEmpty(session.refresh_token))
                {
                    report(on_fail, GENERIC_MESSAGE);
                    return;
                }

                LoginStore.save(session.refresh_token);
                LoginSession.set(name, session.access_token, session.refresh_token, false);
            }

            if (on_ok != null) on_ok();
        });
    }

    static void retry(string path, string body, Flow flow, string fallback_name, Action on_ok, Action<string> on_fail)
    {
        if (LoginState.is_busy) return;

        LoginState.set_busy(true);
        post_session(path, body, flow, fallback_name,
            () =>
            {
                LoginState.set_busy(false);
                if (on_ok != null) on_ok();
            },
            reason =>
            {
                LoginState.set_busy(false);
                if (on_fail != null) on_fail(reason);
            });
    }

    static void send(string path, string json, Action<Reply> on_done)
    {
        var request = new UnityWebRequest(ApiConfig.BASE_URL + path, UnityWebRequest.kHttpVerbPOST);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", JSON_TYPE);
        request.SetRequestHeader("Accept", JSON_TYPE);
        request.timeout = TIMEOUT_SECONDS;

        var operation = request.SendWebRequest();
        operation.completed += finished =>
        {
            var reply = new Reply
            {
                network_failure = request.result == UnityWebRequest.Result.ConnectionError,
                status = request.responseCode,
                body = request.downloadHandler != null ? request.downloadHandler.text : string.Empty
            };
            request.Dispose();
            on_done(reply);
        };
    }

    static bool is_success(Reply reply)
    {
        return reply.status >= STATUS_OK_MIN && reply.status <= STATUS_OK_MAX;
    }

    static bool read_session(Reply reply, out SessionReply session)
    {
        session = null;
        try
        {
            session = JsonUtility.FromJson<SessionReply>(reply.body);
        }
        catch (Exception)
        {
            return false;
        }

        return session != null && !string.IsNullOrEmpty(session.access_token);
    }

    static string session_name(SessionReply session, string fallback_name)
    {
        var from_server = session.user != null ? session.user.username : null;
        return string.IsNullOrEmpty(from_server) ? fallback_name : from_server;
    }

    static string error_code(Reply reply)
    {
        if (string.IsNullOrEmpty(reply.body)) return string.Empty;

        try
        {
            var parsed = JsonUtility.FromJson<ErrorReply>(reply.body);
            if (parsed == null || parsed.error == null || parsed.error.code == null) return string.Empty;
            return parsed.error.code;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    static string friendly(string code)
    {
        switch (code)
        {
            case "INVALID_CREDENTIALS": return INVALID_CREDENTIALS_MESSAGE;
            case "RATE_LIMITED": return RATE_LIMITED_MESSAGE;
            case "VALIDATION_FAILED": return VALIDATION_FAILED_MESSAGE;
            case "GUEST_NAME_INVALID": return GUEST_NAME_INVALID_MESSAGE;
            case "USERNAME_TAKEN": return USERNAME_TAKEN_MESSAGE;
            case "EMAIL_TAKEN": return EMAIL_TAKEN_MESSAGE;
            case "TOKEN_EXPIRED": return TOKEN_EXPIRED_MESSAGE;
            case "INVALID_TOKEN": return INVALID_TOKEN_MESSAGE;
            default: return GENERIC_MESSAGE;
        }
    }

    static void report(Action<string> on_fail, string message)
    {
        if (on_fail != null) on_fail(message);
    }

    static void fail_restore(Action<string> on_fail, string message)
    {
        LoginStore.clear();
        LoginSession.clear();
        report(on_fail, message);
    }

    static void clear_local(Action on_done)
    {
        LoginStore.clear();
        LoginSession.clear();
        if (on_done != null) on_done();
    }
}
