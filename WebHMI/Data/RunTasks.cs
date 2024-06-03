using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace WebHMI.Data
{
    public class RunTasks : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private TaskManagerService _service;
        public ObservableCollection<TaskViewModel> Tasks { get; } = new ObservableCollection<TaskViewModel>();



        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand RefreshCommand { get; }



        // Constructor
        public RunTasks()
        {
            _service = new TaskManagerService(UpdateTaskStatus);
            _service.Run();

            StartCommand = new RelayCommand(_ => _service.Run());
            StopCommand = new RelayCommand(_ => _service.StopAllTasks());
            RefreshCommand = new RelayCommand(_ => _service.UpdateTasks());
        }

        private void UpdateTaskStatus(List<Task> runningTasks, List<string> taskNames)
        {
            // Update existing tasks or add new ones
            for (int i = 0; i < runningTasks.Count; i++)
            {
                var task = runningTasks[i];
                var taskName = (taskNames != null && i < taskNames.Count) ? taskNames[i] : "Unknown";

                var existingTaskViewModel = Tasks.FirstOrDefault(t => t.Name == taskName);
                if (existingTaskViewModel != null)
                {
                    // Only update the status if it's an existing task
                    existingTaskViewModel.Status = task.Status.ToString();
                }
                else
                {
                    // Add a new task if it doesn't already exist in the Tasks collection
                    var taskViewModel = new TaskViewModel
                    {
                        Name = taskName,
                        Status = task.Status.ToString()
                    };
                    Tasks.Add(taskViewModel);
                }
            }

            // Remove tasks that no longer exist
            var tasksToRemove = Tasks.Where(t => !taskNames.Contains(t.Name)).ToList();
            foreach (var taskToRemove in tasksToRemove)
            {
                Tasks.Remove(taskToRemove);
            }

            // Notify that the Tasks property has changed
            OnPropertyChanged(nameof(Tasks));
        }


    }
}

