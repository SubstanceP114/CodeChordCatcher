using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using LuaInterface;
using CodeChordCatcher.Views;
using CodeChordCatcher.Core;
using System.Collections.ObjectModel;
using System;
using System.IO;

namespace CodeChordCatcher.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private const string CONFIG_PATH = "Config.lua";
        private const string DEFAULT_CHORD_PATH = "Traid.lua";
        private const string EXTENDED_CHORD_PATH = "TraidAndSeventh.lua";
        private const string DEFAULT_CHORD =
@"Tonic = -- 主功能组
{
    '1 3 5',
    '3 5 7',
    '5 7 9',
}
Subdominant = -- 下属功能组
{
    '4 6 8',
    '6 8 10',
    '2 4 6',
}
Dominant = -- 属功能组
{
    '5 7 9',
    '7 9 11',
    '3 5 7',
}";
        private const string EXTENDED_CHORD =
@"Tonic = -- 主功能组
{
    '1 3 5',
    '3 5 7',
    '5 7 9',
    '1 3 5 7',
    '3 5 7 9',
    '5 7 9 11',
}
Subdominant = -- 下属功能组
{
    '4 6 8',
    '6 8 10',
    '2 4 6',
    '4 6 8 10',
    '6 8 10 12',
    '2 4 6 8',
}
Dominant = -- 属功能组
{
    '5 7 9',
    '7 9 11',
    '3 5 7',
    '5 7 9 11',
    '7 9 11 13',
    '3 5 7 9',
}";
        public MainWindowViewModel()
        {
            if (!File.Exists(CONFIG_PATH))
                File.WriteAllLines(CONFIG_PATH, [
                    $"ChordSrc = \"{DEFAULT_CHORD_PATH}\"",
                    "Direction = false",
                    "Parallel = false",
                    "Reverse = false",
                ]);
            if (!File.Exists(DEFAULT_CHORD_PATH))
                File.WriteAllText(DEFAULT_CHORD_PATH, DEFAULT_CHORD);
            if (!File.Exists(EXTENDED_CHORD_PATH))
                File.WriteAllText(EXTENDED_CHORD_PATH, EXTENDED_CHORD);
            using (Lua lua = new())
            {
                lua.DoFile(CONFIG_PATH);
                ChordSrc = lua.GetString("ChordSrc");
                Direction = (bool)lua["Direction"];
                Parallel = (bool)lua["Parallel"];
                Reverse = (bool)lua["Reverse"];
            }
        }
        [ObservableProperty]
        private string? chordSrc;
        [ObservableProperty]
        private string? melodySeq;
        [ObservableProperty]
        private bool direction, parallel, reverse;
        [ObservableProperty]
        private ObservableCollection<ObservableCollection<Chord>>? result;
        [ObservableProperty]
        private string? tips;
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
            Tips = null;
            Executor executor = new()
            {
                Direction = !Direction,
                Parallel = !Parallel,
                Reverse = !Reverse
            };
            using (Lua lua = new())
            {
                if (!File.Exists(ChordSrc))
                {
                    Tips = "文件路径有误";
                    return;
                }
                lua.DoFile(ChordSrc);
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
                try
                {
                    AddChords("Tonic", Executor.Group.I);
                    AddChords("Subdominant", Executor.Group.IV);
                    AddChords("Dominant", Executor.Group.V);
                }
                catch { Tips = "和弦配置有误"; }
            }
            try
            {
                Result = new(executor.Process(MelodySeq!.Split()
                    .Where(x => x != "").ToList()
                    .ConvertAll<Note>(x => int.Parse(x)))
                    .ConvertAll<ObservableCollection<Chord>>(x => new(x)));
                Tips = $"共{Result.Count}条结果";
            }
            catch { Tips = "和弦生成失败"; }
        }
    }
}
