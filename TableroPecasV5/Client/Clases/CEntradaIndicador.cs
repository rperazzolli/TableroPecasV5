using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Clases
{
  public class CEntradaIndicador
  {
    public Int32 CodigoIndicador;
    public Int32 CodigoElementoDimension;

    public CEntradaIndicador(Int32 CodInd, Int32 CodDim)
    {
      CodigoIndicador = CodInd;
      CodigoElementoDimension = CodDim;
    }

  }

}
