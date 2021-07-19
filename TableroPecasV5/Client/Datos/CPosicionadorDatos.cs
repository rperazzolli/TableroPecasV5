using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Plantillas;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Datos
{
  public class CPosicionadorDatos
  {
    private CProveedorComprimido mProveedor;
    private CVinculoIndicadorCompletoCN mVinculador;
    private string mszColumnaLng;
    private string mszColumnaLat;
    private string mszColumnaGeo;
    private string mszColumnaNueva;
    private CCapaWFSCN mCapaVinculo; // para traducir a coordenadas.
    private CCapaWFSCN mCapaValor; // para traducir a un valor desde coordenadas.
    private List<Point> mPuntos;
    private Int32 mPosLat;
    private Int32 mPosLng;
    private Int32 mPosColumnaGIS;
    private List<string> mszTextoPosicion;
    private bool mbDesdeWFS;
    private string mszValorResto;
    private List<Int32> mPosiciones;

    public List<Location> PuntosFiltro { get; set; }
    public bool FiltrarPorPoligono { get; set; }
    public bool PorCodigo { get; set; }

    public CPosicionadorDatos()
    {
      PorCodigo = false;
    }

    protected Datos.CColumnaTexto AgregarColumnaGIS(string Nombre)
    {
      Datos.CColumnaTexto ColExtra = new Datos.CColumnaTexto();
      ColExtra.Clase = ClaseVariable.Texto;
      ColExtra.Nombre = Nombre;
      ColExtra.Orden = mProveedor.Columnas.Count;
      mProveedor.Columnas.Add(ColExtra);
      return ColExtra;
    }

    private void AgregarPunto(string Valor)
    {
      foreach (CVinculoDetalleCN Detalle in mVinculador.Detalles)
      {
        if (Detalle.ValorAsociado.Equals(Valor, StringComparison.OrdinalIgnoreCase))
        {
          mPuntos.Add(CRutinas.PosicionValor(Detalle.Posicion, mCapaVinculo));
          return;
        }
      }
      mPuntos.Add(CRutinas.PuntoIncorrecto()); // si no lo encuentra pone un dato como faltante.
    }

    private void ArmarVectorPosiciones()
    {
      mPuntos = new List<Point>();
      switch (mVinculador.Vinculo.ClaseVinculada)
      {
        case ClaseVinculo.Coordenadas:
        case ClaseVinculo.Areas:
        case ClaseVinculo.Marcador:
          CColumnaBase Columna = mProveedor.ColumnaNombre(mszColumnaGeo);
          if (Columna == null)
          {
            throw new Exception("No encuentra " + mszColumnaGeo);
          }
          foreach (string Valor in Columna.ListaValores)
          {
            AgregarPunto(Valor);
          }
          break;
      }
    }

    private void DeterminarPosicionesColumnas(CVinculoIndicadorCN Vinculo)
    {
      switch (Vinculo.ClaseVinculada)
      {
        case ClaseVinculo.ColumnasGIS:
          mPosLat = mProveedor.PosicionColumna(Vinculo.ColumnaLat);
          mPosLng = mProveedor.PosicionColumna(Vinculo.ColumnaLng);
          break;
        default:
          mPosColumnaGIS = mProveedor.PosicionColumna(mszColumnaGeo);
          mPosLat = (mszColumnaLat == null || mszColumnaLat.Length == 0 ? -1 : mProveedor.PosicionColumna(mszColumnaLat));
          mPosLng = (mszColumnaLng == null || mszColumnaLng.Length == 0 ? -1 : mProveedor.PosicionColumna(mszColumnaLng));
          break;
      }
    }


    private void TraducirVectorPosiciones(double Rango, bool UsaCentro)
    {
      mszTextoPosicion = new List<string>();
      if (mbDesdeWFS)
      {
        switch (mVinculador.Vinculo.ClaseVinculada)
        {
          case ClaseVinculo.Areas:
          case ClaseVinculo.Coordenadas:
          case ClaseVinculo.Marcador:
          case ClaseVinculo.Lineas:
            foreach (Point Punto in mPuntos)
            {
              mszTextoPosicion.Add(CRutinas.TextoPunto(Punto,
                  mCapaValor, Rango, mszValorResto, PorCodigo, UsaCentro).ToUpper());
            }
            break;
          case ClaseVinculo.ColumnasGIS:
            mszTextoPosicion.AddRange(CRutinas.ExtraerListaElementosWFS(
                  mCapaValor, PorCodigo));
            break;
        }
        //    break;
        //}
        mszTextoPosicion.Add(mszValorResto);
      }
      else
      {
        mszTextoPosicion.Add(CRutinas.ADENTRO);
        mszTextoPosicion.Add(CRutinas.AFUERA);
      }
    }

    private void ObtenerVectorEnteroPosiciones(Datos.CColumnaBase ColAgregada)
    {
      mPosiciones = new List<int>();
      for (Int32 i = 0; i < mszTextoPosicion.Count; i++)
      {
        mPosiciones.Add(ColAgregada.PosicionValorIgual(mszTextoPosicion[i])); //i);
      }
    }

    private Point DeterminarCoordenadasDesdeColumnas(Datos.CLineaComprimida Linea)
    {
      if (mPosLat >= 0 && mPosLng >= 0)
      {
        return new Point((double)mProveedor.Columnas[mPosLng].Valores[Linea.Codigos[mPosLng]],
              (double)mProveedor.Columnas[mPosLat].Valores[Linea.Codigos[mPosLat]]);
      }
      else
      {
        return CRutinas.PuntoIncorrecto();
      }
    }

    private List<CPosicionWFSCN> PosicionesDesdePuntos(
          List<Location> Puntos)
    {
      List<CPosicionWFSCN> Respuesta = new List<CPosicionWFSCN>();
      foreach (Location Punto in Puntos)
      {
        CPosicionWFSCN Posicion = new CPosicionWFSCN();
        Posicion.X = Punto.Longitude;
        Posicion.Y = Punto.Latitude;
        Respuesta.Add(Posicion);
      }
      return Respuesta;
    }

    private Point DeterminarPosicionPunto(Datos.CLineaComprimida Linea)
    {
      switch (mVinculador.Vinculo.ClaseVinculada)
      {
        case ClaseVinculo.Areas:
        case ClaseVinculo.Coordenadas:
        case ClaseVinculo.Lineas:
        case ClaseVinculo.Marcador:
          return mPuntos[Linea.Codigos[mPosColumnaGIS]];
        default:
          return DeterminarCoordenadasDesdeColumnas(Linea);
      }
      //switch (mVinculador.ClaseVinculada)
      //{
      //  case ClaseVinculo.Coordenadas:
      //  case ClaseVinculo.Areas:
      //  case ClaseVinculo.Marcador:
      //    return mPuntos[Linea.Codigos[mPosColumnaGIS]];
      //  case ClaseVinculo.ColumnasGIS:
      //    return DeterminarCoordenadasDesdeColumnas(Linea);
      //  default:
      //    throw new Exception("FncPos no soportada");
      //}
    }

    public void ProcesarAgregadoDeColumna(string NombreColumnaAgregada,
          string NombreColumnaGeo, CVinculoIndicadorCompletoCN Vinculo,
          string NombreColumnaLat, string NombreColumnaLng,
          CCapaWFSCN CapaVinculo, CProveedorComprimido Proveedor,
          bool DesdeWFS, string ValorResto, double Rango,
          CCapaWFSCN CapaPosicionador = null, bool UsarCentro = false)
    {
      mszColumnaNueva = NombreColumnaAgregada;
      mszColumnaGeo = NombreColumnaGeo;
      mszColumnaLat = NombreColumnaLat;
      mszColumnaLng = NombreColumnaLng;
      mProveedor = Proveedor;
      mVinculador = Vinculo;
      mCapaVinculo = CapaVinculo;
      mCapaValor = (CapaPosicionador == null ? CapaVinculo : CapaPosicionador);
      mbDesdeWFS = DesdeWFS;

      mszValorResto = ValorResto;

      if (mProveedor.ColumnaNombre(NombreColumnaAgregada) != null)
      {
        return;
      }

      // Barrer los registros.
      // Para c/u buscar las coordenadas.
      // Buscar a que elemento del WFS corresponden y ponerlas.
      // Si no encuentra poner un nombre como RESTO o ??.
      Datos.CColumnaTexto ColAgregada = AgregarColumnaGIS(mszColumnaNueva);
      try
      {

        // arma un vector con la posiciones posibles de la columna que se usa como georeferencia.
        // usa mCapaVinculo.
        ArmarVectorPosiciones();

        DeterminarPosicionesColumnas(Vinculo.Vinculo);

        // pone los nombres de los elementos WFS o Adentro-Resto.
        TraducirVectorPosiciones(Rango, UsarCentro);

        foreach (string Valor in mszTextoPosicion)
        {
          ColAgregada.AgregarValor(Valor);
        }
        ColAgregada.DatosSucios = true;

        ObtenerVectorEnteroPosiciones(ColAgregada);

        if (mbDesdeWFS)
        {
          foreach (Datos.CLineaComprimida Linea in Proveedor.Datos)
          {
            switch (Vinculo.Vinculo.ClaseVinculada)
            {
              case ClaseVinculo.Areas:
              case ClaseVinculo.Coordenadas:
              case ClaseVinculo.Lineas:
              case ClaseVinculo.Marcador:
                Linea.Codigos.Add(mPosiciones[Linea.Codigos[mPosColumnaGIS]]);
                break;
              case ClaseVinculo.ColumnasGIS:
                string Valor = CRutinas.TextoPunto(
                      DeterminarCoordenadasDesdeColumnas(Linea), mCapaValor,
                      Rango, mszValorResto).ToUpper();
                Linea.Codigos.Add(ColAgregada.PosicionValorIgual(Valor));
                break;
            }
          }
        }
        else // Poligono o puntos.
        {
          double RangoRefe = Rango * 180 / (6378137.0 * Math.PI);
          RangoRefe = RangoRefe * RangoRefe;
          List<CPosicionWFSCN> Posiciones = PosicionesDesdePuntos(PuntosFiltro);
          foreach (Datos.CLineaComprimida Linea in Proveedor.Datos)
          {
            Point PuntoRefe = DeterminarPosicionPunto(Linea);
            if (FiltrarPorPoligono)
            {
              Linea.Codigos.Add(mPosiciones[
                  CRutinas.PoligonoContienePunto(Posiciones, PuntoRefe) ? 0 : 1]);
            }
            else
            {
              Linea.Codigos.Add(mPosiciones[
                  CRutinas.PuntoEnCirculos(PuntoRefe, Posiciones, RangoRefe) ? 0 : 1]);
            }
          }
        }

        //        Proveedor.AjustarVentanasAsociadas();

      }
      catch (Exception ex)
      {
        Proveedor.Columnas.Remove(ColAgregada);
        CRutinas.DesplegarMsg(ex);
      }
    }

    private void AgregarElementosColumna(CColumnaBase ColAgregada, CCapaWFSCN Capa)
    {
      switch (Capa.Elemento)
      {
        case ElementoWFS.Punto:
          foreach (CPuntoWFSCN Punto in Capa.Puntos)
          {
            ColAgregada.AgregarValor(Punto.Codigo);
          }
          break;
        case ElementoWFS.Superficie:
          foreach (CAreaWFSCN Area in Capa.Areas)
          {
            ColAgregada.AgregarValor(Area.Codigo);
          }
          break;
      }
      ColAgregada.AgregarValor(mszValorResto);
    }

    private string BuscarPuntoMasCercano(Point Punto, CCapaWFSCN Capa, double Rango, string ValorResto,
        List<CPuntoWFSCN> PuntosOrdenados)
    {
      CPuntoWFSCN Cercano=  CRutinas.PuntoMasCercano(Capa, Punto, Rango, PuntosOrdenados);
      return (Cercano == null ? ValorResto : Cercano.Codigo);
    }

    public string BuscarArea(Point Punto, CCapaWFSCN Capa, List<System.Drawing.Rectangle> Rectangulos, string ValorResto)
    {
      CAreaWFSCN Area = CRutinas.AreaContenedoraPunto(Capa, Rectangulos, Punto);
      return (Area == null ? ValorResto : Area.Codigo);
    }

    private Dictionary<Location, Int32> ExtraerDiccionarioPosiciones(CProveedorComprimido Proveedor,
        Int32 OrdenLng, Int32 OrdenLat)
    {
      Dictionary<Location, Int32> Respuesta = new Dictionary<Location, int>();
      foreach (CLineaComprimida Linea in Proveedor.Datos)
      {
        double Abscisa = Proveedor.Columnas[OrdenLng].ValorReal(Linea.Codigos[OrdenLng]);
        double Ordenada = Proveedor.Columnas[OrdenLat].ValorReal(Linea.Codigos[OrdenLat]);
        if (!double.IsNaN(Abscisa) && !double.IsNaN(Ordenada) && Abscisa != 0 && Ordenada != 0)
        {
          Location Posicion = new Location()
          {
            Longitude = Abscisa,
            Latitude = Ordenada
          };
          if (!Respuesta.ContainsKey(Posicion))
          {
            Respuesta.Add(Posicion, -1);
          }
        }
      }
      return Respuesta;
    }

    private List<CPuntoWFSCN> ExtraerListaPuntosOrdenados(CProveedorComprimido Proveedor,
        Int32 OrdenLng, Int32 OrdenLat)
    {
      List<CPuntoWFSCN> Respuesta = new List<CPuntoWFSCN>();
      foreach (CLineaComprimida Linea in Proveedor.Datos)
      {
        double Abscisa = Proveedor.Columnas[OrdenLng].ValorReal(Linea.Codigos[OrdenLng]);
        double Ordenada = Proveedor.Columnas[OrdenLat].ValorReal(Linea.Codigos[OrdenLat]);
        if (!double.IsNaN(Abscisa) && !double.IsNaN(Ordenada))
        {
          CPuntoWFSCN Posicion = new CPuntoWFSCN();
          Posicion.Punto=new CPosicionWFSCN()
          {
            X = Abscisa,
            Y = Ordenada
          };
          Respuesta.Add(Posicion);
        }
      }
      Respuesta.Sort(delegate (CPuntoWFSCN L1, CPuntoWFSCN L2)
        {
          return (L1.Punto.X == L2.Punto.X ? L1.Punto.Y.CompareTo(L2.Punto.Y) : L1.Punto.X.CompareTo(L2.Punto.X));
        });
      for (Int32 i = Respuesta.Count - 1; i > 0; i--)
      {
        if (Respuesta[i].Equals(Respuesta[i - 1]))
        {
          Respuesta.RemoveAt(i);
        }
      }
      return Respuesta;
    }

    private List<System.Drawing.Rectangle> mRectangulosAreas = null;

    private static System.Drawing.Rectangle ExtraerRectanguloArea(CAreaWFSCN Area)
		{
      System.Drawing.Rectangle Respuesta = new System.Drawing.Rectangle();
      if (Area != null && Area.Contorno != null && Area.Contorno.Count > 1)
      {
        Respuesta.X = (Int32)(from P in Area.Contorno
                              select P.X).Min();
        Respuesta.Y = (Int32)(from P in Area.Contorno
                              select P.Y).Min();
        Respuesta.Width = (Int32)(from P in Area.Contorno
                                  select P.X).Max() - Respuesta.X;
        Respuesta.Height = (Int32)(from P in Area.Contorno
                                  select P.Y).Max() - Respuesta.Y;
      }
      else
			{
        Respuesta.X = Int32.MinValue;
        Respuesta.Y = Int32.MinValue;
        Respuesta.Width = 0;
        Respuesta.Height = 0;
			}
      return Respuesta;
    }

    private List<CPuntoWFSCN> mPuntosOrdenados = null;

    public void ProcesarAgregadoDeColumnaDesdeCoordenadas(string NombreColumnaAgregada,
          Int32 OrdenLat, Int32 OrdenLng,
          CCapaWFSCN Capa, CProveedorComprimido Proveedor,
          string ValorResto, double Rango)
    {
      if (OrdenLat < 0 || OrdenLng < 0)
      {
        return;
      }
      mszColumnaNueva = NombreColumnaAgregada;
      mProveedor = Proveedor;
      mVinculador = null;
      mCapaVinculo = Capa;
      mCapaValor = Capa;

      mszValorResto = ValorResto;

      if (mProveedor.ColumnaNombre(NombreColumnaAgregada) != null)
      {
        return;
      }

      // Barrer los registros.
      // Para c/u buscar las coordenadas.
      // Buscar a que elemento del WFS corresponden y ponerlas.
      // Si no encuentra poner un nombre como RESTO o ??.
      Datos.CColumnaTexto ColAgregada = AgregarColumnaGIS(mszColumnaNueva);

      try
      {

        AgregarElementosColumna(ColAgregada, Capa);

        ColAgregada.DatosSucios = true;

        Dictionary<Location, Int32> Posiciones = null;
        if (Capa.Elemento == ElementoWFS.Superficie)
        {
          mRectangulosAreas = (from A in Capa.Areas
                               select ExtraerRectanguloArea(A)).ToList();
          Posiciones = ExtraerDiccionarioPosiciones(Proveedor, OrdenLng, OrdenLat);
        }

        if (Capa.Elemento == ElementoWFS.Punto)
        {
          mPuntosOrdenados = ExtraerListaPuntosOrdenados(Proveedor, OrdenLng, OrdenLat);
        }

        foreach (Datos.CLineaComprimida Linea in Proveedor.Datos)
        {
          double Abscisa = Proveedor.Columnas[OrdenLng].ValorReal(Linea.Codigos[OrdenLng]);
          double Ordenada = Proveedor.Columnas[OrdenLat].ValorReal(Linea.Codigos[OrdenLat]);
          if (!double.IsNaN(Abscisa) && !double.IsNaN(Ordenada) && Abscisa != 0 && Ordenada != 0)
          {
            if (Capa.Elemento == ElementoWFS.Superficie)
            {
              Location Posicion = new Location()
              {
                Longitude = Abscisa,
                Latitude = Ordenada
              };

              Int32 Valor;
              if (Posiciones.TryGetValue(Posicion, out Valor))
              {
                if (Valor < 0)
                {
                  string CodigoArea = BuscarArea(new Point(Abscisa, Ordenada), Capa, mRectangulosAreas, ValorResto);
                  Valor = ColAgregada.PosicionValorIgualTexto(CodigoArea);
                  Posiciones[Posicion] = Valor;
                }
                Linea.Codigos.Add(Valor);
              }
              else
              {
                Linea.Codigos.Add(ColAgregada.PosicionValorIgualTexto(ValorResto));
              }
            }
            else
            {
              Point Punto = new Point(Abscisa, Ordenada);
              string Nombre;
              switch (Capa.Elemento)
              {
                case ElementoWFS.Punto:
                  Nombre = BuscarPuntoMasCercano(Punto, Capa, Rango, ValorResto, mPuntosOrdenados);
                  break;
                case ElementoWFS.Superficie:
                  Nombre = BuscarArea(Punto, Capa, mRectangulosAreas, ValorResto);
                  break;
                default:
                  throw new Exception("No es capa de puntos o areas");
              }
              Linea.Codigos.Add(ColAgregada.PosicionValorIgualTexto(Nombre));
            }
          }
          else
					{
            Linea.Codigos.Add(ColAgregada.PosicionValorIgualTexto(ValorResto));
          }
        }

      }
      catch (Exception ex)
      {
        Proveedor.Columnas.Remove(ColAgregada);
        CRutinas.DesplegarMsg(ex);
      }
    }

  }

  public class Location : IEquatable<Location>
  {
    public double Longitude { get; set; }
    public double Latitude { get; set; }

    public bool Equals(Location o)
    {
      return Longitude == o.Longitude && Latitude == o.Latitude;
    }

    public override int GetHashCode()
		{
      return Longitude.GetHashCode() ^ Latitude.GetHashCode();

    }
	}
}
