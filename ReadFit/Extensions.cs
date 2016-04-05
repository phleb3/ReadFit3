using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using System.Text;
using ReadFit.FileModel;

namespace ReadFit
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> iEnumerable)
        {
            // Cheers to Joel Mueller for the bugfix. Was .Count(), now it's .Any()
            return iEnumerable == null || !iEnumerable.Any();
        }

        public static double ToRadians(this double val)
        {
            return (Math.PI / 180) * val;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this List<T> items)
        {
            ObservableCollection<T> collection = new ObservableCollection<T>();

            foreach (var item in items)
            {
                collection.Add(item);
            }

            return collection;
        }

        public static ObservableCollection<T> AsObservableCollection<T>(this IEnumerable<T> en)
        {
            return new ObservableCollection<T>(en);
        }

        public static CollectionView ToCollectionView<T>(this IEnumerable<T> items)
        {
            return (CollectionView)CollectionViewSource.GetDefaultView(items);
        }

        public static ObservableCollection<T> toObs<T>(this ICollectionView source)
        {
            return source.Cast<T>().AsObservableCollection();
        }

        public static AsyncObservableCollection<T> AsAsyncObservableCollection<T>(this IEnumerable<T> items)
        {
            return new AsyncObservableCollection<T>(items);
        }

        //http://blogs.thesitedoctor.co.uk/tim/Trackback.aspx?guid=6a9ca083-94b9-4ba3-b7e6-d29948179db9
        /// <summary>
        /// Simple method to chunk a source IEnumerable into smaller (more manageable) lists
        /// </summary>
        /// <param name="source">The large IEnumerable to split</param>
        /// <param name="chunkSize">The maximum number of items each subset should contain</param>
        /// <returns>An IEnumerable of the original source IEnumerable in bite size chunks</returns>
        public static IEnumerable<IEnumerable<TSource>> ChunkData<TSource>(this IEnumerable<TSource> source, int chunkSize)
        {
            for (int i = 0; i < source.Count(); i += chunkSize)
            {
                yield return source.Skip(i).Take(chunkSize);
            }
        }
    }
}
