using System.Diagnostics;

namespace Leaderboard.Core.DataStructures;

public partial class SkipList<T>
{
    /// <summary>
    /// redis source code as follow:
    /// typedef struct  {
    ///     struct zskiplistNode *forward;
    ///     unsigned long span;
    /// } zskiplistLevel;
    /// </summary>
    [DebuggerDisplay("--{Span}-->{Forward}")]
    private sealed class SkipListLevel
    {
        public SkipListNode Forward { get; set; }

        /// <summary>
        /// Current element to forward rank relative distance
        /// </summary>
        public int Span { get; set; }
    }
}