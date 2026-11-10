class MiningResult
{
    public int Found { get; set; }
    public int Registered { get; set; }
    public int Skipped { get; set; }

    public List<string> Errors { get; } = new();
}