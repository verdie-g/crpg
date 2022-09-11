using System;
using System.Collections.Generic;
using System.Linq;

namespace Crpg.Module.Balancing;

/// <summary>
/// i don't know yet.
/// </summary>
internal interface IcRPGTrueSkillRatingSystem
{

}

internal class CRPGTrueSkillRatingSystem : IcRPGTrueSkillRatingSystem
{
    private double V(double t, double epsilon)
    {
        return MathHelper.N(t - epsilon, 0, 1);
    }

    private double W(double t, double epsilon)
    {
        return V(t, epsilon) * (V(t, epsilon) + t - epsilon);
    } 
}
