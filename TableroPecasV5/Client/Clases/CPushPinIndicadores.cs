using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Clases
{
  public class CPushPinIndicadores: CPuntoTextoColor
  {
    private ColorBandera mColorIndicador;
    //private double mDeltaLat;
    //private double mDeltaLong;
    //private bool mbMouseAbajo;
    private Int32 mCodigoLayer;
    private CElementoPreguntasWISCN mDatos;

    public bool PuntoDesdeBing { get; set; }
    public bool PuntoDesdeWIS { get; set; }
    public CParValores Par { get; set; }

    public CPushPinIndicadores()
    {
      PuntoDesdeBing = false;
      PuntoDesdeWIS = false;
//      mbMouseAbajo = false;
      mCodigoLayer = -1;
      mDatos = null;
      mColorIndicador = ColorBandera.SinDatos;
    }

    public Int32 CodigoLayer
    {
      get { return mCodigoLayer; }
      set { mCodigoLayer = value; }
    }

    public CElementoPreguntasWISCN CrearDatos(ClaseCapa ClaseCapa, Int32 CodigoCapa)
    {
      CElementoPreguntasWISCN Respuesta = CRutinas.CrearElementoPreguntas(this);
      Respuesta.ClaseWIS = ClaseCapa;
      Respuesta.CodigoWIS = CodigoCapa;
      return Respuesta;
    }

    public CElementoPreguntasWISCN Datos
    {
      get
      {
        if (mDatos == null)
        {
          if (PuntoDesdeBing)
          {
            mDatos = CrearDatos(ClaseCapa.Bing, mCodigoLayer);
          }
        }
        return mDatos;
      }
      set { mDatos = value; }
    }

    public Int32 CodigoPregunta
    {
      get
      {
        if (mDatos == null)
        {
          return -1;
        }
        else
        {
          return mDatos.Codigo;
        }
      }
    }

    public Int32 CodigoPreguntaEntidad
    {
      get
      {
        if (mDatos == null)
        {
          return -1;
        }
        else
        {
          return mDatos.Dimension;
        }
      }
    }

    public string TextoAyuda
    {
      get
      {
        if (mDatos == null)
        {
          return "";
        }
        else
        {
          return mDatos.Nombre;
        }
      }
    }

    public ColorBandera ColorRelleno
    {
      get { return mColorIndicador; }
      set
      {
        mColorIndicador = value;
      }
    }

    public static ColorBandera ColorDesdeColorPregunta(CRutinas.CColoresPregunta ColorBase)
    {
      switch (ColorBase.ColorPregunta)
      {
        case ColoresParaPreguntas.Amarillo:
          return ColorBandera.Amarillo;
        case ColoresParaPreguntas.Azul:
          return ColorBandera.Azul;
        case ColoresParaPreguntas.Rojo:
          return ColorBandera.Rojo;
        case ColoresParaPreguntas.Verde:
          return ColorBandera.Verde;
        default:
          return ColorBandera.SinDatos;
      }
    }

    public void AjustarContenido(CRutinas.CColoresPregunta ColorBase)
    {
      if (ColorBase != null)
      {
        mColorIndicador = ColorDesdeColorPregunta(ColorBase);
      }
//      AjustarContenido();
    }

    //public async System.Threading.Tasks.Task AjustarColorElipseAsync(CDetallePreguntaIcono Elemento,
    //    double TamanioFuente)
    //{
    //  if (mDatos == null)
    //  {
    //    mColorIndicador = ColorBandera.SinDatos;
    //  }
    //  else
    //  {
    //    if (Elemento != null && !CRutinas.EstaComprendido(Elemento, mDatos.Contenidos))
    //    {
    //      mColorIndicador = ColorBandera.SinDatos;
    //    }
    //    else
    //    {
    //      mColorIndicador = await CRutinas.ObtenerColorBanderaAsync(null, Elemento, mDatos.Contenidos);
    //    }
    //  }

    //  if (Elemento != null && Elemento.Detalle.Clase == ClaseDetalle.Indicador)
    //  {
    //    PonerValorAsociado(Elemento.Detalle.Codigo, TamanioFuente);
    //  }
    //  else
    //  {
    //    Texto = "";
    //  }

    //  return;

    //}

    //private async Task<double> ValorIndicador(Int32 Codigo)
    //{
    //  WCFBPI.CInformacionAlarmaCN Valores=
    //      await Contenedores.CContenedorDatos.DatosDisponiblesIndicadorFechaAsync(Codigo);
    //  return (Valores == null ? double.NaN : Valores.Valor);
    //}

    private void PonerValorAsociado(Int32 CodigoIndicador,
        double TamanioFuente)
    {
      Texto = CRutinas.ValorATexto(Contenedores.CContenedorDatos.ValorGIS(
              CodigoIndicador, mDatos), 3);
    }

  }

  public delegate void FncSinParametros();

}
