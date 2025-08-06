namespace Stellar.Common;

[Flags]
public enum TrimmingOptions
{
    None = 0,
    Start = 1,
    End = 2,
    Both = Start | End
}