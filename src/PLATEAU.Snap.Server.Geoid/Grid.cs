namespace PLATEAU.Snap.Server.Geoid;

public class Grid
{
    public GridInfo GridInfo { get; }

    public AxisLatitude AxisLatitude { get; } = new AxisLatitude();

    public AxisLongitude this[int indx] => this.AxisLatitude[indx];

    public Grid(GridInfo gridInfo)
    {
        this.GridInfo = gridInfo;
    }

    public void Add(List<double> geoidList)
    {

        if (geoidList.Count != this.GridInfo.CountX)
        {
            throw new ArgumentException("Invalid longitude count");
        }

        AxisLongitude axisLongitude = new AxisLongitude();
        axisLongitude.AddRange(geoidList);

        this.AxisLatitude.Add(axisLongitude);
        if (this.AxisLatitude.Count > this.GridInfo.CountY)
        {
            throw new ArgumentException("Invalid latitude index");
        }
    }

    public double GetGeoidHeight(double x, double y)
    {
        var grid = this.GridInfo;

        double gridX = (x - grid.MinX) * grid.DenominatorX;
        double gridY = (y - grid.MinY) * grid.DenominatorY;

        if (gridX < 0.0 || gridY < 0.0)
        {
            return double.NaN;
        }

        int ix = (int)Math.Floor(gridX);
        int iy = (int)Math.Floor(gridY);
        double xResidual = gridX - ix;
        double yResidual = gridY - iy;

        if (ix >= grid.CountX || iy >= grid.CountY)
        {
            return double.NaN;
        }
        else
        {
            return Bilinear(
                xResidual,
                yResidual,
                this.AxisLatitude[iy][ix],
                LookupOrNan(ix + 1, iy),
                LookupOrNan(ix, iy + 1),
                LookupOrNan(ix + 1, iy + 1)
            );
        }
    }

    private double LookupOrNan(int x, int y)
    {
        if (y > this.AxisLatitude.Count - 1 || x > this.AxisLatitude[y].Count - 1)
        {
            return double.NaN;
        }
        return this.AxisLatitude[y][x];
    }

    private double Bilinear(double x, double y, double v00, double v01, double v10, double v11)
    {
        if (x == 0.0 && y == 0.0)
        {
            return v00;
        }
        else if (x == 0.0)
        {
            return v00 * (1.0 - y) + v10 * y;
        }
        else if (y == 0.0)
        {
            return v00 * (1.0 - x) + v01 * x;
        }
        else
        {
            return v00 * (1.0 - x) * (1.0 - y) + v01 * x * (1.0 - y) + v10 * (1.0 - x) * y + v11 * x * y;
        }
    }
}
