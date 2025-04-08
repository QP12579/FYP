public interface IBuff
{
    float time { get; set; }
    float baseStats {  get; set; }
    float buffStats {  get; set; }
    void Buff();
}
