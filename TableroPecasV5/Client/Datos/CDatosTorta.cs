using System;
using System.Net;
using System.Windows;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Datos
{

  public class CDatosTorta
  {
    public const double NODATA = -2000000000;

    public bool RestoDeDatos { get; set; }
    public string Nombre { get; set; }
    public string NombreOriginal { get; set; } // Largo=0 no se usa.
    public double Cantidad { get; set; }
    public double Valor { get; set; }
    public double MinimoRango { get; set; }
    public double MaximoRango { get; set; }
    public bool TieneDatos { get; set; }

    public CDatosTorta()
    {
      Nombre="";
      NombreOriginal = "";
      Cantidad = 0;
      Valor = NODATA;
      RestoDeDatos = false;
      MinimoRango = NODATA;
      MaximoRango = NODATA;
      TieneDatos = false;
    }

    public override string ToString()
    {
      return Nombre;
    }

    public CCondicion CondicionGajo(Int32 Orden, ModoFiltrar Modo,
          ClaseVariable Clase)
    {
      CCondicion Condicion = new CCondicion();
      Condicion.ColumnaCondicion = Orden;
      Condicion.Modo = Modo;
      Condicion.Clase = Clase;
      Condicion.ValorIgual = NombreOriginal;
      Condicion.ValorMinimo = MinimoRango;
      Condicion.ValorMaximo = MaximoRango;
      return Condicion;
    }

  }
}
