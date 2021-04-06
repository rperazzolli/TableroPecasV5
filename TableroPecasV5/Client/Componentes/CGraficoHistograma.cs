using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas.Model;
using Blazor.Extensions.Canvas.Canvas2D;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Componentes
{
  public class CGraficoHistograma
  {

    private double mAnchoComponente;
    private double mAltoComponente;
    private double mAbscisaGrafico;
    private double mOrdenadaGrafico;
    private double mOrdenadaMaxGrafico;
    private double mAltoGraficoNeto;
    private double mAltoGrafico;
    private double mMaximo;
    private double mDimensionCaracter;
    private double mAnchoEscala;
    private double mAnchoGraficoNecesario; // Ancho necesario para todas las barras.
    private double mAnchoGraficoCompleto; // Todo el grafico mas la escala y los margenes.
    private double mAnchoReferenciaMaxima; // El ancho maximo entre las referencias.
    private double mAnchoColumna; // lo que requiere la columna o las referencias.

    private List<DatosTortaColor> mFajas;
    public async Task HacerGraficoHistogramaAsync(Canvas2DContext Contexto, double AnchoComponente, double AltoComponente)
    {
      if (mFajas != null && mFajas.Count > 0)
      {
        // Limpiar.
        double AnchoLimpiar = Math.Max(AnchoComponente, mAnchoGraficoCompleto);
        await Contexto.ClearRectAsync(0, 0, AnchoLimpiar, AltoComponente);
        await Contexto.SetFillStyleAsync("white");
        await Contexto.FillRectAsync(0, 0, AnchoLimpiar, AltoComponente);

        mAnchoComponente = AnchoComponente;
        mAltoComponente = AltoComponente;
        ObtenerDimensionesGrafico();
        await DibujarEscalaAsync(Contexto);
        await DibujarGrillaAsync(Contexto);
        await DibujarBarrasAsync(Contexto);
        await DibujarReferenciasAsync(Contexto);
      }
    }

    private void ObtenerDimensionesGrafico()
    {
      mAbscisaGrafico = mAnchoEscala + mDimensionCaracter;
      mOrdenadaGrafico = mDimensionCaracter / 2;
      //      mAnchoGrafico = mAnchoComponente - mAbscisaGrafico - mDimensionCaracter;
      if (mAnchoGraficoCompleto > mAnchoComponente)
      {
        mAltoGraficoNeto = mAltoComponente - CRutinas.ALTO_BARRA_SCROLL;
      }
      else
      {
        mAltoGraficoNeto = mAltoComponente;
      }
      mAltoGrafico = (mAltoGraficoNeto - mOrdenadaGrafico - 4.5 * mDimensionCaracter - 4);
      mOrdenadaMaxGrafico = mOrdenadaGrafico + mAltoGrafico;
    }

    public double OrdenadaValor(double Valor)
    {
      return mOrdenadaGrafico + mAltoGrafico * (mMaximo - Valor) / mMaximo;
    }

    private async Task DibujarEscalaAsync(Canvas2DContext Contexto)
    {
      await Contexto.SetFillStyleAsync("black");
      string Fmt = Rutinas.CRutinas.FormatoPorSalto(0, mMaximo);
      for (Int32 i = 0; i <= 5; i++)
      {
        double Valor = mMaximo - mMaximo * i / 5;
        string Aa = Valor.ToString(Fmt);
        TextMetrics Dimensiones = await Contexto.MeasureTextAsync(Aa);
        await Contexto.FillTextAsync(Aa, mAbscisaGrafico - mDimensionCaracter / 2 - Dimensiones.Width,
           OrdenadaValor(Valor) + mDimensionCaracter / 2);
      }
    }

    private async Task DibujarGrillaAsync(Canvas2DContext Contexto)
    {
      await Contexto.BeginPathAsync();
      await Contexto.SetLineWidthAsync(1);
      await Contexto.SetStrokeStyleAsync("black");
      float[] Saltos = new float[0];
      await Contexto.SetLineDashAsync(Saltos);
      await Contexto.MoveToAsync(mAbscisaGrafico, mOrdenadaGrafico);
      await Contexto.LineToAsync(mAbscisaGrafico + mAnchoGraficoNecesario, mOrdenadaGrafico);
      await Contexto.LineToAsync(mAbscisaGrafico + mAnchoGraficoNecesario, mOrdenadaGrafico + mAltoGrafico);
      await Contexto.LineToAsync(mAbscisaGrafico, mOrdenadaGrafico + mAltoGrafico);
      await Contexto.LineToAsync(mAbscisaGrafico, mOrdenadaGrafico);
      await Contexto.StrokeAsync();
      await Contexto.ClosePathAsync();

      await Contexto.BeginPathAsync();
      await Contexto.SetLineWidthAsync(1);
      await Contexto.SetStrokeStyleAsync("#c0c0c0");
      await Contexto.SetLineDashAsync(new float[] { 2, 1 });
      for (Int32 i = 1; i < 5; i++)
      {
        double Ord00 = mOrdenadaGrafico + mAltoGrafico * i / 5;
        await Contexto.MoveToAsync(mAbscisaGrafico, Ord00);
        await Contexto.LineToAsync(mAbscisaGrafico + mAnchoGraficoNecesario, Ord00);
      }
      await Contexto.StrokeAsync();

      await Contexto.ClosePathAsync();

    }

    /// <summary>
    /// Es la abscisa al inicio del bloque barra - referencias.
    /// </summary>
    /// <param name="Posicion"></param>
    /// <returns></returns>
    private double AbscisaPosicion(Int32 Posicion)
    {
      return mAnchoEscala + mDimensionCaracter + SEP_FAJAS + Posicion * (SEP_FAJAS + mAnchoColumna);
    }

    private async Task DibujarBarrasAsync(Canvas2DContext Contexto)
    {

      Int32 Pos = 0;
      foreach (DatosTortaColor Faja in mFajas)
      {
        double Abscisa = AbscisaPosicion(Pos)+(mAnchoColumna-CTendencias.ANCHO_BARRA)/2;
        double Ordenada = OrdenadaValor(Faja.Datos.Valor);
        await Contexto.SetFillStyleAsync((Faja.Datos.Seleccionado ? "red" : Rutinas.CRutinas.ColorOrden(0)));
        await Contexto.FillRectAsync(Abscisa, Ordenada,
            CTendencias.ANCHO_BARRA, mOrdenadaMaxGrafico - Ordenada);
        Faja.Datos.ImponerContornoRectangular(Abscisa, Ordenada, CTendencias.ANCHO_BARRA, mOrdenadaMaxGrafico - Ordenada);
        Pos++;
      }

    }

    private async Task PonerReferenciasBarraAsync(Canvas2DContext Contexto,
          double Abscisa, double Minimo, double Maximo)
    {
      string Texto = CRutinas.ValorATexto(Minimo, 3);
      TextMetrics Medida = await Contexto.MeasureTextAsync(Texto);
      double Ordenada = mAltoGrafico + 2 * mDimensionCaracter;
      await Contexto.FillTextAsync(Texto, Abscisa + (mAnchoColumna - Medida.Width) / 2, Ordenada);

      Texto = "a";
      Ordenada += mDimensionCaracter + 2;
      Medida = await Contexto.MeasureTextAsync(Texto);
      await Contexto.FillTextAsync(Texto, Abscisa + (mAnchoColumna - Medida.Width) / 2, Ordenada);

      Texto = CRutinas.ValorATexto(Maximo, 3);
      Ordenada += mDimensionCaracter + 2;
      Medida = await Contexto.MeasureTextAsync(Texto);
      await Contexto.FillTextAsync(Texto, Abscisa + (mAnchoColumna - Medida.Width) / 2, Ordenada);

    }

    private async Task DibujarReferenciasAsync(Canvas2DContext Contexto)
    {

      await Contexto.SetFillStyleAsync("#000000");

      Int32 Pos = 0;
      foreach (DatosTortaColor Faja in mFajas)
      {
        await PonerReferenciasBarraAsync(Contexto, AbscisaPosicion(Pos), Faja.Datos.Datos.MinimoRango, Faja.Datos.Datos.MaximoRango);
        Pos++;
      }

    }

    private string FmtTexto(long AltoTotal)
    {
      return (AltoTotal > 400 ? "12" : "10") + "px serif";
    }

    private double AnchoBarra
    {
      get { return CTendencias.ANCHO_BARRA; }
    }

    private void ObtenerExtremosEscala()
    {
      mMaximo = (from P in mFajas
                 select P.Datos.Valor).Max();
      double Minimo = 0;
      CRutinas.AjustarExtremosEscala(ref Minimo, ref mMaximo, true);
    }

    private async Task<double> DeterminarAnchoEscalaAsync(Canvas2DContext Contexto)
    {
      TextMetrics M = await Contexto.MeasureTextAsync("H");
      mDimensionCaracter = M.Width;

      string Fmt = Rutinas.CRutinas.FormatoPorSalto(0, mMaximo);
      TextMetrics MedidasMax = await Contexto.MeasureTextAsync(mMaximo.ToString(Fmt));
      TextMetrics MedidasMin = await Contexto.MeasureTextAsync(((double)0).ToString(Fmt));

      return Math.Max(MedidasMax.Width, MedidasMin.Width);

    }

    private async Task<double> DeterminarAnchoReferenciaMaximaAsync(Canvas2DContext Contexto)
    {
      double Respuesta = 0;
      foreach (DatosTortaColor Faja in mFajas)
      {
        TextMetrics Medidas = await Contexto.MeasureTextAsync(CRutinas.ValorATexto(Faja.Datos.Datos.MinimoRango, 3));
        Respuesta = Math.Max(Respuesta, Medidas.Width);
        Medidas = await Contexto.MeasureTextAsync(CRutinas.ValorATexto(Faja.Datos.Datos.MaximoRango, 3));
        Respuesta = Math.Max(Respuesta, Medidas.Width);
      }

      return Respuesta;

    }

    private const double SEP_FAJAS = 10;

    public async Task<long> DeterminarAnchoNecesarioAsync(Canvas2DContext Contexto, List<DatosTortaColor> Fajas,
        long AnchoTotal, long AltoTotal)
    {
      if (Fajas != null && Fajas.Count > 0)
      {
        mFajas = Fajas;
        await Contexto.SetFontAsync(FmtTexto(AltoTotal));
        ObtenerExtremosEscala();
        mAnchoEscala = await DeterminarAnchoEscalaAsync(Contexto);
        mAnchoReferenciaMaxima = await DeterminarAnchoReferenciaMaximaAsync(Contexto);
        mAnchoColumna = Math.Max(mAnchoReferenciaMaxima, CTendencias.ANCHO_BARRA);
        mAnchoGraficoNecesario = Math.Max(Fajas.Count * (mAnchoColumna + SEP_FAJAS) + SEP_FAJAS,
            AnchoTotal - 1.5 * mDimensionCaracter - mAnchoEscala);
        mAnchoGraficoCompleto = mAnchoGraficoNecesario + mAnchoEscala + 1.5 * mDimensionCaracter;
        return (long)Math.Floor(mAnchoGraficoCompleto);
      }
      else
      {
        return -1;
      }
    }

  }
}
