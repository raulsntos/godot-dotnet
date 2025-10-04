// Implementation derived from Roslyn.

// https://github.com/dotnet/roslyn/blob/969c5177a3fe0a13171c561b45b2d808b9659e56/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Collections/SimpleMutableIntervalTree%602.cs

using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Godot.UpgradeAssistant;

partial class IntervalTree
{
    internal sealed class Node
    {
        internal TextChange Value { get; }

        internal Node? Left { get; private set; }

        internal Node? Right { get; private set; }

        internal int Height { get; private set; }

        internal Node MaxEndNode { get; private set; }

        internal Node(TextChange value)
        {
            Value = value;
            Height = 1;
            MaxEndNode = this;
        }

        internal void SetLeftRight(Node? left, Node? right)
        {
            Left = left;
            Right = right;

            Height = 1 + int.Max(left?.Height ?? 0, right?.Height ?? 0);

            // We now must store the node that produces the maximum end. Since we might have tracking spans
            // (or something similar) defining our values of "end", we can't store the int itself.
            int thisEndValue = Value.Span.End;
            int leftEndValue = left?.MaxEndNode.Value.Span.End ?? 0;
            int rightEndValue = right?.MaxEndNode.Value.Span.End ?? 0;

            if (thisEndValue >= leftEndValue && thisEndValue >= rightEndValue)
            {
                MaxEndNode = this;
            }
            else if ((leftEndValue >= rightEndValue) && left is not null)
            {
                MaxEndNode = left.MaxEndNode;
            }
            else if (right is not null)
            {
                MaxEndNode = right.MaxEndNode;
            }
            else
            {
                throw new UnreachableException();
            }
        }

        // Sample:
        //       1              2
        //      / \          /     \
        //     2   d        3       1
        //    / \     =>   / \     / \
        //   3   c        a   b   c   d
        //  / \
        // a   b
        internal Node RightRotation()
        {
            Debug.Assert(Left is not null);

            Node? oldLeft = Left;
            SetLeftRight(Left.Right, Right);
            oldLeft.SetLeftRight(oldLeft.Left, this);

            return oldLeft;
        }

        // Sample:
        //   1                  2
        //  / \              /     \
        // a   2            1       3
        //    / \     =>   / \     / \
        //   b   3        a   b   c   d
        //      / \
        //     c   d
        internal Node LeftRotation()
        {
            Debug.Assert(Right is not null);

            var oldRight = Right;
            SetLeftRight(Left, Right.Left);
            oldRight.SetLeftRight(this, oldRight.Right);
            return oldRight;
        }

        // Sample:
        //   1            1                  3
        //  / \          / \              /     \
        // a   2        a   3            1       2
        //    / \   =>     / \     =>   / \     / \
        //   3   d        b   2        a   b   c   d
        //  / \              / \
        // b   c            c   d
        internal Node InnerRightOuterLeftRotation()
        {
            Debug.Assert(Right is not null);
            Debug.Assert(Right.Left is not null);

            Node? newTop = Right.Left;
            Node? oldRight = Right;

            SetLeftRight(Left, Right.Left.Left);
            oldRight.SetLeftRight(oldRight.Left.Right, oldRight.Right);
            newTop.SetLeftRight(this, oldRight);

            return newTop;
        }

        // Sample:
        //     1              1              3
        //    / \            / \          /     \
        //   2   d          3   d        2       1
        //  / \     =>     / \     =>   / \     / \
        // a   3          2   c        a   b   c   d
        //    / \        / \
        //   b   c      a   b
        internal Node InnerLeftOuterRightRotation()
        {
            Debug.Assert(Left is not null);
            Debug.Assert(Left.Right is not null);

            Node? newTop = Left.Right;
            Node? oldLeft = Left;

            SetLeftRight(Left.Right.Right, Right);
            oldLeft.SetLeftRight(oldLeft.Left, oldLeft.Right.Left);
            newTop.SetLeftRight(oldLeft, this);

            return newTop;
        }
    }
}
