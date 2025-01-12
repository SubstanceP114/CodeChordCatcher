using System;
using System.Collections.Generic;

namespace CodeChordCatcher;

public readonly struct Chord(Note s, Note a, Note t, Note b, int offset = 0)
{
    private readonly Note soprano = s;
    private readonly Note alto = a;
    private readonly Note tenor = t;
    private readonly Note bass = b;
    public int Offset { get; init; } = offset;
    /// <summary>女高音</summary>
    public readonly Note Soprano => soprano.Octivate(Offset);
    /// <summary>女低音</summary>
    public readonly Note Alto => alto.Octivate(Offset);
    /// <summary>男高音</summary>
    public readonly Note Tenor => tenor.Octivate(Offset);
    /// <summary>男低音</summary>
    public readonly Note Bass => bass.Octivate(Offset);
    public readonly Note this[int index] => index switch
    {
        1 => Bass,
        2 => Tenor,
        3 => Alto,
        4 => Soprano,
        _ => throw new IndexOutOfRangeException()
    };
    public readonly Chord Octivate(int num) => new(soprano, alto, tenor, bass, Offset + num);
    /// <summary>
    /// 根据原位和弦产生一组和弦
    /// </summary>
    /// <param name="root">根音</param>
    /// <param name="mid">中音</param>
    /// <param name="upper">高音</param>
    /// <returns>一组可用于四部和声的和弦</returns>
    public static List<Chord> Generate(Note root, Note mid, Note upper) => [
        new(upper - 7, mid - 7, root - 7, root - 7),
        new(mid, upper - 7, root - 7, root - 7),
        new(root, upper - 7, mid - 7, root - 7),
        new(upper, root, mid - 7, root - 7),
        new(mid, root, upper - 7, root - 7),
        new(root + 7, mid, upper - 7, root - 7),
        new(mid, root, upper - 7, root - 7),
        new(mid + 7, upper, root, root - 7),
    ];
    /// <summary>
    /// 检查下一个和弦正确性
    /// </summary>
    /// <param name="next">待检查和弦</param>
    /// <param name="direction">是否检查四部同向</param>
    /// <param name="parallel">是否检查平五平八</param>
    /// <returns>待检查和弦是否可以放在该和弦之后</returns>
    public readonly bool CheckNext(Chord next, bool direction, bool parallel) =>
        (!direction || !CheckDirection(next)) && (!parallel || !CheckParallel(next));
    /// <summary>
    /// 检查四部同向
    /// </summary>
    /// <param name="next">待检查和弦</param>
    /// <returns>是否出现四部同向</returns>
    private readonly bool CheckDirection(Chord next) =>
        (Soprano - next.Soprano > 0 && Alto - next.Alto > 0 && Tenor - next.Tenor > 0 && Bass - next.Bass > 0)
        || (Soprano - next.Soprano < 0 && Alto - next.Alto < 0 && Tenor - next.Tenor < 0 && Bass - next.Bass < 0);
    /// <summary>
    /// 检查平五平八
    /// </summary>
    /// <param name="next">待检查和弦</param>
    /// <returns>是否出现平五平八</returns>
    private readonly bool CheckParallel(Chord next)
    {
        if (Bass == next.Bass && Tenor == next.Tenor && Alto == next.Alto && Soprano == next.Soprano) return false;
        for (int i = 1; i < 4; i++)
            for (int j = i + 1; j <= 4; j++)
            {
                int thisInterval = (this[j] - this[i]) % 7;
                if (thisInterval != 0 || thisInterval != 4) continue;
                int nextInterval = (next[j] - next[i]) % 7;
                if (nextInterval == thisInterval) return true;
            }
        return false;
    }
}
