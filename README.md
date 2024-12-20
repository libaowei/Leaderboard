# Leaderboard

<p>一开始准备使用红黑树实现的，因为C#的内置类型SortedSet就是红黑树实现的，但最后放弃了，没找到可以根据排名快速查找的办法，折腾了一两天，还是没找到查询速度接近O(logN)的方式</p>
<p>实在没办法，想到redis zset数据结构中的skiplist完美契合了Leaderboard的需求，参考redis的源码，写了一个C#版的SkipList，SkipList的查询，插入和删除的时间复杂度都是接近O(logN)</p>
<p>redis实现的skiplist与我们了解到的skiplist不一样，多了span和backward属性，span表示节点之间的排名距离，正好可以用来做排名范围查询，backward用于逆序查询</p>

## 红黑树 VS 跳表

#### 红黑树
+ 红黑树是平衡二叉树
+ 空间占用少，每个节点都是数据节点
+ 时间复杂度稳定, 查找，插入和删除的时间复杂度为O(log n)
+ 范围查询不友好
#### 跳表
- 跳表是多层有序链表
- 最下层有所有数据，上面的层是索引层，空间占用比红黑树大
- 时间复杂度不稳定，查找，插入和删除的时间间复杂度为O(log n)
- 由于底层有所有的数据，所有适合范围查询

###### 从上面的比较可以得出，Leaderboard更适合使用跳表作为数据存储，至于跳表的时间复杂度不稳定是因为每个节点的层级数是随机的，但redis帮我们证明了当节点数接近256时，时间复杂度就会趋于稳定

## B+树？
B+树是多路平衡搜索树，底层也是数据层，上层是索引层，跟SkipList一样，但不同的是B+树的单个节点可以有多个数据（索引层节点可以有多个关键字），正好可以配合磁盘的page页缓存, 所以B+树更适合组织磁盘数据，而不是内存数据

[数据结构可视化](https://www.cs.usfca.edu/~galles/visualization/RedBlack.html)
