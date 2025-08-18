using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Godot.EditorIntegration;

// TODO: This class contains the hardcoded templates copied from https://github.com/godotengine/godot/tree/825ef2387f87de1c350696886e6c50b039204cef/modules/mono/editor/script_templates. It will be replaced by `dotnet new` templates.
internal static class Templates
{
    public readonly struct TemplateData
    {
        public required string Id { get; init; }
        public required string DisplayName { get; init; }
        public required string Description { get; init; }
        public required string Content { get; init; }
    }

    public const string DefaultTemplateId = nameof(Object_empty);

    private static readonly Dictionary<string, TemplateData> _templatesById = [];
    private static readonly Dictionary<string, List<TemplateData>> _templatesByBaseType = [];

    static Templates()
    {
        AddTemplate(nameof(CharacterBody2D), new TemplateData()
        {
            Id = nameof(CharacterBody2D_basic_movement),
            DisplayName = "Basic Movement",
            Description = "Classic movement for gravity games (platformer, ...)",
            Content = CharacterBody2D_basic_movement,
        });
        AddTemplate(nameof(CharacterBody3D), new TemplateData()
        {
            Id = nameof(CharacterBody3D_basic_movement),
            DisplayName = "Basic Movement",
            Description = "Classic movement for gravity games (FPS, TPS, ...)",
            Content = CharacterBody3D_basic_movement,
        });
        AddTemplate(nameof(EditorPlugin), new TemplateData()
        {
            Id = nameof(EditorPlugin_plugin),
            DisplayName = "Plugin",
            Description = "Basic plugin template",
            Content = EditorPlugin_plugin,
        });
        AddTemplate(nameof(EditorScenePostImport), new TemplateData()
        {
            Id = nameof(EditorScenePostImport_basic_import_script),
            DisplayName = "Basic Import Script",
            Description = "Basic import script template",
            Content = EditorScenePostImport_basic_import_script,
        });
        AddTemplate(nameof(EditorScenePostImport), new TemplateData()
        {
            Id = nameof(EditorScenePostImport_no_comments),
            DisplayName = "No Comments",
            Description = "Basic import script template (no comments)",
            Content = EditorScenePostImport_no_comments
        });
        AddTemplate(nameof(EditorScript), new TemplateData()
        {
            Id = nameof(EditorScript_basic_editor_script),
            DisplayName = "Basic Editor Script",
            Description = "Basic editor script template",
            Content = EditorScript_basic_editor_script,
        });
        AddTemplate(nameof(Node), new TemplateData()
        {
            Id = nameof(Node_default),
            DisplayName = "Default",
            Description = "Base template for Node with default Godot cycle methods",
            Content = Node_default,
        });
        AddTemplate(nameof(GodotObject), new TemplateData()
        {
            Id = nameof(Object_empty),
            DisplayName = "Empty",
            Description = "Empty template suitable for all Objects",
            Content = Object_empty,
        });
        AddTemplate(nameof(VisualShaderNodeCustom), new TemplateData()
        {
            Id = nameof(VisualShaderNodeCustom_basic),
            DisplayName = "Basic",
            Description = "Basic template for a custom Visual Shader node",
            Content = VisualShaderNodeCustom_basic,
        });
    }

    private static void AddTemplate(string baseClassName, TemplateData template)
    {
        _templatesById[template.Id] = template;

        if (!_templatesByBaseType.TryGetValue(baseClassName, out var templates))
        {
            templates = [];
            _templatesByBaseType[baseClassName] = templates;
        }

        templates.Add(template);
    }

    public static bool ContainsTemplate(string templateId)
    {
        return _templatesById.ContainsKey(templateId);
    }

    public static IEnumerable<string> GetTemplateIds(string baseClassName)
    {
        if (_templatesByBaseType.TryGetValue(baseClassName, out var templates))
        {
            return templates.Select(template => template.Id);
        }

        return [];
    }

    public static string GetTemplateDisplayName(string templateId)
    {
        if (_templatesById.TryGetValue(templateId, out var template))
        {
            return template.DisplayName;
        }

        return "";
    }

    public static string GetTemplateDescription(string templateId)
    {
        if (_templatesById.TryGetValue(templateId, out var template))
        {
            return template.Description;
        }

        return "";
    }


    public static string GetTemplateContent(string templateId, string className, string baseClassName)
    {
        if (_templatesById.TryGetValue(templateId, out var template))
        {
            return template.Content
                .Replace("_CLASS_", className)
                .Replace("_BASE_", baseClassName);
        }

        return "";
    }

    public static bool TryGetTemplate(string templateId, [MaybeNullWhen(false)] out TemplateData template)
    {
        return _templatesById.TryGetValue(templateId, out template);
    }

    private const string CSharp = nameof(CSharp);

    [StringSyntax(CSharp)]
    private const string CharacterBody2D_basic_movement = """
        using Godot;
        using System;

        [GodotClass]
        public partial class _CLASS_ : _BASE_
        {
            public const float Speed = 300.0f;
            public const float JumpVelocity = -400.0f;

            protected override void _PhysicsProcess(double delta)
            {
                Vector2 velocity = Velocity;

                // Add the gravity.
                if (!IsOnFloor())
                {
                    velocity += GetGravity() * (float)delta;
                }

                // Handle Jump.
                if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
                {
                    velocity.Y = JumpVelocity;
                }

                // Get the input direction and handle the movement/deceleration.
                // As good practice, you should replace UI actions with custom gameplay actions.
                Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
                if (direction != Vector2.Zero)
                {
                    velocity.X = direction.X * Speed;
                }
                else
                {
                    velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
                }

                Velocity = velocity;
                MoveAndSlide();
            }
        }
        """;

    [StringSyntax(CSharp)]
    private const string CharacterBody3D_basic_movement = """
        using Godot;
        using System;

        [GodotClass]
        public partial class _CLASS_ : _BASE_
        {
            public const float Speed = 5.0f;
            public const float JumpVelocity = 4.5f;

            protected override void _PhysicsProcess(double delta)
            {
                Vector3 velocity = Velocity;

                // Add the gravity.
                if (!IsOnFloor())
                {
                    velocity += GetGravity() * (float)delta;
                }

                // Handle Jump.
                if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
                {
                    velocity.Y = JumpVelocity;
                }

                // Get the input direction and handle the movement/deceleration.
                // As good practice, you should replace UI actions with custom gameplay actions.
                Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
                Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
                if (direction != Vector3.Zero)
                {
                    velocity.X = direction.X * Speed;
                    velocity.Z = direction.Z * Speed;
                }
                else
                {
                    velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
                    velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
                }

                Velocity = velocity;
                MoveAndSlide();
            }
        }
        """;

    [StringSyntax(CSharp)]
    private const string EditorPlugin_plugin = """
        #if TOOLS
        using Godot;
        using System;

        [GodotClass(Tool = true)]
        public partial class _CLASS_ : _BASE_
        {
            protected override void _EnterTree()
            {
                // Initialization of the plugin goes here.
            }

            protected override void _ExitTree()
            {
                // Clean-up of the plugin goes here.
            }
        }
        #endif
        """;

    [StringSyntax(CSharp)]
    private const string EditorScenePostImport_basic_import_script = """
        #if TOOLS
        using Godot;
        using System;

        [GodotClass(Tool = true)]
        public partial class _CLASS_ : _BASE_
        {
            protected override GodotObject _PostImport(Node scene)
            {
                // Modify the contents of the scene upon import.
                return scene; // Return the modified root node when you're done.
            }
        }
        #endif
        """;

    [StringSyntax(CSharp)]
    private const string EditorScenePostImport_no_comments = """
        #if TOOLS
        using Godot;
        using System;

        [GodotClass(Tool = true)]
        public partial class _CLASS_ : _BASE_
        {
            protected override GodotObject _PostImport(Node scene)
            {
                return scene;
            }
        }
        #endif
        """;

    [StringSyntax(CSharp)]
    private const string EditorScript_basic_editor_script = """
        #if TOOLS
        using Godot;
        using System;

        [GodotClass(Tool = true)]
        public partial class _CLASS_ : _BASE_
        {
            // Called when the script is executed (using File -> Run in Script Editor).
            protected override void _Run()
            {
            }
        }
        #endif
        """;

    [StringSyntax(CSharp)]
    private const string Node_default = """
        using Godot;
        using System;

        [GodotClass]
        public partial class _CLASS_ : _BASE_
        {
            // Called when the node enters the scene tree for the first time.
            protected override void _Ready()
            {
            }

            // Called every frame. 'delta' is the elapsed time since the previous frame.
            protected override void _Process(double delta)
            {
            }
        }
        """;

    [StringSyntax(CSharp)]
    private const string Object_empty = """
        using Godot;
        using System;

        [GodotClass]
        public partial class _CLASS_ : _BASE_
        {
        }
        """;

    [StringSyntax(CSharp)]
    private const string VisualShaderNodeCustom_basic = """
        using Godot;
        using Godot.Collections;
        using System;

        [GodotClass(Tool = true)]
        public partial class VisualShaderNode_CLASS_ : _BASE_
        {
            protected override string _GetName()
            {
                return "_CLASS_";
            }

            protected override string _GetCategory()
            {
                return "";
            }

            protected override string _GetDescription()
            {
                return "";
            }

            protected override VisualShaderNode.PortType _GetReturnIconType()
            {
                return 0;
            }

            protected override int _GetInputPortCount()
            {
                return 0;
            }

            protected override string _GetInputPortName(int port)
            {
                return "";
            }

            protected override VisualShaderNode.PortType _GetInputPortType(int port)
            {
                return 0;
            }

            protected override int _GetOutputPortCount()
            {
                return 1;
            }

            protected override string _GetOutputPortName(int port)
            {
                return "result";
            }

            protected override VisualShaderNode.PortType _GetOutputPortType(int port)
            {
                return 0;
            }

            protected override string _GetCode(GodotArray<string> inputVars, GodotArray<string> outputVars, Shader.Mode mode, VisualShader.Type type)
            {
                return "";
            }
        }
        """;
}
