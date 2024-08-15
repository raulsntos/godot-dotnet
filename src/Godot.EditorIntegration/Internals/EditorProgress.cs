using System;

namespace Godot.EditorIntegration.Internals;

internal sealed class EditorProgress : IDisposable
{
    public string Task { get; }

    private EditorProgress(string task, string label, int steps, bool canCancel = false)
    {
        Task = task;
        EditorInternal.ProgressAddTask(task, label, steps, canCancel);
    }

    ~EditorProgress()
    {
        // Should never rely on the GC to dispose EditorProgress.
        // It should be disposed immediately when the task finishes.
        GD.PushError(SR.EditorProgressDisposedByGC);
        Dispose();
    }

    public void Dispose()
    {
        EditorInternal.ProgressEndTask(Task);
        GC.SuppressFinalize(this);
    }

    public void Step(string state, int step = -1, bool forceRefresh = true)
    {
        EditorInternal.ProgressTaskStep(Task, state, step, forceRefresh);
    }

    public static void Invoke(string task, string label, int amount, Action<EditorProgress> action)
    {
        using var editorProgress = new EditorProgress(task, label, amount);
        action(editorProgress);
    }

    public static TResult Invoke<TResult>(string task, string label, int amount, Func<EditorProgress, TResult> func)
    {
        using var editorProgress = new EditorProgress(task, label, amount);
        return func(editorProgress);
    }
}
