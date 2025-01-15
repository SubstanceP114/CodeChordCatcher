using System.Collections.Generic;

namespace CodeChordCatcher.Core;

public class Executor
{
    public enum Group { I, IV, V }
    /// <summary>
    /// 检查四部同向
    /// </summary>
    public bool Direction { get; set; } = true;
    /// <summary>
    /// 检查平五平八
    /// </summary>
    public bool Parallel { get; set; } = true;
    /// <summary>
    /// 检查反功能进行
    /// </summary>
    public bool Reverse { get; set; } = true;
    /// <summary>
    /// 和弦总集，通过功能组和旋律音索引到一组和弦
    /// </summary>
    private readonly Dictionary<Group, Dictionary<Note, List<Chord>>> chords = new()
    {
        {Group.I,[]},
        {Group.IV,[]},
        {Group.V,[]},
    };
    private readonly Dictionary<Group, List<Group>> groups = new()
    {
        {Group.I,[Group.V,Group.IV]},
        {Group.IV,[Group.V,Group.I]},
        {Group.V,[Group.I]},
    };
    /// <summary>
    /// 向指定功能组添加一个和弦
    /// </summary>
    /// <param name="group">目标功能组</param>
    /// <param name="chord">待添加和弦</param>
    public void AddChord(Group group, Chord chord)
    {
        var targetGroup = chords[group];
        if (targetGroup.TryGetValue(chord.Soprano, out var targetList)) targetList.Add(chord);
        else targetGroup.Add(chord.Soprano, [chord]);
    }
    private List<List<Chord>> Predicate(Group group, Chord chord, List<Note> notes, int index, List<Chord> sequence)
    {
        sequence.Add(chord);
        if (index == notes.Count) return [sequence];
        List<List<Chord>> result = [];
        var note = notes[index];
        foreach (var g in groups[group])
            if (chords[g].TryGetValue(note, out var list))
                foreach (var c in list)
                    if (chord.CheckNext(c, Direction, Parallel))
                        result.AddRange(Predicate(g, c, notes, index + 1, new(sequence)));
        return result;
    }
    private List<List<Chord>> Predicate(Chord chord, List<Note> notes, int index, List<Chord> sequence)
    {
        sequence.Add(chord);
        if (index == notes.Count) return [sequence];
        List<List<Chord>> result = [];
        var note = notes[index];
        foreach (var kvp in chords)
            if (kvp.Value.TryGetValue(note, out var list))
                foreach (var c in list)
                    result.AddRange(Predicate(c, notes, index + 1, new(sequence)));
        return result;
    }
    public List<List<Chord>> Process(List<Note> notes)
    {
        List<List<Chord>> result = [];
        var note = notes[0];
        foreach (var kvp in chords)
            if (kvp.Value.TryGetValue(note, out var list))
                foreach (var c in list)
                    result.AddRange(Reverse ?
                        Predicate(kvp.Key, c, notes, 1, []) :
                        Predicate(c, notes, 1, []));
        return result;
    }
}
