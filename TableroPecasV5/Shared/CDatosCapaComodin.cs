using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace TableroPecasV5.Shared
{
  public class CDatosCapaComodin
  {
    //[JsonProperty("CodigoCapa")]
    public Int32 CodigoCapa { get; set; }

    //[JsonProperty("Clase")]
    public ClaseCapa Clase { get; set; }

    //[JsonProperty("CapaWFS")]
    public CCapaWFSCN CapaWFS { get; set; }

    //[JsonProperty("CapaWIS")]
    public CCapaWISCN CapaWIS { get; set; }

    //[JsonProperty("CapaWMS")]
    public CCapaWMSCN CapaWMS { get; set; }

    //[JsonProperty("Pares")]
    public List<CParValores> Pares;

    //[JsonProperty("Preguntas")]
    public List<CElementoPreguntasWISCN> Preguntas { get; set; } = null;

    //[JsonProperty("ColorWFS")]
    public Color ColorWFS { get; set; }

    //[JsonProperty("Pushpins")]
    public List<CPunto> Pushpins { get; set; } = null;

    //[JsonProperty("Opacidad")]
    public double Opacidad { get; set; } = 1;

    public CDatosCapaComodin()
    {
      CapaWFS = null;
      CapaWIS = null;
      CapaWMS = null;
      Clase = ClaseCapa.NoDefinida;
      Pares = new List<CParValores>();
    }

    public CDatosCapaComodin(CCapaWFSCN Capa)
    {
      CapaWFS = Capa;
      ColorWFS = System.Drawing.Color.Blue;
      Clase = ClaseCapa.WFS;
      Pares = new List<CParValores>();
    }

    public CDatosCapaComodin(CCapaWISCompletaCN Capa)
    {
      CapaWIS = Capa.Capa;
      Preguntas = Capa.Vinculos;
      Clase = ClaseCapa.WIS;
      Pares = new List<CParValores>();
    }

    public CDatosCapaComodin(CCapaWMSCN Capa)
    {
      CapaWMS = Capa;
      Clase = ClaseCapa.WMS;
      Pares = new List<CParValores>();
    }

  }

}
