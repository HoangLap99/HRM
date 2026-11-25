using System.Net.Http.Headers;
using System.Text;
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using HRM.Api.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HRM.Api.ExternalServices.SmartEos;

/// <summary>
/// Lớp cơ sở dùng chung cho tất cả SmartEOS HTTP Clients, quản lý Auth header, Cookie và serialize/deserialize JSON.
/// </summary>
public abstract class SmartEosHttpClientBase
{
    protected readonly HttpClient HttpClient;
    protected readonly ICurrentUserContext CurrentUserContext;
    protected readonly HrmOptions Options;
    protected readonly ILogger Logger;

    protected SmartEosHttpClientBase(
        HttpClient httpClient,
        ICurrentUserContext currentUserContext,
        IOptions<HrmOptions> options,
        ILogger logger)
    {
        HttpClient = httpClient;
        CurrentUserContext = currentUserContext;
        Options = options.Value;
        Logger = logger;
    }

    protected HttpRequestMessage CreateRequest(
        HttpMethod method,
        string relativeUrl,
        object? payload = null,
        string? overrideAuthorization = null,
        IReadOnlyList<clsCookie>? overrideCookies = null,
        bool includeAuth = true)
    {
        var normalizedUrl = relativeUrl.TrimStart('/');
        var request = new HttpRequestMessage(method, normalizedUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (includeAuth)
        {
            AttachAuthentication(request, overrideAuthorization, overrideCookies);
        }

        if (payload is not null)
        {
            var json = payload as string ?? JsonConvert.SerializeObject(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    protected async Task<string?> SendAndReadStringAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        try
        {
            using var response = await HttpClient.SendAsync(request, ct);
            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning(
                    "SmartEOS HTTP call {Method} {Uri} failed with status {StatusCode}: {Response}",
                    request.Method,
                    request.RequestUri,
                    (int)response.StatusCode,
                    content);
                return null;
            }

            return content;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "SmartEOS HTTP call {Method} {Uri} threw exception.", request.Method, request.RequestUri);
            return null;
        }
    }

    protected async Task<T?> GetJsonAsync<T>(
        string relativeUrl,
        string? overrideAuthorization = null,
        IReadOnlyList<clsCookie>? overrideCookies = null,
        bool includeAuth = true,
        CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Get, relativeUrl, null, overrideAuthorization, overrideCookies, includeAuth);
        var content = await SendAndReadStringAsync(request, ct);
        return DeserializeOrDefault<T>(content);
    }

    protected async Task<TResponse?> PostJsonAsync<TResponse>(
        string relativeUrl,
        object? payload,
        string? overrideAuthorization = null,
        IReadOnlyList<clsCookie>? overrideCookies = null,
        bool includeAuth = true,
        CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Post, relativeUrl, payload, overrideAuthorization, overrideCookies, includeAuth);
        var content = await SendAndReadStringAsync(request, ct);
        return DeserializeOrDefault<TResponse>(content);
    }

    protected T? DeserializeOrDefault<T>(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(content);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to deserialize SmartEOS response to {TargetType}.", typeof(T).Name);
            return default;
        }
    }

    private void AttachAuthentication(
        HttpRequestMessage request,
        string? overrideAuthorization,
        IReadOnlyList<clsCookie>? overrideCookies)
    {
        var authorization = !string.IsNullOrWhiteSpace(overrideAuthorization)
            ? overrideAuthorization
            : CurrentUserContext.Authorization;

        if (!string.IsNullOrWhiteSpace(authorization))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authorization);
            return;
        }

        var cookies = overrideCookies ?? CurrentUserContext.Cookies;
        if (cookies is null || cookies.Count == 0)
        {
            return;
        }

        var cookieHeader = string.Join("; ",
            cookies
                .Where(c => !string.IsNullOrWhiteSpace(c.name) && !(c.value?.Contains(',') ?? false))
                .Select(c => $"{c.name}={c.value}"));

        if (!string.IsNullOrWhiteSpace(cookieHeader))
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        }
    }
}
