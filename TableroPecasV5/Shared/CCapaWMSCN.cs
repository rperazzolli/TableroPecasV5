using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public class CCapaWMSCN
  {

    public string Capa { get; set; }

    public int Codigo { get; set; }

    public int CodigoProveedor { get; set; }

    public string Descripcion { get; set; }

    public string EPGS { get; set; }

    public double LatMaxima { get; set; }

    public double LatMinima { get; set; }

    public double LongMaxima { get; set; }

    public double LongMinima { get; set; }

    public bool Query { get; set; }

    public string URLProveedor { get; set; }

    public string VersionProveedor { get; set; }

  }

}
