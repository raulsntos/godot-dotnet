// Implementation derived from Roslyn.

// https://github.com/dotnet/roslyn/blob/969c5177a3fe0a13171c561b45b2d808b9659e56/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Collections/SimpleMutableIntervalTree%602.cs

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Delegate used to pass in the particular interval testing operation to perform on an interval tree.
/// For example checking if an interval 'contains', 'intersects', or 'overlaps' with a requested span.
/// </summary>
internal delegate bool IntervalTester(TextChange value, int start, int length);

/// <summary>
/// An interval tree represents an ordered tree data structure to store intervals of the form [start, end).
/// It allows to efficiently find all intervals that intersect or overlap a provided interval.
/// </summary>
/// <remarks>
/// This is the root type for all interval trees that store their data in a binary tree format.
/// This format is good for when mutation of the tree is expected, and a client wants to perform tests
/// before and after such mutation.
/// </remarks>
internal sealed partial class IntervalTree : IEnumerable<TextChange>
{
    private Node? _root;

    public bool TryGetRoot([NotNullWhen(true)] out Node? root)
    {
        root = _root;
        return root is not null;
    }

    /// <summary>
    /// Warning: Mutates the tree in place.
    /// </summary>
    public void AddIntervalInPlace(TextChange value)
    {
        _root = Insert(_root, new Node(value));
    }

    private static Node Insert(Node? root, Node newNode)
    {
        int newNodeStart = newNode.Value.Span.Start;
        return Insert(root, newNode, newNodeStart);
    }

    private static Node Insert(Node? root, Node newNode, int newNodeStart)
    {
        if (root is null)
        {
            return newNode;
        }

        Node? newLeft, newRight;

        if (newNodeStart < root.Value.Span.Start)
        {
            newLeft = Insert(root.Left, newNode, newNodeStart);
            newRight = root.Right;
        }
        else
        {
            newLeft = root.Left;
            newRight = Insert(root.Right, newNode, newNodeStart);
        }

        root.SetLeftRight(newLeft, newRight);

        Node newRoot = root;
        return Balance(newRoot);

        static Node Balance(Node node)
        {
            int balanceFactor = BalanceFactor(node);
            if (balanceFactor == -2)
            {
                int rightBalance = BalanceFactor(node.Right);
                if (rightBalance == -1)
                {
                    return node.LeftRotation();
                }
                else
                {
                    Debug.Assert(rightBalance == 1);
                    return node.InnerRightOuterLeftRotation();
                }
            }
            else if (balanceFactor == 2)
            {
                int leftBalance = BalanceFactor(node.Left);
                if (leftBalance == 1)
                {
                    return node.RightRotation();
                }
                else
                {
                    Debug.Assert(leftBalance == -1);
                    return node.InnerLeftOuterRightRotation();
                }
            }

            return node;
        }

        static int BalanceFactor(Node? node)
        {
            if (node is null)
            {
                return 0;
            }

            int leftEndValue = node.Left?.Height ?? 0;
            int rightEndValue = node.Right?.Height ?? 0;
            return leftEndValue - rightEndValue;
        }
    }

    public void FillWithIntervalsThatOverlapWith(int start, int length, ImmutableArray<TextChange>.Builder builder)
    {
        FillWithIntervalsThatMatch(start, length, builder, OverlapsWith, stopAfterFirst: false);
    }

    public void FillWithIntervalsThatIntersectWith(int start, int length, ImmutableArray<TextChange>.Builder builder)
    {
        FillWithIntervalsThatMatch(start, length, builder, IntersectsWith, stopAfterFirst: false);
    }

    private static bool IntersectsWith(TextChange value, int start, int length)
    {
        int otherStart = start;
        int otherEnd = start + length;

        TextSpan thisSpan = value.Span;
        int thisStart = thisSpan.Start;
        int thisEnd = thisSpan.End;

        return otherStart <= thisEnd && otherEnd >= thisStart;
    }

    private static bool OverlapsWith(TextChange value, int start, int length)
    {
        int otherStart = start;
        int otherEnd = start + length;

        TextSpan thisSpan = value.Span;
        int thisStart = thisSpan.Start;
        int thisEnd = thisSpan.End;

        // TODO(cyrusn): This doesn't actually seem to match what TextSpan.OverlapsWith does.
        // It doesn't specialize empty length in any way. Preserving this behavior for now,
        // but we should consider changing this.
        if (length == 0)
        {
            return thisStart < otherStart && otherStart < thisEnd;
        }

        int overlapStart = int.Max(thisStart, otherStart);
        int overlapEnd = int.Min(thisEnd, otherEnd);

        return overlapStart < overlapEnd;
    }

    /// <summary>
    /// Adds all intervals within the tree within the given start/length pair that match the given
    /// <paramref name="intervalTester"/> predicate. Results are added to the <paramref name="builder"/> array.
    /// The <paramref name="stopAfterFirst"/> indicates if the search should stop after the first interval is found.
    /// Results will be returned in a sorted order based on the start point of the interval.
    /// </summary>
    /// <returns>The number of matching intervals found by the method.</returns>
    private int FillWithIntervalsThatMatch(int start, int length, ImmutableArray<TextChange>.Builder builder, IntervalTester intervalTester, bool stopAfterFirst)
    {
        int matchCount = 0;
        int end = start + length;

        using var enumerator = new Enumerator(this, start, end);
        while (enumerator.MoveNext())
        {
            var currentNodeValue = enumerator.Current;
            if (intervalTester(currentNodeValue, start, length))
            {
                matchCount++;
                builder.Add(currentNodeValue);

                if (stopAfterFirst)
                {
                    return 1;
                }
            }
        }

        return matchCount;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<TextChange> IEnumerable<TextChange>.GetEnumerator() => GetEnumerator();

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }
}
