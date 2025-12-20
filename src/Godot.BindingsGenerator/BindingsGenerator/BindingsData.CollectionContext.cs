using System;
using System.Collections.Generic;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;
using Godot.BindingsGenerator.Logging;

namespace Godot.BindingsGenerator;

partial class BindingsData
{
    /// <summary>
    /// Context provided to <see cref="BindingsDataCollector"/> instances to access
    /// input data (i.e.: Godot API information, bindings generator options) and
    /// provide the output data (i.e.: the generated type information).
    /// </summary>
    internal sealed class CollectionContext
    {
        private readonly BindingsData _data;

        /// <summary>
        /// Contains the Godot API information dumped from a Godot build.
        /// <see cref="BindingsDataCollector"/> instances should use this as input
        /// to populate the generated API.
        /// </summary>
        public GodotApi Api { get; }

        /// <summary>
        /// Options that were provided to the generator to configure how the
        /// bindings should be generated.
        /// </summary>
        public BindingsGeneratorOptions Options { get; }

        /// <summary>
        /// Logger that can be used to log messages, warnings, and errors.
        /// </summary>
        public ILogger Logger { get; }

        public CollectionContext(BindingsData data, GodotApi api, BindingsGeneratorOptions options, ILogger logger)
        {
            _data = data;
            Api = api;
            Options = options;
            Logger = logger;
        }

        public TypeDB TypeDB => _data._typeDB;

        public IReadOnlyDictionary<string, GodotSingletonInfo> Singletons => _data._singletons;

        /// <summary>
        /// Add a <see cref="TypeInfo"/> to the collection of generated types
        /// with the path that the type will be written to.
        /// </summary>
        /// <param name="path">The path that the generated type will be written to.</param>
        /// <param name="type">The type that will be generated.</param>
        /// <param name="configure">Callback to configure the generation.</param>
        public void AddGeneratedType(string path, TypeInfo type, Action<GeneratedTypeData>? configure = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            ArgumentNullException.ThrowIfNull(type);

            if (_data._generatedTypesByPath.ContainsKey(path))
            {
                throw new ArgumentException($"A generated type has already been added with path '{path}'.", nameof(path));
            }

            var generatedTypeData = new GeneratedTypeData(path, type);
            configure?.Invoke(generatedTypeData);
            _data._generatedTypesByPath[path] = generatedTypeData;
            _data._generatedTypes.Add(type);
        }

        /// <summary>
        /// Check if <paramref name="type"/> is a generated type.
        /// </summary>
        /// <param name="type">The type to check for.</param>
        /// <returns><see langword="true"/> if the type is generated.</returns>
        public bool IsTypeGenerated(TypeInfo type)
        {
            return _data._generatedTypes.Contains(type);
        }

        /// <summary>
        /// Get the <see cref="TypeInfo"/> for an inline array of the given length.
        /// If one doesn't exist, it will be created, added, and returned.
        /// </summary>
        /// <param name="length">The length of the inline array.</param>
        /// <returns>An inline array type of the requested length.</returns>
        public TypeInfo GetOrAddInlineArray(int length)
        {
            if (length is >= 2 and <= 16)
            {
                // Use runtime provided inline arrays for lengths 2 to 16.
                return KnownTypes.InlineArrayOf(length);
            }

            if (_data._inlineArrays.TryGetValue(length, out var inlineArrayType))
            {
                // Inline array already defined for this length.
                return inlineArrayType;
            }

            inlineArrayType = new TypeInfo($"Buffer{length}", "Godot.NativeInterop")
            {
                TypeAttributes = TypeAttributes.ValueType,
                VisibilityAttributes = VisibilityAttributes.Public,
                GenericTypeArgumentCount = 1,
                Attributes = { $"[global::System.Runtime.CompilerServices.InlineArray({length})]" },
                DeclaredFields =
                {
                    new FieldInfo("_element0", new TypeParameterInfo("T"))
                    {
                        VisibilityAttributes = VisibilityAttributes.Private,
                    },
                },
            };
            _data._inlineArrays[length] = inlineArrayType;

            AddGeneratedType($"InlineArrays/{inlineArrayType.Name}.cs", inlineArrayType);
            return inlineArrayType;
        }

        /// <summary>
        /// Parse the <see cref="GodotArgumentInfo.DefaultValue"/> and assign the resulting
        /// expression to the <see cref="ParameterInfo.DefaultValue"/> of the given
        /// <paramref name="parameter"/>.
        /// </summary>
        /// <param name="engineArgument">Argument information from the Godot API dump.</param>
        /// <param name="parameter">Parameter information for the generated bindings.</param>
        public void ApplyDefaultValue(GodotArgumentInfo engineArgument, ParameterInfo parameter)
        {
            if (string.IsNullOrEmpty(engineArgument.DefaultValue))
            {
                // This parameter does not have a default value.
                return;
            }

            parameter.DefaultValue = TypeDB.GetDefaultValueExpression(parameter.Type, engineArgument.DefaultValue);

            // Default parameter values must be compile-time constants,
            // otherwise use Nullable<T> and assign the default value inside
            // the method's body.
            if (parameter.DefaultValue != "default" && !TypeDB.CanTypeBeConstant(parameter.Type))
            {
                parameter.DefaultValue = "default";

                // Some types can't be Nullable<T> (i.e.: ref structs),
                // we'll still assign the default value inside the method's body
                // but won't change the parameter's type.
                if (ShouldTypeBeNullableForDefaultParameter(parameter.Type))
                {
                    parameter.Type = KnownTypes.NullableOf(parameter.Type);
                }
            }

            static bool ShouldTypeBeNullableForDefaultParameter(TypeInfo type)
            {
                if (TypeDB.CanTypeBeConstant(type))
                {
                    // If the type can be compile-time constant we don't need to
                    // use Nullable<T>.
                    return false;
                }

                if (type == KnownTypes.GodotVariant)
                {
                    // The Variant type already includes null as one of its union types
                    // so instead of wrapping it in a Nullable<T> we should use it with
                    // a default value to represent a null Variant.
                    return false;
                }

                if (!type.IsValueType)
                {
                    // Only unmanaged types can be used as the generic type argument
                    // of Nullable<T>.
                    return false;
                }

                if (type.IsByRefLike)
                {
                    // Ref structs can't be used in generic type arguments so a
                    // Nullable<T> of a ref struct can't be constructed.
                    return false;
                }

                return true;
            }
        }
    }
}
