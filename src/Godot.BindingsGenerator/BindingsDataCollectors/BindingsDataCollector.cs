using Godot.BindingsGenerator.ApiDump;

namespace Godot.BindingsGenerator;

internal abstract class BindingsDataCollector
{
    /// <summary>
    /// Initialize the collector, setting up stubs of <see cref="TypeInfo"/>
    /// that may need to be available for other collectors.
    /// </summary>
    /// <param name="context">
    /// <see cref="BindingsData.CollectionContext"/> instance that contains the
    /// bindings generator input data and can be used to provide the output data.
    /// </param>
    public virtual void Initialize(BindingsData.CollectionContext context) { }

    /// <summary>
    /// Populate <see cref="BindingsData"/> with the necessary data to generate
    /// the bindings.
    /// Stubs of <see cref="TypeInfo"/> that were initialized in
    /// <see cref="Initialize(BindingsData, GodotApi, BindingsGeneratorOptions)"/>
    /// can now be fully populated with members.
    /// </summary>
    /// <param name="context">
    /// <see cref="BindingsData.CollectionContext"/> instance that contains the
    /// bindings generator input data and can be used to provide the output data.
    /// </param>
    public abstract void Populate(BindingsData.CollectionContext context);
}
