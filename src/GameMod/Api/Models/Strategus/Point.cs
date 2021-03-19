namespace Crpg.GameMod.Api.Models.Strategus
{
    // Simplify version of NetTopologySuite.Geometries.Point to avoid adding a new reference.
    public class Point
    {
        public string Type => "Point";
        public double[] Coordinates { get; set; } = new double[2];
    }
}
