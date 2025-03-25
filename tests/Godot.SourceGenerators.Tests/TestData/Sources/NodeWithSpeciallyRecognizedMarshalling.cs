using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace NS;

// TODO: Add tests for Dictionary when it supports the spread operator (potentially coming in .NET 10)

[GodotClass]
public partial class NodeWithSpeciallyRecognizedMarshalling : Node
{
    [BindProperty]
    public int[] ArrayOfInts { get; set; }

    [BindMethod]
    public void MethodThatTakesArrayOfInts(int[] array) { }

    [BindMethod]
    public int[] MethodThatReturnsArrayOfInts() => [];

    [BindProperty]
    public List<int> ListOfInts { get; set; }

    [BindMethod]
    public void MethodThatTakesListOfInts(List<int> list) { }

    [BindMethod]
    public List<int> MethodThatReturnsListOfInts() => [];

    [BindProperty]
    public bool[] ArrayOfBooleans { get; set; }

    [BindMethod]
    public void MethodThatTakesArrayOfBooleans(bool[] array) { }

    [BindMethod]
    public bool[] MethodThatReturnsArrayOfBooleans() => [];

    [BindProperty]
    public List<bool> ListOfBooleans { get; set; }

    [BindMethod]
    public void MethodThatTakesListOfBooleans(List<bool> list) { }

    [BindMethod]
    public List<bool> MethodThatReturnsListOfBooleans() => [];
}
