using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Datos
{
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
      if (ValorMaximo != null && ValorMaximo.Length > 0)
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
