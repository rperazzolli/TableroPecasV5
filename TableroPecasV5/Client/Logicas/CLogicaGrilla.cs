using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaGrilla : CBaseGrafico, IDisposable
  {
    [Parameter]
    public CDatoIndicador Indicador { get; set; }

    private CProveedorComprimido mProveedor = null;
    public CProveedorComprimido Proveedor
    {
      get { return mProveedor; }
      set
      {
        if (mProveedor != value)
        {
          if (mProveedor != null)
          {
            mProveedor.AlAjustarDependientes -= CorregirDatos;
          }
          mProveedor = value;
          if (mProveedor != null)
          {
            mProveedor.AlAjustarDependientes += CorregirDatos;
          }
          StateHasChanged();
        }
      }
    }

    public void Dispose()
		{
      if (mProveedor != null)
			{
        mProveedor.AlAjustarDependientes -= CorregirDatos;
			}
		}

    [CascadingParameter]
    Logicas.CLogicaIndicador Pagina { get; set; }

    public int AbscisaAbajo { get; set; }
    public int OrdenadaAbajo { get; set; }

    public void EventoMouseAbajo(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      Pagina.PonerElementoEncima(false, false, true, -1, -1);
      AbscisaAbajo = (int)e.ScreenX;
      OrdenadaAbajo = (int)e.ScreenY;
      Pagina.Refrescar();
    }

    public void Cerrar()
		{
      Pagina.EliminarGrilla();
		}

    private int mAbscisaPropia;
    private int mOrdenadaPropia;
    private double mAnchoPropio;
    private double mAltoPropio;

    public void Maximizar()
    {
      if (!Ampliado)
      {
        Ampliado = true;
        Seleccionado = true;
        Pagina.PonerElementoEncima(false, false, true, -1, -1);
        mAbscisaPropia = Abscisa;
        mOrdenadaPropia = Ordenada;
        mAnchoPropio = Ancho;
        mAltoPropio = Alto;
        Abscisa = 2;
        Ordenada = 2;
        Ancho = Contenedores.CContenedorDatos.AnchoPantallaAmpliada - 4;
        Alto = Contenedores.CContenedorDatos.AltoPantallaAmpliada - 4;
        Pagina.ReposicionarGrilla(Abscisa, Ordenada, (int)Ancho, (int)Alto);
        Pagina.PonerElementoEncima(false, false, true, -1, -1);
        Pagina.Refrescar();
      }
    }

    public void Reducir()
		{
      if (Ampliado)
      {
        Seleccionado = false;
        Pagina.PonerElementoEncima(false, false, false, -1, -1);
        Abscisa = mAbscisaPropia;
        Ordenada = mOrdenadaPropia;
        Ancho = mAnchoPropio;
        Alto = mAltoPropio;
        Ampliado = false;
        Pagina.ReposicionarGrilla(Abscisa, Ordenada, (int)Ancho, (int)Alto);
        Pagina.Refrescar();
      }
    }

    public void CorregirDatos(object Aa)
    {
      if (PosicionActual >= CantidadPaginas)
      {
        PosicionActual = CantidadPaginas - 1;
      }
      StateHasChanged();
    }

    public List<CLineaComprimida> LineasPagina
    {
      get
      {
        List<CLineaComprimida> Respuesta = new List<CLineaComprimida>();
        for (Int32 i=PosicionActual*LINEAS_PAGINA;i<(PosicionActual+1)*LINEAS_PAGINA && i < Proveedor.DatosVigentes.Count; i++)
        {
          Respuesta.Add(Proveedor.DatosVigentes[i]);
        }
        return Respuesta;
      }
    }
      
    public string EstiloPagina
    {
      get
      {
        return "width: " + Math.Floor(Ancho).ToString() + "px; height: " + Math.Floor(Alto).ToString() + "px;";
      }
    }

    public string EstiloDatos
    {
      get
      {
        return "width: " + Math.Floor(Ancho).ToString() + "px; height: " + Math.Floor(Alto-25).ToString() + "px; overflow-x: auto; overflow-y: auto;";
      }
    }

    public void IrAlInicio()
    {
      PosicionActual = 0;
    }

    public void Retroceder()
    {
      PosicionActual--;
    }

    public void Moverse(Int32 Cantidad)
    {
      PosicionActual += Cantidad;
    }

    public void IrAlCierre()
    {
      PosicionActual = CantidadPaginas-1;
    }

    private const Int32 LINEAS_PAGINA = 20;
    public Int32 CantidadPaginas
    {
      get {
        if (Proveedor == null)
        {
          return 0;
        }
        else
        {
          Int32 Lineas0 = Proveedor.DatosVigentes.Count - Proveedor.DatosVigentes.Count % LINEAS_PAGINA;
          return Lineas0 / LINEAS_PAGINA + ((Proveedor.DatosVigentes.Count % LINEAS_PAGINA) == 0 ? 0 : 1);
        }
      }
    }

    public Int32 PosicionActual { get; set; } = 0;

    public bool HayPaginasAtras
    {
      get { return (PosicionActual > 0); }
    }

    public bool HayPaginaAdelante
    {
      get { return (PosicionActual < (CantidadPaginas - 1)); }
    }

    public bool HayBoton1
    {
      get
      {
        return CantidadPaginas > (PosicionActual + 1);
      }
    }

    public bool HayBoton2
    {
      get
      {
        return CantidadPaginas > (PosicionActual + 2);
      }
    }

    public bool HayBoton3
    {
      get
      {
        return CantidadPaginas > (PosicionActual + 3);
      }
    }

    public bool HayBoton4
    {
      get
      {
        return CantidadPaginas > (PosicionActual + 4);
      }
    }

    public bool HayBoton5
    {
      get
      {
        return CantidadPaginas > (PosicionActual + 5);
      }
    }

    public string TextoBoton1
    {
      get { return (PosicionActual + 1).ToString(); }
    }

    public string TextoBoton2
    {
      get { return (PosicionActual + 2).ToString(); }
    }

    public string TextoBoton3
    {
      get { return (PosicionActual + 3).ToString(); }
    }

    public string TextoBoton4
    {
      get { return (PosicionActual + 4).ToString(); }
    }

    public string TextoBoton5
    {
      get { return (PosicionActual + 5).ToString(); }
    }

  }
}
