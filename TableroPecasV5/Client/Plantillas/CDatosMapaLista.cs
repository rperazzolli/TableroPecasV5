using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Contenedores;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Plantillas
{
  public class CDatosMapaLista
  {
    public Int32 Codigo { get; set; }
    public Int32 CodigoPropio { get; set; }
    public CGrafV2DatosContenedorBlock.ClaseBlock ClaseElemento { get; set; }
    public string Nombre { get; set; }
    public ClaseElemento ClaseOrigen { get; set; }
    public Int32 CodigoIndicador { get; set; }
    public Int32 CodigoElementoDimension { get; set; }
    public string ColumnaDatos { get; set; }
    public string ColumnaLat { get; set; }
    public string ColumnaLng { get; set; }
    public string ColumnaSexo { get; set; }
    public bool UsaFiltroPropio { get; set; }
    public List<CPasoCondicionesBlock> FiltrosBlock { get; set; }
    public Point Posicion { get; set; }
    public double Ancho { get; set; }
    public double Alto { get; set; }
    public double AltoNeto { get { return Alto - 25; } }
    public List<CCapaWFSMapa> CapasWFS { get; set; } = new List<CCapaWFSMapa>();

    public CDatosMapaLista()
    {
      FncComun();
      ClaseElemento = CGrafV2DatosContenedorBlock.ClaseBlock.NoDefinida;
      CodigoPropio = -1;
      Nombre = "";
			ClaseOrigen = TableroPecasV5.Shared.ClaseElemento.NoDefinida;
      CodigoIndicador = -1;
      FiltrosBlock = new List<CPasoCondicionesBlock>();
    }

    private List<CCapaWFSMapa> ExtraerCapas(string Texto)
    {
      List<CCapaWFSMapa> Respuesta = new List<CCapaWFSMapa>();
      if (Texto.Length > 0)
      {
        string[] Datos = Texto.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string Dato in Datos)
        {
          Respuesta.Add(new CCapaWFSMapa(Dato));
        }
      }
      return Respuesta;
    }

    public string TextoClase()
		{
       switch (ClaseElemento)
			{
        case CGrafV2DatosContenedorBlock.ClaseBlock.MapaControl:
          return "Mapa Control";
        case CGrafV2DatosContenedorBlock.ClaseBlock.Pines:
          return "Pines";
        case CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor:
          return "Mapa calor";
        case CGrafV2DatosContenedorBlock.ClaseBlock.MapaGradientes:
          return "Mapa gradientes";
        default:
          return ClaseElemento.ToString();
			}
		}

    public CDatosMapaLista(CGraficoCompletoCN Grafico)
    {
      if (Grafico.Graficos.Count != 1)
      {
        return;
      }

      if (Grafico.Graficos[0].CampoSexo.StartsWith(CRutinas.CTE_ELEMENTO))
      {
        ClaseElemento = CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor;
      }
      else
      {
        ClaseElemento = CGrafV2DatosContenedorBlock.ClaseBlock.Pines;
      }

      if (Grafico.Vinculo2.IndexOf(CRutinas.CTE_SEPARADOR) >= 0)
      {
        Int32 Pos = Grafico.Vinculo2.IndexOf(CRutinas.CTE_SEPARADOR);
        string Resto = Grafico.Vinculo2.Substring(Pos + CRutinas.CTE_SEPARADOR.Length);
        CapasWFS = ExtraerCapas(Resto);
      }
      FncComun();
      CodigoPropio = Grafico.Codigo;
      Nombre = Grafico.Descripcion + " <" + TextoClase() + ">";
			ClaseOrigen = (Grafico.Indicador > 0 ? TableroPecasV5.Shared.ClaseElemento.Indicador : TableroPecasV5.Shared.ClaseElemento.SubConsulta);
      CodigoIndicador = (Grafico.Indicador > 0 ? Grafico.Indicador : Grafico.CodigoSC);
      if (Grafico.Posicionador == PosicionadorGIS.PinesXY || Grafico.Posicionador == PosicionadorGIS.TortasXY)
      {
        ColumnaLat = Grafico.Vinculo1;
        ColumnaLng = Grafico.Vinculo2;
      }
      else
      {
        string ValSexo = Grafico.Graficos[0].CampoSexo;
        Int32 Pos1 = ValSexo.IndexOf(CRutinas.CTE_SEPARADOR);
        if (Pos1 < 0)
        {
          return;
        }
        ValSexo = ValSexo.Substring(Pos1 + CRutinas.CTE_SEPARADOR.Length);
        Pos1 = ValSexo.IndexOf(CRutinas.CTE_SEPARADOR);
        if (Pos1 < 0)
        {
          return;
        }
        ColumnaLng = ValSexo.Substring(0, Pos1);
        ColumnaLat = ValSexo.Substring(Pos1 + CRutinas.CTE_SEPARADOR.Length);
      }
      ColumnaDatos = Grafico.Graficos[0].CampoOrdenadas;
      FiltrosBlock = new List<CPasoCondicionesBlock>();
      switch (ClaseOrigen)
      {
        case TableroPecasV5.Shared.ClaseElemento.Indicador:
					CDatoIndicador Indi = CContenedorDatos.IndicadorDesdeCodigo(CodigoIndicador);
          if (Indi != null)
          {
						Nombre += " <" + Indi.Descripcion + ">";
          }
          break;
        case TableroPecasV5.Shared.ClaseElemento.SubConsulta:
					CSubconsultaExt SC = CContenedorDatos.SubconsultaCodigo(CodigoIndicador);
          if (SC != null)
          {
						Nombre += " <" + SC.Nombre + ">";
          }
          break;
      }
    }

    public CDatosMapaLista(CCapaWSSCN Capa, List<CPasoCondicionesBlock> Filtros)
    {
      ClaseElemento = CGrafV2DatosContenedorBlock.ClaseBlock.MapaGradientes;
      FncComun();
      CodigoPropio = Capa.Codigo;
      Nombre = Capa.Nombre + " <" + TextoClase() + ">";
      ClaseOrigen = Capa.Clase;
      CodigoIndicador = Capa.CodigoElemento;
      FiltrosBlock = Filtros;
      switch (Capa.Clase)
      {
        case TableroPecasV5.Shared.ClaseElemento.Indicador:
					CDatoIndicador Indi = Contenedores.CContenedorDatos.IndicadorDesdeCodigo(Capa.CodigoElemento);
          if (Indi != null)
          {
						Nombre += " <" + Indi.Descripcion + ">";
          }
          break;
        case TableroPecasV5.Shared.ClaseElemento.SubConsulta:
					CSubconsultaExt SC = Contenedores.CContenedorDatos.SubconsultaCodigo(Capa.CodigoElemento);
          if (SC != null)
          {
						Nombre += " <" + SC.Nombre + ">";
          }
          break;
      }
    }

    private void FncComun()
    {
      CDatosGrafLista.gCodigo++;
      Codigo = CDatosGrafLista.gCodigo;
      CodigoElementoDimension = -1;
      Posicion = new Point(-1000, -1000);
      Ancho = -1;
      Alto = -1;
    }

    public CDatosMapaLista(CMapaBingCN Capa)
    {
      ClaseElemento = CGrafV2DatosContenedorBlock.ClaseBlock.MapaControl;
      FncComun();
      CodigoPropio = Capa.Codigo;
      Nombre = Capa.Descripcion + " <" + TextoClase() + ">";
			ClaseOrigen = TableroPecasV5.Shared.ClaseElemento.NoDefinida;
      CodigoIndicador = -1;
      FiltrosBlock = null;
    }

    //public CDatosMapaLista(Logicas.CCapaChinches Capa, Int32 Indicador, List<CPasoCondicionesBlock> Filtros)
    //{
    //  Clase = CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor;
    //  FncComun();
    //  CodigoPropio = -1;
    //  ColumnaLat = Capa.ColumnaLat;
    //  ColumnaLng = Capa.ColumnaLong;
    //  ColumnaDatos = Capa.ColumnaValor;
    //  ClaseIndicador = ClaseElemento.Indicador;
    //  CodigoIndicador = Indicador;
    //  Nombre = Capa.ColumnaValor + " <" + Capa.NombreCapa + ">";
    //  FiltrosBlock = Filtros;
    //}

    public void CargarDesdeXML(XElement Elemento)
    {

      ClaseElemento = (CGrafV2DatosContenedorBlock.ClaseBlock)CRutinas.ExtraerAtributoEntero(
          Elemento, CRutinas.CTE_CLASE_BLOCK);

      Nombre = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_NOMBRE);

      double AnchoLocal;
      double AltoLocal;
      Posicion = CContenedorBlocks.ObtenerPosicion(Elemento, out AnchoLocal, out AltoLocal);
      Ancho = AnchoLocal;
      Alto = AltoLocal;

      CodigoPropio = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CODIGO_2);
      ClaseOrigen = (ClaseElemento)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE_ELEMENTO);
      CodigoIndicador = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CODIGO);
      CodigoElementoDimension = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_ELEMENTO_DIMENSION);

      ColumnaDatos = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_COLUMNA_DATOS);
      ColumnaLat = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_COLUMNA_ABSC);
      ColumnaLng = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_COLUMNA_ORD);
      ColumnaSexo = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_COLUMNA_SEXO);
      if (ColumnaSexo == "ES_CAPA_CALOR")
      {
        ClaseElemento = CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor;
      }

      UsaFiltroPropio = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_FILTRO_PROPIO);

      XElement ElFiltro = Elemento.Element(CRutinas.CTE_FILTROS);
      if (ElFiltro != null)
      {
        foreach (XElement ElF2 in ElFiltro.Elements(CRutinas.CTE_FILTRO))
        {
          CPasoCondicionesBlock F = new CPasoCondicionesBlock();
          F.CargarDesdeXML(ElF2);
          FiltrosBlock.Add(F);
        }
      }

    }

    public CGrafV2DatosContenedorBlock CrearGrafV2()
    {
      return new CGrafV2DatosContenedorBlock()
      {
        CodigoPropio=CodigoPropio,
        Indicador = CodigoIndicador,
        Agrupacion = ModoAgruparDependiente.NoDefinido,
        AgrupIndep = ModoAgruparIndependiente.NoDefinido,
        Alto = Alto,
        Ancho = Ancho,
        Clase = ClaseGrafico.NoDefinido,
        ClaseElemento = ClaseElemento,
        ClaseOrigen = ClaseOrigen,
        CodigoElementoDimension = CodigoElementoDimension,
        ColumnaDatos=ColumnaDatos,
        ColumnaAbscisas = ColumnaLat,
        ColumnaOrdenadas = ColumnaLng,
        ColumnaSexo = ColumnaSexo,
        Filtro = new CPasoCondicionesBlock(),
        FiltrosBlock = (FiltrosBlock == null ? new List<CPasoCondicionesBlock>() : FiltrosBlock),
        Nombre = Nombre,
        Posicion = Posicion,
        SaltoHistograma = 0,
        UsaFiltroPropio = UsaFiltroPropio,
        Visible = true
      };
    }

  }

}
