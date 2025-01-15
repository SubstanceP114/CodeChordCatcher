using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using LuaInterface;
using CodeChordCatcher.Views;
using System.Collections.ObjectModel;

namespace CodeChordCatcher.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? chordSrc;
        [ObservableProperty]
        private string? melodySeq;
        [ObservableProperty]
        private bool parallel, direction, reverse;
        public ObservableCollection<ObservableCollection<Chord>>? Result { get; set; } = new ObservableCollection<ObservableCollection<Chord>>([new([new(1, 3, 5, 7), new(2, 4, 6, 8)]), new([new(1, 1, 4, 5), new(2, 0, 4, 8)]), new([new(1, 1, 4, 5), new(2, 0, 4, 8)]), new([new(1, 1, 4, 5), new(2, 0, 4, 8)])]);
        [RelayCommand]
        private void FindSrc() => FindAsync();
        private async void FindAsync()
        {
            var file = await MainWindow.Instance.StorageProvider.OpenFilePickerAsync(new()
            {
                Title = "选择和弦配置文件",
                AllowMultiple = false,
                FileTypeFilter = [new("Lua文件") { Patterns = ["*.lua"] }]
            });
            if (file.Count == 0) return;
            ChordSrc = file[0].Path.AbsolutePath;
        }
        [RelayCommand]
        private void Generate()
        {
            if (MelodySeq == null || ChordSrc == null) return;
            Executor executor = new()
            {
                Direction = !Direction,
                Parallel = !Parallel,
                Reverse = !Reverse
            };
            using (Lua lua = new())
            {
                lua.DoFile(ChordSrc);
                MelodySeq = lua.GetTable("Chord")[2] as string;
            }
            executor.Process(MelodySeq!.Split().ToList().ConvertAll<Note>(x => int.Parse(x)));
        }
    }
}
