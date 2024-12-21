using System.Diagnostics;
using Leaderboard.Core.Common;

namespace Leaderboard.Core.DataStructures;

/// <summary>
/// <para>
/// SkipList is a multi-level ordered linked list
/// Implementation reference redis zset https://github.com/redis/redis/blob/unstable/src/t_zset.c
/// </para >
/// <para>
/// redis define skiplist and create skiplist source code as follow:
/// typedef struct zskiplist {
///     struct zskiplistNode *header, *tail;
///     unsigned long length;
///     int level;
/// } zskiplist;
/// </para>
/// <para>
/// /* Create a new skiplist. */
/// zskiplist *zslCreate(void) {
///     int j;
///     zskiplist *zsl;
///     zsl = zmalloc(sizeof(*zsl));
///     zsl->level = 1;
///     zsl->length = 0;
///     zsl->header = zslCreateNode(ZSKIPLIST_MAXLEVEL,0,NULL);
///     for (j = 0; j < ZSKIPLIST_MAXLEVEL; j++) {
///         zsl->header->level[j].forward = NULL;
///         zsl->header->level[j].span = 0;
///     }
///     zsl->header->backward = NULL;
///     zsl->tail = NULL;
///     return zsl;
/// }
/// </para>
/// </summary>
[DebuggerDisplay("Header = {_header}, Tail = {_tail}, Level = {_level}, Count = {Count}")]
public partial class SkipList<T> : IEnumerable<T> where T : IComparable<T>
{
    public int Count { get; private set; }
    /// <summary>
    /// Current max level
    /// </summary>
    private int _level;

    // The skip-list first node, non data node
    private SkipListNode _header;
    // The skip-list last node
    private SkipListNode _tail;

    private const int MaxLevel = 32;

    public SkipList()
    {
        Count = 0;
        _level = 1;
        _header = new SkipListNode(default, MaxLevel);
    }

    /// <summary>
    /// Insert a new node in the skiplist. Assumes the element does not already
    /// exist (up to the caller to enforce that). The skiplist takes ownership
    /// of the passed 'ele'.
    /// </summary>
    public void Insert(T item)
    {
        var update = new SkipListNode[MaxLevel];
        Span<int> rank = stackalloc int[MaxLevel];
        var node = _header;

        for (var i = _level - 1; i >= 0; i--)
        {
            /* store rank that is crossed to reach the insert position */
            rank[i] = i == _level - 1 ? 0 : rank[i + 1];
            while (node.Level[i].Forward?.Value.IsLessThan(item) == true)
            {
                rank[i] += node.Level[i].Span;
                node = node.Level[i].Forward;
            }

            update[i] = node;
        }

        /* we assume the element is not already inside, since we allow duplicated
        * scores, reinserting the same element should never happen since the
        * caller of zslInsert() should test in the hash table if the element is
        * already inside or not. */
        int level = GetRandomLevel();

        if (level > _level)
        {
            for (var i = _level; i < level; ++i)
            {
                rank[i] = 0;
                update[i] = _header;
                update[i].Level[i].Span = Count;
            }

            _level = level;
        }

        node = new SkipListNode(item, level);

        // Insert the new node into the skip list
        for (var i = 0; i < level; i++)
        {
            node.Level[i].Forward = update[i].Level[i].Forward;
            update[i].Level[i].Forward = node;

            node.Level[i].Span = update[i].Level[i].Span - (rank[0] - rank[i]);
            update[i].Level[i].Span = rank[0] - rank[i] + 1;
        }

        /* increment span for untouched levels */
        for (var i = level; i < _level; i++)
        {
            update[i].Level[i].Span++;
        }

        node.Backward = update[0] == _header ? null : update[0];
        if (node.Level[0].Forward != null)
            node.Level[0].Forward.Backward = node;
        else
            _tail = node;

        Count++;

        /// <summary>
        /// Returns a random level for the new skiplist node we are going to create.
        /// The return value of this function is between 1 and ZSKIPLIST_MAXLEVEL
        /// (both inclusive), with a powerlaw-alike distribution where higher
        /// levels are less likely to be returned.
        /// </summary>
        /// <returns></returns>
        static int GetRandomLevel()
        {
            int level = 1;
            // 1/2 chances is true
            while (Random.Shared.Next(0, 2) == 0)
                level++;
            return level < MaxLevel ? level : MaxLevel;
        }
    }

#if DEBUG
    public void Insert(T item, int level)
    {
        var update = new SkipListNode[MaxLevel];
        Span<int> rank = stackalloc int[MaxLevel];
        var node = _header;

        for (var i = _level - 1; i >= 0; i--)
        {
            /* store rank that is crossed to reach the insert position */
            rank[i] = i == _level - 1 ? 0 : rank[i + 1];
            while (node.Level[i].Forward?.Value.IsLessThan(item) == true)
            {
                rank[i] += node.Level[i].Span;
                node = node.Level[i].Forward;
            }

            update[i] = node;
        }

        /* we assume the element is not already inside, since we allow duplicated
        * scores, reinserting the same element should never happen since the
        * caller of zslInsert() should test in the hash table if the element is
        * already inside or not. */
        if (level > _level)
        {
            for (var i = _level; i < level; ++i)
            {
                rank[i] = 0;
                update[i] = _header;
                update[i].Level[i].Span = Count;
            }

            _level = level;
        }

        node = new SkipListNode(item, level);

        // Insert the new node into the skip list
        for (var i = 0; i < level; i++)
        {
            node.Level[i].Forward = update[i].Level[i].Forward;
            update[i].Level[i].Forward = node;

            node.Level[i].Span = update[i].Level[i].Span - (rank[0] - rank[i]);
            update[i].Level[i].Span = rank[0] - rank[i] + 1;
        }

        /* increment span for untouched levels */
        for (var i = level; i < _level; i++)
        {
            update[i].Level[i].Span++;
        }

        node.Backward = update[0] == _header ? null : update[0];
        if (node.Level[0].Forward != null)
            node.Level[0].Forward.Backward = node;
        else
            _tail = node;

        Count++;
    }
#endif

    /// <summary>
    /// * Delete an element with matching score/element from the skiplist.
    /// * The function returns 1 if the node was found and deleted, otherwise
    /// * 0 is returned.
    /// *
    /// * If 'node' is NULL the deleted node is freed by zslFreeNode(), otherwise
    /// * it is not freed (but just unlinked) and *node is set to the node pointer,
    /// * so that it is possible for the caller to reuse the node (including the
    /// * referenced SDS string at node->ele).
    /// </summary>
    public bool Remove(T item)
    {
        // Find the node in each of the levels
        var node = _header;
        var update = new SkipListNode[MaxLevel];

        // Walk after all the nodes that have values less than the node we are looking for.
        // Mark all nodes as update.
        for (var i = _level - 1; i >= 0; i--)
        {
            while (node.Level[i].Forward?.Value.IsLessThan(item) == true)
                node = node.Level[i].Forward;

            update[i] = node;
        }

        node = node.Level[0].Forward;

        // Return default value of T if the item was not found
        if (node?.Value.Equals(item) != true)
        {
            return false;
        }

        // We know that the node is in the list.
        // Unlink it from the levels where it exists.
        for (var i = 0; i < _level; i++)
        {
            if (update[i].Level[i].Forward == node)
            {
                update[i].Level[i].Forward = node.Level[i].Forward;
                update[i].Level[i].Span += node.Level[i].Span - 1;
            }
            else
            {
                update[i].Level[i].Span--;
            }
        }

        if (node.Level[0].Forward != null)
            node.Level[0].Forward.Backward = node.Backward;
        else
            _tail = node.Backward;

        // Check to see if we've deleted the highest-level node
        // Decrement level
        while (_level > 1 && _header.Level[_level - 1].Forward == null)
            _level--;

        Count--;
        return true;
    }

    public bool Contains(T item)
    {
        var node = _header;
        var flag = false;

        // Walk after all the nodes that have values less than the node we are looking for
        for (var i = _level - 1; i >= 0; --i)
        {
            flag = false;
            while (node.Level[i].Forward?.Value.IsLessThanOrEqualTo(item) == true)
            {
                node = node.Level[i].Forward;
                flag = true;
            }
            if (flag && node.Value != null && node.Value.Equals(item))
            {
                return true;
            }
        }
        // Return true if we found the element; false otherwise
        return node.Value != null && node.Value.Equals(item);
    }

    /// <summary>
    /// Find the rank by value.
    /// Returns 0 when the element cannot be found, rank otherwise.
    /// Note that the rank is 1-based due to the span of header to the
    /// first element.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int GetRank(T item)
    {
        var node = _header;
        var rank = 0;
        var flag = false;

        for (var i = _level - 1; i >= 0; i--)
        {
            flag = false;
            while (node.Level[i].Forward?.Value.IsLessThanOrEqualTo(item) == true)
            {
                rank += node.Level[i].Span;
                node = node.Level[i].Forward;
                flag = true;
            }
            /* current might be equal to _header, so test if obj is non-NULL */
            if (flag && node.Value != null && node.Value.Equals(item))
            {
                return rank;
            }
        }
        return 0;
    }

    /// <summary>
    /// translate from redis skiplist method GetElementByRankFromNode
    /// /* Finds an element by its rank from start node. The rank argument needs to be 1-based. */
    /// zskiplistNode *zslGetElementByRankFromNode(zskiplistNode *start_node, int start_level, unsigned long rank) {
    ///    zskiplistNode *x;
    ///    unsigned long traversed = 0;
    ///    int i;
    ///
    ///    x = start_node;
    ///    for (i = start_level; i >= 0; i--) {
    ///        while (x->level[i].forward && (traversed + x->level[i].span) <= rank)
    ///        {
    ///            traversed += x->level[i].span;
    ///            x = x->level[i].forward;
    ///        }
    ///        if (traversed == rank) {
    ///            return x;
    ///        }
    ///    }
    ///    return NULL;
    ///}
    /// </summary>
    /// <param name="rank"></param>
    /// <returns></returns>
    public T GetByRank(int rank)
    {
        if (rank < 1 || rank > Count) return default;
        var node = _header;
        var tranversed = 0;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (node.Level[i].Forward != null && (tranversed + node.Level[i].Span <= rank))
            {
                tranversed += node.Level[i].Span;
                node = node.Level[i].Forward;
            }
            if (tranversed == rank)
            {
                return node.Value;
            }
        }
        return default;
    }

    /// <summary>
    /// <see cref="GetByRank(int)"/> variant
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public IDictionary<int, T> GetByRange(int start, int end)
    {
        var list = new Dictionary<int, T>();
        if (start < 1 || end > Count || end < start || start > Count) return list;
        var node = _header;
        var tranversed = 0;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (node.Level[i].Forward != null && (tranversed + node.Level[i].Span <= start))
            {
                tranversed += node.Level[i].Span;
                node = node.Level[i].Forward;
            }
            if (tranversed == start)
            {
                var total = end - start + 1;
                while (node != null && total > 0)
                {
                    list.Add(tranversed++, node.Value);
                    node = node.Level[0].Forward;
                    total--;
                }
                return list;
            }
        }

        return list;
    }

    /// <summary>
    /// <see cref="GetRank(T)"/> variant
    /// </summary>
    /// <param name="item"></param>
    /// <param name="high"></param>
    /// <param name="low"></param>
    /// <returns></returns>
    public IDictionary<int, T> GetNeighbors(T item, int high, int low)
    {
        var list = new List<T>();
        var node = _header;
        var rank = 0;
        var flag = false;

        for (var i = _level - 1; i >= 0; --i)
        {
            flag = false;
            while (node.Level[i].Forward?.Value.IsLessThanOrEqualTo(item) == true)
            {
                rank += node.Level[i].Span;
                node = node.Level[i].Forward;
                flag = true;
            }

            if (flag && node.Value != null && node.Value.Equals(item))
            {
                var pre = node;
                while (pre.Backward != null && high > 0)
                {
                    pre = pre.Backward;
                    high--;
                    rank--;
                    list.Insert(0, pre.Value);
                }
                list.Add(item);

                var next = node;
                while (next != null && low > 0)
                {
                    next = next.Level[0].Forward;
                    low--;
                    if (next != null)
                    {
                        list.Add(next.Value);
                    }
                }
            }
        }

        return list.ToDictionary(_ => rank++, y => y);
    }

    #region IEnumerable<T> Implementation
    /// <summary>
    /// IEnumerable method implementation
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        var node = _header;
        while (node.Level[0].Forward != null && node.Level[0].Forward != _header)
        {
            node = node.Level[0].Forward;
            yield return node.Value;
        }
    }

    /// <summary>
    /// IEnumerable method implementation
    /// </summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion IEnumerable<T> Implementation

    public void Clear()
    {
        Count = 0;
        _level = 1;
        _header = new SkipListNode(default, MaxLevel);

        for (var i = 0; i < MaxLevel; i++)
            _header.Level[i].Forward = null;
    }
}
