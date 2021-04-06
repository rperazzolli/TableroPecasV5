using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Clases
{
  public class CCurvaTendencia
  {
    public bool IndicadorBase { get; set; }
    public CDatoIndicador Indicador { get; set; }
    public bool EscalaDerecha { get; set; }
    public List<CInformacionAlarmaCN> Alarmas { get; set; }
  }
}
