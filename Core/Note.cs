namespace CodeChordCatcher.Core;

public readonly struct Note
{
    public int Pitch { get; init; }
    public int Offset { get; init; }
    public Note(int pitch, int offset)
    {
        Pitch = pitch;
        Offset = offset;
    }
    public Note(int num)
    {
        num--;
        Offset = 0;
        while (num < 0)
        {
            num += 7;
            Offset--;
        }
        Pitch = num % 7 + 1;
        Offset += num / 7;
    }
    public static implicit operator int(Note note) => note.Pitch + note.Offset * 7;
    public static implicit operator Note(int num) => new(num);
    public readonly Note Octivate(int num) => new(Pitch, Offset + num);
}