using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Clases
{
  public class CCapaComodin
  {
    public ClaseCapa Clase;
    public CCapaWFSCN CapaWFS;
    public CCapaWISCN CapaWIS;
    public CCapaWMSCN CapaWMS;
    private string mszFormula;
    private List<CDatosSC> mDatosSC;
    public List<CParValores> Pares;
    public List<CElementoPreguntasWISCN> Preguntas = null;
    public List<CColoresPregunta> Colores = null;

    public Color ColorWFS;
    public List<CPunto> Pushpins { get; set; } = null;
    public CProyectoBing ProyectoBing { get; set; }
//    public Microsoft.Maps.MapControl.MapLayer CapaChinches { get; set; }

    public double Opacidad = 1;
    public Int32 CodigoCorrelativo { get; set; }
    public static Int32 gCorrelativo = 0;

    public CCapaComodin()
    {
      CodigoCorrelativo = gCorrelativo++;
      CapaWFS = null;
      CapaWIS = null;
      CapaWMS = null;
      ProyectoBing = null;
      Clase = ClaseCapa.NoDefinida;
      Pares = new List<CParValores>();
    }

    public CCapaComodin(CCapaWFSCN Capa)
    {
      CapaWFS = Capa;
      ColorWFS = System.Drawing.Color.Blue;
      Clase = ClaseCapa.WFS;
      Pares = new List<CParValores>();
    }

    public CCapaComodin(CCapaWISCompletaCN Capa)
    {
      CapaWIS = Capa.Capa;
      Preguntas = Capa.Vinculos;
      Clase = ClaseCapa.WIS;
      Pares = new List<CParValores>();
    }

    public CCapaComodin(CCapaWMSCN Capa)
    {
      CapaWMS = Capa;
      Clase = ClaseCapa.WMS;
      Pares = new List<CParValores>();
    }

    private void AjustarExtremosPunto(CPosicionWFSCN Punto, ref double LatMin, ref double LngMin,
        ref double LatMax, ref double LngMax)
    {
      if (Math.Abs(Punto.Y) <= 360)
      {
        LatMin = Math.Min(LatMin, Punto.Y);
        LatMax = Math.Max(LatMax, Punto.Y);
        LngMin = Math.Min(LngMin, Punto.X);
        LngMax = Math.Max(LngMax, Punto.X);
      }
    }

    private void AjustarExtremosContorno(List<CPosicionWFSCN> Contorno, ref double LatMin, ref double LngMin,
        ref double LatMax, ref double LngMax)
    {
      foreach (CPosicionWFSCN Punto in Contorno)
      {
        AjustarExtremosPunto(Punto, ref LatMin, ref LngMin, ref LatMax, ref LngMax);
      }
    }

    public void BuscarExtremos(ref double LatMin, ref double LngMin, ref double LatMax, ref double LngMax)
    {
      switch (Clase)
      {
        case ClaseCapa.WMS:
          LatMin = Math.Min(LatMin, CapaWMS.LatMinima);
          LatMax = Math.Max(LatMax, CapaWMS.LatMaxima);
          LngMin = Math.Min(LngMin, CapaWMS.LongMinima);
          LngMax = Math.Max(LngMax, CapaWMS.LongMaxima);
          break;
        default:
          if (CapaWFS != null)
          {
            foreach (CAreaWFSCN Area in CapaWFS.Areas)
            {
              AjustarExtremosContorno(Area.Contorno, ref LatMin, ref LngMin, ref LatMax, ref LngMax);
            }
            foreach (CLineaWFSCN Linea in CapaWFS.Lineas)
            {
              AjustarExtremosContorno(Linea.Contorno, ref LatMin, ref LngMin, ref LatMax, ref LngMax);
            }
            foreach (CPuntoWFSCN Punto in CapaWFS.Puntos)
            {
              AjustarExtremosPunto(Punto.Punto, ref LatMin, ref LngMin, ref LatMax, ref LngMax);
            }
          }
          break;
      }
    }

    private bool AreaEnVentana(CAreaWFSCN Area, double LongMin, double LongMax,
          double LatMin, double LatMax)
    {
      return (Area.Centro == null || (Area.Centro.X >= LongMin && Area.Centro.X <= LongMax && Area.Centro.Y >= LatMin && Area.Centro.Y <= LatMax));
    }

    public const Int32 CANT_AREAS_DIBUJADAS = 6000;
    private List<CAreaWFSCN> UbicarAreasEnVentana(double LongMin, double LongMax,
          double LatMin, double LatMax)
    {
      if (CapaWFS.Areas == null)
      {
        return null;
      }

      if (CapaWFS.Areas.Count < CANT_AREAS_DIBUJADAS)
      {
        return CapaWFS.Areas; // Dibuja todas.
      }

      List<CAreaWFSCN> Respuesta = new List<CAreaWFSCN>();
      foreach (CAreaWFSCN Area in CapaWFS.Areas)
      {
        if (AreaEnVentana(Area, LongMin, LongMax, LatMin, LatMax))
        {
          Respuesta.Add(Area);
          if (Respuesta.Count >= CANT_AREAS_DIBUJADAS)
          {
            return new List<CAreaWFSCN>(); // No dibuja.
          }
        }
      }

      return Respuesta;
    }

    private void ModificarFormula(CCapaWSSCN Capa)
    {
      string[] Frm = Capa.Formula.Split('@');
      if (Frm.Length > 0 && Frm[0].Length > 0)
      {
        mszFormula = Frm[0];
        foreach (CDatosSC Dato in mDatosSC)
        {
          string Str = "[" + Dato.Nombre + "]";
          mszFormula = mszFormula.Replace("[" + Dato.Nombre + "]",
              "[" + Dato.Codigo.ToString() + "]");
        }
      }
    }

    private CAreaWFSCN CrearSubArea(CAreaWFSCN Area, Int32 Desde, Int32 Hasta)
    {
      CAreaWFSCN Respuesta = new CAreaWFSCN();
      Respuesta.Codigo = Area.Codigo;
      Respuesta.Contorno = new List<CPosicionWFSCN>();
      Respuesta.Dimensiones = new List<CValorDimensionCN>();
      Respuesta.Dimensiones.AddRange(Area.Dimensiones);
      Respuesta.Nombre = Area.Nombre;
      for (Int32 i = Desde; i < Hasta; i++)
      {
        Respuesta.Contorno.Add(Area.Contorno[i]);
      }
      double dArea;
      Respuesta.Centro = BuscarCentroContorno(Respuesta.Contorno, out dArea);
      Respuesta.Area = dArea;
      return Respuesta;
    }

    public static CPosicionWFSCN BuscarCentroContorno(List<CPosicionWFSCN> Puntos, out double Area)
    {
      Area = 0;
      CPosicionWFSCN Respuesta = new CPosicionWFSCN();
      if (Puntos.Count == 0)
      {
        Respuesta.X = 0;
        Respuesta.Y = 0;
      }
      else
      {
        double Abscisa = 0;
        double Ordenada = 0;
        for (Int32 i = 0; i < Puntos.Count; i++)
        {
          Int32 iAntes = (i == 0 ? Puntos.Count - 1 : i - 1);
          double x1 = Puntos[iAntes].X;
          double x2 = Puntos[i].X;
          double y1 = Puntos[iAntes].Y;
          double y2 = Puntos[i].Y;
          Area += (x2 - x1) * (y2 + y1) / 2;

          if (Math.Abs(x2 - x1) > 0.0001)
          {
            double B = (y2 - y1) / (x2 - x1);
            double A = y1 - x1 * B;
            Abscisa += A * (x2 * x2 - x1 * x1) / 2 +
                B * (x2 * x2 * x2 - x1 * x1 * x1) / 3;
            Ordenada += A * A * (x2 - x1) / 2 +
                A * B * (x2 * x2 - x1 * x1) / 2 +
                B * B * (x2 * x2 * x2 - x1 * x1 * x1) / 6;
          }
          else
          {
            Abscisa += (x1 + x2) * (x2 - x1) * (y1 + y2) / 4;
            Ordenada += (y1 + y2) * (y1 + y2) * (x2 - x1) / 8;
          }
        }
        Respuesta.X = Abscisa / Area;
        Respuesta.Y = Ordenada / Area;
      }
      return Respuesta;
    }

    private List<CAreaWFSCN> ListarSubAreas(CAreaWFSCN MacroArea)
    {
      List<CAreaWFSCN> Respuesta = new List<CAreaWFSCN>();
      if ((from P in MacroArea.Contorno
           where P.X > 999 && P.Y > 999
           select P).FirstOrDefault() == null)
      {
        Respuesta.Add(MacroArea);
      }
      else
      {
        Int32 i0 = 0;
        for (Int32 i = 0; i < MacroArea.Contorno.Count; i++)
        {
          if (MacroArea.Contorno[i].X > 999 && MacroArea.Contorno[i].Y > 999)
          {
            Respuesta.Add(CrearSubArea(MacroArea, i0, i));
            i0 = i + 1;
          }
        }
        Respuesta.Add(CrearSubArea(MacroArea, i0, MacroArea.Contorno.Count));

        double AreaMax = 0;
        CAreaWFSCN AreaConNombre = null;
        foreach (CAreaWFSCN Area in Respuesta)
        {
          if (Area.Area > AreaMax)
          {
            AreaMax = Area.Area;
            AreaConNombre = Area;
          }
        }
        if (AreaConNombre != null)
        {
          foreach (CAreaWFSCN Area in Respuesta)
          {
            if (Area != AreaConNombre)
            {
              Area.Centro = null;
            }
          }
        }

      }

      return Respuesta;
    }

    private string ColorDesdePar(CParValores Par)
    {
      switch (Par.ColorElemento)
      {
        case ColorBandera.NoCorresponde: return Par.ColorImpuesto;
        default: return CRutinas.ColorBanderaATexto(Par.ColorElemento, false);
      }
    }

    public async Task DibujarAreasGradienteAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {

      foreach (CAreaWFSCN MacroArea in CapaWFS.Areas)
      {
        CParValores Par = (from P in Pares
                           where P.CodigoElemento == MacroArea.Codigo.ToUpper()
                           select P).FirstOrDefault();
        if (Par != null)
        {
          string ColorArea = ColorDesdePar(Par);
          List<CAreaWFSCN> Areas = ListarSubAreas(MacroArea);
          CAreaWFSCN AreaCentro = (from A in Areas
                                   where A.Centro != null
                                   select A).FirstOrDefault();
          foreach (CAreaWFSCN Area in Areas)
          {
            if (Area != AreaCentro)
            {
              await DibujarAreaColorAsync(JSRuntime, Area, ColorArea, Posicion,
                  CRutinas.ValorATexto(Par.ValorElemento), false);
            }
          }
          await DibujarAreaColorAsync(JSRuntime, AreaCentro, ColorArea, Posicion,
              CRutinas.ValorATexto(Par.ValorElemento), true);
        }
      }
    }

    public async Task DibujarAsync(IJSRuntime JSRuntime, Int32 Posicion, bool BloquearWFS)
    {
      try
      {
        switch (Clase)
        {
          case ClaseCapa.WMS:
            await DibujarWMSAsync(JSRuntime, Posicion);
            break;
          case ClaseCapa.WFS:
            if (!BloquearWFS)
            {
              await DibujarWFSAsync(JSRuntime, Posicion);
            }
            break;
          case ClaseCapa.WIS:
            await DibujarWISAsync(JSRuntime, Posicion);
            break;
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    private async Task DibujarWMSAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      try
      {
        object[] Args = new object[6];
        Args[0] = Posicion;
        Args[1] = CapaWMS.URLProveedor + "?REQUEST=GetMap&SERVICE=WMS&VERSION=" + CapaWMS.VersionProveedor;
        Args[2] = CapaWMS.LatMaxima;
        Args[3] = CapaWMS.LongMinima;
        Args[4] = CapaWMS.LatMinima;
        Args[5] = CapaWMS.LongMaxima;
        await JSRuntime.InvokeAsync<Task>("AgregarLayerWMS", Args);
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    private async Task DibujarWFSAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      try
      {
        if (CapaWFS.Areas != null && CapaWFS.Areas.Count > 0)
        {
          await DibujarAreasWFSAsync(JSRuntime, Posicion);
        }
        if (CapaWFS.Lineas != null && CapaWFS.Lineas.Count > 0)
        {
          await DibujarLineasWFSAsync(JSRuntime, Posicion);
        }
        if (CapaWFS.Puntos != null && CapaWFS.Puntos.Count > 0)
        {
          await DibujarPuntosWFSAsync(JSRuntime, Posicion);
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    private async Task DibujarWISAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      try
      {
        if (CapaWFS.Areas != null && CapaWFS.Areas.Count > 0)
        {
          await DibujarAreasWISAsync(JSRuntime, Posicion);
        }
        if (CapaWFS.Lineas != null && CapaWFS.Lineas.Count > 0)
        {
          await DibujarLineasWISAsync(JSRuntime, Posicion);
        }
        if (CapaWFS.Puntos != null && CapaWFS.Puntos.Count > 0)
        {
          await DibujarPuntosWISAsync(JSRuntime, Posicion);
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    private async Task DibujarAreasWFSAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      foreach (CAreaWFSCN Area in CapaWFS.Areas)
      {
        List<CAreaWFSCN> Areas = ListarSubAreas(Area);
        CAreaWFSCN AreaCentro = (from A in Areas
                                 where A.Centro != null
                                 select A).FirstOrDefault();
        foreach (CAreaWFSCN SubArea in Areas)
        {
          if (SubArea != AreaCentro)
          {
            await DibujarAreaColorAsync(JSRuntime, Area, "lightblue", Posicion, "", false);
          }
        }
        await DibujarAreaColorAsync(JSRuntime, AreaCentro, "lightblue", Posicion, "", true);
      }
    }

    private async Task DibujarAreaColorAsync(IJSRuntime JSRuntime, CAreaWFSCN Area, string Color, 
      Int32 Posicion, string Valor = "", bool bPrimera = true)
    {
      if (Area.Contorno != null && Area.Contorno.Count > 1)
      {
        double[] Abscisas = new double[Area.Contorno.Count + 1];
        double[] Ordenadas = new double[Area.Contorno.Count + 1];
        for (Int32 i = 0; i < Area.Contorno.Count; i++)
        {
          Abscisas[i] = Area.Contorno[i].X;
          Ordenadas[i] = Area.Contorno[i].Y;
        }
        Abscisas[Area.Contorno.Count] = Area.Contorno[0].X;
        Ordenadas[Area.Contorno.Count] = Area.Contorno[0].Y;
        object[] Args = new object[10];
        Args[0] = Posicion;
        Args[1] = Abscisas;
        Args[2] = Ordenadas;
        Args[3] = (Area.Centro == null || !bPrimera ? -1000 : Area.Centro.X);
        Args[4] = (Area.Centro == null || !bPrimera ? -1000 : Area.Centro.Y);
        Args[5] = Color;
        Args[6] = Area.Nombre;
        Args[7] = Valor;
        Args[8] = "";
        Args[9] = 1;
        await JSRuntime.InvokeAsync<Task>("DibujarPoligono", Args);
      }
    }

      private async Task DibujarLineasWFSAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      foreach (CLineaWFSCN Linea in CapaWFS.Lineas)
      {
        if (Linea.Contorno != null && Linea.Contorno.Count > 1)
        {
          double[] Abscisas = new double[Linea.Contorno.Count];
          double[] Ordenadas = new double[Linea.Contorno.Count];
          for (Int32 i = 0; i < Linea.Contorno.Count; i++)
          {
            Abscisas[i] = Linea.Contorno[i].X;
            Ordenadas[i] = Linea.Contorno[i].Y;
          }
          object[] Args = new object[9];
          Args[0] = Posicion;
          Args[1] = Abscisas;
          Args[2] = Ordenadas;
          Args[3] = Linea.Centro.X;
          Args[4] = Linea.Centro.Y;
          Args[5] = "blue";
          Args[6] = Linea.Nombre;
          Args[7] = "";
          Args[8] = "";
          await JSRuntime.InvokeAsync<Task>("DibujarLinea", Args);
        }
      }

    }

    private async Task DibujarPuntosWFSAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      foreach (CPuntoWFSCN Punto in CapaWFS.Puntos)
      {
        object[] Args = new object[7];
        Args[0] = Posicion;
        Args[1] = Punto.Punto.X;
        Args[2] = Punto.Punto.Y;
        Args[3] = "blue";
        Args[4] = Punto.Nombre;
        Args[5] = "";
        Args[6] = "";
        await JSRuntime.InvokeAsync<Task>("AgregarPushpin", Args);
      }

    }

    private async Task<string> ObtenerColorElementoAsync(string Codigo, bool Puro = false)
    {
      foreach (CElementoPreguntasWISCN Pregunta in Preguntas)
      {
        if (Pregunta.CodigoArea == Codigo)
        {
          return CRutinas.ColorBanderaATexto(await CRutinas.ObtenerColorBanderaAsync(Http, Pregunta.Contenidos), Puro);
        }
      }
      return (Puro ? "lightgray" : "rgba(128, 128, 128, 0.5)");
    }

    private async Task DibujarAreasWISAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      foreach (CAreaWFSCN Area in CapaWFS.Areas)
      {
        string Color = await ObtenerColorElementoAsync(Area.Codigo);
        if (Area.Contorno != null && Area.Contorno.Count > 1)
        {
          double[] Abscisas = new double[Area.Contorno.Count];
          double[] Ordenadas = new double[Area.Contorno.Count];
          for (Int32 i = 0; i < Area.Contorno.Count; i++)
          {
            Abscisas[i] = Area.Contorno[i].X;
            Ordenadas[i] = Area.Contorno[i].Y;
          }
          object[] Args = new object[10];
          Args[0] = Posicion;
          Args[1] = Abscisas;
          Args[2] = Ordenadas;
          Args[3] = (Area.Centro == null ? -1000 : Area.Centro.X);
          Args[4] = (Area.Centro == null ? -1000 : Area.Centro.Y);
          Args[5] = Color;
          Args[6] = Area.Nombre;
          Args[7] = "";
          Args[8] = this.ProyectoBing.Proyecto.Codigo.ToString() + ";" + Area.Codigo.ToString();
          Args[9] = 1;
          await JSRuntime.InvokeAsync<Task>("DibujarPoligono", Args);
        }
      }

    }

    private async Task DibujarLineasWISAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      foreach (CLineaWFSCN Linea in CapaWFS.Lineas)
      {
        string Color = await ObtenerColorElementoAsync(Linea.Codigo);
        if (Linea.Contorno != null && Linea.Contorno.Count > 1)
        {
          double[] Abscisas = new double[Linea.Contorno.Count];
          double[] Ordenadas = new double[Linea.Contorno.Count];
          for (Int32 i = 0; i < Linea.Contorno.Count; i++)
          {
            Abscisas[i] = Linea.Contorno[i].X;
            Ordenadas[i] = Linea.Contorno[i].Y;
          }
          object[] Args = new object[10];
          Args[0] = Posicion;
          Args[1] = Abscisas;
          Args[2] = Ordenadas;
          Args[3] = Linea.Centro.X;
          Args[4] = Linea.Centro.Y;
          Args[5] = Color;
          Args[6] = Linea.Nombre;
          Args[7] = "";
          Args[8] = Linea.Codigo;
          Args[9] = 1;
          await JSRuntime.InvokeAsync<Task>("DibujarPoligono", Args);
        }
      }

    }

    private async Task DibujarPuntosWISAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      foreach (CPuntoWFSCN Punto in CapaWFS.Puntos)
      {
        object[] Args = new object[7];
        Args[0] = Posicion;
        Args[1] = Punto.Punto.X;
        Args[2] = Punto.Punto.Y;
        string Color = await ObtenerColorElementoAsync(Punto.Codigo, true);
        Args[3] = Color;
        Args[4] = Punto.Nombre;
        Args[5] = "";
        Args[6] = this.ProyectoBing.Proyecto.Codigo.ToString() + ";" + Punto.Codigo.ToString();
        await JSRuntime.InvokeAsync<Task>("AgregarPushpin", Args);
      }

    }

    private double ObtenerValorParametroWFS(string CodigoElemento, CDatosPrmWFS DatoPrm)
    {
      foreach (string Valor in DatoPrm.ParesValores)
      {
        if (Valor.StartsWith(CodigoElemento))
        {
          return CRutinas.StrVFloat(Valor.Substring(CodigoElemento.Length));
        }
      }
      return double.NaN;
    }

    private double ModificarSegunFormula(CParValores Par, List<CDatosPrmWFS> DatosPrm)
    {
      string Frm0 = mszFormula;
      string Valor = Par.ValorElemento.ToString();
      Frm0 = Frm0.Replace("[Valor]", Valor);

      // Primero ingresa valor prms.
      foreach (CDatosPrmWFS DatoPrm in DatosPrm)
      {
        string ValReemp1 = "[Prm-" + DatoPrm.Parametro + "]";
        double ValorPrm = ObtenerValorParametroWFS(Par.CodigoElemento + "=", DatoPrm);
        if (double.IsNaN(ValorPrm))
        {
          return double.NaN;
        }
        Frm0 = Frm0.Replace(ValReemp1, ValorPrm.ToString());
      }

      // Ahora SC.
      foreach (CDatosSC Dato in mDatosSC)
      {
        string ValReemp = "[" + Dato.Codigo.ToString() + "]";
        if (Frm0.IndexOf(ValReemp) >= 0)
        {
          double ValColumna = Dato.ObtenerValorCorrespondiente(Par.CodigoElemento);
          if (double.IsNaN(ValColumna))
          {
            return double.NaN;
          }
          else
          {
            Frm0 = Frm0.Replace(ValReemp, ValColumna.ToString());
          }
        }
      }

      CInterprete Interprete = new CInterprete();
      double Retorno = 0;
      if (!Interprete.Interpreta(Frm0, ref Retorno))
      {
        return double.NaN;
      }
      else
      {
        return Retorno;
      }

    }

    public void AgregarFormulaWSS(CCapaWSSCN Capa, List<CDatosSC> DatosSC,
          List<CDatosPrmWFS> DatosPrm)
    {
      if (Capa.Formula.Length > 0)
      {
        mDatosSC = DatosSC;
        ModificarFormula(Capa);
        foreach (CParValores Valor in Pares)
        {
          Valor.ValorElemento = ModificarSegunFormula(Valor, DatosPrm);
        }
      }
    }

    public double ValorParaCodigo(string Codigo)
    {
      if (Pares != null)
      {
        foreach (CParValores Par in Pares)
        {
          if (Par.CodigoElemento == Codigo)
          {
            return Par.ValorElemento;
          }
        }
      }
      return double.NaN;
    }

    public string TextoValorParaCodigo(string Codigo)
    {
      double Valor = ValorParaCodigo(Codigo);
      return (double.IsNaN(Valor) ? "SIN DATOS" : CRutinas.ValorATexto(Valor));
    }

    private void AjustarExtremosPosicion(CPosicionWFSCN Posicion, ref CPunto P1,
      ref CPunto P2)
    {
      if (Math.Abs(Posicion.X) > 0.001 && Math.Abs(Posicion.Y) > 0.001)
      {
        P1.Abscisa = Math.Min(P1.Abscisa, Posicion.X);
        P1.Ordenada = Math.Min(P1.Ordenada, Posicion.Y);
        P2.Abscisa = Math.Max(P2.Abscisa, Posicion.X);
        P2.Ordenada = Math.Max(P2.Ordenada, Posicion.Y);
      }
    }

    public void AjustarExtremos(ref CPunto P1,
          ref CPunto P2)
    {
      if (CapaWFS != null)
      {
        foreach (CAreaWFSCN Area in CapaWFS.Areas)
        {
          foreach (CPosicionWFSCN Posicion in Area.Contorno)
          {
            if (Posicion.X < 999 && Posicion.Y < 999)
            {
              AjustarExtremosPosicion(Posicion, ref P1, ref P2);
            }
          }
        }

        foreach (CPuntoWFSCN Punto in CapaWFS.Puntos)
        {
          AjustarExtremosPosicion(Punto.Punto, ref P1, ref P2);
        }
      }
    }

    public CCapaComodin(CProyectoBing Proy0)
    {
      ProyectoBing = Proy0;
      Clase = ClaseCapa.Bing; // aca indica que se grafican los pushpin desde los datos del proyecto.
    }

    public void GuardarPreguntasPuntos()
    {
      Preguntas = new List<CElementoPreguntasWISCN>();
      if (Pushpins != null) {
        foreach (CPunto Punto in Pushpins)
        {
          CPushPinIndicadores Pushpin = (Punto as CPushPinIndicadores);
          if (Pushpin != null && Pushpin.Datos != null && Pushpin.Datos.Contenidos.Count > 0)
          {
            Preguntas.Add(Pushpin.Datos);
          }
        }
      }
    }

    public override string ToString()
    {
      switch (Clase)
      {
        case ClaseCapa.WFS: return CapaWFS.Descripcion;
        case ClaseCapa.WIS: return CapaWIS.Descripcion;
        case ClaseCapa.WMS: return CapaWMS.Descripcion;
        default: return "";
      }
    }

    public Int32 CodigoCapa
    {
      get
      {
        switch (Clase)
        {
          case ClaseCapa.WFS: return CapaWFS.Codigo;
          case ClaseCapa.WIS: return CapaWIS.Codigo;
          case ClaseCapa.WMS: return CapaWMS.Codigo;
          default: return -1;
        }
      }
    }

    public CElementoPreguntasWISCN PreguntaElemento(string Codigo)
    {
      foreach (CElementoPreguntasWISCN Pregunta in Preguntas)
      {
        if (Pregunta.CodigoArea == Codigo)
        {
          return Pregunta;
        }
      }
      return null;
    }

    private ColoresParaPreguntas ColorDesdeTexto(string Texto)
    {
      switch (Texto.ToUpper())
      {
        case CRutinas.COLOR_ROJO:
          return ColoresParaPreguntas.Rojo;
        case CRutinas.COLOR_AMARILLO:
          return ColoresParaPreguntas.Amarillo;
        case CRutinas.COLOR_VERDE:
          return ColoresParaPreguntas.Verde;
        case CRutinas.COLOR_AZUL:
          return ColoresParaPreguntas.Azul;
        default:
          return ColoresParaPreguntas.Gris;
      }
    }

    private async Task<ColoresParaPreguntas> ObtenerColorPreguntaAsync(Int32 CodigoPregunta, Int32 Dimension,
          Int32 Elemento)
    {
      ColoresParaPreguntas Respuesta;
      List<CPreguntaIndicadorCN> Indicadores = Contenedores.CContenedorDatos.ExtraerIndicadoresPregunta(CodigoPregunta);
      if (Indicadores.Count > 0)
      {
        Respuesta = await ObtenerColorIndicadorAsync(Indicadores[0], Dimension, Elemento);
        for (Int32 i = 1; i < Indicadores.Count; i++)
        {
          ColoresParaPreguntas ColInd = await ObtenerColorIndicadorAsync(Indicadores[i], Dimension, Elemento);
          Respuesta = CColoresPregunta.CompararColores(ColInd, Respuesta);
        }
      }
      else
      {
        Respuesta = ColoresParaPreguntas.Gris;
      }
      return Respuesta;
    }

    private async Task<ColoresParaPreguntas> ObtenerColorSalaReunionAsync(Int32 CodigoSala)
    {
      ColoresParaPreguntas Respuesta;
      List<CPreguntaIndicadorCN> Indicadores = Contenedores.CContenedorDatos.ExtraerIndicadoresSalaReunion(CodigoSala);
      if (Indicadores.Count > 0)
      {
        Respuesta = await ObtenerColorIndicadorAsync(Indicadores[0], -1, -1);
        for (Int32 i = 1; i < Indicadores.Count; i++)
        {
          ColoresParaPreguntas ColInd = await ObtenerColorIndicadorAsync(Indicadores[i], -1, -1);
          Respuesta = CColoresPregunta.CompararColores(ColInd, Respuesta);
        }
      }
      else
      {
        Respuesta = ColoresParaPreguntas.Gris;
      }
      return Respuesta;
    }

    private async Task<ColoresParaPreguntas> ObtenerColorIndicadorAsync(CPreguntaIndicadorCN Indicador,
        Int32 DimensionPreg, Int32 ElementoPreg)
    {
      return await ObtenerColorIndicadorAsync(Indicador.Indicador,
          (Indicador.Dimension >= 0 ? Indicador.Dimension : DimensionPreg),
          (Indicador.Dimension >= 0 ? Indicador.ElementoDimension : ElementoPreg));
    }

    [Inject]
    public HttpClient Http { get; set; }

    private async Task<ColoresParaPreguntas> ObtenerColorIndicadorAsync(Int32 CodigoIndicador,
      Int32 Dimension, Int32 Elemento)
    {
      CInformacionAlarmaCN Datos = await Contenedores.CContenedorDatos.DatosDisponiblesIndicadorFechaAsync(
        Http, CodigoIndicador, Elemento);
      return (Datos == null ? ColoresParaPreguntas.Gris :
          ColorDesdeTexto(Datos.Color));
    }

    private async Task<CColorElemento> CrearColorElementoAsync(CPreguntaPreguntaWISCN Pregunta)
    {
      CColorElemento Respuesta = new CColorElemento();
      Respuesta.Clase = Pregunta.Clase;
      Respuesta.Codigo = Pregunta.CodigoElemento;
      Respuesta.Dimension = Pregunta.CodigoDimension;
      Respuesta.ElementoDimension = Pregunta.CodigoElementoDimension;

      switch (Pregunta.Clase)
      {
        case ClaseDetalle.Indicador:
          Respuesta.ColorElemento = await ObtenerColorIndicadorAsync(Respuesta.Codigo, Respuesta.Dimension, Respuesta.ElementoDimension);
          break;
        case ClaseDetalle.Pregunta:
          Respuesta.ColorElemento = await ObtenerColorPreguntaAsync(Respuesta.Codigo, Respuesta.Dimension, Respuesta.ElementoDimension);
          break;
        case ClaseDetalle.SalaReunion:
          Respuesta.ColorElemento = await ObtenerColorSalaReunionAsync(Respuesta.Codigo);
          break;
        default:
          return null;
      }

      return Respuesta;

    }

    private void AgregarColorElementoCodigo(string CodigoElemento, CPuntoWFSCN Punto = null)
    {
      CColoresPregunta Valor = new CColoresPregunta();
      Valor.CodigoElemento = CodigoElemento;

      CParValores Par = ParParaCodigo(CodigoElemento);
      if (Par != null)
      {
        switch (Par.ColorElemento)
        {
          case ColorBandera.Rojo:
            Valor.ColorPregunta = ColoresParaPreguntas.Rojo;
            break;
          case ColorBandera.Amarillo:
            Valor.ColorPregunta = ColoresParaPreguntas.Amarillo;
            break;
          case ColorBandera.Verde:
            Valor.ColorPregunta = ColoresParaPreguntas.Verde;
            break;
          case ColorBandera.Azul:
            Valor.ColorPregunta = ColoresParaPreguntas.Azul;
            break;
          default:
            Valor.ColorPregunta = ColoresParaPreguntas.Gris;
            break;
        }
      }

      Colores.Add(Valor);

    }

    public CParValores ParParaCodigo(string Codigo)
    {
      Int32 i1 = 0;
      Int32 i2 = Pares.Count - 1;
      while (i2 >= i1)
      {
        Int32 iMedio = (i1 + i2) / 2;
        if (Pares[iMedio].CodigoElemento == Codigo)
        {
          return Pares[iMedio];
        }
        else
        {
          if (Pares[iMedio].CodigoElemento.CompareTo(Codigo) > 0)
          {
            i2 = iMedio - 1;
          }
          else
          {
            i1 = iMedio + 1;
          }
        }
      }
      return null;
    }

    private async Task AgregarColorElementoAsync(string CodigoElemento)
    {
      CColoresPregunta Valor = new CColoresPregunta();
      Valor.CodigoElemento = CodigoElemento;

      foreach (CElementoPreguntasWISCN Elemento in Preguntas)
      {
        if (Elemento.CodigoArea == CodigoElemento)
        {
          foreach (CPreguntaPreguntaWISCN Pregunta in Elemento.Contenidos)
          {
            CColorElemento ValElem = await CrearColorElementoAsync(Pregunta);
            if (ValElem != null)
            {
              Valor.Preguntas.Add(ValElem);
            }
          }
        }
      }

      Valor.AjustarColor();

      Colores.Add(Valor);

    }

    public async Task AjustarColoresElementosCapaAsync()
    {
      if (Clase == ClaseCapa.WIS && CapaWFS != null && Preguntas != null)
      {

        Colores = new List<CColoresPregunta>();

        foreach (CAreaWFSCN Area in CapaWFS.Areas)
        {
          await AgregarColorElementoAsync(Area.Codigo);
        }

        foreach (CPuntoWFSCN Punto in CapaWFS.Puntos)
        {
          await AgregarColorElementoAsync(Punto.Codigo);
        }

        foreach (CLineaWFSCN Linea in CapaWFS.Lineas)
        {
          await AgregarColorElementoAsync(Linea.Codigo);
        }

      }

      if (Clase == ClaseCapa.Bing && Pushpins != null)
      {
        foreach (CPunto Punto in Pushpins)
        {
          CPushPinIndicadores PushPin = Punto as CPushPinIndicadores;
          if (PushPin != null && PushPin.Datos != null)
          {
            PushPin.ColorRelleno = await CRutinas.ObtenerColorBanderaAsync(Http, PushPin.Datos.Contenidos);
          }
        }
      }

    }

    public async Task AjustarColoresPorCodigoAsync()
    {
      Colores = new List<CColoresPregunta>();

      foreach (CAreaWFSCN Area in CapaWFS.Areas)
      {
        AgregarColorElementoCodigo(Area.Codigo);
      }

      foreach (CPuntoWFSCN Punto in CapaWFS.Puntos)
      {
        AgregarColorElementoCodigo(Punto.Codigo, Punto);
      }

      foreach (CLineaWFSCN Linea in CapaWFS.Lineas)
      {
        await AgregarColorElementoAsync(Linea.Codigo);
      }

    }

  }

}
