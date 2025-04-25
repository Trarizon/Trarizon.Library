namespace Trarizon.Library.Collections.Generic;
public interface IBinaryTreeNode<TSelf> where TSelf : IBinaryTreeNode<TSelf>
{
    TSelf? Parent { get; set; }
    TSelf? LeftChild { get; set; }
    TSelf? RightChild { get; set; }
}

public static class BinaryTreeNode
{
    public static void Insert<TNode>(TNode parent, TNode node, bool appendParentsRight, bool appendNodesRight)
        where TNode : IBinaryTreeNode<TNode>
    {
        var oldParent = node.Parent;
        node.Parent = parent;

        TNode? child;
        if (appendParentsRight) {
            child = parent.RightChild;
            parent.RightChild = node;
        }
        else {
            child = parent.LeftChild;
            parent.LeftChild = node;
        }
        if (child is not null)
            child.Parent = node;

        TNode? oldChild;
        if (appendNodesRight) {
            oldChild = node.RightChild;
            node.RightChild = child;
            if (oldParent is not null)
                oldParent.RightChild = oldChild;
        }
        else {
            oldChild = node.LeftChild;
            node.LeftChild = child;
            if (oldParent is not null)
                oldParent.LeftChild = oldChild;
        }
    }

    public static void ConcatLeft<TNode>(TNode parent, TNode leftChild, out TNode? originalParent, out TNode? originalLeftChild)
        where TNode : IBinaryTreeNode<TNode>
    {
        originalLeftChild = parent.LeftChild;
        if (originalLeftChild is not null)
            originalLeftChild.Parent = default;
        originalParent = leftChild.Parent;
        if (originalParent is not null)
            originalParent.LeftChild = default;

        parent.LeftChild = leftChild;
        leftChild.Parent = parent;
    }

    public static void ConcatLeft<TNode>(TNode parent, TNode leftChild)
        where TNode : IBinaryTreeNode<TNode>
        => ConcatLeft(parent, leftChild, out _, out _);

    public static void ConcatRight<TNode>(TNode parent, TNode rightChild, out TNode? originalParent, out TNode? originalRightChild)
        where TNode : IBinaryTreeNode<TNode>
    {
        originalRightChild = parent.RightChild;
        if (originalRightChild is not null)
            originalRightChild.Parent = default;
        originalParent = rightChild.Parent;
        if (originalParent is not null)
            originalParent.RightChild = default;

        parent.RightChild = rightChild;
        rightChild.Parent = parent;
    }

    public static void ConcatRight<TNode>(TNode parent, TNode rightChild)
        where TNode : IBinaryTreeNode<TNode>
        => ConcatRight(parent, rightChild, out _, out _);

    #region Collection

    public static TNode GetRoot<TNode>(TNode node)
        where TNode : IBinaryTreeNode<TNode>
    {
        var parent = node.Parent;
        while (parent is not null) {
            node = parent;
            parent = node.Parent;
        }
        return node;
    }

    #endregion

    #region Traverse

    public static IEnumerable<TNode> TraverseBreadthFirst<TNode>(TNode root)
        where TNode : IBinaryTreeNode<TNode>
    {
        var queue = new Queue<TNode>();
        queue.Enqueue(root);
        while (queue.TryDequeue(out var node)) {
            yield return node;
            if (node.LeftChild is not null)
                queue.Enqueue(node.LeftChild);
            if (node.RightChild is not null)
                queue.Enqueue(node.RightChild);
        }
    }

    public static IEnumerable<TNode> TraverseDepthFirst<TNode>(TNode root)
        where TNode : IBinaryTreeNode<TNode>
    {
        var stack = new Stack<TNode>();
        stack.Push(root);
        while (stack.TryPop(out var node)) {
            yield return node;
            if (root.RightChild is { } right)
                stack.Push(right);
            if (root.LeftChild is { } left)
                stack.Push(left);
        }
    }

    public static IEnumerable<TNode> EnumerateParents<TNode>(TNode node, bool includeSelf)
        where TNode : IBinaryTreeNode<TNode>
    {
        if (includeSelf)
            yield return node;

        var n = node.Parent;
        while (n is not null) {
            yield return n;
            n = n.Parent;
        }
    }

    #endregion
}
