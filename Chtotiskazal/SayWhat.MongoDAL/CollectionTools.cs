using System;
using System.Collections.Generic;
using System.Linq;

namespace SayWhat.MongoDAL;

public static class CollectionTools {
    public static TOutput[] SelectToArray<TInput, TOutput>(this IReadOnlyList<TInput> input,
        Func<TInput, TOutput> mapFunc) {
        var ans = new TOutput[input.Count];
        for (int i = 0; i < input.Count; i++)
            ans[i] = mapFunc(input[i]);
        return ans;
    }

    public static TOutput[] SelectToArray<TInput, TOutput>(this IReadOnlyList<TInput> input,
        Func<TInput, int, TOutput> mapFunc) {
        var ans = new TOutput[input.Count];
        for (int i = 0; i < input.Count; i++)
            ans[i] = mapFunc(input[i], i);
        return ans;
    }

    public static int IndexOf<T>(this T[] targetArray, T value) {
        for (int i = 0; i < targetArray.Length; i++) {
            if (targetArray[i].Equals(value))
                return i;
        }

        return -1;
    }

    public static int IndexOf<T>(this T[] targetArray, Func<T, bool> predicate) {
        for (int i = 0; i < targetArray.Length; i++) {
            if (predicate(targetArray[i]))
                return i;
        }

        return -1;
    }

    public static void AddValuesInplace(this int[] targetArray, IReadOnlyList<int> other) {
        if (other == null)
            return;
        for (int i = 0; i < targetArray.Length && i < other.Count; i++)
            targetArray[i] += other[i];
    }

    public static int[] Sum(this IReadOnlyList<int> targetArray, IReadOnlyList<int> other) {
        if (other == null)
            return targetArray.ToArray();
        if (targetArray == null)
            return other.ToArray();
        int[] res = new int[targetArray.Count];
        for (int i = 0; i < targetArray.Count; i++)
            res[i] = targetArray[i] + other[i];
        return res;
    }

    public static void SetLowLimitInplace(this int[] targetArray, int lowLimit) {
        for (int i = 0; i < targetArray.Length; i++) {
            if (targetArray[i] < lowLimit)
                targetArray[i] = lowLimit;
        }
    }
}