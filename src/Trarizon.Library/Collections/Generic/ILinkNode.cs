namespace Trarizon.Library.Collections.Generic;
public interface ILinkNode<TSelf> where TSelf : class, ILinkNode<TSelf>
{
    TSelf? Next { get; set; }
    TSelf? Prev { get; set; }
}

public static class LinkNodeHelper
{
    #region Modification

    public static void UnlinkAndKeepChain<TNode>(TNode node) where TNode : class, ILinkNode<TNode>
    {
        var prev = node.Prev;
        var next = node.Next;
        if (prev is not null) {
            prev.Next = next;
            node.Prev = null;
        }
        if (next is not null) {
            next.Prev = prev;
            node.Next = null;
        }
    }

    public static void Prepend<TNode>(TNode source, TNode node) where TNode : class, ILinkNode<TNode>
    {
        UnlinkAndKeepChain(node);

        var prev = source.Prev;
        if (prev is not null) {
            prev.Next = node;
            node.Prev = prev;
        }
        node.Next = source;
        source.Prev = node;
    }

    public static void Append<TNode>(TNode source, TNode node) where TNode : class, ILinkNode<TNode>
    {
        UnlinkAndKeepChain(node);
        var next = node.Next;
        if (next is not null) {
            next.Prev = node;
            node.Next = next;
        }
        node.Prev = source;
        source.Next = node;
    }

    public static void Bridge<TNode>(TNode frontEndNode, TNode backStartNode) where TNode : class, ILinkNode<TNode>
        => Bridge(frontEndNode, backStartNode, out _, out _);

    public static void Bridge<TNode>(TNode frontEndNode, TNode backStartNode, out TNode? endsNextNode, out TNode? startsPrevNode) where TNode : class, ILinkNode<TNode>
    {
        endsNextNode = frontEndNode.Next;
        if (endsNextNode is not null) {
            endsNextNode.Prev = null;
        }
        startsPrevNode = backStartNode.Prev;
        if (startsPrevNode is not null) {
            startsPrevNode.Next = null;
        }
        frontEndNode.Next = backStartNode;
        backStartNode.Prev = frontEndNode;
    }

    #endregion

    public static IReadOnlyCollection<TNode> GetFullLink<TNode>(TNode node) where TNode : class, ILinkNode<TNode>
    {
        var deque = new Deque<TNode>();

        TNode? tmp = node;
        while (tmp is not null) {
            deque.EnqueueLast(tmp);
            tmp = tmp.Next;
        }
        tmp = node.Prev;
        while (tmp is not null) {
            deque.EnqueueFirst(tmp);
            tmp = tmp.Prev;
        }
        return deque;
    }

    public static TNode GetLinkFirst<TNode>(TNode node) where TNode : class, ILinkNode<TNode>
    {
        var prev = node.Prev;
        while (prev is not null) {
            node = prev;
            prev = prev.Prev;
        }
        return node;
    }

    public static TNode GetLinkLast<TNode>(TNode node) where TNode : class, ILinkNode<TNode>
    {
        var next = node.Next;
        while (next is not null) {
            node = next;
            next = next.Next;
        }
        return node;
    }

    public static bool IsLinkFirst<TNode>(TNode node) where TNode : class, ILinkNode<TNode>
        => node.Prev is null;

    public static bool IsLinkLast<TNode>(TNode node) where TNode : class, ILinkNode<TNode>
        => node.Next is null;

    #region Iteration

    public static IEnumerable<TNode> EnumerateFrom<TNode>(TNode? node) where TNode : class, ILinkNode<TNode>
    {
        var tmp = node;
        while (tmp is not null) {
            yield return tmp;
            tmp = tmp.Next;
        }
    }

    public static IEnumerable<TNode> EnumerateBackFrom<TNode>(TNode? node) where TNode : class, ILinkNode<TNode>
    {
        var tmp = node;
        while (tmp is not null) {
            yield return tmp;
            tmp = tmp.Prev;
        }
    }

    public static IEnumerable<TNode> EnumerateRingFrom<TNode>(TNode node) where TNode : class, ILinkNode<TNode>
    {
        foreach (var item in EnumerateFrom(node))
            yield return item;

        foreach (var item in EnumerateBackFrom(node.Prev).Reverse())
            yield return item;
    }

    public static IEnumerable<TNode> EnumerateBackRingFrom<TNode>(TNode node) where TNode : class, ILinkNode<TNode>
    {
        foreach (var item in EnumerateBackFrom(node))
            yield return item;

        foreach (var item in EnumerateFrom(node.Next).Reverse())
            yield return item;
    }

    #endregion
}
