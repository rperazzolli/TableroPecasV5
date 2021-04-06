using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Plantillas
{
  public class CDatosOtroLista
  {
    public Int32 Codigo { get; set; }
    public Int32 CodigoPropio { get; set; }
    public CGrafV2DatosContenedorBlock.ClaseBlock Clase { get; set; }
    public string Nombre { get; set; }
    public ClaseElemento ClaseIndicador { get; set; }
    public Int32 CodigoElementoDimension { get; set; }
    public Point Posicion { get; set; }
    public double Ancho { get; set; }
    public double Alto { get; set; }
    public string Parametros { get; set; }

    public CDatosOtroLista()
    {
      FncComun();
      Clase = CGrafV2DatosContenedorBlock.ClaseBlock.NoDefinida;
      CodigoPropio = -1;
      Nombre = "";
      ClaseIndicador = ClaseElemento.NoDefinida;
      CodigoElementoDimension = -1;
      Parametros = "";
    }

    //public CDatosOtroLista(CElementoIndicador IndicadorGrilla)
    //{
    //  Clase = CGrafV2DatosContenedorBlock.ClaseBlock.Grilla;
    //  FncComun();
    //  CodigoPropio = IndicadorGrilla.Datos.Codigo;
    //  Nombre = IndicadorGrilla.Datos.Descripcion;
    //  ClaseIndicador = ClaseElemento.Indicador;
    //  CodigoElementoDimension = -1;
    //}

    private void FncComun()
    {
      CDatosGrafLista.gCodigo++;
      Codigo = CDatosGrafLista.gCodigo;
      CodigoElementoDimension = -1;
      Posicion = new Point(-1000, -1000);
      Ancho = -1;
      Alto = -1;
    }

    public CDatosOtroLista(CElementoMimicoCN Mimico)
    {
      Clase = CGrafV2DatosContenedorBlock.ClaseBlock.Mimico;
      FncComun();
      CodigoPropio = Mimico.Codigo;
      Nombre = Mimico.Nombre;
      ClaseIndicador = ClaseElemento.NoDefinida;
      CodigoElementoDimension = -1;
    }

    public void CargarDesdeXML(XElement Elemento)
    {

      Clase = (CGrafV2DatosContenedorBlock.ClaseBlock)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE_BLOCK);

      Nombre = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_NOMBRE);
      CodigoPropio = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CODIGO);

      double AnchoLocal;
      double AltoLocal;
      Posicion = CContenedorBlocks.ObtenerPosicion(Elemento, out AnchoLocal, out AltoLocal);
      Ancho = AnchoLocal;
      Alto = AltoLocal;

      ClaseIndicador = (ClaseElemento)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE_ELEMENTO);
      CodigoElementoDimension = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_ELEMENTO_DIMENSION);
      Parametros = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_PARAMETROS);

    }

    public CGrafV2DatosContenedorBlock CrearGrafV2()
    {
      return new CGrafV2DatosContenedorBlock()
      {
        CodigoPropio = CodigoPropio,
        Agrupacion = ModoAgruparDependiente.NoDefinido,
        AgrupIndep = ModoAgruparIndependiente.NoDefinido,
        Alto = Alto,
        Ancho = Ancho,
        Clase = ClaseGrafico.NoDefinido,
        ClaseElemento = Clase,
        ClaseOrigen = ClaseIndicador,
        CodigoElementoDimension = CodigoElementoDimension,
        ColumnaDatos = "",
        ColumnaAbscisas = "",
        ColumnaOrdenadas = "",
        ColumnaSexo = "",
        Filtro = new CPasoCondicionesBlock(),
        FiltrosBlock = new List<CPasoCondicionesBlock>(),
        Nombre = Nombre,
        Posicion = Posicion,
        SaltoHistograma = 0,
        UsaFiltroPropio = false,
        Visible = true,
        Parametros = Parametros
      };
    }

  }

}
