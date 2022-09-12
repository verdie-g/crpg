using System;
using System.Collections.Generic;
using System.Text;
using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Balancing;
internal class RatedUser
{
    public RatedUser(CrpgUser user, RatingCalculator calculator, float rating, float ratingDeviation, float volatility)
    {
        Rating = new Rating(calculator, rating, ratingDeviation, volatility);
    }

    public CrpgUser Crpguser { get; set; } = new CrpgUser();
    public Rating Rating;
}
