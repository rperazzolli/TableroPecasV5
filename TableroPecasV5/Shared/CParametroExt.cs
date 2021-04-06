using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
  public class CParametroExt
  {
    //[JsonProperty("CodigoSubconsulta")]
    public int CodigoSubconsulta { get; set; }

    //[JsonProperty("Nombre")]
    public string Nombre { get; set; }

    //[JsonProperty("TieneQuery")]
    public bool TieneQuery { get; set; }

    //[JsonProperty("Tipo")]
    public string Tipo { get; set; }

    //[JsonProperty("ValorDateTime")]
    public System.DateTime ValorDateTime { get; set; }

    //[JsonProperty("ValorFloat")]
    public double ValorFloat { get; set; }

    //[JsonProperty("ValorInteger")]
    public int ValorInteger { get; set; }

    //[JsonProperty("ValorString")]
    public string ValorString { get; set; }


  }
}
