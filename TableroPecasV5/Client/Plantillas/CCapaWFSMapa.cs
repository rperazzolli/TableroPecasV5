using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Plantillas
{
  public class CCapaWFSMapa
  {
    public Int32 Capa { get; set; }
    public Int32 A { get; set; }
    public Int32 R { get; set; }
    public Int32 G { get; set; }
    public Int32 B { get; set; }

    public CCapaWFSMapa(Int32 Capa0, Int32 A0, Int32 R0, Int32 G0, Int32 B0)
    {
      Capa = Capa0;
      A = A0;
      R = R0;
      G = G0;
      B = B0;
    }

    public CCapaWFSMapa(string Datos)
    {
      string[] Valores = Datos.Split(new char[] { ';' });
      Capa = Int32.Parse(Valores[0]);
      A = Int32.Parse(Valores[1]);
      R = Int32.Parse(Valores[2]);
      G = Int32.Parse(Valores[3]);
      B = Int32.Parse(Valores[4]);
    }

    public string Texto
    {
      get
      {
        return Capa.ToString() + ";" + A.ToString() + ";" + R.ToString() + ";" +
            G.ToString() + ";" + B.ToString();
      }
    }
  }

}
