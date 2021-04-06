using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TableroPecasV5
{
  public class CEventosJS
  {
    public delegate void FncEvResize(string Datos);
    public static event FncEvResize OnResize;

    [JSInvokable]
    public static void OnElementResize(string Datos)
    {
      if (OnResize != null)
      {
        OnResize(Datos);
      }
    }

  }
}
