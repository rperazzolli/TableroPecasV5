using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;
using System.Xml.Linq;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaTarjeta : Componentes.CBaseGrafico, IDisposable
  {
    public static long ANCHO_TARJETA
    {
      get
      {
        return 290;
      }
    }

    public static long ALTO_TARJETA
    {
      get
      {
        return 140;
      }
    }

//    public event CLogicaTab.FncRefrescar AlRefrescar;

public string EstiloIndicador(CDatoIndicador Indicador)
    {
      return "margin: 1px; padding: 0px; background: " + ColorAlarma(Indicador) + ";";
    }

    private bool mbCerrado = false;

    public void Dispose()
		{
      mbCerrado = true;
		}

    public string EstiloTextos
    {
      get
      {
        return "position: absolute; width: " + (ANCHO_TARJETA - 100).ToString() + "px; height: " + (ALTO_TARJETA - 70).ToString() +
          "px; text-align: center; margin-left: 90px; margin-top: -85px; overflow: auto; font-size: 10px; font-family: sans-serif;";
      }
    }

    //[Parameter]
    //public new Int32 Abscisa { get; set; } = 0;

    //[Parameter]
    //public new Int32 Ordenada { get; set; } = 0;

    [Parameter]
    public CPreguntaCN Pregunta { get; set; } = null;

    public BECanvas CanvasGrafico { get; set; }
    private Canvas2DContext mContexto;

    public void AbrirTarjeta()
    {
      if (CanvasGrafico != null)
      {
        return;
      }
    }

    private List<CPreguntaIndicadorCN> mIndicadoresEnPregunta;
    private List<CDatoIndicador> mIndicadores;
    private List<CInformacionAlarmaCN> mDatosAlarma;

    private void AgregarPreguntaIndicador(List<CPreguntaIndicadorCN> Lista, Int32 CodigoIndicador, Int32 CodigoElemento)
    {
      CPreguntaIndicadorCN Agregar = (from P in Lista
                                      where P.Indicador == CodigoIndicador && P.ElementoDimension == CodigoElemento
                                      select P).FirstOrDefault();
      if (Agregar == null)
      {
        Agregar = new CPreguntaIndicadorCN();
        Agregar.Pregunta = Pregunta.Codigo;
        Agregar.Indicador = CodigoIndicador;
        Agregar.ElementoDimension = CodigoElemento;
        CDatoIndicador DatosI = Contenedores.CContenedorDatos.IndicadorDesdeCodigo(CodigoIndicador);
        if (DatosI != null)
        {
          Agregar.Dimension = DatosI.Dimension;
          Lista.Add(Agregar);
        }
      }
    }

    private List<CPreguntaIndicadorCN> ExtraerIndicadoresDeXML()
    {
      List<CPreguntaIndicadorCN> Lista = new List<CPreguntaIndicadorCN>();
      CPuntoPregunta Punto = Contenedores.CContenedorDatos.PuntoPreguntaDesdeCodigo(Pregunta.Codigo);
      if (Punto.Pregunta.Block.Length == 0)
      {
        return Contenedores.CContenedorDatos.IndicadoresEnPregunta(Pregunta.Codigo);
      }
      else
      {
        XDocument Documento = XDocument.Parse(Punto.Pregunta.Block);
        XElement Inicio = Documento.Element(CRutinas.CTE_INICIO);
        // Mantener una lista de los datos cargados.
        foreach (XElement Elemento in Inicio.Elements(CRutinas.CTE_ELEMENTO))
        {
          XAttribute AtrClase = Elemento.Attribute(CRutinas.CTE_CLASE_BLOCK);
          switch ((CRutinas.ClaseBlock)Int32.Parse(AtrClase.Value))
          {
            case CRutinas.ClaseBlock.Indicador:
            case CRutinas.ClaseBlock.Tendencia:
              AgregarPreguntaIndicador(Lista,
                CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CODIGO),
                CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_ELEMENTO_DIMENSION));
              break;
            case CRutinas.ClaseBlock.Grafico:
            case CRutinas.ClaseBlock.Conjunto:
              AgregarPreguntaIndicador(Lista,
                CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CODIGO),
                CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_ELEMENTO_DIMENSION));
              break;
            case CRutinas.ClaseBlock.Grilla:
              if ((ClaseElemento)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE_ELEMENTO) == ClaseElemento.Indicador)
              {
                AgregarPreguntaIndicador(Lista,
                  CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CODIGO),
                  CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_ELEMENTO_DIMENSION));
              }
              break;
            case CRutinas.ClaseBlock.MapaCalor:
            case CRutinas.ClaseBlock.MapaControl:
            case CRutinas.ClaseBlock.MapaGradientes:
              if ((ClaseElemento)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE_ELEMENTO) == ClaseElemento.Indicador)
              {
                AgregarPreguntaIndicador(Lista,
                  CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CODIGO),
                  CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_ELEMENTO_DIMENSION));
              }
              break;
          }
        }
      }

      return Lista;

    }

    public void LimpiarIndicadores()
    {
      mIndicadores = null;
      mIndicadoresEnPregunta = null;
      mDatosAlarma = null;
    }

    [Inject]
    public HttpClient Http { get; set; }

    public async Task AgregarIndicadoresATimerAsync()
    {
      mIndicadoresEnPregunta = new List<CPreguntaIndicadorCN>();
      mIndicadores = new List<CDatoIndicador>();
      mDatosAlarma = new List<CInformacionAlarmaCN>();

      List<CPreguntaIndicadorCN> CodigosIndicadores = ExtraerIndicadoresDeXML();

      foreach (CPreguntaIndicadorCN IndPreg in CodigosIndicadores)
      {
        mIndicadoresEnPregunta.Add(IndPreg);
        CDatoIndicador DatoIndicador =
            Contenedores.CContenedorDatos.IndicadorDesdeCodigo(IndPreg.Indicador);
        if (DatoIndicador != null)
        {
          mIndicadores.Add(DatoIndicador);
          CInformacionAlarmaCN DatosLocales =
              await Contenedores.CContenedorDatos.DatosAlarmaIndicadorAsync(Http, IndPreg.Indicador,
                  IndPreg.Dimension, IndPreg.ElementoDimension);
          if (DatosLocales != null)
          {
            mDatosAlarma.Add(DatosLocales);
          }
          else
          {
            CInformacionAlarmaCN Alarma =
                new CInformacionAlarmaCN();
            Alarma.Color = "GRIS";
            mDatosAlarma.Add(Alarma);
          }
        }
      }

      AjustarColorAlarma();

    }

    private Int32 mColorAlarma;

    public string ColorAlarma(CDatoIndicador Indicador)
    {
      if (Indicador != null && mDatosAlarma != null)
      {
        List<CInformacionAlarmaCN> InfAl = (from I in mDatosAlarma
                                            where I != null && I.CodigoIndicador == Indicador.Codigo
                                            orderby I.FechaInicial descending
                                            select I).ToList();
        if (InfAl != null && InfAl.Count > 0)
        {
          switch (InfAl[0].Color.ToUpper())
          {
            case CRutinas.COLOR_AZUL:
              return "rgba(128,128,255,0.5);"; // "lightblue";
            case CRutinas.COLOR_VERDE:
              return "rgba(128,255,128,0.5);";
            case CRutinas.COLOR_AMARILLO:
              return "rgba(255,255,128,0.5);";
            case CRutinas.COLOR_ROJO:
              return "rgba(255,128,128,0.5);";
          }
        }
      }
      return "rgba(128,128,128,0.5);";
    }

    public List<CDatoIndicador> IndicadoresCriticos { get; set; } = null;

    public void AjustarColorAlarma()
    {
      IndicadoresCriticos = new List<CDatoIndicador>();
      mColorAlarma = -1;
      foreach (CInformacionAlarmaCN Alarma in mDatosAlarma)
      {
        if (Alarma.Color.Length == 0)
        {
          Alarma.Color = "GRIS";
        }
        mColorAlarma = Math.Max(mColorAlarma,
              CRutinas.CodigoColorAlarma(Alarma.Color));
      }

      foreach (CInformacionAlarmaCN Alarma in mDatosAlarma)
      {
        //if (mColorAlarma == CRutinas.CodigoColorAlarma(Alarma.Color) &&
        //    (Alarma.Color != "GRIS" || CRutinas.TendenciasEnTarjeta))
        //{
        CDatoIndicador Indicador = Contenedores.CContenedorDatos.IndicadorDesdeCodigo(Alarma.CodigoIndicador);
        if (Indicador != null && !IndicadoresCriticos.Contains(Indicador))
        {
          IndicadoresCriticos.Add(Indicador);
        }
        //}
      }

    }

    public string Estilo
    {
      get
      {
        return "width: " + ANCHO_TARJETA.ToString() + "px; height: " + ALTO_TARJETA.ToString() +
            "px; background-color: transparent; position: absolute; margin-left: 0px; margin-top: 0px; pointer: cursor;";
      }
    }

    private async Task DibujarRelojAsync(Canvas2DContext Contexto)
    {
      // Elipse.
      await Contexto.SetLineWidthAsync((float)2.99);
      await Contexto.SetStrokeStyleAsync("black");
      await Contexto.ArcAsync(40, 95, 30, 0, 2 * Math.PI, false);

      // sector rojo.
      await Contexto.BeginPathAsync();
      await Contexto.MoveToAsync(19.2154, 107);
      await Contexto.ArcAsync(40, 95, 24, 5 * Math.PI / 6, 7 * Math.PI / 6, false);
      await Contexto.LineToAsync(23.5455, 85.5);
      await Contexto.ArcAsync(40, 95, 19, 7 * Math.PI / 6, 5 * Math.PI / 6, true);
      await Contexto.LineToAsync(19.2154, 107);
      await Contexto.ClosePathAsync();
      await Contexto.SetFillStyleAsync(mColorAlarma < 0 ? "gray" : "#CC4726");
      await Contexto.FillAsync();

      // sector amarillo.
      await Contexto.BeginPathAsync();
      await Contexto.MoveToAsync(19.2154, 83);
      await Contexto.ArcAsync(40, 95, 24, 7 * Math.PI / 6, 3 * Math.PI / 2, false);
      await Contexto.LineToAsync(40, 71);
      await Contexto.ArcAsync(40, 95, 19, 3 * Math.PI / 2, 7 * Math.PI / 6, true);
      await Contexto.LineToAsync(19.2154, 83);
      await Contexto.ClosePathAsync();
      await Contexto.SetFillStyleAsync(mColorAlarma < 0 ? "gray" : "#F19E36");
      await Contexto.FillAsync();

      // sector verde.
      await Contexto.BeginPathAsync();
      await Contexto.MoveToAsync(40, 71);
      await Contexto.ArcAsync(40, 95, 24, 3 * Math.PI / 2, 11 * Math.PI / 6, false);
      await Contexto.LineToAsync(60.7846, 83);
      await Contexto.ArcAsync(40, 95, 19, 11 * Math.PI / 6, 3 * Math.PI / 2, true);
      await Contexto.LineToAsync(40, 71);
      await Contexto.ClosePathAsync();
      await Contexto.SetFillStyleAsync(mColorAlarma < 0 ? "gray" : "#489432");
      await Contexto.FillAsync();

      // sector azul.
      await Contexto.BeginPathAsync();
      await Contexto.MoveToAsync(60.7846, 83);
      await Contexto.ArcAsync(40, 95, 24, 11 * Math.PI / 6, 2 * Math.PI, false);
      await Contexto.ArcAsync(40, 95, 24, 0, Math.PI / 6, false);
      await Contexto.LineToAsync(56.4545, 104.5);
      await Contexto.ArcAsync(40, 95, 19, Math.PI / 6, 0, true);
      await Contexto.ArcAsync(40, 95, 19, 2 * Math.PI, 11 * Math.PI / 6, true);
      await Contexto.LineToAsync(60.7846, 83);
      await Contexto.ClosePathAsync();
      await Contexto.SetFillStyleAsync(mColorAlarma < 0 ? "gray" : "#5683E8");
      await Contexto.FillAsync();

      // Flecha.
      double Angulo;
      switch (mColorAlarma)
      {
        case 4:
          Angulo = Math.PI;
          break;
        case 3:
          Angulo = 4 * Math.PI / 3;
          break;
        case 2:
          Angulo = 5 * Math.PI / 3;
          break;
        case 1:
          Angulo = 0;
          break;
        default:
          return; // no grafica.
      }

      double Cos = Math.Cos(Angulo);
      double Sin = Math.Sin(Angulo);
      double AbscMod;
      double OrdMod;
      await Contexto.BeginPathAsync();
      ConvertirPunto(-2, 0, Cos, Sin, out AbscMod, out OrdMod);
      await Contexto.MoveToAsync(AbscMod, OrdMod);
      ConvertirPunto(0, -2, Cos, Sin, out AbscMod, out OrdMod);
      await Contexto.LineToAsync(AbscMod, OrdMod);
      ConvertirPunto(25, 0, Cos, Sin, out AbscMod, out OrdMod);
      await Contexto.LineToAsync(AbscMod, OrdMod);
      ConvertirPunto(0, 2, Cos, Sin, out AbscMod, out OrdMod);
      await Contexto.LineToAsync(AbscMod, OrdMod);
      ConvertirPunto(-2, 0, Cos, Sin, out AbscMod, out OrdMod);
      await Contexto.LineToAsync(AbscMod, OrdMod);
      await Contexto.ClosePathAsync();
      await Contexto.SetFillStyleAsync("black"); // CRutinas.ColorDesdeCodigo(mColorAlarma));
      await Contexto.FillAsync();

    }

    private void ConvertirPunto(double Absc0, double Ord0, double Cos, double Sin, out double Absc1, out double Ord1)
    {
      Absc1 = 40 + Absc0 * Cos - Ord0 * Sin;
      Ord1 = 95 + Absc0 * Sin + Ord0 * Cos;
    }

    private double ConvertirOrdenada(double Valor, double Angulo)
    {
      return 95 + Valor * Math.Sin(Angulo);
    }

    private double mDimensionCaracter;

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {

      try
      {

        if (mbCerrado)
				{
          return;
				}

        if (mIndicadores == null)
        {
          await AgregarIndicadoresATimerAsync();
          StateHasChanged();
        }

        try
        {

          if (CanvasGrafico == null)
					{
            return;
					}

          mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasGrafico);

          await mContexto.BeginBatchAsync();

          try
          {

            await mContexto.ClearRectAsync(0, 0, ANCHO_TARJETA, ALTO_TARJETA);

            await mContexto.SetFontAsync("12px serif");
            TextMetrics Medida = await mContexto.MeasureTextAsync("H");
            mDimensionCaracter = Medida.Width + 2;

            // Rectangulo base.
            await mContexto.BeginPathAsync();
            //await mContexto.MoveToAsync(10, 0);
            //await mContexto.LineToAsync(ANCHO_TARJETA - 10, 0);
            //await mContexto.ArcToAsync(ANCHO_TARJETA, 0, ANCHO_TARJETA, 10, 10);
            //await mContexto.LineToAsync(ANCHO_TARJETA, ALTO_TARJETA);
            //await mContexto.LineToAsync(0, ALTO_TARJETA);
            //await mContexto.LineToAsync(0, 10);
            //await mContexto.ArcToAsync(0, 0, 10, 0, 10);
            await mContexto.MoveToAsync(0, 0);
            await mContexto.LineToAsync(ANCHO_TARJETA, 0);
            await mContexto.LineToAsync(ANCHO_TARJETA, ALTO_TARJETA);
            await mContexto.LineToAsync(0, ALTO_TARJETA);
            await mContexto.LineToAsync(0, 0);
            await mContexto.ClosePathAsync();
            await mContexto.SetFillStyleAsync("#D1D1D1");
            await mContexto.FillAsync();
            await mContexto.SetLineWidthAsync(1);
            await mContexto.SetStrokeStyleAsync("gray");
            await mContexto.StrokeAsync();

            // Lineas de separacion.
            await mContexto.BeginPathAsync();
            await mContexto.MoveToAsync(2, 50);
            await mContexto.LineToAsync(ANCHO_TARJETA - 2, 50);
            await mContexto.MoveToAsync(80, 50);
            await mContexto.LineToAsync(80, ALTO_TARJETA - 2);
            await mContexto.ClosePathAsync();
            await mContexto.SetLineWidthAsync((float)0.5);
            await mContexto.SetStrokeStyleAsync("gray");
            await mContexto.StrokeAsync();

            // Texto superior.
            await mContexto.SetFillStyleAsync("#000000");
            await CRutinas.PonerEtiquetaHorizontalAsync(mContexto, 0, 0, ANCHO_TARJETA, 50, Pregunta.Pregunta, mDimensionCaracter);

            // Reloj.
            await DibujarRelojAsync(mContexto);

          }
          finally
          {
            await mContexto.EndBatchAsync();
          }

        }
        catch (Exception)
        {
          //CRutinas.DesplegarMsg(ex);
          //await OnAfterRenderAsync(firstRender);
        }
      }
      catch (Exception)
      {
//        await OnAfterRenderAsync(firstRender);
      }
    }

  }

}
