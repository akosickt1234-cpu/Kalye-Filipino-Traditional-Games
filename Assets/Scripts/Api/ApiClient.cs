using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class ApiClient
{
    const string JSON_TYPE = "application/json";
    const string BEARER_PREFIX = "Bearer ";
    const string EMPTY_BODY = "{}";
    const string PATH_PREFIX = "/";
    const string PATH_DOUBLE_PREFIX = "//";
    const long STATUS_OK_MIN = 200;
    const long STATUS_OK_MAX = 299;

    sealed class Call
    {
        public string verb;
        public string path;
        public string json;
        public int timeout;
        public string token_used;
        public Action<string> on_ok;
        public Action<ApiError> on_fail;
    }

    sealed class Reply
    {
        public bool network_failure;
        public long status;
        public string body;
    }

    static readonly List<Action<bool>> refresh_waiting = new List<Action<bool>>();
    static bool refresh_running;

    public static void get<T>(string path, Action<T> on_ok, Action<ApiError> on_fail) where T : class
    {
        send<T>(UnityWebRequest.kHttpVerbGET, path, null, ApiConfig.TIMEOUT_SECONDS, on_ok, on_fail);
    }

    public static void get_long_poll<T>(string path, Action<T> on_ok, Action<ApiError> on_fail) where T : class
    {
        send<T>(UnityWebRequest.kHttpVerbGET, path, null, ApiConfig.LONG_POLL_TIMEOUT_SECONDS, on_ok, on_fail);
    }

    public static void post<T>(string path, object body, Action<T> on_ok, Action<ApiError> on_fail) where T : class
    {
        send<T>(UnityWebRequest.kHttpVerbPOST, path, body_json(body), ApiConfig.TIMEOUT_SECONDS, on_ok, on_fail);
    }

    public static void patch<T>(string path, object body, Action<T> on_ok, Action<ApiError> on_fail) where T : class
    {
        send<T>("PATCH", path, body_json(body), ApiConfig.TIMEOUT_SECONDS, on_ok, on_fail);
    }

    public static void delete(string path, Action on_ok, Action<ApiError> on_fail)
    {
        send_raw(UnityWebRequest.kHttpVerbDELETE, path, null, ApiConfig.TIMEOUT_SECONDS, text =>
        {
            if (on_ok != null) on_ok();
        }, on_fail);
    }

    static void send<T>(string verb, string path, string json, int timeout, Action<T> on_ok, Action<ApiError> on_fail) where T : class
    {
        send_raw(verb, path, json, timeout, text =>
        {
            var parsed = parse<T>(text);
            if (parsed == null)
            {
                report(on_fail, ApiError.bad_reply());
                return;
            }

            if (on_ok != null) on_ok(parsed);
        }, on_fail);
    }

    static void send_raw(string verb, string path, string json, int timeout, Action<string> on_ok, Action<ApiError> on_fail)
    {
        if (!valid_path(path))
        {
            report(on_fail, ApiError.bad_reply());
            return;
        }

        var call = new Call { verb = verb, path = path, json = json, timeout = timeout, on_ok = on_ok, on_fail = on_fail };
        ApiRunner.run(execute(call));
    }

    static IEnumerator execute(Call call)
    {
        var reply = new Reply();
        yield return transmit(call, reply);

        if (is_session_failure(reply))
        {
            var refreshed = false;
            var finished = false;
            refresh_session(call.token_used, ok =>
            {
                refreshed = ok;
                finished = true;
            });

            while (!finished) yield return null;

            if (!refreshed)
            {
                var ended = ApiError.from_reply(reply.status, reply.body);
                ended.session_ended = true;
                report(call.on_fail, ended);
                yield break;
            }

            reply = new Reply();
            yield return transmit(call, reply);

            if (is_session_failure(reply))
            {
                lose_session();
                var ended = ApiError.from_reply(reply.status, reply.body);
                ended.session_ended = true;
                report(call.on_fail, ended);
                yield break;
            }
        }

        complete(call, reply);
    }

    static IEnumerator transmit(Call call, Reply reply)
    {
        call.token_used = LoginSession.access_token;

        using (var request = new UnityWebRequest(ApiConfig.BASE_URL + call.path, call.verb))
        {
            if (call.json != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(call.json));
                request.SetRequestHeader("Content-Type", JSON_TYPE);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Accept", JSON_TYPE);
            if (!string.IsNullOrEmpty(call.token_used)) request.SetRequestHeader("Authorization", BEARER_PREFIX + call.token_used);
            request.timeout = call.timeout;

            yield return request.SendWebRequest();

            reply.network_failure = request.result == UnityWebRequest.Result.ConnectionError;
            reply.status = request.responseCode;
            reply.body = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
        }
    }

    static void complete(Call call, Reply reply)
    {
        if (reply.network_failure)
        {
            report(call.on_fail, ApiError.network());
            return;
        }

        if (is_success(reply))
        {
            if (call.on_ok != null) call.on_ok(reply.body ?? string.Empty);
            return;
        }

        report(call.on_fail, ApiError.from_reply(reply.status, reply.body));
    }

    static void refresh_session(string token_used, Action<bool> done)
    {
        var current = LoginSession.access_token;
        if (!string.IsNullOrEmpty(current) && current != token_used)
        {
            done(true);
            return;
        }

        refresh_waiting.Add(done);
        if (refresh_running) return;

        if (LoginSession.is_guest || string.IsNullOrEmpty(LoginSession.refresh_token))
        {
            finish_refresh(false);
            return;
        }

        refresh_running = true;
        LoginService.restore(() => finish_refresh(true), reason => finish_refresh(false));
    }

    static void finish_refresh(bool ok)
    {
        refresh_running = false;
        var pending = new List<Action<bool>>(refresh_waiting);
        refresh_waiting.Clear();

        if (!ok) lose_session();
        for (var i = 0; i < pending.Count; i++) pending[i](ok);
    }

    static void lose_session()
    {
        LoginService.sign_out(() =>
        {
            if (!LoginOverlay.is_open) LoginOverlay.open(null);
        });
    }

    static bool is_session_failure(Reply reply)
    {
        if (reply.network_failure || is_success(reply)) return false;
        return ApiError.is_session_code(ApiError.read_code(reply.body));
    }

    static bool is_success(Reply reply)
    {
        return reply.status >= STATUS_OK_MIN && reply.status <= STATUS_OK_MAX;
    }

    static bool valid_path(string path)
    {
        return !string.IsNullOrEmpty(path) && path.StartsWith(PATH_PREFIX, StringComparison.Ordinal) && !path.StartsWith(PATH_DOUBLE_PREFIX, StringComparison.Ordinal);
    }

    static string body_json(object body)
    {
        return body == null ? EMPTY_BODY : JsonUtility.ToJson(body);
    }

    static T parse<T>(string text) where T : class
    {
        if (string.IsNullOrEmpty(text)) return null;

        try
        {
            return JsonUtility.FromJson<T>(text);
        }
        catch (Exception)
        {
            return null;
        }
    }

    static void report(Action<ApiError> on_fail, ApiError error)
    {
        if (on_fail != null) on_fail(error);
    }
}
