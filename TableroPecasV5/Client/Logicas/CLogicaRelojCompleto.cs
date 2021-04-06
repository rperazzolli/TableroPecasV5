using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaRelojCompleto : CBaseGrafico
  {

    public static Int32 ANCHO_RELOJ_COMPLETO = 260;
    public const Int32 ALTO_RELOJ_COMPLETO = 200;

    public string EstiloReloj
    {
      get
      {
        return "width: " + ANCHO_RELOJ_COMPLETO.ToString() + "px; height: " + ALTO_RELOJ_COMPLETO.ToString() +
            "px; left: 0px; top: 0px; position: absolute; text-align: center; z-index: " +
            NivelFlotante.ToString() + ";";
      }
    }

    private CDatoIndicador mIndicador = null;

    [Parameter]
    public CDatoIndicador Indicador
    {
      get { return mIndicador; }
      set
      {
        if (mIndicador != value)
        {
          mIndicador = value;
        }
      }
    }

    [Parameter]
    public List<CInformacionAlarmaCN> Alarmas { get; set; }

    [Parameter]
    public Int32 CodigoElementoDimension { get; set; }

    public CLogicaReloj ComponenteReloj;

    private Int32 mCodigo = -1;

    [Parameter]
    public Int32 Codigo
    {
      get { return mCodigo; }
      set
      {
        if (mCodigo != value)
        {
          mCodigo = value;
          CDatoIndicador IndicadorLocal = (from I in Contenedores.CContenedorDatos.ListaIndicadores
                       where I.Codigo == mCodigo
                       select I).FirstOrDefault();
          if (IndicadorLocal != null)
          {
            Indicador = IndicadorLocal;
            StateHasChanged();
          }
        }
      }
    }

    public string NombreIndicador
    {
      get { return (Indicador == null ? "No definido" : Indicador.Descripcion); }
    }

    public string Unidades
    {
      get { return (Indicador == null ? "" : Indicador.Unidades); }
    }

    public string ValorIndicador
    {
      get
      {
        return (Indicador == null || ComponenteReloj == null ||
              ComponenteReloj.Alarmas == null || ComponenteReloj.Alarmas.Count == 0 ?
              "--" : ComponenteReloj.Alarmas.Last().Valor.ToString(Rutinas.CRutinas.FormatoDecimales(Indicador.Decimales)));
      }
    }

    [Inject]
    public HttpClient Http { get; set; }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
      if (ComponenteReloj != null && Indicador != null)
      {
        if (ComponenteReloj.AlarmasLeidas == null)
        {
          ComponenteReloj.AlarmasLeidas = await Contenedores.CContenedorDatos.ObtenerAlarmasIndicadorAsync(
            Http, Indicador);
          ComponenteReloj.Redibujar();
          StateHasChanged();
        }
      }
    }

  }
}
