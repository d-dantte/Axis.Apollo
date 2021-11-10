using Axis.Apollo.Config.Models;
using Axis.Apollo.Config.Models.ValueTemplate;
using Axis.Luna.Common.Types.Basic;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Axis.Apollo.Config.Test")]

namespace Axis.Apollo.Config
{
    internal static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> SplitAtEvery<T>(this IEnumerable<T> enumerable, int interval)
        {
            var list = new List<T>();
            int count = 0;
            foreach(var t in enumerable)
            {
                list.Add(t);
                count++;

                if(count == interval)
                {
                    count = 0;
                    yield return list.ToArray();
                    list.Clear();
                }
            }

            if (list.Count > 0)
                yield return list.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="revisions"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static SearchResult CustomBinarySearch(this
            ValueRevision[] revisions,
            EntityVersion version)
            => CustomBinarySearch(new ArraySegment<ValueRevision>(revisions), version);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="revisions"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static SearchResult CustomBinarySearch(this ArraySegment<ValueRevision> revisions, EntityVersion version)
        {
            if (revisions.Count == 1 && revisions[0].Version == version)
                return new SearchResult(revisions.Offset, revisions[0]);

            if (version < revisions.FirstValue().Version)
                return new SearchResult(revisions.Offset, null);

            if (version > revisions.LastValue().Version)
                return new SearchResult(revisions.Offset + revisions.Count, null);

            // split
            var (left, right) = revisions.Split(revisions.Count / 2);

            #region optimizations: try finding a match at the extremes of the left and right sub-arrays
            if (TryFindMatchAtExtremes(left, version, out var result))
                return result;

            if (TryFindMatchAtExtremes(right, version, out result))
                return result;
            #endregion

            //because of the optimization above I can afford to avoid using '>=' and '<='
            if (version > left.LastValue().Version && version < right.LastValue().Version)
                return right.CustomBinarySearch(version);

            //because of the optimization above I can afford to avoid using '>=' and '<='
            if (version < right.FirstValue().Version && version > left.LastValue().Version)
                return left.CustomBinarySearch(version);

            //our value doesn't exist, but SHOULD be between left and right
            ///else if (version > left.Last().Version && version < right.First().Version)
            return new SearchResult(right.Offset, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="revisions"></param>
        /// <param name="version"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryFindMatchAtExtremes(this ArraySegment<ValueRevision> revisions, EntityVersion version, out SearchResult result)
        {
            result = default;

            if (revisions.Count == 0)
                return false;

            if (revisions.FirstValue().Version == version)
            {
                result = new SearchResult(revisions.Offset, revisions.FirstValue());
                return true;
            }

            if (revisions.LastValue().Version == version)
            {
                result = new SearchResult(revisions.Offset + revisions.Count - 1, revisions.LastValue());
                return true;
            }

            return false;
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
        /// Returns the value found at the given index, or null if the index is out of bounds.
        /// </summary>
        /// <param name="revisions"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static BasicValue? ValueAtOrDefault(this SortedList<EntityVersion, ValueRevision> revisions, int index)
        {
            if (index < 0 || index >= revisions.Count)
                return null;

            return revisions[revisions.Keys[index]].Value;
        }
    }


    internal struct SearchResult
    {
        public int Index { get; }

        public ValueRevision Revision { get; }

        public SearchResult(int index, ValueRevision revision = null)
        {
            Index = index;
            Revision = revision;
        }
    }
}
