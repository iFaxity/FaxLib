using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;

// Part of FaxLib since v 1.3
namespace FaxLib.Web {
    /// <summary>
    /// Used to convert from/to prefixes
    /// </summary>
    public enum UnitPrefix {
        Kilo = 1000,
        Mega = 1000000,
        Giga = 1000000000
    };
    /// <summary>
    /// Used to specify which Http request method to use
    /// </summary>
    public enum HttpRequestMethod {
        GET,
        PUT,
        POST,
        DELETE
    }

    /// <summary>
    /// Class for all Web functions inside of FaxLib
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class Web {
        /// <summary>
        /// Sends a HTTP Request with the chosen request method
        /// </summary>
        /// <param name="url">Url to send request to</param>
        /// <param name="method">HTTP Request Method to use</param>
        /// <param name="data">Query string data to send</param>
        /// <param name="count">How many times to try the connection before giving up</param>
        public static string SendRequest(string url, HttpRequestMethod method, Dictionary<string, string> data = null, int count = 3) {
            // Try sending n times
            for(int i = 0; i < count; i++) {
                switch(method) {
                    case HttpRequestMethod.POST:
                        return SendPost(url, GetRequestString(data));
                    case HttpRequestMethod.GET:
                        return SendGet(url + "?" + GetRequestString(data));
                    case HttpRequestMethod.PUT:
                        return SendPut(url, GetRequestString(data));
                    case HttpRequestMethod.DELETE:
                        return SendDelete(url);
                }
            }
            throw new Exception("Request failed to send. Unknown error. Please check the parameters for errors.");
        }
        /// <summary>
        /// Gets a files size from an url in bytes
        /// </summary>
        /// <param name="url">Url to connect to</param>
        public long GetFileSize(string url) {
            using(var response = WebRequest.Create(url).GetResponse())
                return response.ContentLength;
        }

        // Request types supported
        static string SendPost(string url, string query) {
            var request = WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            var post = Encoding.ASCII.GetBytes(query);
            request.ContentLength = post.Length;

            using(var stream = request.GetRequestStream())
                stream.Write(post, 0, post.Length);

            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                using(var stream = new StreamReader(response.GetResponseStream()))
                    return stream.ReadToEnd();
            }
        }
        static string SendGet(string url) {
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                using(var stream = new StreamReader(response.GetResponseStream()))
                    return stream.ReadToEnd();
            }
        }
        static string SendPut(string url, string query) {
            var request = WebRequest.Create(url);
            request.Method = "PUT";

            var post = Encoding.ASCII.GetBytes(query);
            using(var stream = request.GetRequestStream())
                stream.Write(post, 0, post.Length);

            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                using(var stream = new StreamReader(response.GetResponseStream()))
                    return stream.ReadToEnd();
            }
        }
        static string SendDelete(string url) {
            var request = WebRequest.Create(url);
            request.Method = "DELETE";

            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                using(var stream = new StreamReader(response.GetResponseStream()))
                    return stream.ReadToEnd();
            }
        }
        // Used for parsing Dictionary to Query string
        static string GetRequestString(Dictionary<string, string> dict) {
            string req = "";
            foreach(var pair in dict)
                req += pair.Key + "=" + pair.Value + "&";
            return req.Substring(0, req.Length - 1);
        }
    }
}
