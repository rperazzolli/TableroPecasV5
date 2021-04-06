using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Datos
{
  public class CPasoCondicionesBlock
  {
    public bool CumplirTodas { get; set; }
    public List<CGrupoCondicionesBlock> Grupos { get; set; }

    public CPasoCondicionesBlock()
    {
      CumplirTodas = false;
      Grupos = new List<CGrupoCondicionesBlock>();
    }

    public bool CorrespondeAColumna(string Columna)
    {
      if (Grupos.Count != 1)
      {
        return false;
      }

      foreach (CGrupoCondicionesBlock Gr in Grupos)
      {
        foreach (CCondicionBlock Cnd in Gr.Condiciones)
        {
          if (Cnd.Columna != Columna)
          {
            return false;
          }
        }
      }
      return true;
    }

    public void AgregarAXML(XElement Elemento)
    {
      XElement Contenido = new XElement(CRutinas.CTE_FILTRO);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CUMPLIR_TODAS, (CumplirTodas ? "S" : "N"));

      foreach (CGrupoCondicionesBlock Grupo in Grupos)
      {
        Grupo.AgregarAXML(Contenido);
      }

      Elemento.Add(Contenido);

    }

    public void CargarDesdeXML(XElement Elemento)
    {

      CumplirTodas = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_CUMPLIR_TODAS);

      foreach (XElement ElemGrupo in Elemento.Elements(CRutinas.CTE_GRUPO))
      {
        CGrupoCondicionesBlock GrupoLocal = new CGrupoCondicionesBlock();
        GrupoLocal.CargarDesdeXML(ElemGrupo);
        Grupos.Add(GrupoLocal);
      }

    }

  }

}
