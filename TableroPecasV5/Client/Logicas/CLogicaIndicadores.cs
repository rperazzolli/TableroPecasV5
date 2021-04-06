using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaIndicadores : ComponentBase
  {

    private string mszFiltro = "";
    public string Filtro
    {
      get { return mszFiltro; }
      set { mszFiltro = value; }
    }

    private List<CDatoIndicador> mListaIndicadores = null;
    public List<CDatoIndicador> ListaIndicadores
    {
      get
      {
        return (mListaIndicadores == null ? Contenedores.CContenedorDatos.ListaIndicadores : mListaIndicadores);
      }
      set
      {
        mListaIndicadores = value;
      }
    }

    public string EstiloComando(Int32 Posicion)
		{
      switch (Posicion)
			{
        case 1: return "width: 120px; background-color: #ccd5f0; color: #000; ";
        case 2: return "width: 200px;";
        default: return "width: 120px;";
      }
		}

    public void FncClickIndicador(Int32 Codigo)
    {
      if (Codigo < 0)
      {
        return;
      }

      // Navega a la pagina.

    }

    public void FiltrarIndicadores()
    {
      if (Filtro == null || Filtro.Length == 0)
      {
        mListaIndicadores = null;
      }
      else
      {
        mListaIndicadores = (from I in Contenedores.CContenedorDatos.ListaIndicadores
                             where I.Descripcion.IndexOf(Filtro, StringComparison.InvariantCultureIgnoreCase) >= 0
                             select I).ToList();
      }
      StateHasChanged();
    }
  }
}
