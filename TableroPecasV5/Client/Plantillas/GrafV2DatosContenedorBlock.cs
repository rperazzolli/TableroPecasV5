using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Plantillas
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
      Pines = 14,
      Subconsulta = 15,
      NoDefinida = -1
    }

    public ClaseElemento ClaseOrigen { get; set; } = TableroPecasV5.Shared.ClaseElemento.NoDefinida;
    public Int32 CodigoPropio { get; set; } = -1;
    public Int32 Indicador { get; set; } = -1;
    public Int32 CodigoDimension { get; set; } = -1;
    public Int32 CodigoElementoDimension { get; set; } = -1;
    public string Nombre { get; set; } = "";
    public ClaseGrafico Clase { get; set; } = ClaseGrafico.NoDefinido;
    public ClaseBlock ClaseElemento { get; set; } = ClaseBlock.NoDefinida;
    public ModoAgruparIndependiente AgrupIndep { get; set; } = ModoAgruparIndependiente.NoDefinido;
    public ModoAgruparDependiente Agrupacion { get; set; } = ModoAgruparDependiente.NoDefinido;
    public string ColumnaDatos { get; set; } = "";
    public string ColumnaAbscisas { get; set; } = "";
    public string ColumnaOrdenadas { get; set; } = "";
    public string ColumnaSexo { get; set; } = "";
    public double SaltoHistograma { get; set; } = 10;
    public bool Visible { get; set; } = true;
    public CPasoCondicionesBlock Filtro { get; set; } = null;
    public List<CPasoCondicionesBlock> FiltrosBlock { get; set; } = new List<CPasoCondicionesBlock>();
    public bool UsaFiltroPropio { get; set; } = false;
    public Point Posicion { get; set; } = new Point(0, 0);
    public double Ancho { get; set; } = 100;
    public double Alto { get; set; } = 100;
    public string Parametros { get; set; } = "";

    public CGrafV2DatosContenedorBlock()
    {
      UsaFiltroPropio = false;
      Filtro = new CPasoCondicionesBlock();
      FiltrosBlock = new List<CPasoCondicionesBlock>();
      Posicion = new Point(-1000, -1000);
      Ancho = -1;
      Alto = -1;
    }

    public XElement CrearElemento()
		{
      XElement Elemento = new XElement(CRutinas.CTE_ELEMENTO);

      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_CLASE, ((Int32)Clase).ToString());
      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_CLASE_BLOCK, ((Int32)ClaseElemento).ToString());
      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_CLASE_2, ((Int32)ClaseOrigen).ToString());
      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_NOMBRE, Nombre);
      CRutinas.AgregarAtributosPosicion(Elemento, Posicion.X, Posicion.Y, Ancho, Alto);
      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_CODIGO, Indicador.ToString());
      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_CODIGO_2, CodigoPropio.ToString());
      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_DIMENSION, CodigoDimension.ToString());
      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_ELEMENTO_DIMENSION, CodigoElementoDimension.ToString());

      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_COLUMNA_DATOS, ColumnaDatos);
      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_COLUMNA_ABSC, ColumnaAbscisas);
      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_COLUMNA_ORD, ColumnaOrdenadas);
      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_COLUMNA_SEXO, ColumnaSexo);

      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_AGRUPACION_DEP, ((Int32)Agrupacion).ToString());
      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_AGRUPACION_IND, ((Int32)AgrupIndep).ToString());

      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_SALTO_HISTOGRAMA, CRutinas.FloatVStr(SaltoHistograma));

      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_VISIBLE, CRutinas.BoolToStr(Visible));

      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_FILTRO_PROPIO, CRutinas.BoolToStr(UsaFiltroPropio));

      CRutinas.AgregarAtributo(Elemento, CRutinas.CTE_PARAMETROS, Parametros);

      if (Filtro != null)
			{
        Filtro.AgregarAXML(Elemento);
			}

      if (FiltrosBlock!=null && FiltrosBlock.Count > 0)
			{
        XElement Filtros = new XElement(CRutinas.CTE_FILTROS);
        foreach (CPasoCondicionesBlock Paso in FiltrosBlock)
				{
          Paso.AgregarAXML(Filtros);
				}
        Elemento.Add(Filtros);
			}

      return Elemento;
    }

  }

  public class CPasoCondicionesBlock
  {
    public bool CumplirTodas { get; set; }
    public List<CGrupoCondicionesBlock> Grupos { get; set; }

    public CPasoCondicionesBlock()
    {
      CumplirTodas = false;
      Grupos = new List<CGrupoCondicionesBlock>();
    }

    public bool CorrespondeAColumna(string Columna)
    {
      if (Grupos.Count != 1)
      {
        return false;
      }

      foreach (CGrupoCondicionesBlock Gr in Grupos)
      {
        foreach (CCondicionBlock Cnd in Gr.Condiciones)
        {
          if (Cnd.Columna != Columna)
          {
            return false;
          }
        }
      }
      return true;
    }

    public void CargarDesdeXML(XElement Elemento)
    {

      CumplirTodas = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_CUMPLIR_TODAS);

      foreach (XElement ElemGrupo in Elemento.Elements(CRutinas.CTE_GRUPO))
      {
        CGrupoCondicionesBlock GrupoLocal = new CGrupoCondicionesBlock();
        GrupoLocal.CargarDesdeXML(ElemGrupo);
        Grupos.Add(GrupoLocal);
      }

    }

    public void AgregarAXML(XElement Elemento)
		{
      XElement Contenido = new XElement(CRutinas.CTE_FILTRO);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CUMPLIR_TODAS, CRutinas.BoolToStr(CumplirTodas));

      foreach (CGrupoCondicionesBlock Grupo in Grupos)
			{
        Grupo.AgregarAXML(Contenido);
			}

      Elemento.Add(Contenido);

    }

    public static List<CCondicionFiltradorCN> ConvertirACCondicionesFiltradorCN(List<CPasoCondicionesBlock> Filtros)
    {
      List<CCondicionFiltradorCN> Respuesta = new List<CCondicionFiltradorCN>();
      foreach (CPasoCondicionesBlock Paso in Filtros)
      {
        foreach (CGrupoCondicionesBlock Grupo in Paso.Grupos)
        {
          if (Grupo.Condiciones.Count > 0)
          {
            CCondicionFiltradorCN Cnd = new CCondicionFiltradorCN();
            Cnd.Coincidencias = 1;
            Cnd.NombreColumna = Grupo.Condiciones[0].Columna;
            Cnd.NombreColumnaAND = "";
            foreach (CCondicionBlock CndBlock in Grupo.Condiciones)
            {
              if (CndBlock.Modo == ModoFiltrar.Igual)
              {
                Cnd.ValoresImpuestos.Add(CndBlock.Valor);
              }
              else
              {
                Cnd.RangoMinimo = CRutinas.StrVFloat(CndBlock.Valor);
                Cnd.RangoMaximo = CRutinas.StrVFloat(CndBlock.ValorMaximo);
              }
            }
            Respuesta.Add(Cnd);
          }
        }
      }
      return Respuesta;
    }

  }

  public class CCondicionBlock
  {
    public string Columna { get; set; }
    public ModoFiltrar Modo { get; set; }
    public string Valor { get; set; } // se usa tambien para minimo.
    public string ValorMaximo { get; set; }

    public CCondicionBlock()
    {
      Modo = ModoFiltrar.NoDefinido;
      Valor = "";
      ValorMaximo = "";
    }

    public void AgregarAXML(XElement Elemento)
		{
      XElement Contenido = new XElement(CRutinas.CTE_CONDICION);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_COLUMNA, Columna);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_MODO, ((Int32)Modo).ToString());
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_VALOR, Valor);
      if (ValorMaximo!=null && ValorMaximo.Length > 0)
			{
        CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_MAXIMO, ValorMaximo);
      }
      Elemento.Add(Contenido);

    }

    public void CargarDesdeXML(XElement Elemento)
    {
      Columna = Elemento.Attribute(CRutinas.CTE_COLUMNA).Value;
      Modo = (ModoFiltrar)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_MODO);
      Valor = Elemento.Attribute(CRutinas.CTE_VALOR).Value;
      XAttribute Atr = Elemento.Attribute(CRutinas.CTE_MAXIMO);
      if (Atr != null)
      {
        ValorMaximo = Atr.Value;
      }
    }

  }

  public class CGrupoCondicionesBlock
  {
    public bool CumplirTodas { get; set; }
    public bool Incluye { get; set; }
    public Int32 CantidadMinima { get; set; }
    public List<CCondicionBlock> Condiciones { get; set; }

    public CGrupoCondicionesBlock()
    {
      CumplirTodas = false;
      Incluye = true;
      CantidadMinima = -1;
      Condiciones = new List<CCondicionBlock>();
    }

    public void AgregarAXML(XElement Elemento)
		{
      XElement Contenido = new XElement(CRutinas.CTE_GRUPO);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CUMPLIR_TODAS, CRutinas.BoolToStr(CumplirTodas));
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_INCLUYE, CRutinas.BoolToStr(Incluye));
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CANTIDAD, CantidadMinima.ToString());

      if (Condiciones!=null && Condiciones.Count > 0)
			{
        XElement ElCnd = new XElement(CRutinas.CTE_CONDICIONES);
        foreach (CCondicionBlock Cnd in Condiciones)
				{
          Cnd.AgregarAXML(ElCnd);
				}
        Contenido.Add(ElCnd);
			}

      Elemento.Add(Contenido);

    }

    public void CargarDesdeXML(XElement Elemento)
    {
      CumplirTodas = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_CUMPLIR_TODAS);
      Incluye = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_INCLUYE);
      CantidadMinima = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CANTIDAD);

      XElement ElCnds = Elemento.Element(CRutinas.CTE_CONDICIONES);
      if (ElCnds != null)
      {
        foreach (XElement ElCnd in ElCnds.Elements(CRutinas.CTE_CONDICION))
        {
          CCondicionBlock CndLocal = new CCondicionBlock();
          CndLocal.CargarDesdeXML(ElCnd);
          Condiciones.Add(CndLocal);
        }
      }

    }

  }

  public class Point
  {
    public double X { get; set; }
    public double Y { get; set; }

    public Int32 AbscisaI
		{
      get { return (Int32)X; }
		}

    public Int32 OrdenadaI
    {
      get { return (Int32)Y; }
    }

    public Point(double Absc, double Ord)
    {
      X = Absc;
      Y = Ord;
    }
  }

}
