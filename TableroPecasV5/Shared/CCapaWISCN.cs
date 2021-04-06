using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public class CCapaWISCN
  {

    public int Codigo{ get; set; }

    public int CodigoWFS{ get; set; }

    public string Descripcion{ get; set; }

  }

  public class CCapaWISCompletaCN
  {

    public CCapaWISCN Capa{ get; set; }

    public List<CElementoPreguntasWISCN> Vinculos{ get; set; }

    public CCapaWISCompletaCN()
		{
      Vinculos = new List<CElementoPreguntasWISCN>();
		}

  }

}
