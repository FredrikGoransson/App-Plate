using System;
using System.Collections.Generic;
using System.Linq;

namespace AppPlate.Core.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T NextItem<T>(this IEnumerable<T> enumerable, T current, bool throwOnNotFound = false)
        {
            var returnNext = false;
            foreach (var item in enumerable)
            {
                if (returnNext) return item;
                if (item.Equals(current))
                {
                    returnNext = true;
                }
            }
            if (throwOnNotFound)
                throw new ArgumentOutOfRangeException("current", "The current item is the last item in the enumerable");
            return default(T);
        }

        public static T PreviousItem<T>(this IEnumerable<T> enumerable, T current, bool throwOnNotFound = false)
        {
            var lastItem = default(T);
            foreach (var item in enumerable)
            {
                if (item.Equals(current))
                {
                    return lastItem;
                }
                lastItem = item;
            }
            if (throwOnNotFound)
                throw new ArgumentOutOfRangeException("current", "The current item is the first item in the enumerable");
            return default(T);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) return true;
            if (enumerable is T[]) return ((T[])enumerable).Length == 0;
            if (enumerable is IList<T>) return ((IList<T>)enumerable).Count == 0;

            return !enumerable.Any();
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> forEach)
        {
            // ReSharper disable once PossibleMultipleEnumeration, yes, this is exactly what we want...
            foreach (var item in enumerable)
            {
                forEach(item);
            }
            return enumerable;
        }

        public static IList<TDestination> Sync<TDestination, TSource>(
            this IList<TDestination> destination,
            IEnumerable<TSource> source,
            Func<TSource, TDestination, bool> comparer,
            Func<TSource, TDestination> onAdd = null,
            Action<TSource, TDestination> onUpdate = null,
            Action<TDestination> onDelete = null)
        {

            var removedDestinationItems = destination.ToList();
            foreach (var sourceItem in source)
            {
                var destinationItem = destination.FirstOrDefault(d => comparer(sourceItem, d));
                var isAdded = Equals(destinationItem, default(TDestination));

                if (isAdded)
                {
                    if (onAdd != null)
                    {
                        var addedItem = onAdd(sourceItem);
                        destination.Add(addedItem);
                    }
                }
                else
                {
                    if (onUpdate != null)
                    {
                        onUpdate(sourceItem, destinationItem);
                    }
                    removedDestinationItems.Remove(destinationItem);
                }
            }
            foreach (var destinationItem in removedDestinationItems)
            {
                destination.Remove(destinationItem);
                if (onDelete != null)
                {
                    onDelete(destinationItem);
                }
            }
            return destination;
        }
    }

    public class ListSyncContext<TDestination, TSource>
    {
        private Func<TDestination, TSource, bool> Comparer { get; set; }
        private IList<TDestination> Destination { get; set; }
        private IEnumerable<TSource> Source { get; set; }

        private Func<TSource, TDestination> Add { get; set; }
        private Action<TSource, TDestination> Update { get; set; }
        private Action<TDestination> Delete { get; set; }

        public ListSyncContext(IList<TDestination> destination, IEnumerable<TSource> source, Func<TDestination, TSource, bool> comparer)
        {
            Destination = destination;
            Source = source;
            Comparer = comparer;
        }

        public IList<TDestination> Do()
        {
            var removedDestinationItems = Destination.ToList();
            foreach (var source in Source)
            {
                var destination = Destination.FirstOrDefault(d => Comparer(d, source));
                var isAdded = Equals(destination, default(TDestination));

                if (isAdded)
                {
                    if (Add != null)
                    {
                        var addedItem = Add(source);
                        Destination.Add(addedItem);
                    }
                }
                else
                {
                    if (Update != null)
                    {
                        Update(source, destination);
                    }
                    removedDestinationItems.Remove(destination);
                }
            }
            foreach (var destination in removedDestinationItems)
            {
                Destination.Remove(destination);
                if (Delete != null)
                {
                    Delete(destination);
                }
            }
            return Destination;
        }

        public ListSyncContext<TDestination, TSource> OnAdd(Func<TSource, TDestination> add)
        {
            Add = add;
            return this;
        }

        public ListSyncContext<TDestination, TSource> OnUpdate(Action<TSource, TDestination> update)
        {
            Update = update;
            return this;
        }

        public ListSyncContext<TDestination, TSource> OnDelete(Action<TDestination> delete)
        {
            Delete = delete;
            return this;
        }
    }
}