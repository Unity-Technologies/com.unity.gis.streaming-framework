using System;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    internal static class PathUtility
    {
        private static async Task<T> Download<T>(Uri uri, Func<DownloadHandler, T> callback)
        {
            Assert.IsFalse(uri is null, "Invalid Uri");
            Assert.IsTrue(uri.IsAbsoluteUri, "URI must be absolute");

            using UnityWebRequest webRequest = UnityWebRequest.Get(uri);
            AsyncOperation op = webRequest.SendWebRequest();

            while (!op.isDone)
            {
                await Task.Yield();
            }

            Assert.IsTrue(webRequest.isDone);

            return webRequest.result switch
            {
                UnityWebRequest.Result.Success => callback(webRequest.downloadHandler),
                _ => throw new IOException($"Failed to download {uri} due to: {webRequest.error}")
            };
        }

        internal static async Task<byte[]> DownloadFileData(Uri uri)
        {
            return await Download(uri, handler => handler.data);
        }

        internal static async Task<string> DownloadFileText(Uri uri)
        {
            return await Download(uri, handler => handler.text);
        }

        /// <summary>
        /// Get URI that is potentially relative to another URI
        /// </summary>
        /// <param name="uri">Absolute or relative URI</param>
        /// <param name="baseUri">Base URI</param>
        /// <returns>Absolute URI that is potentially relative to baseUri</returns>
        internal static Uri StringToUri(string uri, Uri baseUri)
        {
            if (baseUri is null)
                return StringToUri(uri);
            
            return Uri.IsWellFormedUriString(uri, UriKind.Absolute)
                ? new Uri(uri)
                : new Uri(baseUri, uri);
        }

        /// <summary>
        /// Convert string into URI, resolving relative paths in relation to <see cref="Application.streamingAssetsPath"/>.
        /// </summary>
        /// <param name="uri">The URI string.</param>
        /// <returns></returns>
        internal static Uri StringToUri(string uri)
        {
            // If uri is an absolute path
            if (Uri.TryCreate(uri, UriKind.Absolute, out Uri result))
            {
                return result;
            }

            // If uri is a relative path
            // Support path relative to the streaming assets folder on the target device:
            // https://docs.unity3d.com/Manual/StreamingAssets.html
            try
            {
                return new Uri(Path.Combine(Application.streamingAssetsPath, uri));
            }
            catch
            {
                throw new IOException($"Invalid relative directory: {uri} with base directory: {Application.streamingAssetsPath}");
            }
        }
    }
}
