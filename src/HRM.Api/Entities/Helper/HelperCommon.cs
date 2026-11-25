#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Helper
{
    public static class HelperCommon
    {
        public static bool IsEmpty(this string aValue) => string.IsNullOrEmpty(aValue);

        public static bool IsEmpty<T>(this List<T> aListValue) => aListValue == null || aListValue.Count == 0;

        public static bool IsNotEmpty(this string aValue) => !string.IsNullOrEmpty(aValue);

        public static bool IsNotEmpty<T>(this List<T> aListValue) => aListValue is { Count: > 0 };

        public static Dictionary<Tkey, Tvalue> ToDict<Tkey, Tvalue>(this List<Tvalue> aListValue, Func<Tvalue, Tkey> keySelector)
        {
            var result = new Dictionary<Tkey, Tvalue>();
            if (aListValue.IsEmpty()) return result;

            foreach (var item in aListValue)
            {
                var key = keySelector(item);
                if (key != null)
                {
                    result.TryAdd(key, item);
                }
            }

            return result;
        }

        public static Dictionary<Tkey, List<Tvalue>> ToGroup<Tkey, Tvalue>(this List<Tvalue> aListValue, Func<Tvalue, Tkey> keySelector)
        {
            var result = new Dictionary<Tkey, List<Tvalue>>();
            if (aListValue.IsEmpty()) return result;

            foreach (var item in aListValue)
            {
                var key = keySelector(item);
                if (key == null) continue;

                if (!result.TryGetValue(key, out var group))
                {
                    group = new List<Tvalue>();
                    result[key] = group;
                }
                group.Add(item);
            }

            return result;
        }

        public static TResult GetValueFromDic<Tkey, Tvalue, TResult>(this Tkey aKey, Dictionary<Tkey, Tvalue> aToDict, Func<Tvalue, TResult> selector)
        {
            if (aToDict == null || aKey == null || !aToDict.TryGetValue(aKey, out var model) || model == null)
            {
                return default;
            }

            var result = selector(model);
            return result != null ? result : default;
        }

        public static Tvalue GetValueFromDic<Tkey, Tvalue>(this Tkey aKey, Dictionary<Tkey, Tvalue> aToDict) where Tvalue : class, new()
            => aToDict != null && aKey != null
                ? (aToDict.TryGetValue(aKey, out var value) ? value : new Tvalue())
                : null;

        public static List<Func<T, bool>> CreateBuilder<T>() where T : class => new();

        public static List<Func<T, bool>> CreateBuilder<T>(Func<T, bool> func) where T : class => new() { func };

        public static void And<T>(this List<Func<T, bool>> aListValue, Func<T, bool> func) => aListValue.Add(func);

        public static List<T> Build<T>(this List<T> aListValue, List<Func<T, bool>> aListFunc) where T : class, new()
        {
            if (aListValue.IsEmpty()) return aListValue?.ToList() ?? new List<T>();
            if (aListFunc.IsEmpty()) return aListValue;

            return aListFunc
                .Aggregate((IEnumerable<T>)aListValue, (current, predicate) => current.Where(predicate))
                .ToList();
        }

        public static bool In<T>(this T aValue, List<T> aListValue)
            => aListValue.IsNotEmpty() && aListValue.Contains(aValue);

        public static bool NotIn<T>(this T aValue, List<T> aListValue)
            => aListValue.IsNotEmpty() && !aListValue.Contains(aValue);

        public static string Join(this List<string> aListValue, string separator)
            => aListValue.IsEmpty() ? string.Empty : string.Join(separator, aListValue);

        public static IEnumerable<string> Split(this string input,
                                                string separator,
                                                char escapeCharacter)
        {
            int startOfSegment = 0;
            int index = 0;
            while (index < input.Length)
            {
                index = input.IndexOf(separator, index);
                if (index > 0 && input[index - 1] == escapeCharacter)
                {
                    index += separator.Length;
                    continue;
                }
                if (index == -1)
                {
                    break;
                }
                yield return input.Substring(startOfSegment, index - startOfSegment);
                index += separator.Length;
                startOfSegment = index;
            }
            yield return input.Substring(startOfSegment);
        }

        public static List<T> ClearIsEmpty<T>(this List<T> aListValue)
            => aListValue.IsEmpty() ? new List<T>() : aListValue.Where(item => item != null).ToList();
    }
}

