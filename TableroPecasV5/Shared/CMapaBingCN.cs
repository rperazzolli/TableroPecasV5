using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public class CCapaBingCN : CContenedorBaseSinDescripcion
  {

    //[JsonProperty("Azul")]
    public byte Azul { get; set; }

    //[JsonProperty("Clase")]
    public ClaseCapa Clase { get; set; }

    //[JsonProperty("CodigoCapa")]
    public int CodigoCapa { get; set; }

    //[JsonProperty("Opacidad")]
    public double Opacidad { get; set; }

    //[JsonProperty("Orden")]
    public int Orden { get; set; }

    //[JsonProperty("Rojo")]
    public byte Rojo { get; set; }

    //[JsonProperty("Verde")]
    public byte Verde { get; set; }

  }

  public class CBaseBingCN : CContenedorBaseSinDescripcion
  {

    //[JsonProperty("AbscisaCentro")]
    public double AbscisaCentro { get; set; }

    //[JsonProperty("Descripcion")]
    public string Descripcion { get; set; }

    //[JsonProperty("OrdenadaCentro")]
    public double OrdenadaCentro { get; set; }

  }

  public class CPreguntaPreguntaWISCN : CContenedorBaseSinDescripcion
  {

    //[JsonProperty("Clase")]
    public ClaseDetalle Clase { get; set; }

    //[JsonProperty("CodigoDimension")]
    public int CodigoDimension { get; set; }

    //[JsonProperty("CodigoElemento")]
    public int CodigoElemento { get; set; }

    //[JsonProperty("CodigoElementoDimension")]
    public int CodigoElementoDimension { get; set; }

    //[JsonProperty("CodigoPregunta")]
    public int CodigoPregunta { get; set; }

    public string Color { get; set; } = "";

  }

  public class CElementoPreguntasWISCN : CContenedorBaseSinDescripcion
  {

    //[JsonProperty("Abscisa")]
    public double Abscisa{get;set;}

    //[JsonProperty("ClaseWIS")]
    public ClaseCapa ClaseWIS{get;set;}

    //[JsonProperty("CodigoArea")]
    public string CodigoArea{get;set;}

    //[JsonProperty("CodigoWIS")]
    public int CodigoWIS{get;set;}

    //[JsonProperty("Contenidos")]
    public List<CPreguntaPreguntaWISCN> Contenidos{get;set;}

    //[JsonProperty("Dimension")]
    public int Dimension{get;set;}

    //[JsonProperty("ElementoDimension")]
    public int ElementoDimension{get;set;}

    //[JsonProperty("Nombre")]
    public string Nombre{get;set;}

    //[JsonProperty("Ordenada")]
    public double Ordenada{get;set;}

    public string Color { get; set; } = "";

  }

  public class CMapaBingCN : CBaseBingCN
  {

    //[JsonProperty("Autor")]
    public int Autor {get;set;}

    //[JsonProperty("Capas")]
    public List<CCapaBingCN> Capas {get;set;}

    //[JsonProperty("NivelZoom")]
    public double NivelZoom {get;set;}

    //[JsonProperty("Preguntas")]
    public List<CElementoPreguntasWISCN> Preguntas {get;set;}

    //[JsonProperty("Publicador")]
    public int Publicador {get;set;}

    public CMapaBingCN()
		{
      Capas = new List<CCapaBingCN>();
      Preguntas = new List<CElementoPreguntasWISCN>();
		}

  }

}
