
using System.Collections.Generic;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal class VarargCallMethodBodyContext : CallMethodBodyContext
{
    public ParameterInfo VarargParameter { get; set; } = null!;

    public IList<VariantMarshallerWriter> ParameterMarshallers { get; set; } = [];
    public VariantMarshallerWriter? ReturnTypeMarshaller { get; set; }

    /// <summary>
    /// Indicates which parameters, if any, should use the aux variable
    /// thas was initialized by the marshaller during the setup phase.
    /// </summary>
    public IList<bool> ParametersWithPreSetup { get; set; } = [];

    /// <summary>
    /// Indicates whether the return parameter should use the aux variable
    /// that was initialized by the marshaller during the setup phase.
    /// </summary>
    public bool ReturnTypeWithPreSetup { get; set; }

    public string ArgsCountVariableName { get; set; } = "__argsCount";
}
