using Verse;

namespace FrontierDevelopments.Cyberization.Harmony
{
    public interface AddedPartEffectivenessModifier
    {
        float PartEffectiveness { get; }
    }

    public class AddedPartEffectivenessModifierUtils
    {
        public static string EffectivenessString(string title, float effectiveness)
        {
            return title + ": -" + 100 * (1 - effectiveness) + "% " + "PartEfficiency".Translate();
        }
    }
}