using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public class CContenedorProcesoCN : CContenedorBase
  {

    public int CodigoProceso{ get; set; }

  }

  public partial class CFlechaCN : CContenedorProcesoCN
  {

    public ClaseElemento ClaseDesde{ get; set; }

    public ClaseElemento ClaseHasta{ get; set; }

    public string NombreLocal{ get; set; }

    public int ParametroEjecutor{ get; set; }

    public int ParametroFecha{ get; set; }

    public int RolTQSD{ get; set; }

    public int TareaDesde{ get; set; }

    public int TareaHasta{ get; set; }

    public bool UneConFlechasPrevias{ get; set; }

  }

  public class CRolCN
  {

    public int RolSuperior{ get; set; }

  }

  public class CParametroCN : CContenedorProcesoCN
  {

    public ClaseVariable Clase{ get; set; }

    public int ConsultaParaLista{ get; set; }

    public int OrdenCampoContenido{ get; set; }

    public int OrdenCampoDescripcion{ get; set; }

    public string ParametrosConsulta{ get; set; }

    public string ValorPorDefecto{ get; set; }

  }

  public class CTareaCN : CContenedorProcesoCN
  {

    public ClaseElemento ClaseDeTarea{ get; set; }

    public int CodigoRol{ get; set; }

    public string NombreLocal{ get; set; }

    public int ParametroEjecutor{ get; set; }

    public int ParametroFecha{ get; set; }

    public int RolTQSD{ get; set; }

  }

  public class CParametroVariableCN : CContenedorProcesoCN
  {

    public ClaseElemento ClaseSuperior{ get; set; }

    public int CodigoParametro{ get; set; }

    public int CodigoSuperior{ get; set; }

    public int Decimales{ get; set; }

    public int Enteros{ get; set; }

    public bool EsParteDeLaClave{ get; set; }

    public int VariableConsulta{ get; set; }

  }

  public class CRomboGraficoCN
  {

    //[JsonProperty("Abscisa")]
    public double Abscisa{ get; set; }

    //[JsonProperty("Alto")]
    public double Alto{ get; set; }

    //[JsonProperty("Ancho")]
    public double Ancho{ get; set; }

    //[JsonProperty("Clase")]
    public ClaseRombo Clase{ get; set; }

    //[JsonProperty("Codigo")]
    public string Codigo{ get; set; }

    //[JsonProperty("Ordenada")]
    public double Ordenada{ get; set; }

    //[JsonProperty("Prefijo")]
    public string Prefijo{ get; set; }

  }

  public class CDetallePreguntaCN : CContenedorBaseSinDescripcion
  {

    //[JsonProperty("ClaseDeDetalle")]
    public ClaseDetalle ClaseDeDetalle{ get; set; }

    //[JsonProperty("ClaseEntidad")]
    public int ClaseEntidad{ get; set; }

    //[JsonProperty("CodigoEntidad")]
    public int CodigoEntidad{ get; set; }

    //[JsonProperty("CodigoPregunta")]
    public int CodigoPregunta{ get; set; }

    public ColorBandera Color { get; set; } = ColorBandera.NoCorresponde;

  }

  public class CTareaGraficaCN
  {

    //[JsonProperty("Abscisa")]
    public double Abscisa{ get; set; }

    //[JsonProperty("Alto")]
    public double Alto{ get; set; }

    //[JsonProperty("Ancho")]
    public double Ancho{ get; set; }

    //[JsonProperty("Clase")]
    public ClaseElemento Clase{ get; set; }

    //[JsonProperty("Codigo")]
    public string Codigo{ get; set; }

    //[JsonProperty("Descripcion")]
    public string Descripcion{ get; set; }

    //[JsonProperty("EsTransaccion")]
    public bool EsTransaccion{ get; set; }

    //[JsonProperty("Ordenada")]
    public double Ordenada{ get; set; }

    //[JsonProperty("TareaOrigen")]
    public string TareaOrigen{ get; set; }

  }

  public class CPuntoGraficoCN
  {

    //[JsonProperty("Abscisa")]
    public double Abscisa{ get; set; }

    //[JsonProperty("Ordenada")]
    public double Ordenada{ get; set; }

  }

  public class CFlechaGraficaCN
  {

    //[JsonProperty("Clase")]
    public ClaseFlecha Clase{ get; set; }

    //[JsonProperty("ClaseDesde")]
    public ClaseElemento ClaseDesde{ get; set; }

    //[JsonProperty("ClaseHasta")]
    public ClaseElemento ClaseHasta{ get; set; }

    //[JsonProperty("Codigo")]
    public string Codigo{ get; set; }

    //[JsonProperty("CodigoDesde")]
    public string CodigoDesde{ get; set; }

    //[JsonProperty("CodigoHasta")]
    public string CodigoHasta{ get; set; }

    //[JsonProperty("Descripcion")]
    public string Descripcion{ get; set; }

    //[JsonProperty("PrefijoDesde")]
    public string PrefijoDesde{ get; set; }

    //[JsonProperty("PrefijoHasta")]
    public string PrefijoHasta{ get; set; }

    //[JsonProperty("Puntos")]
    public List<CPuntoGraficoCN> Puntos{ get; set; }

    public CFlechaGraficaCN()
		{
      Puntos = new List<CPuntoGraficoCN>();
		}

  }

  public class CLineaGraficaCN
  {

    //[JsonProperty("Abscisa")]
    public double Abscisa{ get; set; }

    //[JsonProperty("Alto")]
    public double Alto{ get; set; }

    //[JsonProperty("Ancho")]
    public double Ancho{ get; set; }

    //[JsonProperty("Codigo")]
    public string Codigo{ get; set; }

    //[JsonProperty("Descripcion")]
    public string Descripcion{ get; set; }

    //[JsonProperty("Ordenada")]
    public double Ordenada{ get; set; }

  }

  public partial class CPoolGraficoCN
  {

    //[JsonProperty("Codigo")]
    public string Codigo{ get; set; }

    //[JsonProperty("Descripcion")]
    public string Descripcion{ get; set; }

    //[JsonProperty("Lineas")]
    public List<CLineaGraficaCN> Lineas{ get; set; }

    public CPoolGraficoCN()
		{
      Lineas = new List<CLineaGraficaCN>();
		}

  }

  public class CProcesoGraficoCN
  {

    //[JsonProperty("Codigo")]
    public int Codigo{ get; set; }

    //[JsonProperty("Flechas")]
    public List<CFlechaGraficaCN> Flechas{ get; set; }

    //[JsonProperty("Pools")]
    public List<CPoolGraficoCN> Pools{ get; set; }

    //[JsonProperty("Rombos")]
    public List<CRomboGraficoCN> Rombos{ get; set; }

    //[JsonProperty("Tareas")]
    public List<CTareaGraficaCN> Tareas{ get; set; }

    public CProcesoGraficoCN()
		{
      Flechas = new List<CFlechaGraficaCN>();
      Pools = new List<CPoolGraficoCN>();
      Rombos = new List<CRomboGraficoCN>();
      Tareas = new List<CTareaGraficaCN>();
		}

  }

  public class CElementoPreguntasCN : CBaseElementoRectCN
  {

    //[JsonProperty("ClaseEntidad")]
    public int ClaseEntidad { get; set; }

    //[JsonProperty("CodigoEntidad")]
    public int CodigoEntidad { get; set; }

    //[JsonProperty("PreguntasAsociadas")]
    public List<CDetallePreguntaCN> PreguntasAsociadas { get; set; }

    //[JsonProperty("Vinculo")]
    public string Vinculo { get; set; }

    public CElementoPreguntasCN()
		{
      PreguntasAsociadas = new List<CDetallePreguntaCN>();
		}

  }

	public class CImagenBinariaCN
	{
    //[JsonProperty("Codigo")]
    public Int32 Codigo { get; set; }

    //[JsonProperty("Mimico")]
    public Int32 Mimico { get; set; }

    //[JsonProperty("DatosSucios")]
    public bool DatosSucios { get; set; }

		//[JsonProperty("Imagen")]
		public byte[] Imagen { get; set; }

	}

	public class CMimicoCN
  {

    //[JsonProperty("GruposDePreguntasDelMimico")]
    public List<CElementoPreguntasCN> GruposDePreguntasDelMimico { get; set; }

    //[JsonProperty("MimicoPropio")]
    public CElementoMimicoCN MimicoPropio { get; set; }

    //[JsonProperty("VinculosComentarios")]
    public List<string> VinculosComentarios { get; set; }

    //[JsonProperty("ImgBin")]
    public List<CImagenBinariaCN> ImagenesBinarias { get; set; } // usadas en la definición.

    public CMimicoCN()
    {
      MimicoPropio = new CElementoMimicoCN();
      GruposDePreguntasDelMimico = new List<CElementoPreguntasCN>();
      VinculosComentarios = new List<string>();
      ImagenesBinarias = new List<CImagenBinariaCN>();
    }

  }

}
