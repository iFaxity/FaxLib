using System;
using System.Collections.Generic;
using System.Linq;

namespace FaxLib {
    /// <summary>
    /// Contains most basic functions from my library.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class ExtensionMethods {
        /// <summary>
        /// Split a string from a string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="splitter"></param>
        /// <returns></returns>
        public static string[] Split(this string input, string splitter) {
            return input.Split(splitter.ToArray(), StringSplitOptions.None);
        }

        /// <summary>
        /// Gets all indexes occurrences in a string 
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="match">Char to match</param>
        /// <returns></returns>
        public static int[] IndexesOf(this string input, char match) {
            return IndexesOf(input, match.ToString());
        }
        /// <summary>
        /// Gets all indexes occurrences in a string 
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="match">String to match</param>
        /// <returns></returns>
        public static int[] IndexesOf(this string input, string match) {
            var list = new List<int>();
            for(int i = input.IndexOf(match); i > -1; i = input.IndexOf(match, i + 1))
                list.Add(i);
            return list.ToArray();
        }
    }
}

