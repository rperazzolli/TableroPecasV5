using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace TableroPecasV5.Client.Clases
{
  public class CReloj : ComponentBase
  {

    public string NombreIndicador { get; set; } = "No definido";

    public string Estilo
    {
      get { return "width: 150px; height: 150px; margin-left: 20px; margin-top: 25px;"; }
    }
  }
}
