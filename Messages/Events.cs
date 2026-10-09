namespace Derevo;

internal sealed record GameFrame(
    IReadOnlyList<string> Rows,
    SessionState State,
    long Score,
    int Moves,
    string Caption,
    bool Intermediate) : IMessage;

internal sealed record Notice(string Text) : IMessage;
