#nullable enable
using System;
using System.Collections.Generic;

public class CustomPrioQ<T>
{
    List<T> Items = new List<T>();

    Func<T, T, bool> comparator = (T a, T b) =>
    {
        return false;
    };

    public CustomPrioQ(Func<T, T, bool> comparator, List<T> items)
    {
        Items = items;
        this.comparator = comparator;
    }

    public int Count
    {
        get { return Items.Count; }
    }

    public bool IsEmpty()
    {
        return Count == 0;
    }

    public int Enqueue(List<T> values)
    {
        values.ForEach((value) =>
        {
            placeValue(value);
        });
        return Count;
    }

    private void placeValue(T value)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (comparator(value, Items[i]))
            {
                List<T> firstHalf = Util.Slice(Items, 0, i);
                firstHalf.Add(value);
                List<T> secondHalf = Util.Slice(Items, i, Items.Count - 1);
                List<T> newList = new List<T>(firstHalf);
                newList.AddRange(secondHalf);
                return;
            }
        }

        Items.Add(value);
    }

    public T? Dequeue()
    {
        if (Items.Count <= 0) return default;

        T poppedValue = Items[0];
        Items.RemoveAt(0);
        return poppedValue;
    }
}
