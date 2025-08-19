using System.Diagnostics.CodeAnalysis;

namespace Stellar.Common;

[ExcludeFromCodeCoverage(Justification = "No use cases yet.")]
public class ProgressMeter(int capacity = 100, int step = 10)
{
    private readonly int step = step.Clamp(1, capacity);
    
    private readonly HashSet<int> marks = [];

    public decimal Percent { get; private set; } = 0;

    public bool Check(int progress)
    {
        Percent = (decimal)progress / capacity;

        if (progress <= 0 ||
            progress >= capacity ||
            progress % step != 0 ||
            marks.Contains(progress))
        {
            return false;
        }

        marks.Add(progress);

        return true;
    }
}