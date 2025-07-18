﻿namespace Trarizon.Library.Collections.Generic;
public interface ILinkedListNode<TSelf> where TSelf : ILinkedListNode<TSelf>
{
    TSelf? Next { get; set; }
    TSelf? Prev { get; set; }
}

public static class LinkedListNode
{
    #region Modification

    public static void RemoveFromChain<TNode>(TNode node)
        where TNode : ILinkedListNode<TNode>
    {
        var prev = node.Prev;
        var next = node.Next;
        if (prev is not null) {
            prev.Next = next;
            node.Prev = default;
        }
        if (next is not null) {
            next.Prev = prev;
            node.Next = default;
        }
    }

    public static void Prepend<TNode>(TNode source, TNode node)
        where TNode : ILinkedListNode<TNode>
    {
        RemoveFromChain(node);
        var prev = source.Prev;
        if (prev is not null) {
            prev.Next = node;
            node.Prev = prev;
        }
        node.Next = source;
        source.Prev = node;
    }

    public static void Append<TNode>(TNode source, TNode node)
        where TNode : ILinkedListNode<TNode>
    {
        RemoveFromChain(node);
        var next = source.Next;
        if (next is not null) {
            next.Prev = node;
            node.Next = next;
        }
        node.Prev = source;
        source.Next = node;
    }

    public static void Concat<TNode>(TNode prevNode, TNode nextNode, out TNode? endsNextNode, out TNode? startsPrevNode)
        where TNode : ILinkedListNode<TNode>
    {
        endsNextNode = prevNode.Next;
        if (endsNextNode is not null) {
            endsNextNode.Prev = default;
        }
        startsPrevNode = nextNode.Prev;
        if (startsPrevNode is not null) {
            startsPrevNode.Next = default;
        }
        prevNode.Next = nextNode;
        nextNode.Prev = prevNode;
    }

    public static void Concat<TNode>(TNode prevNode, TNode nextNode)
        where TNode : ILinkedListNode<TNode>
        => Concat(prevNode, nextNode, out _, out _);

    #endregion

    #region Collection

    public static IReadOnlyCollection<TNode> GetFullLinkedList<TNode>(TNode node)
        where TNode : ILinkedListNode<TNode>
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

    public static TNode GetLinkedListFirst<TNode>(TNode node)
        where TNode : ILinkedListNode<TNode>
    {
        var prev = node.Prev;
        while (prev is not null) {
            node = prev;
            prev = prev.Prev;
        }
        return node;
    }

    public static TNode GetLinkedListLast<TNode>(TNode node)
        where TNode : ILinkedListNode<TNode>
    {
        var next = node.Next;
        while (next is not null) {
            node = next;
            next = next.Next;
        }
        return node;
    }

    public static bool IsLinkedListFirst<TNode>(TNode node)
        where TNode : ILinkedListNode<TNode>
        => node.Prev is null;

    public static bool IsLinkedListLast<TNode>(TNode node)
        where TNode : ILinkedListNode<TNode>
        => node.Next is null;

    #endregion

    #region Iteration

    public static IEnumerable<TNode> EnumerateFromFirst<TNode>(TNode node)
        where TNode : ILinkedListNode<TNode>
    {
        var stack = new Stack<TNode>();
        var prev = node.Prev;
        while (prev is not null) {
            stack.Push(prev);
            prev = prev.Prev;
        }
        foreach (var n in stack) {
            yield return n;
        }

        var tmp = node;
        while (tmp is not null) {
            yield return tmp;
            tmp = tmp.Next;
        }
    }

    public static IEnumerable<TNode> EnumerateBackFromLast<TNode>(TNode node)
        where TNode : ILinkedListNode<TNode>
    {
        var stack = new Stack<TNode>();
        var next = node.Next;
        while (next is not null) {
            stack.Push(next);
            next = next.Next;
        }
        foreach (var n in stack) {
            yield return n;
        }

        var tmp = node;
        while (tmp is not null) {
            yield return tmp;
            tmp = tmp.Prev;
        }
    }

    public static IEnumerable<TNode> EnumerateFrom<TNode>(TNode? node)
        where TNode : ILinkedListNode<TNode>
    {
        var tmp = node;
        while (tmp is not null) {
            yield return tmp;
            tmp = tmp.Next;
        }
    }

    public static IEnumerable<TNode> EnumerateBackFrom<TNode>(TNode? node) where TNode : class, ILinkedListNode<TNode>
    {
        var tmp = node;
        while (tmp is not null) {
            yield return tmp;
            tmp = tmp.Prev;
        }
    }

    public static IEnumerable<TNode> EnumerateRingFrom<TNode>(TNode node) where TNode : class, ILinkedListNode<TNode>
    {
        foreach (var item in EnumerateFrom(node))
            yield return item;

        foreach (var item in EnumerateBackFrom(node.Prev).Reverse())
            yield return item;
    }

    public static IEnumerable<TNode> EnumerateBackRingFrom<TNode>(TNode node) where TNode : class, ILinkedListNode<TNode>
    {
        foreach (var item in EnumerateBackFrom(node))
            yield return item;

        foreach (var item in EnumerateFrom(node.Next).Reverse())
            yield return item;
    }

    #endregion
}
