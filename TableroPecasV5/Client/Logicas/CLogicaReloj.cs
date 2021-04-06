using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Blazor.Extensions;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaReloj : CBaseGrafico , IDisposable
  {

    public CLogicaReloj()
    {
      Ancho = 110;
      Alto = 60;
    }

    private bool mbCerrado = false;

    public void Dispose()
		{
      mbCerrado = true;
		}

    public long AnchoCanvas
    {
      get { return 110; }
    }

    public long AltoCanvas
    {
      get { return 60; }
    }

    [Parameter]
    public CDatoIndicador Indicador { get; set; }

    [Parameter]
    public Int32 CodigoElementoDimension { get; set; } = -1;

    [Parameter]
    public List<CInformacionAlarmaCN> Alarmas { get; set; } = null;

    public void ImponerAlarmas(List<CInformacionAlarmaCN> Datos)
		{
      Alarmas = Datos;
		}

    public List<CInformacionAlarmaCN> AlarmasLeidas
    {
      get { return Alarmas; }
      set
      {
        if (value != Alarmas)
        {
          Alarmas = value;
        }
      }
    }

    protected BECanvas mCanvasReloj = null;

		public BECanvas CanvasReloj
    {
      get { return mCanvasReloj; }
      set { mCanvasReloj = value; }
    }
//    public BECanvas CanvasReloj;
    protected Canvas2DContext mContexto;


    private async Task AgregarSectorAsync(string Color, double[] Abscisas, double[] Ordenadas, string SubOrden)
    {
      await mContexto.BeginPathAsync();

      await mContexto.SetFillStyleAsync(Color);
      await mContexto.SetStrokeStyleAsync(Color);
      await mContexto.SetLineWidthAsync(1);

      await mContexto.MoveToAsync(Abscisas[0], Ordenadas[0]);

      for (int i = 1; i < Abscisas.Length; i++)
      {
        await mContexto.LineToAsync(Abscisas[i], Ordenadas[i]);
      }

      await mContexto.StrokeAsync();
      await mContexto.FillAsync();

      await mContexto.ClosePathAsync();

    }

    private double AnguloDesdeAlarmas()
    {
      if (Alarmas == null || Alarmas.Count == 0)
      {
        return 0;
      }
      switch (Alarmas.Last().Color.ToUpper())
      {
        case Rutinas.CRutinas.COLOR_AZUL:
          return -67.5;
        case Rutinas.CRutinas.COLOR_VERDE:
          return -22.5;
        case Rutinas.CRutinas.COLOR_AMARILLO:
          return 22.5;
        case Rutinas.CRutinas.COLOR_ROJO:
          return 67.5;
        default:
          return 0;
      }
    }

    private const double ANCHO_FLECHA = 1.5;

    private const double ABSC_CENTRO = 55;
    private const double ORD_CENTRO = 55;
    private const double R1 = 54.875;
    private const double R2 = 47;
    private const double R_FLECHA = 53;

    private async Task AgregarFlechaAsync()
    {

      double Angulo = Math.PI * AnguloDesdeAlarmas() / 180;

      // Contra punta.
      double Abscisa = ABSC_CENTRO - ANCHO_FLECHA * Math.Sin(Angulo);
      double Ordenada = ORD_CENTRO + ANCHO_FLECHA * Math.Cos(Angulo);
      double AbscPunta = ABSC_CENTRO + R_FLECHA * Math.Sin(Angulo);
      double OrdPunta = ORD_CENTRO - R_FLECHA * Math.Cos(Angulo);

      await mContexto.BeginPathAsync();
      await mContexto.SetLineWidthAsync(1);
      await mContexto.SetStrokeStyleAsync("black");
      await mContexto.SetFillStyleAsync("black");

      await mContexto.MoveToAsync(Abscisa, Ordenada);

      // abajo izq.
      double Absc2 = ABSC_CENTRO - ANCHO_FLECHA * Math.Cos(Angulo);
      double Ord2 = ORD_CENTRO - ANCHO_FLECHA * Math.Sin(Angulo);
      await mContexto.LineToAsync(Absc2, Ord2);

      // Arriba izq.
      double AngLocal = Angulo - Math.PI / 4;
      Absc2 = AbscPunta + 1.4142 * ANCHO_FLECHA * Math.Sin(AngLocal);
      Ord2 = OrdPunta + 1.4142 * ANCHO_FLECHA * Math.Cos(AngLocal);
      await mContexto.LineToAsync(Absc2, Ord2);

      // Punta.
      await mContexto.LineToAsync(AbscPunta, OrdPunta);

      // Arriba derecha.
      AngLocal = Math.PI / 4 - Angulo;
      Absc2 = AbscPunta + 1.4142 * ANCHO_FLECHA * Math.Cos(AngLocal);
      Ord2 = OrdPunta + 1.4142 * ANCHO_FLECHA * Math.Sin(AngLocal);
      await mContexto.LineToAsync(Absc2, Ord2);

      // abajo derecha.
      Absc2 = ABSC_CENTRO + ANCHO_FLECHA * Math.Cos(Angulo);
      Ord2 = ORD_CENTRO - ANCHO_FLECHA * Math.Sin(Angulo);
      await mContexto.LineToAsync(Absc2, Ord2);

      // Contra punta.
      await mContexto.LineToAsync(Abscisa, Ordenada);

      await mContexto.StrokeAsync();
      await mContexto.FillAsync();

      await mContexto.ClosePathAsync();
    }

    protected BECanvas mCanvas0;
    public BECanvas Canvas0
		{
      get { return mCanvas0; }
      set { mCanvas0 = value; }
		}

    protected bool HayAlarmas
    {
      get
      {
        if (Alarmas == null || Alarmas.Count == 0)
        {
          return false;
        }
        CInformacionAlarmaCN Alarma = Alarmas.Last();
        return (Alarma.Minimo != Alarma.Satisfactorio || Alarma.Satisfactorio != Alarma.Sobresaliente);
      }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

      if (CanvasReloj == null || mbCerrado)
      {
        return;
      }

      try
      {
        mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasReloj);
//
        await mContexto.BeginBatchAsync();

        try
        {

          await mContexto.ClearRectAsync(0, 0, AnchoCanvas, AltoCanvas);
          await mContexto.SetFillStyleAsync("white");
          await mContexto.FillRectAsync(0, 0, AnchoCanvas, AltoCanvas);

          if (Alarmas == null)
          {

            await mContexto.SetFontAsync("12px serif");
            string Msg = "Aguarde por favor";
            TextMetrics Medida = await mContexto.MeasureTextAsync("H");
            double AltoCaracter = Medida.Width;
            Medida = await mContexto.MeasureTextAsync(Msg);
            await mContexto.SetFillStyleAsync("black");
            await mContexto.FillTextAsync(Msg, (Ancho - Medida.Width) / 2, (Alto + AltoCaracter) / 2);
          }
          else
          {
            await AgregarSectorAsync((HayAlarmas ? "#e3eaf6" : "#ebf0f9"),
              new double[] { 0.124619071668629, 3.4562471794718, 7.41621565904, 4.12382788786642, 0.170570534942717 },
              new double[] { 51.2996529932281, 35.8103792331299, 37.4087026481646, 51.5905296432393, 50.6717596489673 },
              "1");

            await AgregarSectorAsync((HayAlarmas ? "#D9E2F3" : "#ebf0f9"),
              new double[] { 4.87434559384446, 13.5327204052723, 17.1682443625825, 8.80714959134239, 4.87434559384446 },
              new double[] { 32.3633312884899, 18.8687846452312, 22.3746676570173, 34.2933978066743, 32.3633312884899 },
              "2");

            await AgregarSectorAsync((HayAlarmas ? "#D4E8C6" : "#ebf0f9"),
              new double[] { 16.1199820880667, 29.2912339049685, 31.8384251249064, 19.6240830508008, 19.6240830508008 },
              new double[] { 16.0982750103836, 6.37840658852327, 12.0834059097628, 20.0064029342944, 20.0064029342944 },
              "3");

            await AgregarSectorAsync((HayAlarmas ? "#BEDCAA" : "#ebf0f9"),
              new double[] { 32.7863826274726, 49.014330424218, 49.2941408719179, 35.0021611399866, 32.7863826274726 },
              new double[] { 4.68543746362345, 0.326681464085631, 8.03096717373095, 10.8064580305103, 4.68543746362345 },
              "4");

            await AgregarSectorAsync((HayAlarmas ? "#FFFF82" : "#ebf0f9"),
              new double[] { 52.9962081194302, 70.1404832781533, 66.9978388600133, 52.7058591280821, 52.9962081194302 },
              new double[] { 0.0365137741485438, 2.12499866568361, 10.8064580305103, 8.03096717373095, 0.0365137741485438 },
              "5");

            await AgregarSectorAsync((HayAlarmas ? "#FFFF8C" : "#ebf0f9"),
              new double[] { 74.0384752908443, 89.4452444013966, 82.3759169491991, 70.1615748750936, 74.0384752908443 },
              new double[] { 3.40022811484619, 12.1219737146396, 20.0064029342944, 12.0834059097628, 3.40022811484619 },
              "6");

            await AgregarSectorAsync((HayAlarmas ? "#FF7272" : "#ebf0f9"),
              new double[] { 92.61245909499, 103.572867174313, 93.1928504086576, 84.8317556374174, 92.61245909499 },
              new double[] { 14.8714201493785, 29.1992912022442, 34.2933978066742, 22.3746676570173, 14.8714201493785 },
              "7");

            await AgregarSectorAsync((HayAlarmas ? "#FF7D7D" : "#ebf0f9"),
              new double[] { 105.423051172649, 109.83328228419, 97.8761721121336, 94.58378434096, 105.423051172649 },
              new double[] { 33.0337552039399, 50.7208465857867, 51.5905296432392, 37.4087026481646, 33.0337552039399 },
              "8");

            await AgregarFlechaAsync();

          }

        }
        finally
        {
          await mContexto.EndBatchAsync();
        }

      }
      catch (Exception ex)
      {
        Rutinas.CRutinas.DesplegarMsg(ex);
      }
    }

  }
}
