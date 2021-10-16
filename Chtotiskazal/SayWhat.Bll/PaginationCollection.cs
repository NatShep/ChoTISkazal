using System;
using System.Collections.Generic;
using System.Linq;

namespace SayWhat.Bll {

public class PaginationCollection<T> {
    public PaginationCollection() { }

    public PaginationCollection(IReadOnlyList<T> origin) { _items = origin; }
    public int Count => _items.Count;

    public void Set(IReadOnlyList<T> items) {
        if (items == null)
            throw new ArgumentNullException(nameof(items));
        _items = items;
        _numberOfPaginate = 0;
    }

    private IReadOnlyList<T> _items = Array.Empty<T>();
    private int _numberOfPaginate;
    public int Page
    {
        get
        {
            if (_items.Count == 0)
                return -1;
            else
                return _numberOfPaginate;
        }
        set
        {
            if (value >= _items.Count)
                _numberOfPaginate = _items.Count - 1;
            else if (value < 0)
                _numberOfPaginate = 0;
            else
                _numberOfPaginate = value;
        }
    }
    public void MoveNext() => Page++;
    public void MovePrev() => Page--;
    public T Current => _items.Any() ? _items[Page] : default;
}

}