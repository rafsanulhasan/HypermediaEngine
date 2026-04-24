using HypermediaEngine.Responses;

using LanguageExt;
using LanguageExt.Common;

using Microsoft.AspNetCore.Http;

namespace HypermediaEngine.Abstractions;

/// <summary>
/// Interface for generating hypermedia links.
/// </summary>
public interface ILinkGenerationService
{
    /// <summary>
    /// The Http Context
    /// </summary>
    Option<HttpContext> HttpContext { get; }

    /// <summary>
    /// Generates a link based on the provided parameters.
    /// </summary>
    /// <param name="endpointName">The name of the route to generate the link for.</param>
    /// <param name="httpMethod">Http method for the generated link.</param>
    /// <param name="routeValues">An object containing the route values to be used in link generation.</param>
    /// <param name="type">The type of the resource being linked to.</param>
    /// <param name="rel">The relationship of the linked resource.</param>
    /// <param name="title">The title of the linked resource.</param>
    /// <returns>The generated link.</returns>
    OptionalResult<HypermediaLink> GenerateLink(
        string endpointName,
        string httpMethod,
        object? routeValues = null,
        string? type = null,
        string? rel = null,
        string? title = null);

    /// <summary>
    /// Generates a GET link based on the provided parameters.
    /// </summary>
    /// <param name="endpointName">The name of the route to generate the link for.</param>
    /// <param name="routeValues">An object containing the route values to be used in link generation.</param>
    /// <param name="rel">The relationship of the linked resource.</param>
    /// <param name="title">The title of the linked resource.</param>
    /// <returns>The generated link.</returns>
    OptionalResult<HypermediaLink> GenerateGetLink<T>(
        string endpointName,
        object? routeValues,
        string? rel = null,
        string? title = null)
    {
        return GenerateLink(endpointName, HttpMethods.Get, routeValues, typeof(T).Name, rel, title);
    }

    /// <summary>
    /// Generates a POST link based on the provided parameters.
    /// </summary>
    /// <param name="endpointName">The name of the route to generate the link for.</param>
    /// <param name="routeValues">An object containing the route values to be used in link generation.</param>
    /// <param name="rel">The relationship of the linked resource.</param>
    /// <param name="title">The title of the linked resource.</param>
    /// <returns>The generated link.</returns>
    OptionalResult<HypermediaLink> GeneratePostLink<T>(
        string endpointName,
        object routeValues,
        string? rel = null,
        string? title = null)
    {
        return GenerateLink(endpointName, HttpMethods.Post, routeValues, typeof(T).Name, rel, title);
    }

    /// <summary>
    /// Generates a PATCH link based on the provided parameters.
    /// </summary>
    /// <param name="endpointName">The name of the route to generate the link for.</param>
    /// <param name="routeValues">An object containing the route values to be used in link generation.</param>
    /// <param name="rel">The relationship of the linked resource.</param>
    /// <param name="title">The title of the linked resource.</param>
    /// <returns>The generated link.</returns>
    OptionalResult<HypermediaLink> GeneratePatchLink<T>(
        string endpointName,
        object routeValues,
        string? rel = null,
        string? title = null)
    {
        return GenerateLink(endpointName, HttpMethods.Patch, routeValues, typeof(T).Name, rel, title);
    }

    /// <summary>
    /// Generates a Delete link based on the provided parameters.
    /// </summary>
    /// <param name="endpointName">The name of the route to generate the link for.</param>
    /// <param name="routeValues">An object containing the route values to be used in link generation.</param>
    /// <param name="rel">The relationship of the linked resource.</param>
    /// <param name="title">The title of the linked resource.</param>
    /// <returns>The generated link.</returns>
    OptionalResult<HypermediaLink> GenerateDeleteLink(
        string endpointName,
        object routeValues,
        string? rel = null,
        string? title = null)
    {
        return GenerateLink(endpointName, HttpMethods.Delete, routeValues, null, rel, title);
    }
}
