using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Axis.Apollo.Config.Test
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Fact]
        public void Test1()
        {
            var values = Enumerable
                .Range(0, 10000000)
                .ToArray();

            var stopwatch = Stopwatch.StartNew();
            var result = values.RegularBinarySearch(values[values.Length - 2]);
            stopwatch.Stop();
            stopwatch = Stopwatch.StartNew();
            result = values.CustomBinarySearch(values[values.Length - 2]);
            stopwatch.Stop();


            stopwatch = Stopwatch.StartNew();
            result = values.RegularBinarySearch(values[values.Length - 2]);
            stopwatch.Stop();
            output.WriteLine($"Regular binary search ({result.Result}): {stopwatch.Elapsed}");

            stopwatch = Stopwatch.StartNew();
            result = values.CustomBinarySearch(values[values.Length - 2]);
            stopwatch.Stop();
            output.WriteLine($"Custom binary search  ({result.Result}): {stopwatch.Elapsed}");


            output.WriteLine("\n");
            stopwatch = Stopwatch.StartNew();
            result = values.RegularBinarySearch(values[values.Length - 1]);
            stopwatch.Stop();
            output.WriteLine($"Regular binary search ({result.Result}): {stopwatch.Elapsed}");

            stopwatch = Stopwatch.StartNew();
            result = values.CustomBinarySearch(values[values.Length - 1]);
            stopwatch.Stop();
            output.WriteLine($"Custom binary search  ({result.Result}): {stopwatch.Elapsed}");


            output.WriteLine("\n");
            stopwatch = Stopwatch.StartNew();
            result = values.RegularBinarySearch(values[12021]);
            stopwatch.Stop();
            output.WriteLine($"Regular binary search ({result.Result}): {stopwatch.Elapsed}");

            stopwatch = Stopwatch.StartNew();
            result = values.CustomBinarySearch(values[12021]);
            stopwatch.Stop();
            output.WriteLine($"Custom binary search  ({result.Result}): {stopwatch.Elapsed}");


            output.WriteLine("\n");
            stopwatch = Stopwatch.StartNew();
            result = values.RegularBinarySearch(-1);
            stopwatch.Stop();
            output.WriteLine($"Regular binary search ({result.Result}): {stopwatch.Elapsed}");

            stopwatch = Stopwatch.StartNew();
            result = values.CustomBinarySearch(-1);
            stopwatch.Stop();
            output.WriteLine($"Custom binary search  ({result.Result}): {stopwatch.Elapsed}");


            output.WriteLine("\n");
            values[2000400] = 2000400;
            stopwatch = Stopwatch.StartNew();
            result = values.RegularBinarySearch(2000400);
            stopwatch.Stop();
            output.WriteLine($"Regular binary search ({result.Result}): {stopwatch.Elapsed}");

            stopwatch = Stopwatch.StartNew();
            result = values.CustomBinarySearch(2000400);
            stopwatch.Stop();
            output.WriteLine($"Custom binary search  ({result.Result}): {stopwatch.Elapsed}");


            output.WriteLine("\n");
            stopwatch = Stopwatch.StartNew();
            result = values.RegularBinarySearch(2000399);
            stopwatch.Stop();
            output.WriteLine($"Regular binary search ({result.Result}): {stopwatch.Elapsed}");

            stopwatch = Stopwatch.StartNew();
            result = values.CustomBinarySearch(2000399);
            stopwatch.Stop();
            output.WriteLine($"Custom binary search  ({result.Result}): {stopwatch.Elapsed}");
        }
    }

    public static class Ext
    {

        public static SearchResult<T> CustomBinarySearch<T>(this
            T[] values,
            T version)
            where T : IComparable<T>
            => CustomBinarySearch(new ArraySegment<T>(values), version);

        public static SearchResult<T> RegularBinarySearch<T>(this
            T[] values,
            T version)
            where T : IComparable<T>
            => RegularBinarySearch(new ArraySegment<T>(values), version);

        public static SearchResult<T> CustomBinarySearch<T>(this ArraySegment<T> values, T value)
        where T : IComparable<T>
        {
            if (values.Count == 1 && values[0].CompareTo(value) == 0)
                return new SearchResult<T>(values.Offset, values[0]);

            if (value.CompareTo(values.FirstValue()) < 0)
                return new SearchResult<T>(values.Offset, default);

            if (value.CompareTo(values.LastValue()) > 0)
                return new SearchResult<T>(values.Offset + values.Count, default);

            // split
            var (left, right) = values.Split(values.Count / 2);
            var leftLastValue = left.LastValue().CompareTo(value);
            var rightFirstValue = right.FirstValue().CompareTo(value);

            #region optimizations: try finding a match at the extremes of the left and right sub-arrays
            if (TryFindMatchAtExtremes(left, value, out var result))
                return result;

            if (TryFindMatchAtExtremes(right, value, out result))
                return result;
            #endregion

            // Because of the optimization above I can afford to avoid using '>=' and '<='
            if (leftLastValue > 0)
                return left.CustomBinarySearch(value);

            // Because of the optimization above I can afford to avoid using '>=' and '<='
            else if (rightFirstValue < 0)
                return right.CustomBinarySearch(value);

            //else if (leftLastValue < 0 && rightFirstValue > 0)
                return new SearchResult<T>(right.Offset, default);
        }

        public static SearchResult<T> RegularBinarySearch<T>(this ArraySegment<T> values, T value)
        where T : IComparable<T>
        {
            if (values.Count == 1)
                return new SearchResult<T>(values.Offset, values[0].CompareTo(value) == 0 ? value : default);

            var (left, right) = values.Split(values.Count / 2);
            var llv = left.LastValue().CompareTo(value);
            var rfv = right.FirstValue().CompareTo(value);

            if (llv >= 0)
                return left.RegularBinarySearch(value);

            else if (rfv <= 0)
                return right.RegularBinarySearch(value);

            //else if(llv < 0 && rfv > 0)
                return new SearchResult<T>(right.Offset, default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="version"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryFindMatchAtExtremes<T>(this ArraySegment<T> values, T version, out SearchResult<T> result)
        where T : IComparable<T>
        {
            result = default;

            if (values.Count == 0)
                return false;

            if (values.FirstValue().CompareTo(version) == 0)
            {
                result = new SearchResult<T>(values.Offset, values.FirstValue());
                return true;
            }

            if (values.LastValue().CompareTo(version) == 0)
            {
                result = new SearchResult<T>(values.Offset + values.Count - 1, values.LastValue());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Splits an array into 2 <see cref="ArraySegment{T}"/> instances, using the given index as a pivot. Note that the element at the pivot index will always
        /// become the first element in the "right" <see cref="ArraySegment{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="pivotIndex"></param>
        /// <returns></returns>
        public static (ArraySegment<T>, ArraySegment<T>) Split<T>(this T[] array, int pivotIndex)
        {
            ArraySegment<T> segment = array;
            return segment.Split(pivotIndex);
        }

        /// <summary>
        /// Splits an input into 2 <see cref="ArraySegment{T}"/> instances, using the given index as a pivot. Note that the element at the pivot index will always
        /// become the first element in the "right" <see cref="ArraySegment{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="pivotIndex"></param>
        /// <returns></returns>
        public static (ArraySegment<T>, ArraySegment<T>) Split<T>(this ArraySegment<T> segment, int pivotIndex)
        {
            return (segment.Slice(0, pivotIndex), segment.Slice(pivotIndex));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static ArraySegment<T> Slice<T>(this T[] array, int offset, int? length = null)
        {
            return new ArraySegment<T>(array, offset, length ?? array.Length - offset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static T LastValue<T>(this ArraySegment<T> segment) => segment[segment.Count - 1];

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static T FirstValue<T>(this ArraySegment<T> segment) => segment[0];
    }


    public struct SearchResult<T>
    {
        public int Index { get; }

        public T Result { get; }

        public SearchResult(int index, T value = default)
        {
            Index = index;
            Result = value;
        }
    }
}
