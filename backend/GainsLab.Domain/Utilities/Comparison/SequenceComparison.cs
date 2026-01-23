namespace GainsLab.Domain.Comparison;

public static class SequenceComparison
{
   
   public static bool SetEqualWithDiff<T, TKey>(
      IEnumerable<T>? a,
      IEnumerable<T>? b,
      Func<T, TKey> keySelector,
      out SetDifference<T> diff,
      IEqualityComparer<TKey>? comparer = null) where TKey : notnull
   {
      
      comparer ??= EqualityComparer<TKey>.Default;
      
      a ??= Enumerable.Empty<T>();
      b ??= Enumerable.Empty<T>();
      
  
      var aByKey = new Dictionary<TKey, T>(comparer);
      foreach (var item in a)
         aByKey[keySelector(item)] = item;

      var bByKey = new Dictionary<TKey, T>(comparer);
      foreach (var item in b)
         bByKey[keySelector(item)] = item;

      var toAdd = new List<T>();
      foreach (var (key, item) in aByKey)
      {
         if (!bByKey.ContainsKey(key))
            toAdd.Add(item);
      }

      var toRemove = new List<T>();
      foreach (var (key, item) in bByKey)
      {
         if (!aByKey.ContainsKey(key))
            toRemove.Add(item);
      }

      diff = new SetDifference<T>(toAdd, toRemove);

      // equal if nothing to add/remove
      return toAdd.Count == 0 && toRemove.Count == 0;
      
   }


}

public readonly record struct SetDifference<T>(
   IReadOnlyCollection<T> ToAdd,
   IReadOnlyCollection<T> ToRemove)
{
   
};