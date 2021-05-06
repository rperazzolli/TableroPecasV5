using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaDatosIndicador : ComponentBase
	{
		[Parameter]
		public TableroPecasV5.Shared.CDatoIndicador Datos { get; set; }

		[Parameter]
		public Int32 CodigoElemento { get; set; } = -1;

		[Parameter]
		public TableroPecasV5.Shared.CInformacionAlarmaCN Alarma { get; set; }

		public string LinkFlecha
		{
			get
			{
				if (Alarma.Minimo == Alarma.Sobresaliente)
				{
					return "";
				}
				else
				{
					string Post = (Alarma.Valor > Alarma.ValorAnterior ? "crec" :
						(Alarma.Valor == Alarma.ValorAnterior ? "igu" : "dec"));
					string Color = "";
					if (Alarma.Sobresaliente > Alarma.Minimo)
					{
						if (Alarma.Valor >= Alarma.Sobresaliente)
						{
							Color = "azul";
						}
						else
						{
							if (Alarma.Valor >= Alarma.Satisfactorio)
							{
								Color = "ver";
							}
							else
							{
								if (Alarma.Valor >= Alarma.Minimo)
								{
									Color = "am";
								}
								else
								{
									Color = "rojo";
								}
							}
						}
					}
					else
					{
						if (Alarma.Valor <= Alarma.Sobresaliente)
						{
							Color = "azul";
						}
						else
						{
							if (Alarma.Valor <= Alarma.Satisfactorio)
							{
								Color = "ver";
							}
							else
							{
								if (Alarma.Valor <= Alarma.Minimo)
								{
									Color = "am";
								}
								else
								{
									Color = "rojo";
								}
							}
						}
					}
					return "Imagenes/" + Color + "_" + Post + ".png";
				}
			}
		}

		protected override Task OnInitializedAsync()
		{
			Contenedores.CContenedorDatos.AlarmaIndicadorDesdeGlobal(Datos.Codigo, CodigoElemento);
			return base.OnInitializedAsync();
		}
	}
}
