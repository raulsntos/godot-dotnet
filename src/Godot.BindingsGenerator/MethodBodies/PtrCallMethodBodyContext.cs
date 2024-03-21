
using System.Collections.Generic;

namespace Godot.BindingsGenerator;

internal class PtrCallMethodBodyContext : CallMethodBodyContext
{
    public IList<PtrMarshallerWriter> ParameterMarshallers { get; set; } = [];
    public PtrMarshallerWriter? ReturnTypeMarshaller { get; set; }

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
}
