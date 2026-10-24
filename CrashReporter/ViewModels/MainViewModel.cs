using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CrashReporter.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string ErrorMessage { get; set; }

        public async Task LoadCrashFile()
        {
            try
            {
                var path = Path.Combine(Path.GetTempPath(), "crash-report.json");

                var msg = File.ReadAllText(path);

                ErrorMessage = msg;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
