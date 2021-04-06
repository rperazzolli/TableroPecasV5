using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Logicas;

namespace TableroPecasV5.Client.Plantillas
{
  public class CLinkOtro : CLinkBase
  {
    private CDatosOtroLista mDatos = null;
    public CDatosOtroLista Datos
    {
      get
      {
        return mDatos;
      }
      set
      {
        if (value != mDatos)
        {
          mDatos = value;
          Abscisa = (Int32)mDatos.Posicion.X;
          Ordenada = (Int32)mDatos.Posicion.Y;
          Ancho = mDatos.Ancho;
          Alto = mDatos.Alto;
        }
      }
    }

    public List<LineaFiltro> Lineas
		{
      get
			{
        if (ComponenteFiltros!=null && ComponenteFiltros.Lineas == null && Filtros!=null &&
            Filtros.Filtrador!=null)
				{
          ComponenteFiltros.CrearLineas();
        }
        return (ComponenteFiltros == null ? null : ComponenteFiltros.Lineas);
			}
		}

    private CLinkContenedorFiltros mFiltros = null;

    public CLinkContenedorFiltros Filtros
    {
      get { return mFiltros; }
      set
      {
        if (mFiltros != value)
        {
          mFiltros = value;
        }
      }
    }

    public CFiltrador Filtrador
    {
      get
      {
        return (Filtros == null ? null : Filtros.Filtrador);
      }
    }

    [Parameter]
    public bool Minimizado { get; set; } = true;

    private double mAbscisaPropia;
    private double mOrdenadaPropia;
    private double mAnchoPropio;
    private double mAltoPropio;

    public void Maximizar()
    {
      if (Minimizado)
      {
        NivelFlotantePropio = 90;
        mAbscisaPropia = Datos.Posicion.X;
        mOrdenadaPropia = Datos.Posicion.Y;
        mAnchoPropio = Datos.Ancho;
        mAltoPropio = Datos.Alto;
        Datos.Posicion.X = 2;
        Datos.Posicion.Y = 2;
        Datos.Ancho = Contenedores.CContenedorDatos.AnchoPantallaAmpliada - 4;
        Datos.Alto = Contenedores.CContenedorDatos.AltoPantalla - 100;
        Minimizado = false;
        if (ComponenteMimico != null)
        {
          ComponenteMimico.AjustarFactorEscala(Datos.Ancho, Datos.Alto);
        }
        if (ComponenteGradiente != null)
        {
          InformarComponente();
        }
      }
    }

    public void ImponerProveedor(Datos.CProveedorComprimido Proveedor, bool Refrescar = true)
    {
      //if (ComponenteMimico != null)
      //{
      //  ComponenteMimico.Proveedor = Proveedor;
      //  ComponenteMimico.DatosSinFiltroPropio = Proveedor.Datos;
      //  ComponenteCalor.DatosCompletos = false;
      //  if (Refrescar)
      //  {
      //    ComponenteCalor.Refrescar();
      //  }
      //}
    }

    public string Direccion
    {
      get { return Datos.Clase.ToString() + CodigoUnico.ToString(); }
    }

    public Int32 NivelFlotantePropio { get; set; } = 1;

    public void Reducir()
    {
      if (!Minimizado)
      {
        NivelFlotantePropio = 1;
        Datos.Posicion.X = mAbscisaPropia;
        Datos.Posicion.Y = mOrdenadaPropia;
        Datos.Ancho = mAnchoPropio;
        Datos.Alto = mAltoPropio;
        Minimizado = true;
        if (ComponenteMimico != null)
        {
          ComponenteMimico.AjustarFactorEscala(Datos.Ancho, Datos.Alto);
        }
        if (ComponenteGradiente != null)
        {
          InformarComponente();
        }
      }
    }

    private void InformarComponente()
    {
      if (ComponenteGradiente != null)
      {
        //ComponenteMapa.ImponerAncho(Ancho);
        //ComponenteMapa.ImponerAlto(Alto);
        ComponenteGradiente.ReubicarCentro = true;
      }
    }
    public Logicas.CLogicaMapaGradiente ComponenteGradiente { get; set; }

    public CLogicaMimico ComponenteMimico { get; set; }

    private CLogicaContenedorFiltros mComponenteFiltros = null;
    public CLogicaContenedorFiltros ComponenteFiltros
    {
      get { return mComponenteFiltros; }
      set
      {
        if (mComponenteFiltros != value)
        {
          mComponenteFiltros = value;
          if (mComponenteFiltros.Lineas == null)
          {
            mComponenteFiltros.CrearLineas();
            mComponenteFiltros.Redibujar();
          }
        }
      }
    }

    public CProveedorComprimido Proveedor
		{
      get { return (Filtrador == null ? null : Filtrador.Proveedor); }
		}

    public CGrillaDatos ComponenteGrilla
    {
      get { return Componente as CGrillaDatos; }
      set
      {
        if (value != Componente)
        {
          Componente = value;
          // asocia el componente a un filtrador de datos.
          if (Filtros.Filtrador == null && !Filtros.ComponentesAsociados.Contains(this))
          {
            // si no se creo el filtro, queda a la espera de los eventos.
            Filtros.ComponentesAsociados.Add(this);
          }
          else
          {
            value.Proveedor = Filtros.Filtrador.Proveedor;
          }
        }
      }
    }

    public CLinkOtro()
    {
    }
  }

}
