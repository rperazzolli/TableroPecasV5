using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Plantillas
{
  public class CLinkTendencia : CLinkBase
  {
    public bool MostrarReferencias { get; set; }
    public bool MostrarBarras { get; set; }
    public bool MostrarAcumulado { get; set; }
    public bool MostrarPromedio { get; set; }
    public bool MostrarEvolutivo { get; set; }
    public List<Int32> IndicadoresAsociados { get; set; }
    public List<Int32> CodigosElementosAsociados { get; set; }
    public List<bool> EscalasDerechas { get; set; }

    private CLinkReloj mReloj = null;
    public CLinkReloj Reloj
    {
      get { return mReloj; }
      set
      {
        if (value != mReloj)
        {
          mReloj = value;
          if (mReloj != null && mReloj.Alarmas == null)
          {
            BuscarAlarmasRelojAsync();
          }
        }
      }
    }

    private CDatoIndicador mIndicador = null;
    public override CDatoIndicador Indicador
    {
      get { return (mReloj == null ? mIndicador : mReloj.Indicador); }
      set
      {
        if (mReloj == null)
        {
          mIndicador = value;
        }
        else
        {
          mReloj.Indicador = value;
        }
      }
    }

    public Componentes.CComponenteTendencias ComponentePropio
    {
      get
      {
        return (Componentes.CComponenteTendencias)Componente;
      }
      set
      {
        Componente = value;
        if (value != null)
        {
          value.ImponerAncho(Ancho);
          value.ImponerAlto(Alto);
        }
        value.EjecutarRefresco = FncRefrescar;
      }
    }

    public void FncRefrescar()
    {
      if (Componente != null)
      {
        Componente.Redibujar();
      }
    }

    public long AnchoGrafico
    {
      get { return (ComponentePropio == null ? (Int32)Ancho : ComponentePropio.AnchoGrafico); }
    }

    public long AltoGrafico
    {
      get { return (ComponentePropio == null ? (Int32)Alto : ComponentePropio.AltoGrafico); }
    }

    private Int32 mCodigoElementoDimension = -1;
    public Int32 CodigoElementoDimension
    {
      get { return (mReloj == null ? mCodigoElementoDimension : mReloj.CodigoElemento); }
      set
      {
        if (mReloj == null)
        {
          mCodigoElementoDimension = value;
        }
        else
        {
          mReloj.CodigoElemento = value;
        }
      }
    }

    [Inject]
    public HttpClient Http { get; set; }

    private async void BuscarAlarmasRelojAsync()
    {
      mReloj.Alarmas = await Contenedores.CContenedorDatos.ObtenerAlarmasIndicadorAsync(Http, mReloj.Indicador);
      PosicionPuntoSeleccionado = (mReloj.Alarmas == null || mReloj.Alarmas.Count == 0 ? -1 : mReloj.Alarmas.Count - 1);
      if (Componente != null)
      {
        Componente.Redibujar();
      }
    }

    public Int32 PosicionPuntoSeleccionado { get; set; } = -1;

    public CLinkTendencia()
    {
      MostrarAcumulado = false;
      MostrarBarras = false;
      MostrarEvolutivo = true;
      MostrarPromedio = false;
      MostrarReferencias = false;
      IndicadoresAsociados = new List<int>();
      CodigosElementosAsociados = new List<int>();
      EscalasDerechas = new List<bool>();
    }
  }

}
