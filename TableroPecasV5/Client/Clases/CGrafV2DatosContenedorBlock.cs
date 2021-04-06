using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Clases
{
  public class CGrafV2DatosContenedorBlock
  {
    public enum ClaseBlock
    {
      Grafico = 1,
      Conjunto = 2,
      Comparativo = 3,
      MapaCalor = 4,
      MapaGradientes = 5,
      MapaControl = 6,
      Mimico = 7,
      Grilla = 8,
      Indicador = 9,
      Tendencia = 10,
      Ficha = 11,
      Consulta = 12,
      Pines = 14,
      NoDefinida = -1
    }

    public ClaseElemento ClaseOrigen { get; set; }
    public Int32 Indicador { get; set; }
    public Int32 CodigoElementoDimension { get; set; }
    public string Nombre { get; set; }
    public ClaseGrafico Clase { get; set; }
    public ClaseBlock ClaseElemento { get; set; }
    public ModoAgruparIndependiente AgrupIndep { get; set; }
    public ModoAgruparDependiente Agrupacion { get; set; }
    public string ColumnaAbscisas { get; set; }
    public string ColumnaOrdenadas { get; set; }
    public string ColumnaSexo { get; set; }
    public double SaltoHistograma { get; set; }
    public bool Visible { get; set; }
    public Datos.CPasoCondicionesBlock Filtro { get; set; }
    public List<Datos.CPasoCondicionesBlock> FiltrosBlock { get; set; }
    public bool UsaFiltroPropio { get; set; }
    public CPunto Posicion { get; set; }
    public double Ancho { get; set; }
    public double Alto { get; set; }

    public CGrafV2DatosContenedorBlock()
    {
      UsaFiltroPropio = false;
      Filtro = new Datos.CPasoCondicionesBlock();
      FiltrosBlock = new List<Datos.CPasoCondicionesBlock>();
      Posicion = new CPunto(-1000, -1000);
      Ancho = -1;
      Alto = -1;
    }

    public void AgregarAXML(XElement Superior, double Abscisa, double Ordenada, double Ancho, double Alto)
    {
      XElement Contenido = new XElement(CRutinas.CTE_ELEMENTO);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CLASE_ORG, ((Int32)ClaseOrigen).ToString());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CLASE_0, ((Int32)ClaseElemento).ToString());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CLASE, ((Int32)Clase).ToString());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_NOMBRE, Nombre);

      CRutinas.AgregarAtributosPosicion(Contenido, Abscisa, Ordenada, Ancho, Alto);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_INDICADOR, Indicador.ToString());
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_ELEMENTO_DIMENSION, CodigoElementoDimension.ToString());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_AGRUPACION_IND, ((Int32)AgrupIndep).ToString());
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_AGRUPACION_DEP, ((Int32)Agrupacion).ToString());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_COLUMNA_ABSC, ColumnaAbscisas);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_COLUMNA_ORD, ColumnaOrdenadas);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_COLUMNA_SEXO, ColumnaSexo);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_SALTO_HISTOGRAMA, CRutinas.FloatVStr(SaltoHistograma));

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_VISIBLE, (Visible ? "Y" : "N"));
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_FILTRO_PROPIO, (UsaFiltroPropio ? "Y" : "N"));

      if (Filtro != null)
      {
        Filtro.AgregarAXML(Contenido);
      }

      if (FiltrosBlock != null && FiltrosBlock.Count > 0)
      {
        XElement Filtros = new XElement(CRutinas.CTE_FILTROS);
        foreach (Datos.CPasoCondicionesBlock FilLocal in FiltrosBlock)
        {
          FilLocal.AgregarAXML(Filtros);
        }
        Contenido.Add(Filtros);
      }

      Superior.Add(Contenido);

    }

  }

  //public class CCondicionBlock
  //{
  //  public string Columna { get; set; }
  //  public ModoFiltrar Modo { get; set; }
  //  public string Valor { get; set; } // se usa tambien para minimo.
  //  public string ValorMaximo { get; set; }

  //  public CCondicionBlock()
  //  {
  //    Modo = ModoFiltrar.NoDefinido;
  //    Valor = "";
  //    ValorMaximo = "";
  //  }

  //  public void AgregarAXML(XElement Elemento)
  //  {
  //    XElement Contenido = new XElement(CRutinas.CTE_CONDICION);
  //    CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_COLUMNA, Columna);
  //    CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_MODO, ((Int32)Modo).ToString());
  //    CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_VALOR, Valor);
  //    if (ValorMaximo != null && ValorMaximo.Length > 0)
  //    {
  //      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_MAXIMO, ValorMaximo);
  //    }
  //    Elemento.Add(Contenido);
  //  }

  //  public void CargarDesdeXML(XElement Elemento)
  //  {
  //    Columna = Elemento.Attribute(CRutinas.CTE_COLUMNA).Value;
  //    Modo = (ModoFiltrar)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_MODO);
  //    Valor = Elemento.Attribute(CRutinas.CTE_VALOR).Value;
  //    XAttribute Atr = Elemento.Attribute(CRutinas.CTE_MAXIMO);
  //    if (Atr != null)
  //    {
  //      ValorMaximo = Atr.Value;
  //    }
  //  }

  //}

  //public class CGrupoCondicionesBlock
  //{
  //  public bool CumplirTodas { get; set; }
  //  public bool Incluye { get; set; }
  //  public Int32 CantidadMinima { get; set; }
  //  public List<CCondicionBlock> Condiciones { get; set; }

  //  public CGrupoCondicionesBlock()
  //  {
  //    CumplirTodas = false;
  //    Incluye = true;
  //    CantidadMinima = -1;
  //    Condiciones = new List<CCondicionBlock>();
  //  }

  //  public void AgregarAXML(XElement Elemento)
  //  {
  //    XElement Contenido = new XElement(CRutinas.CTE_GRUPO);
  //    CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CUMPLIR_TODAS, CumplirTodas ? "S" : "N");
  //    CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_INCLUYE, Incluye ? "S" : "N");
  //    CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CANTIDAD, CantidadMinima.ToString());

  //    if (Condiciones != null && Condiciones.Count > 0)
  //    {
  //      XElement ElCnd = new XElement(CRutinas.CTE_CONDICIONES);
  //      foreach (CCondicionBlock Cnd in Condiciones)
  //      {
  //        Cnd.AgregarAXML(ElCnd);
  //      }
  //      Contenido.Add(ElCnd);
  //    }

  //    Elemento.Add(Contenido);

  //  }

  //  public void CargarDesdeXML(XElement Elemento)
  //  {
  //    CumplirTodas = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_CUMPLIR_TODAS);
  //    Incluye = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_INCLUYE);
  //    CantidadMinima = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CANTIDAD);

  //    XElement ElCnds = Elemento.Element(CRutinas.CTE_CONDICIONES);
  //    if (ElCnds != null)
  //    {
  //      foreach (XElement ElCnd in ElCnds.Elements(CRutinas.CTE_CONDICION))
  //      {
  //        CCondicionBlock CndLocal = new CCondicionBlock();
  //        CndLocal.CargarDesdeXML(ElCnd);
  //        Condiciones.Add(CndLocal);
  //      }
  //    }

  //  }

  //}

  //public class CPasoCondicionesBlock
  //{
  //  public bool CumplirTodas { get; set; }
  //  public List<CGrupoCondicionesBlock> Grupos { get; set; }

  //  public CPasoCondicionesBlock()
  //  {
  //    CumplirTodas = false;
  //    Grupos = new List<CGrupoCondicionesBlock>();
  //  }

  //  public bool CorrespondeAColumna(string Columna)
  //  {
  //    if (Grupos.Count != 1)
  //    {
  //      return false;
  //    }

  //    foreach (CGrupoCondicionesBlock Gr in Grupos)
  //    {
  //      foreach (CCondicionBlock Cnd in Gr.Condiciones)
  //      {
  //        if (Cnd.Columna != Columna)
  //        {
  //          return false;
  //        }
  //      }
  //    }
  //    return true;
  //  }

  //  public void AgregarAXML(XElement Elemento)
  //  {
  //    XElement Contenido = new XElement(CRutinas.CTE_FILTRO);
  //    CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CUMPLIR_TODAS, (CumplirTodas ? "S" : "N"));

  //    foreach (CGrupoCondicionesBlock Grupo in Grupos)
  //    {
  //      Grupo.AgregarAXML(Contenido);
  //    }

  //    Elemento.Add(Contenido);

  //  }

  //  public void CargarDesdeXML(XElement Elemento)
  //  {

  //    CumplirTodas = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_CUMPLIR_TODAS);

  //    foreach (XElement ElemGrupo in Elemento.Elements(CRutinas.CTE_GRUPO))
  //    {
  //      CGrupoCondicionesBlock GrupoLocal = new CGrupoCondicionesBlock();
  //      GrupoLocal.CargarDesdeXML(ElemGrupo);
  //      Grupos.Add(GrupoLocal);
  //    }

  //  }

  //}

  public enum ClaseGraficoDetalle
  {
    Torta,
    Barras,
    Pareto,
    Puntos,
    Histograma,
    NoDefinido
  }

}
