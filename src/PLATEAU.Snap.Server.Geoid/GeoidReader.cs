namespace PLATEAU.Snap.Server.Geoid;

public class GeoidReader : IDisposable
{
    private Stream GsiGeoStream { get; }

    public GeoidReader(string path)
    {
        this.GsiGeoStream = new FileStream(path, FileMode.Open, FileAccess.Read);
    }

    public GeoidReader(Stream stream)
    {
        this.GsiGeoStream = stream;
    }

    public Grid Read()
    {
        ReadOnlySpan<char> Error = " 999.0000".AsSpan();

        using var reader = new StreamReader(this.GsiGeoStream);
        string? header = reader.ReadLine();
        if (header == null)
        {
            throw new Exception("Header is null");
        }
        var headerSpan = header.TrimStart(' ').Split(' ').ToArray().AsSpan();
        var gridInfo = new GridInfo(
            Convert.ToInt32(float.Parse(headerSpan[0])),
            Convert.ToInt32(float.Parse(headerSpan[1])),
            float.Parse(headerSpan[2]),
            float.Parse(headerSpan[3]),
            int.Parse(headerSpan[4]),
            int.Parse(headerSpan[5]),
            int.Parse(headerSpan[6]),
            headerSpan[7].ToString()
        );

        var grid = new Grid(gridInfo);
        string? line;
        for (var latCount = 0; latCount < gridInfo.CountY; latCount++)
        {
            var list = new List<double>();
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                var geoidBuffer = line.AsSpan();
                for (var i = 0; i < 28; i++)
                {
                    var value = geoidBuffer.Slice(0 + i * 9, 9);
                    list.Add(value.SequenceEqual(Error) ? double.NaN : double.Parse(value));
                    if (list.Count == gridInfo.CountX)
                    {
                        break;
                    }
                }

                if (list.Count == gridInfo.CountX)
                {
                    break;
                }
                if (list.Count > gridInfo.CountX)
                {
                    throw new Exception("Invalid grid");
                }
            }
            grid.Add(list);
        }

        return grid;
    }

    public void Dispose()
    {
        if (this.GsiGeoStream != null)
        {
            this.GsiGeoStream.Dispose();
        }
    }
}
