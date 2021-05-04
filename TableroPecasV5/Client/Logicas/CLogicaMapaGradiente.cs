using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaMapaGradiente : CLogicaBaseGradientes
  {

    public CLogicaMapaGradiente()
		{
      mPosicionMapBing = -1;
		}

    [Parameter]
    public CDatoIndicador Indicador
    {
      get { return IndicadorBase; }
      set { IndicadorBase = value; }
    }

    [Parameter]
    public Int32 CodigoElementoDimension
    {
      get { return CodigoElementoDimensionBase; }
      set { CodigoElementoDimensionBase = value; }
    }

    [Parameter]
    public Int32 CodigoCapa
    {
      get { return CodigoCapaBase; }
      set { CodigoCapaBase = value; }
    }

    [Parameter]
    public Plantillas.CLinkMapa Mapa
    {
      get { return MapaBase; }
      set { MapaBase = value; }
    }

    [Parameter]
    public string Direccion
    {
      get { return DireccionBase; }
      set { DireccionBase = value; }
    }

    [Parameter]
    public double Abscisa
    {
      get { return AbscisaBase; }
      set { AbscisaBase = value; }
    }

    [Parameter]
    public double Ordenada {
			get
			{
				return OrdenadaBase;
			}
			set
			{
				OrdenadaBase = value;
			}
		}

    [Parameter]
    public double Ancho {
			get
			{
				return AnchoBase;
			}
			set
			{
				AnchoBase = value;
			}
		}

    [Parameter]
    public double Alto
    {
      get { return AltoBase; }
      set { AltoBase = value; }
    }

    [Parameter]
    public Int32 NivelFlotante
    {
      get { return NivelFlotanteBase; }
      set { NivelFlotanteBase = value; }
    }

    [Parameter]
    public bool ComoComponente
		{
      get { return ComoComponenteBase; }
      set { ComoComponenteBase = value; }
		}

    [Parameter]
    public CCapaWSSCN Capa
    {
      get { return mCapa; }
      set { mCapa = value; }
    }

    [Parameter]
    public CFiltrador Filtrador
    {
      get { return mFiltrador; }
      set
      {
        if (mFiltrador != value)
        {
          mFiltrador = value;
          if (mFiltrador != null && mFiltrador.Proveedor != null)
          {
            Proveedor = mFiltrador.Proveedor;
          }
        }
      }
    }

    [Parameter]
    public CProveedorComprimido Proveedor
    {
      get { return ProveedorBase; }
      set { ProveedorBase = value; }
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
      await base.OnAfterRenderAsync(firstRender);
    }

  }

}
