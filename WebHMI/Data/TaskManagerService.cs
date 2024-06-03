using CommonTools;

namespace WebHMI.Data
{
    /// <summary>
    /// Represents information associated with each task.
    /// </summary>
    public class TaskInfo
    {
        public string PlcName { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
    }

    /// <summary>
    /// Service to manage tasks associated with PLCs.
    /// </summary>
    public class TaskManagerService
    {
        private readonly Dictionary<Task, TaskInfo> _taskInfoMap = new Dictionary<Task, TaskInfo>();
        private readonly TagManager _tagManager;
        private readonly List<PLCManager> _plcManagers = new List<PLCManager>();
        private readonly Dictionary<Task, string> _taskToPlcNameMap = new Dictionary<Task, string>();
        private readonly List<Task> _runningTasks = new List<Task>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Action<List<Task>, List<string>> _updateCallback;

        /// <summary>
        /// Initializes a new instance of TaskManagerService.
        /// </summary>
        /// <param name="updateCallback">Callback method to update tasks' status.</param>
        public TaskManagerService(Action<List<Task>, List<string>> updateCallback)
        {
            _updateCallback = updateCallback ?? throw new ArgumentNullException(nameof(updateCallback));
            _tagManager = new TagManager();
        }

        /// <summary>
        /// Start the service, load PLC configurations, and initialize tasks for each PLC.
        /// </summary>
        public void Run()
        {
            _cts = new CancellationTokenSource();

            var plcConfigs = _tagManager.LoadConfig();
            foreach (var plcConfig in plcConfigs)
            {
                InitializePLCTask(plcConfig.IPAddress, plcConfig.Name);
            }

            UpdateTasks();
        }

        /// <summary>
        /// Initializes a task to monitor the specified PLC.
        /// </summary>
        private void InitializePLCTask(string ipAddress, string plcName)
        {
            var localCts = new CancellationTokenSource();
            var plcManager = new PLCManager(ipAddress, plcName);
            plcManager.ConnectToPLC();

            var tagsToMonitor = _tagManager.LoadConfig().FirstOrDefault(p => p.Name == plcName)?.Tags;

            if (tagsToMonitor == null)
            {
                Log.ToFile($"No tags found for PLC {plcName}.");
                return;
            }

            var task = Task.Run(() =>
            {
                while (!localCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        plcManager.GetPlcTagValues(tagsToMonitor);
                    }
                    catch (OperationCanceledException)
                    {
                        // Task has been canceled.
                    }
                    catch (Exception ex)
                    {
                        Log.ToFile($"Error reading from PLC {plcName}: {ex.Message}");
                    }

                    Thread.Sleep(1000);
                }
            }, localCts.Token);

            var taskInfo = new TaskInfo
            {
                PlcName = plcName,
                CancellationTokenSource = localCts
            };

            _taskInfoMap[task] = taskInfo;
            _plcManagers.Add(plcManager);
            _runningTasks.Add(task);
            _taskToPlcNameMap[task] = plcName;
        }

        /// <summary>
        /// Stops all the running tasks.
        /// </summary>
        public void StopAllTasks()
        {
            foreach (var taskInfo in _taskInfoMap.Values)
            {
                taskInfo.CancellationTokenSource.Cancel();
            }
            _cts.Cancel();
            var taskNames = _runningTasks.Select(t => _taskToPlcNameMap[t]).ToList();
            _updateCallback?.Invoke(_runningTasks, taskNames);
        }


        /// <summary>
        /// Notifies the UI about the status of tasks.
        /// </summary>
        public void UpdateTasks()
        {
            var taskNames = _runningTasks.Select(t => _taskToPlcNameMap[t]).ToList();
            _updateCallback?.Invoke(_runningTasks, taskNames);
        }

        /// <summary>
        /// Stops a specific task associated with the given PLC.
        /// </summary>
        public void StopTask(string plcName)
        {
            var taskInfo = _taskInfoMap.Values.FirstOrDefault(info => info.PlcName == plcName);

            if (taskInfo != null)
            {
                taskInfo.CancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Starts a task to monitor the specified PLC.
        /// </summary>
        public void StartTask(string ipAddress, string plcName)
        {
            // Only start the task if it's not already running
            if (!_taskInfoMap.Values.Any(info => info.PlcName == plcName))
            {
                InitializePLCTask(ipAddress, plcName);
            }
        }
    }
}