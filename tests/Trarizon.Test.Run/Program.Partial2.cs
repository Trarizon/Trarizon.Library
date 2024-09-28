partial class Program
{
   static void Run_Tempo()
    {
        var tempo = new Tempo(120f, 130f, 130f, 110f);
        tempo.GetTime(0).Print();
        tempo.GetTime(1).Print();
        tempo.GetTime(2).Print();
    }

    struct Tempo(float startTime, float endTime, float startBpm, float endBpm)
    {
        public readonly float GetTime(float beatIndex)
        {
            float deltaBpm = endBpm - startBpm;
            float deltaTime = endTime - startTime;
            float k = deltaBpm / deltaTime;
            float b = startBpm - startTime * k;
            float C = -k / 120f * startTime * startTime - b / 60f * startTime;
            return k switch {
                > 0 => (-b - MathF.Sqrt(b * b - 120f * k * (C - beatIndex))) / k,
                0 => 60 * (beatIndex - C) / b,
                < 0 => (-b + MathF.Sqrt(b * b - 120f * k * (C - beatIndex))) / k,
                _ => throw new InvalidOperationException(),
            };
        }
    }

}
