using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
  public class CGraficoCN
  {
    //[JsonProperty("Absc")]
    public int Abscisas { get; set; }

    //[JsonProperty("AgAbsc")]
    public ModoAgruparIndependiente AgrupacionAbscisas { get; set; }

    //[JsonProperty("AgOrd")]
    public ModoAgruparDependiente AgrupacionOrdenadas { get; set; }

    //[JsonProperty("Agrupar")]
    public bool Agrupar { get; set; }

    //[JsonProperty("Alto")]
    public int Alto { get; set; }

    //[JsonProperty("Ancho")]
    public int Ancho { get; set; }

    //[JsonProperty("CampoAbsc")]
    public string CampoAbscisas { get; set; }

    //[JsonProperty("CampoOrd")]
    public string CampoOrdenadas { get; set; }

    //[JsonProperty("CampoSexo")]
    public string CampoSexo { get; set; }

    //[JsonProperty("CapaWFS")]
    public int CapaWFS { get; set; }

    //[JsonProperty("CapaWFSAgr")]
    public int CapaWFSAgrupadora { get; set; }

    //[JsonProperty("ClaseGr")]
    public ClaseGrafico ClaseDeGrafico { get; set; }

    //[JsonProperty("CodigoAgr")]
    public int CodigoAgrupador { get; set; }

    //[JsonProperty("CodigoElem")]
    public int CodigoElemento { get; set; }

    //[JsonProperty("CondicionesF")]
    public List<CCondicionFiltroCN> CondicionesFiltro { get; set; }

    //[JsonProperty("Dimension")]
    public int Dimension { get; set; }

    //[JsonProperty("MapaColor")]
    public bool MapaColor { get; set; }

    //[JsonProperty("Minimo")]
    public double Minimo { get; set; }

    //[JsonProperty("Ordenadas")]
    public int Ordenadas { get; set; }

    //[JsonProperty("PasoEdad")]
    public int PasoEdad { get; set; }

    //[JsonProperty("Rango")]
    public double Rango { get; set; }

    //[JsonProperty("Satisf")]
    public double Satisfactorio { get; set; }

    //[JsonProperty("Sobresal")]
    public double Sobresaliente { get; set; }

    //[JsonProperty("ValSexo1")]
    public string ValorSexo1 { get; set; }

    //[JsonProperty("ValSexo2")]
    public string ValorSexo2 { get; set; }


  }
}
