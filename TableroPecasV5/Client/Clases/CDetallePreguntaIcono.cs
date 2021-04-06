using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Clases
{
  public class CDetallePreguntaIcono
  {
    private CPreguntaPreguntaWISCN mDetalle;
    private string mszNombre;

    public CDetallePreguntaIcono(CPreguntaPreguntaWISCN Elemento)
    {
      mDetalle = Elemento;
      mszNombre = "";
    }

    public CDetallePreguntaIcono(CDetallePreguntaCN Elemento)
    {
      mDetalle = CrearPreguntaPregunta(Elemento);
      mszNombre = "";
    }

    public CPreguntaPreguntaWISCN Detalle
    {
      get { return mDetalle; }
      set { mDetalle = value; }
    }

    public string Nombre
    {
      get { return mszNombre; }
      set { mszNombre = value; }
    }

    private CPreguntaPreguntaWISCN CrearPreguntaPregunta(CDetallePreguntaCN Detalle)
    {
      CPreguntaPreguntaWISCN Respuesta = new CPreguntaPreguntaWISCN();
      Respuesta.Clase = Detalle.ClaseDeDetalle;
      Respuesta.Codigo = -1;
      Respuesta.CodigoElemento = Detalle.Codigo;
      Respuesta.CodigoDimension = Detalle.ClaseEntidad;
      Respuesta.CodigoElementoDimension = Detalle.CodigoEntidad;
      Respuesta.CodigoPregunta = Detalle.CodigoPregunta;
      return Respuesta;
    }

    public override string ToString()
    {
      return mszNombre;
    }

  }
}
