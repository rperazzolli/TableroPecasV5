using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Logicas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Plantillas
{
  public class CLinkGraficoCnt : CLinkBase
  {
    public CDatosGrafLista Datos { get; set; }

    public CLinkReloj Reloj { get; set; } // Cuando usa el filtrador propio.

    private CLinkContenedorFiltros mFiltros = null;
    public CLinkContenedorFiltros Filtros
    {
      get { return mFiltros; }
      set
      {
        if (mFiltros != value)
        {
          mFiltros = value;
          if (!mFiltros.ComponentesAsociados.Contains(this))
          {
            mFiltros.ComponentesAsociados.Add(this);
          }
        }
      }
    }

    public CColumnaBase ColumnaAbscisas
    {
      get
      {
        if (Datos == null || Datos.Datos == null || Filtros == null || Filtros.Filtrador == null || Filtros.Filtrador.Proveedor == null)
        {
          return null;
        }
        else
        {
          return Filtros.Filtrador.Proveedor.ColumnaNombre(Datos.Datos.ColumnaAbscisas);
        }
      }
    }

    public CColumnaBase ColumnaOrdenadas
    {
      get
      {
        if (Datos == null || Datos.Datos == null || Filtros == null || Filtros.Filtrador == null || Filtros.Filtrador.Proveedor == null)
        {
          return null;
        }
        else
        {
          return Filtros.Filtrador.Proveedor.ColumnaNombre(Datos.Datos.ColumnaOrdenadas);
        }
      }
    }

    public CColumnaBase ColumnaSexo
    {
      get
      {
        if (Datos == null || Datos.Datos == null || Filtros == null || Filtros.Filtrador == null || Filtros.Filtrador.Proveedor == null)
        {
          return null;
        }
        else
        {
          return Filtros.Filtrador.Proveedor.ColumnaNombre(Datos.Datos.ColumnaSexo);
        }
      }
    }

    public ClaseGrafico Clase
    {
      get
      {
        if (Datos == null || Datos.Datos == null)
        {
          return ClaseGrafico.NoDefinido;
        }
        else
        {
          return Datos.Datos.Clase;
        }
      }
    }

    public ModoAgruparIndependiente AgrupamientoIndependiente
    {
      get { return (ComponentePropio == null ? ModoAgruparIndependiente.NoDefinido : ComponentePropio.AgrupamientoIndependiente); }
      set
      {
        if (ComponentePropio != null)
        {
          ComponentePropio.ImponerAgrupamientoIndependiente(value);
        }
      }
    }

    public ModoAgruparDependiente AgrupamientoDependiente
    {
      get { return (ComponentePropio == null ? ModoAgruparDependiente.NoDefinido : ComponentePropio.AgrupamientoDependiente); }
      set
      {
        if (ComponentePropio != null)
        {
          ComponentePropio.ImponerAgrupamientoDependiente(value);
        }
      }
    }

    public List<string> ValoresSeleccionados
    {
      get { return (ComponentePropio == null ? null : ComponentePropio.ValoresSeleccionados); }
      set
      {
        if (ComponentePropio != null)
        {
          ComponentePropio.ImponerValoresSeleccionados(value);
        }
      }
    }

    public List<CLogicaGrafico> GraficosDependientes
    {
      get { return (ComponentePropio == null ? new List<CLogicaGrafico>() : ComponentePropio.GraficosDependientes); }
      set
      {
        if (ComponentePropio != null)
        {
          ComponentePropio.ImponerGraficosDependientes(value);
        }
      }
    }

    public bool Detallado
    {
      get { return (ComponentePropio == null ? false : ComponentePropio.Detallado); }
      set
      {
        if (ComponentePropio != null)
        {
          ComponentePropio.ImponerDetallado(value);
        }
      }
    }

    public void ImponerProveedor(CProveedorComprimido Proveedor)
    {
      ComponentePropio.ImponerColumnaAbscisas(Proveedor.ColumnaNombre(Datos.Datos.ColumnaAbscisas));
      ComponentePropio.ImponerColumnaOrdenadas(Proveedor.ColumnaNombre(Datos.Datos.ColumnaOrdenadas));
      ComponentePropio.ImponerColumnaSexo(Proveedor.ColumnaNombre(Datos.Datos.ColumnaSexo));
      ComponentePropio.Proveedor = Proveedor;
    }

    public async Task ObtenerProveedorPropioAsync(HttpClient Http)
    {
      ImponerProveedor(await new CContenedorBlocks().CrearProveedorAsync(Http,
          Indicador.Codigo, Indicador.Dimension, CodigoElemento, Reloj.Clase, Reloj.ParametrosSC));
      //      AjustarPorCambioDatos(null);
    }

    public static CLogicaFiltroTextos.FncEventoTextoBool gFncRefrescarGraficos { get; set; } = null;

    public CLogicaGrafico ComponentePropio
    {
      get { return (CLogicaGrafico)Componente; }
      set
      {
        if (Componente != null)
        {
          ((CLogicaGrafico)Componente).AlCerrarGrafico -= gFncRefrescarGraficos;
        }
        Componente = value;
        if (value != null)
        {
          value.AlCerrarGrafico += gFncRefrescarGraficos;
          if (Filtros != null || !Datos.Datos.UsaFiltroPropio)
          {
            if (Filtros != null)
            {
              if (Filtros.Filtrador == null && !Filtros.ComponentesAsociados.Contains(this))
              {
                Filtros.ComponentesAsociados.Add(this);
              }
              else
              {
                ImponerProveedor(Filtros.Filtrador.Proveedor);
              }
            }
          }
        }
      }
    }

    public void FncRefrescarGrafico(object sender)
		{
      if (Componente != null)
			{
        Componente.Redibujar();
			}
		}

    private void AjustarPorCambioDatos(object sender)
    {
      ComponentePropio.RefrescarDatosTortaDesdeProveedor(
          CPasoCondicionesBlock.ConvertirACCondicionesFiltradorCN(Datos.Datos.FiltrosBlock));
      ComponentePropio.Redibujar();
    }

    public CLinkGraficoCnt()
    {
    }
  }

}
