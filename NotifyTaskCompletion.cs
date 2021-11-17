using System.ComponentModel;

namespace TrainingTestApplication;

public class NotifyTaskCompletion : INotifyPropertyChanged
{
    private PropertyChangedEventHandler? _propertyChanged;

    public Task Task { get; protected set; }

    /// <summary>
    /// Allows Asynchronous Tasks to run and Notify the UI when completed
    /// </summary>
    /// <typeparam name="TResult">Return Type</typeparam>
    /// <param name="task">The Task to run</param>
    public NotifyTaskCompletion(Task task)
    {
        Task = task;

        switch (Task.IsCompleted)
        {
            case true:
                TaskCompletion = System.Threading.Tasks.Task.FromResult(0);
                break;
            case false:
                TaskCompletion = WatchTaskAsync();
                break;
        }
    }
    protected async Task WatchTaskAsync()
    {
        try
        {
            await Task;
        }
        catch
        {
            // ignore
        }

        var propertyChanged = _propertyChanged;

        if (propertyChanged == null) return;

        propertyChanged(this, new PropertyChangedEventArgs("Status"));
        propertyChanged(this, new PropertyChangedEventArgs("IsCompleted"));
        propertyChanged(this, new PropertyChangedEventArgs("IsNotCompleted"));

        if (Task.IsCanceled)
        {
            propertyChanged(this, new PropertyChangedEventArgs("IsCanceled"));
        }
        else if (Task.IsFaulted)
        {
            propertyChanged(this, new PropertyChangedEventArgs("IsFaulted"));
            propertyChanged(this, new PropertyChangedEventArgs("Exception"));
            propertyChanged(this, new PropertyChangedEventArgs("InnerException"));
            propertyChanged(this, new PropertyChangedEventArgs("ErrorMessage"));
            throw new Exception("Error Executing NotifyTaskCompletion", Task.Exception);
        }
        else
        {
            propertyChanged(this, new PropertyChangedEventArgs("IsSuccessfullyCompleted"));
            propertyChanged(this, new PropertyChangedEventArgs("Result"));
        }
    }
    public virtual Task TaskCompletion { get; private set; }
    public TaskStatus Status => Task.Status;
    public bool IsCompleted => Task.IsCompleted;
    public bool IsNotCompleted => !Task.IsCompleted;
    public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;
    public bool IsCanceled => Task.IsCanceled;
    public bool IsFaulted => Task.IsFaulted;
    public bool IsRunning => Task.Status == TaskStatus.Running;
    public AggregateException? Exception => Task.Exception;
    public Exception? InnerException => Exception?.InnerException;
    public string? ErrorMessage => InnerException?.Message;

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add
        {
            _propertyChanged += value;

            if(_propertyChanged != null)
                _propertyChanged(this, new PropertyChangedEventArgs("Status"));

        }
        remove => _propertyChanged -= value;
    }
}

public sealed class NotifyTaskCompletion<TResult> : NotifyTaskCompletion
{
    /// <summary>
    /// Allows Asynchronous Tasks to run and Notify the UI when completed
    /// </summary>
    /// <typeparam name="TResult">Return Type</typeparam>
    /// <param name="task">The Task to run</param>
    public NotifyTaskCompletion(Task<TResult> task) : base(task)
    {
        Task = task;
    }

    public new Task<TResult> Task { get; }

    public TResult? Result => (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default;
}
