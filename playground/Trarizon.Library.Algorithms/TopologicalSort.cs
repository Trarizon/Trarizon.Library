using System.Diagnostics;
using Trarizon.Library.Linq;
using Trarizon.Library.Mathematics;
using CommunityToolkit.HighPerformance;

namespace Trarizon.Library.Algorithms;

static class TopologicalSort
{
    /*
        y   v left most
        |   xx <- far most
        |   xx
        2   xx <- right most
        |   6 <- near most
        ----45-- x
    */


    class Item
    {
        public (int X, int Y)[] Footprints = [];

        public int LeftMost;
        public int RightMost;
        public int NearMost;
        public int FarMost;

        private void Initialize()
        {
            (LeftMost, RightMost) = Footprints.MinMax(tpl => tpl.X - tpl.Y);
            (NearMost, FarMost) = Footprints.MinMax(tpl => tpl.X + tpl.Y);
        }

        public static bool? GreaterThan(Item left, Item right)
        {
            // 竖直方向不重合
            if (left.LeftMost > right.RightMost || right.LeftMost > left.RightMost)
                return null;

            foreach (var foot in left.Footprints)
            {
                // 如果left中存在任意一点，在其重合区域内有right的点，则可通过这两个点判定谁是高层
                var column = foot.X - foot.Y;
                if (right.Footprints.TryFirst(x =>
                {
                    var col = x.X - x.Y;
                    return col >= column - 1 && col <= column + 1;
                }, out var rightFoot))
                {
                    return foot.X + foot.Y < rightFoot.X + rightFoot.Y;
                }
            }

            Debug.Fail("GreaterThan: no collision");
            return null;
        }
    }

    class Graph
    {
        public List<Item> Items;
        public bool[] Edges;
        public int[] Counts;


        public static Graph CreateGraph(List<Item> items)
        {
            var edgeArr = new bool[items.Count * items.Count];
            var edges = edgeArr.AsSpan().AsSpan2D(items.Count, items.Count); // edges[i, j] 表示i->j的边
            var counts = new int[items.Count]; // counts[j] 表示j的入边数量

            for (int index = 0; index < items.Count; index++)
            {
                var item = items[index];
                for (int j = 0; j < index; j++)
                {
                    var node = items[j];
                    var cmp = Item.GreaterThan(item, node);
                    if (cmp is null)
                        continue;
                    if (cmp is true)
                    {
                        edges[index, j] = true;
                        counts[j]++;
                    }
                    if (cmp is false)
                    {
                        edges[j, index] = true;
                        counts[index]++;
                    }
                }
            }

            return new Graph { Items = items, Edges = edgeArr, Counts = counts };
        }

        public void SetDirty(Item item)
        {
            var index = Items.IndexOf(item);
        }

        public bool Sort(Span<Item> sorted)
        {
            // Kahn
            var iSorted = 0;
            var counts = Counts.ToArray();
            var queue = new Queue<int>();
            for (int i = 0; i < Items.Count; i++)
            {
                if (counts[i] == 0)
                    queue.Enqueue(i);
            }

            var edges = Edges.AsSpan2D(Items.Count, Items.Count);
            while (queue.TryDequeue(out var index))
            {
                sorted[iSorted++] = Items[index];
                for (int j = 0; j < Items.Count; j++)
                {
                    if (edges[index, j])
                    {
                        counts[j]--;
                        if (counts[j] == 0)
                            queue.Enqueue(j);
                    }
                }
            }

            if (iSorted != Items.Count)
                return false;
            return true;
        }
    }


    static void Sort(Graph graph)
    {
        // Kahn
        var zeroIn = new List<Item>();
        for (int i = 0; i < graph.Items.Count; i++)
        {
            if (graph.Counts[i] == 0)
                zeroIn.Add(graph.Items[i]);
        }

        var sorted = new List<Item>();
    }
}