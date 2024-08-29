using System;

public class ArtComponentes
{
    public Guid Guid { get; set; }
    public string Articulo { get; set; }
    public string ArtDes { get; set; }
    public decimal? ArtPrecioCoste { get; set; }
    public decimal? ArtPrecioPVP { get; set; }
    public string CalcArticuloDesc { get; set; }
    public string ArtTipoVenta { get; set; }

    public decimal? Peso { get; set; }
    public decimal? PesoSinMerma { get; set; }
}