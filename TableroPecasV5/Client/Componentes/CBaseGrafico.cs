using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TableroPecasV5.Client.Logicas;

namespace TableroPecasV5.Client.Componentes
{

  public class CBaseGrafico: ComponentBase
  {

    protected readonly string Id;
    protected ElementReference _canvasRef;

    public static Int32 BordeSuperior { get; set; } = 0;

    public delegate void FncRefrescarContenedor();
    public event FncRefrescarContenedor AlRefrescarHaciaSuperior;
    public static FncRefrescarContenedor gFncRefresco { get; set; }

    private static Int32 gCodigoUnico = 0;

    public const Int32 NIVEL_SELECCIONADO = 8;

    public CBaseGrafico()
    {
      CodigoUnico = gCodigoUnico++;
      if (gFncRefresco != null)
      {
        AlRefrescarHaciaSuperior += gFncRefresco;
      }
    }

    ~CBaseGrafico()
    {
      if (gFncRefresco != null)
      {
        AlRefrescarHaciaSuperior -= gFncRefresco;
      }
    }



    public Int32 NivelPropio { get; set; } = 1;

    [Parameter]
    public bool Seleccionado { get; set; } = false;

    [Parameter]
    public bool Encima
    {
      get { return (Seleccionado || Ampliado ? true: false); }
      set
      {
        Seleccionado = value;
      }
    }

    [CascadingParameter]
    public CLogicaIndicador Contenedor { get; set; }

    public void DeSeleccionar()
    {
      if (Seleccionado)
      {
        Seleccionado = false;
      }
    }

    public FncRefrescarContenedor FncReposicionarArriba { get; set; }

    private bool mbAmpliado = false;

    [Parameter]
    public bool Ampliado
    {
      get { return mbAmpliado; }
      set { mbAmpliado = value; }
    }

    public void ImponerAmpliado(bool B)
    {
      Ampliado = B;
    }

    [Parameter]
    public Int32 Abscisa { get; set; }

    [Parameter]
    public Int32 Ordenada { get; set; }

    [Parameter]
    public double Ancho { get; set; }

    [Parameter]
    public double Alto { get; set; }

    public Action EjecutarRefresco { get; set; }

    [Inject]
    public IJSRuntime Runtime { get; set; }

    private Int32 mCodigoUnico = -1;

    [Parameter]
    public Int32 CodigoUnico
    {
      get { return mCodigoUnico; }
      set
      {
        if (value != mCodigoUnico)
        {
          mCodigoUnico = value;
        }
      }
    }

    public virtual void RefrescarSuperior()
    {
      AlRefrescarHaciaSuperior?.Invoke();
    }

    public void ImponerEncima(bool bEncima)
    {
      Encima = bEncima;
    }

    public string Nombre_SVG
    {
      get
      {
        return "CMP_SVG_" + CodigoUnico.ToString();
      }
    }

    public void ImponerAbscisa(double Valor)
    {
      Abscisa = (Int32)Valor;
    }

    public void ImponerOrdenada(double Valor)
    {
      Ordenada = (Int32)Valor;
    }

    public void ImponerAlto(double Valor)
    {
      Alto = Valor;
    }

    public void ImponerAncho(double Valor)
    {
      Ancho = Valor;
    }

    public bool Seleccionar(CBaseGrafico GrafSeleccionado)
    {
      if (FncReposicionarArriba != null)
			{
        FncReposicionarArriba();
			}

      bool Ahora = (GrafSeleccionado == this);
      if (Ahora != Seleccionado)
      {
        Seleccionado = Ahora;
        StateHasChanged();
        return true;
      }
      else
      {
        return false;
      }
    }

    public void BloquearGrafico()
		{
      mbBloquear = true;
		}

    protected bool mbBloquear = false;

    public void Redibujar()
    {
      mbBloquear = false;
      StateHasChanged();
    }

    public void ImponerPosicion(string Posicion)
    {
      string[] Posiciones = Posicion.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      if (Posiciones.Length == 4)
      {
        ImponerPosicion(Int32.Parse(Posiciones[0]) - Contenedores.CContenedorDatos.DefasajeAbscisasPantallaIndicadores,
            Int32.Parse(Posiciones[1]) - Contenedores.CContenedorDatos.DefasajeOrdenadasPantallaIndicadores,
            Int32.Parse(Posiciones[2]), Int32.Parse(Posiciones[3]));
      }
    }

    public virtual void ImponerPosicion(Int32 Absc0, Int32 Ord0, Int32 Ancho0, Int32 Alto0)
    {
      Abscisa = Absc0;
      Ordenada = Ord0;
      bool bCambio = (Ancho != Ancho0 || Alto != Alto0);
      Ancho = Ancho0;
      Alto = Alto0;
      if (bCambio)
      {
        IntentarRefrescarGraficoPorResize();
      }
    }

    protected virtual void IntentarRefrescarGraficoPorResize()
    {

    }

    public delegate void FncEventoRefresco(object Referencia);
    public delegate void FncEventoTextoBool(string Nombre, bool B);
    public delegate void FncEventoCrearGrafDependiente(CLogicaGrafico Superior, Int32 OrdenColumna);
    public delegate void FncSeleccionarGrafico(CBaseGrafico Grafico);

    public async virtual Task<bool> AjustarDimensionesAsync(string Nombre)
    {
      object[] Prms = new object[1];
      Prms[0] = Nombre.Substring(1);
      double DimAbsc = await Runtime.InvokeAsync<double>("FuncionesJS.getInnerWidthElemento", Prms);
      double DimOrd = await Runtime.InvokeAsync<double>("FuncionesJS.getInnerHeightElemento", Prms);
      if (Ancho != DimAbsc || Alto != DimOrd)
      {
        if (Ancho != DimAbsc)
        {
          Ancho = DimAbsc;
        }
        if (Alto != DimOrd)
        {
          Alto = DimOrd;
        }
        if (EjecutarRefresco != null)
        {
          EjecutarRefresco();
        }
        return true;
      }
      else
      {
        return false;
      }
    }

  }
}
