using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Datos
{
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
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CUMPLIR_TODAS, CumplirTodas ? "S" : "N");
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_INCLUYE, Incluye ? "S" : "N");
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CANTIDAD, CantidadMinima.ToString());

      if (Condiciones != null && Condiciones.Count > 0)
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

}
