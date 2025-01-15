using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using LuaInterface;
using CodeChordCatcher.Views;
using System.Collections.ObjectModel;
using System;

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
        public ObservableCollection<ObservableCollection<Chord>>? Result { get; set; }
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
                void AddChords(string name, Executor.Group group)
                {
                    foreach (string chord in lua.GetTable(name).Values)
                    {
                        var notes = chord.Split();
                        var chords = notes.Length switch
                        {
                            3 => Chord.Generate(int.Parse(notes[0]), int.Parse(notes[1]), int.Parse(notes[2])),
                            4 => Chord.Generate(int.Parse(notes[3]), int.Parse(notes[2]), int.Parse(notes[1]), int.Parse(notes[0])),
                            _ => throw new ArgumentException()
                        };
                        chords.ForEach(c =>
                        {
                            executor.AddChord(group, c.Octivate(-1));
                            executor.AddChord(group, c);
                            executor.AddChord(group, c.Octivate(1));
                        });
                    }
                }
                AddChords("Tonic", Executor.Group.I);
                AddChords("Subdominant", Executor.Group.IV);
                AddChords("Dominant", Executor.Group.V);
            }
            Result = new(executor.Process(MelodySeq!.Split().ToList()
                .ConvertAll<Note>(x => int.Parse(x)))
                .ConvertAll<ObservableCollection<Chord>>(x => new(x)));
        }
    }
}
