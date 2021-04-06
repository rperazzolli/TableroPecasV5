
using TableroPecasV5.Client.Logicas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Plantillas
{
  public class CLinkFicha : CLinkBase
  {
    public CPreguntaCN Pregunta { get; set; }

    public CLogicaTarjeta ComponentePropio
    {
      get { return (CLogicaTarjeta)Componente; }
      set { Componente = ComponentePropio; }
    }
  }

}
