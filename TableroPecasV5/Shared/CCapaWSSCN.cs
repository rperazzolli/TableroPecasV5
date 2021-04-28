using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public class CPosicionWFSCN
  {

    //[JsonProperty("X")]
    public double X{ get; set; }

    //[JsonProperty("Y")]
    public double Y{ get; set; }

  }

  public class CValorDimensionCN
  {

    //[JsonProperty("Dimension")]
    public int Dimension { get; set; }

    //[JsonProperty("Valor")]
    public int Valor { get; set; }

  }

  public class CAreaWFSCN
  {

    //[JsonProperty("Area")]
    public double Area{ get; set; }

    //[JsonProperty("Centro")]
    public CPosicionWFSCN Centro{ get; set; }

    //[JsonProperty("Codigo")]
    public string Codigo{ get; set; }

    //[JsonProperty("Contorno")]
    public List<CPosicionWFSCN> Contorno{ get; set; }

    //[JsonProperty("Dimensiones")]
    public List<CValorDimensionCN> Dimensiones{ get; set; }

    //[JsonProperty("Nombre")]
    public string Nombre{ get; set; }

    public CAreaWFSCN()
		{
      Contorno = new List<CPosicionWFSCN>();
      Dimensiones = new List<CValorDimensionCN>();
		}

   }

  public class CLineaWFSCN
  {

    //[JsonProperty("Centro")]
    public CPosicionWFSCN Centro { get; set; }

    //[JsonProperty("Codigo")]
    public string Codigo { get; set; }

    //[JsonProperty("Contorno")]
    public List<CPosicionWFSCN> Contorno{ get; set; }

    //[JsonProperty("Nombre")]
    public string Nombre{ get; set; }

    public CLineaWFSCN()
		{
      Contorno = new List<CPosicionWFSCN>();
		}

  }

  public class CPuntoWFSCN
  {

    //[JsonProperty("Codigo")]
    public string Codigo{ get; set; }

    //[JsonProperty("Nombre")]
    public string Nombre{ get; set; }

    //[JsonProperty("Punto")]
    public CPosicionWFSCN Punto{ get; set; }

  }

  public class CCapaWFSCN
  {

    //[JsonProperty("Areas")]
    public List<CAreaWFSCN> Areas{ get; set; }

    //[JsonProperty("CamposInformacion")]
    public string CamposInformacion { get; set; } = "";

    //[JsonProperty("Capa")]
    public string Capa { get; set; } = "";

    //[JsonProperty("Codigo")]
    public int Codigo { get; set; } = -1;

    //[JsonProperty("CodigoProveedor")]
    public int CodigoProveedor { get; set; } = -1;

    //[JsonProperty("Descripcion")]
    public string Descripcion { get; set; } = "";

    //[JsonProperty("Detalle")]
    public string Detalle { get; set; } = "";

    //[JsonProperty("DireccionURL")]
    public string DireccionURL { get; set; } = "";

    //[JsonProperty("Elemento")]
    public ElementoWFS Elemento { get; set; } = ElementoWFS.NoDefinido;

    //[JsonProperty("FechaRefresco")]
    public System.DateTime FechaRefresco { get; set; } = DateTime.Now;

    //[JsonProperty("GuardaCompactada")]
    public bool GuardaCompactada { get; set; } = false;

    //[JsonProperty("Lineas")]
    public List<CLineaWFSCN> Lineas{ get; set; }

    //[JsonProperty("NombreCampoCodigo")]
    public string NombreCampoCodigo { get; set; } = "";

    //[JsonProperty("NombreCampoDatos")]
    public string NombreCampoDatos { get; set; } = "";

    //[JsonProperty("NombreElemento")]
    public string NombreElemento { get; set; } = "";

    //[JsonProperty("Puntos")]
    public List<CPuntoWFSCN> Puntos{ get; set; }

    //[JsonProperty("PuntosMaximosContorno")]
    public int PuntosMaximosContorno { get; set; } = 199;

    //[JsonProperty("Version")]
    public string Version { get; set; } = "";

    public CCapaWFSCN()
		{
      Lineas = new List<CLineaWFSCN>();
      Puntos = new List<CPuntoWFSCN>();
      Areas = new List<CAreaWFSCN>();
		}

  }

  public class CCapaWSSCN : CContenedorBaseSinDescripcion
  {

    //[JsonProperty("Agrupacion")]
    public ModoAgruparDependiente Agrupacion { get; set; } = ModoAgruparDependiente.NoDefinido;

    //[JsonProperty("CapaWFS")]
    public int CapaWFS { get; set; } = -1;

    //[JsonProperty("Clase")]
    public ClaseElemento Clase { get; set; } = ClaseElemento.NoDefinida;

    //[JsonProperty(CodigoElemento")]
    public int CodigoElemento { get; set; } = -1;

    //[JsonProperty("ColorCompuestoA")]
    public byte ColorCompuestoA { get; set; } = 255;

    //[JsonProperty("ColorCompuestoB")]
    public byte ColorCompuestoB { get; set; } = 255;

    //[JsonProperty("ColorCompuestoG")]
    public byte ColorCompuestoG { get; set; } = 255;

    //[JsonProperty("ColorCompuestoR")]
    public byte ColorCompuestoR { get; set; } = 255;

    //[JsonProperty("ColumnaGeoreferencia")]
    public string ColumnaGeoreferencia { get; set; } = "";

    //[JsonProperty(ColumnaLatitud")]
    public string ColumnaLatitud { get; set; } = "";

    //[JsonProperty("ColumnaLongitud")]
    public string ColumnaLongitud { get; set; } = "";

    //[JsonProperty("ColumnaValor")]
    public string ColumnaValor { get; set; } = "";

    //[JsonProperty("Formula")]
    public string Formula { get; set; } = "";

    //[JsonProperty("Intervalos")]
    public ClaseIntervalo Intervalos { get; set; } = ClaseIntervalo.NoDefinido;

    //[JsonProperty("Minimo")]
    public double Minimo { get; set; } = 0;

    //[JsonProperty("Modo")]
    public ModoGeoreferenciar Modo { get; set; } = ModoGeoreferenciar.NoDefinido;

    //[JsonProperty("Nombre")]
    public string Nombre { get; set; } = "";

    //[JsonProperty("Rango")]
    public double Rango { get; set; } = 1d / 1440d;

    //[JsonProperty("Referencias")]
    public List<double> Referencias { get; set; } = new List<double>();

    //[JsonProperty("Satisfactorio")]
    public double Satisfactorio { get; set; } = 0;

    //[JsonProperty("Segmentos")]
    public int Segmentos { get; set; } = 0;

    //[JsonProperty("Sobresaliente")]
    public double Sobresaliente { get; set; } = 0;

    //[JsonProperty("Vinculo")]
    public int Vinculo { get; set; } = -1;

    public CCapaWSSCN()
		{
      //
		}

  }
}
