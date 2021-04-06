using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
	public class CEstIndicadoresCN
	{
    //[JsonProperty("Indicadores")]
    public List<CDatoIndicador> Indicadores { get; set; }

    //[JsonProperty("PreguntasSueltas")]
    public List<CPuntoPregunta> PreguntasSueltas { get; set; }

    //[JsonProperty("Salas")]
    public List<CPuntoSala> Salas { get; set; }

    public CEstIndicadoresCN()
		{
      Indicadores = new List<CDatoIndicador>();
      PreguntasSueltas = new List<CPuntoPregunta>();
      Salas = new List<CPuntoSala>();
		}

  }
}
