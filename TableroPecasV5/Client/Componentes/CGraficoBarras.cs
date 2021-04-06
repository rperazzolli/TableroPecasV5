using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas.Model;
using Blazor.Extensions.Canvas.Canvas2D;
using TableroPecasV5.Client.Logicas;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Componentes
{
  public class CGraficoBarras
  {

    private double mAnchoEscala;
    private double mDimensionCaracter;
    private double mAbscisaGrafico;
    private double mOrdenadaGrafico;
    private double mOrdenadaMaxGrafico;
    private double mAnchoGrafico;
    private double mAnchoGraficoCompleto;
    private double mAltoGrafico;
    private double mAltoGraficoNeto;
    private double mAnchoComponente;
    private double mAltoComponente;
    private double mDimensionReferenciaMax;
    private double mDisponibleReferencias;

    /// <summary>
    /// Grafica barras en un canvas.
    /// </summary>
    /// <param name="Contexto"></param>
    /// <param name="Porciones">Porciones a graficar. Incluye Porciones20.</param>
    public async Task HacerGraficoBarrasAsync(Canvas2DContext Contexto, double AnchoComponente, double AltoComponente)
    {
      if (mPorciones != null && mPorciones.Count > 0)
      {
        // Limpiar.
        double AnchoLimpiar = Math.Max(AnchoComponente, mAnchoGrafico + mAnchoEscala + 1.5 * mDimensionCaracter);
        await Contexto.ClearRectAsync(0, 0, AnchoLimpiar, AltoComponente);
        await Contexto.SetFillStyleAsync("white");
        await Contexto.FillRectAsync(0, 0, AnchoLimpiar, AltoComponente);

        mAnchoComponente = AnchoComponente;
        mAltoComponente = AltoComponente;
        await ObtenerDimensionesReferenciasAsync(Contexto);
        ObtenerDimensionesGrafico();
        await DibujarEscalaAsync(Contexto);
        await DibujarGrillaAsync(Contexto);
        await DibujarBarrasAsync(Contexto);
        await DibujarReferenciasAsync(Contexto);
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

    public async Task<long> DeterminarAnchoNecesarioAsync(Canvas2DContext Contexto, List<CPorcionTorta> Porciones, long AltoTotal)
    {
      if (Porciones != null && Porciones.Count > 0)
      {
        mPorciones = Porciones;
        await Contexto.SetFontAsync(FmtTexto(AltoTotal));
        ObtenerExtremosEscala();
        mAnchoEscala = await DeterminarAnchoEscalaAsync(Contexto);
        mAnchoGrafico = Porciones.Count * (AnchoBarra + SEP_BARRAS) + SEP_BARRAS;
        mAnchoGraficoCompleto = mAnchoGrafico + mAnchoEscala + 1.5 * mDimensionCaracter;
        return (long)Math.Floor(mAnchoGraficoCompleto);
      }
      else
      {
        return -1;
      }
    }

    private List<CPorcionTorta> mPorciones;
    private double mMaximo;

    private void ObtenerExtremosEscala()
    {
      mMaximo = (from P in mPorciones
                 select P.Valor).Max();
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

    private async Task ObtenerDimensionesReferenciasAsync(Canvas2DContext Contexto)
    {
      mDimensionReferencias = new List<double>();
      foreach (CPorcionTorta Porcion in mPorciones)
      {
        TextMetrics Medida = await Contexto.MeasureTextAsync(Porcion.Denominacion);
        mDimensionReferencias.Add(Medida.Width);
      }
      mDimensionReferenciaMax = (from M in mDimensionReferencias
                                 select M).Max();
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
      if (mDimensionReferenciaMax > ((mAltoComponente - 1.5 * mDimensionCaracter) * 0.4))
      {
        mAltoGrafico = (mAltoGraficoNeto - mOrdenadaGrafico - mDimensionCaracter) * 0.6;
        mDisponibleReferencias = mAltoGraficoNeto - mDimensionCaracter;
      }
      else
      {
        mAltoGrafico = mAltoGraficoNeto - mOrdenadaGrafico - mDimensionCaracter - mDimensionReferenciaMax;
        mDisponibleReferencias = mDimensionReferenciaMax;
      }
      mOrdenadaMaxGrafico = mOrdenadaGrafico + mAltoGrafico;
    }

    private const double SEP_BARRAS = 5;
    private List<double> mDimensionReferencias;

    private async Task DeterminarDimensionesAsync(Canvas2DContext Contexto)
    {

      // Obtener valores escalas.
      ObtenerExtremosEscala();

      mAnchoEscala = await DeterminarAnchoEscalaAsync(Contexto);

      await ObtenerDimensionesReferenciasAsync(Contexto);

      ObtenerDimensionesGrafico();

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
      double AnchoLineas= mPorciones.Count * (AnchoBarraGraf + SEP_BARRAS) + SEP_BARRAS;
      await Contexto.SetLineDashAsync(Saltos);
      await Contexto.MoveToAsync(mAbscisaGrafico, mOrdenadaGrafico);
      await Contexto.LineToAsync(mAbscisaGrafico + AnchoLineas, mOrdenadaGrafico);
      await Contexto.LineToAsync(mAbscisaGrafico + AnchoLineas, mOrdenadaGrafico + mAltoGrafico);
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
        await Contexto.LineToAsync(mAbscisaGrafico + AnchoLineas, Ord00);
      }
      await Contexto.StrokeAsync();

      await Contexto.ClosePathAsync();

    }

    private double AbscisaPosicion(Int32 Posicion)
    {
      return mAnchoEscala + mDimensionCaracter + SEP_BARRAS + Posicion * (SEP_BARRAS + AnchoBarraGraf);
    }

    private async Task DibujarBarrasAsync(Canvas2DContext Contexto)
    {

      Int32 Pos = 0;
      foreach (CPorcionTorta Porcion in mPorciones)
      {
        double Abscisa = AbscisaPosicion(Pos);
        await Contexto.SetFillStyleAsync((Porcion.Seleccionado ? "red" : Rutinas.CRutinas.ColorOrden(Pos)));
        double Ordenada = OrdenadaValor(Porcion.Valor);
        Porcion.ImponerContornoRectangular(Abscisa, Ordenada, AnchoBarraGraf, mOrdenadaMaxGrafico - Ordenada);
        await Contexto.FillRectAsync(Abscisa, Ordenada,
            AnchoBarraGraf, mOrdenadaMaxGrafico - Ordenada);
        Pos++;
      }

    }

    private async Task PonerEtiquetaBarraAsync(Canvas2DContext Contexto,
          double Abscisa, double Ordenada, string Texto, Int32 Pos)
    {
      await Contexto.SaveAsync();
      await Contexto.TranslateAsync(Abscisa + AnchoBarraGraf - (AnchoBarraGraf - mDimensionCaracter) / 2, Ordenada);
      await Contexto.RotateAsync(4.712f); // 270); // (float)(1.5*Math.PI));
      await Contexto.SetFillStyleAsync("#000000");
      if (mDimensionReferencias[Pos] > mDisponibleReferencias)
      {
        await Contexto.FillTextAsync(
            Texto.Substring(0, (Int32)Math.Floor(mDimensionReferencias[Pos] * Texto.Length / mDisponibleReferencias)), 0, 0,
            mDisponibleReferencias);
      }
      else
      {
        await Contexto.FillTextAsync(Texto, 0, 0);
      }
      await Contexto.RestoreAsync();
    }

    private double AnchoBarraGraf
    {
      get
      {
        return (mAnchoGraficoCompleto >= mAnchoComponente ? AnchoBarra :
            (mAnchoComponente - 1.5 * mDimensionCaracter - mAnchoEscala - (mPorciones.Count + 1) * SEP_BARRAS - 4) / mPorciones.Count);
      }
    }

    private async Task DibujarReferenciasAsync(Canvas2DContext Contexto)
    {
      await Contexto.SaveAsync();
      await Contexto.SetShadowColorAsync("white");
      //await Contexto.SetShadowOffsetXAsync(1);
      //await Contexto.SetShadowOffsetYAsync(1);
      await Contexto.SetShadowBlurAsync(4);

      Int32 Pos = 0;
      foreach (CPorcionTorta Porcion in mPorciones)
      {
        await PonerEtiquetaBarraAsync(Contexto, AbscisaPosicion(Pos), mAltoGraficoNeto - mDimensionCaracter / 2,
            Porcion.Texto, Pos);
        Pos++;
      }

      await Contexto.RestoreAsync();

    }

  }
}
