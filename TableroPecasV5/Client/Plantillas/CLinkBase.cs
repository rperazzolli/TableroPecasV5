using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Plantillas
{
  public class CLinkBase
  {
    public virtual CDatoIndicador Indicador { get; set; }
    public Int32 CodigoElemento { get; set; } // es el codigo del elemento en la dimension.
    public Int32 CodigoUnico { get; set; }

    [Parameter]
    public bool Ampliado
    {
      get { return Componente == null ? false : Componente.Ampliado; }
      set
      {
        if (Componente != null && Componente.Ampliado != value)
        {
          Componente.ImponerAmpliado(value);
        }
      }
    }

    private CBaseGrafico mComponente;
    public CBaseGrafico Componente
    {
      get { return mComponente; }
      set
      {
        if (mComponente != value)
        {
          mComponente = value;
          if (mComponente != null)
          {
            mComponente.ImponerAbscisa(mAbscisa);
            mComponente.ImponerOrdenada(mOrdenada);
          }
        }
      }
    }

    private double mAlto = 250;
    public double Alto
    {
      get
      {
        return (mComponente == null ? mAlto : mComponente.Alto);
      }
      set
      {
        mAlto = Math.Floor(value);
        if (mComponente != null)
        {
          mComponente.ImponerAlto(value);
        }
      }
    }

    private double mAncho = 300;
    public double Ancho
    {
      get
      {
        return (mComponente == null ? mAncho : mComponente.Ancho);
      }
      set
      {
        mAncho = Math.Floor(value);
        if (mComponente != null)
        {
          mComponente.ImponerAncho(value);
        }
      }
    }

    private Int32 mAbscisa = -999;
    public Int32 Abscisa
    {
      get { return (mComponente == null ? mAbscisa : mComponente.Abscisa); }
      set
      {
        mAbscisa = value;
        if (mComponente != null && mComponente.Abscisa != value)
        {
          mComponente.ImponerAbscisa(value);
        }
      }
    }

    private Int32 mOrdenada = -999;
    public Int32 Ordenada
    {
      get { return (mComponente == null ? mOrdenada : mComponente.Ordenada); }
      set
      {
        mOrdenada = value;
        if (mComponente != null && mComponente.Ordenada != value)
        {
          mComponente.ImponerOrdenada(value);
        }
      }
    }

    public Int32 PosicionUnica { get; set; }

    public CLinkBase()
    {
      CodigoUnico = Logicas.CLinkGrafico.gCodigoUnico++;
    }
  }

}
