// Implementation derived from Roslyn.

// https://github.com/dotnet/roslyn/blob/969c5177a3fe0a13171c561b45b2d808b9659e56/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Collections/SimpleMutableIntervalTree%602.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Godot.UpgradeAssistant;

partial class IntervalTree
{
    /// <summary>
    /// Struct-based enumerator, so we can iterate an interval tree without allocating.
    /// </summary>
    public struct Enumerator : IEnumerator<TextChange>
    {
        private readonly IntervalTree _tree;
        private readonly int _start;
        private readonly int _end;

        private readonly Stack<Node>? _stack;

        private bool _started;
        private Node? _currentNode;
        private bool _currentNodeHasValue;

        public Enumerator(IntervalTree tree) : this(tree, start: int.MinValue, end: int.MaxValue) { }

        public Enumerator(IntervalTree tree, int start, int end)
        {
            _tree = tree;
            _start = start;
            _end = end;

            _currentNodeHasValue = tree.TryGetRoot(out _currentNode);

            // Avoid any allocating work if we don't even have a root.
            if (_currentNodeHasValue)
            {
                _stack = [];
            }
        }

        readonly object IEnumerator.Current => Current;

        public readonly TextChange Current => _currentNode!.Value;

        public bool MoveNext()
        {
            // Trivial empty case.
            if (_stack is null)
            {
                return false;
            }

            // The first time through, we just want to start processing with the root node. Every other time through,
            // after we've yielded the current element, we  want to walk down the right side of it.
            if (_started)
            {
                _currentNodeHasValue = ShouldExamineRight(_tree, _start, _end, _currentNode!, out _currentNode);
            }

            // After we're called once, we're in the started point.
            _started = true;

            if (!_currentNodeHasValue && _stack.Count <= 0)
            {
                return false;
            }

            // Traverse all the way down the left side of the tree, pushing nodes onto the stack as we go.
            while (_currentNodeHasValue)
            {
                _stack.Push(_currentNode!);
                _currentNodeHasValue = ShouldExamineLeft(_tree, _start, _currentNode!, out _currentNode);
            }

            Debug.Assert(!_currentNodeHasValue);
            Debug.Assert(_stack.Count != 0);

            _currentNode = _stack.Pop();
            return true;
        }

        public readonly void Dispose() { }

        public readonly void Reset()
        {
            throw new NotImplementedException();
        }

        private static bool ShouldExamineRight(IntervalTree tree, int start, int end, Node currentNode, [NotNullWhen(true)] out Node? right)
        {
            if (start == int.MinValue && end == int.MaxValue)
            {
                return TryGetRightNode(currentNode, out right);
            }

            // Right children's starts will never be to the left of the parent's start so we should consider right
            // subtree only if root's start overlaps with interval's end.
            if (currentNode.Value.Span.Start <= end)
            {
                if (TryGetRightNode(currentNode, out var rightNode)
                 && rightNode.MaxEndNode.Value.Span.End >= start)
                {
                    right = rightNode;
                    return true;
                }
            }

            right = default;
            return false;

            static bool TryGetRightNode(Node node, [NotNullWhen(true)] out Node? rightNode)
            {
                rightNode = node.Right;
                return rightNode is not null;
            }
        }

        private static bool ShouldExamineLeft(IntervalTree tree, int start, Node currentNode, [NotNullWhen(true)] out Node? left)
        {
            if (start == int.MinValue)
            {
                return TryGetLeftNode(currentNode, out left);
            }

            // Only if left's max end overlaps with interval's start, we should consider left subtree.
            if (TryGetLeftNode(currentNode, out left)
             && left.MaxEndNode.Value.Span.End >= start)
            {
                return true;
            }

            return false;

            static bool TryGetLeftNode(Node node, [NotNullWhen(true)] out Node? leftNode)
            {
                leftNode = node.Left;
                return leftNode is not null;
            }
        }
    }
}
