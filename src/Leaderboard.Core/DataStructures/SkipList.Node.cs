using System.Diagnostics;

namespace Leaderboard.Core.DataStructures;

public partial class SkipList<T>
{
    /// <summary>
    /// <para>
    /// redis define skiplistNode and create skiplistNode source code as follow:
    /// typedef struct zskiplistNode {
    ///     sds ele;
    ///     double score;
    ///     struct zskiplistNode *backward;
    ///     struct zskiplistLevel {
    ///         struct zskiplistNode *forward;
    ///         unsigned long span;
    ///     } level[];
    /// } zskiplistNode;
    /// </para>
    /// <para>
    /// /* Create a skiplist node with the specified number of levels.
    ///  * The SDS string 'ele' is referenced by the node after the call. */
    /// zskiplistNode *zslCreateNode(int level, double score, sds ele) {
    ///    zskiplistNode *zn =
    ///        zmalloc(sizeof(*zn)+level*sizeof(struct zskiplistLevel));
    ///    zn->score = score;
    ///    zn->ele = ele;
    ///    return zn;
    /// }
    /// </para>
    /// </summary>
    [DebuggerDisplay("{Value}[{Level.Length}]")]
    private sealed class SkipListNode
    {
        public SkipListNode(T value, int level)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(level);

            Value = value;
            Level = new SkipListLevel[level];
            for (var i = 0; i < level; i++)
            {
                Level[i] = new SkipListLevel();
            }
        }

        public T Value { get; init; }
        public SkipListNode Backward { get; set; }
        public SkipListLevel[] Level { get; init; }
    }
}