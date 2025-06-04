namespace RentAutomation.Models
{ 
public static class RentPredictor
{
    public static (double slope, double intercept) Train(List<int> x, List<double> y)
    {
        int n = x.Count;
        double sumX = x.Sum();
        double sumY = y.Sum();
        double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
        double sumX2 = x.Sum(xi => xi * xi);

        double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        double intercept = (sumY - slope * sumX) / n;

        return (slope, intercept);
    }

    public static double Predict(int nextX, double slope, double intercept)
    {
        return slope * nextX + intercept;
    }
}

}

