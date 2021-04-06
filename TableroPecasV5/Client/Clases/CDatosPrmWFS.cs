using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Clases
{
  public class CDatosPrmWFS
  {
    public string Parametro { get; set; }
    public List<string> ParesValores { get; set; }

    public CDatosPrmWFS(string Prm)
    {
      Parametro = Prm;
    }

  }

}
