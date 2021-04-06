using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Json;

namespace TableroPecasV5.Shared
{
	public class CDatoIndicador
	{

    //[JsonProperty("Acumulado")]
    public bool Acumulado { get; set; }

    //[JsonProperty("Area")]
    public string Area { get; set; }

    //[JsonProperty("Codigo")]
    public int Codigo { get; set; }

    //[JsonProperty("CodigoArea")]
    public int CodigoArea { get; set; }

    //[JsonProperty("CodigoPuesto")]
    public int CodigoPuesto { get; set; }

    //[JsonProperty("CodigoUsuarioMetas")]
    public int CodigoUsuarioMetas { get; set; }

    //[JsonProperty("CodigoUsuarioValores")]
    public int CodigoUsuarioValores { get; set; }

    //[JsonProperty("Comentario")]
    public string Comentario { get; set; }

    //[JsonProperty("Comite")]
    public int Comite { get; set; }

    //[JsonProperty("DatasetComprimido")]
    public bool DatasetComprimido { get; set; }

    //[JsonProperty("Decimales")]
    public int Decimales { get; set; }

    //[JsonProperty(Descripcion")]
    public string Descripcion { get; set; }

    //[JsonProperty("Dimension")]
    public int Dimension { get; set; }

    //[JsonProperty("EsCalculado")]
    public bool EsCalculado { get; set; }

    //[JsonProperty("EscalaCreciente")]
    public bool EscalaCreciente { get; set; }

    //[JsonProperty("FechaInicio")]
    public System.DateTime FechaInicio { get; set; }

    //[JsonProperty("Formula")]
    public string Formula { get; set; }

    //[JsonProperty("Frecuencia")]
    public string Frecuencia { get; set; }

    //[JsonProperty("NombreArea")]
    public string NombreArea { get; set; }

    //[JsonProperty("PeriodosPorCiclo")]
    public int PeriodosPorCiclo { get; set; }

    //[JsonProperty("Puesto")]
    public string Puesto { get; set; }

    //[JsonProperty("SoloDiasLaborables")]
    public bool SoloDiasLaborables { get; set; }

    //[JsonProperty("TiemposRobot")]
    public string TiemposRobot { get; set; }

    //[JsonProperty("TieneBono")]
    public bool TieneBono { get; set; }

    //[JsonProperty("TieneComentarios")]
    public bool TieneComentarios { get; set; }

    //[JsonProperty("TieneDetalle")]
    public bool TieneDetalle { get; set; }

    //[JsonProperty("TipoCiclo")]
    public int TipoCiclo { get; set; }

    //[JsonProperty("Unidades")]
    public string Unidades { get; set; }

    //[JsonProperty("UsuarioCreador")]
    public int UsuarioCreador { get; set; }

    //[JsonProperty("UsuarioMetas")]
    public string UsuarioMetas { get; set; }

    //[JsonProperty("UsuarioResponsable")]
    public string UsuarioResponsable { get; set; }

    //[JsonProperty("UsuarioValores")]
    public string UsuarioValores { get; set; }

    //[JsonProperty("Usuarios")]
    public System.Collections.Generic.List<int> Usuarios { get; set; }

    public CDatoIndicador()
		{
      Usuarios = new List<int>();
		}

  }

}
