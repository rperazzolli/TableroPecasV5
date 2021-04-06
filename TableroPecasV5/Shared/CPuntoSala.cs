using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
  public class CPuntoSala
  {
    //[JsonProperty("Sala")]
    public CSalaCN Sala { get; set; }

    //[JsonProperty("Solapas")]
    public List<CPuntoSolapa> Solapas { get; set; }

    public CPuntoSala()
		{
      Sala = new CSalaCN();
      Solapas = new List<CPuntoSolapa>();
		}

  }

  public class CSalaCN
  {

    //[JsonProperty("MsgErr")]
    public int Codigo { get; set; }

    //[JsonProperty("Comite")]
    public int Comite { get; set; }

    //[JsonProperty("Dimension")]
    public int Dimension { get; set; }

    //[JsonProperty("EdicionRestringida")]
    public bool EdicionRestringida { get; set; }

    //[JsonProperty("ElementoDimension")]
    public int ElementoDimension { get; set; }

    //[JsonProperty("Nombre")]
    public string Nombre { get; set; }

    //[JsonProperty("Registrador")]
    public int Registrador { get; set; }

  }

  public class CPuntoSolapa
  {

    //[JsonProperty("preguntas")]
    public List<CPuntoPregunta> Preguntas { get; set; }

    //[JsonProperty("Solapa")]
    public CSolapaCN Solapa { get; set; }

    public CPuntoSolapa()
		{
      Preguntas = new List<CPuntoPregunta>();
      Solapa = new CSolapaCN();
		}

  }

  public class CSolapaCN
  {

    //[JsonProperty("Azul")]
    public int Azul { get; set; }

    //[JsonProperty("Block")]
    public string Block { get; set; } = "";

    //[JsonProperty("Codigo")]
    public int Codigo { get; set; }

    //[JsonProperty("Dimension")]
    public int Dimension { get; set; }

    //[JsonProperty("ElementoDimension")]
    public int ElementoDimension { get; set; }

    //[JsonProperty("Nombre")]
    public string Nombre { get; set; }

    //[JsonProperty("Orden")]
    public int Orden { get; set; }

    //[JsonProperty("Rojo")]
    public int Rojo { get; set; }

    //[JsonProperty("Sala")]
    public int Sala { get; set; }

    //[JsonProperty("Verde")]
    public int Verde { get; set; }

  }

}
