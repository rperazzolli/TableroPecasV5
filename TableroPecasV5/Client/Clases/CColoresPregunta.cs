using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Clases
{
  public enum ColoresParaPreguntas
  {
    Gris = 1,
    Rojo = 2,
    Amarillo = 3,
    Verde = 4,
    Azul = 5
  }

  public class CColoresPregunta
  {
    public string CodigoElemento { get; set; }
    public ColoresParaPreguntas ColorPregunta { get; set; }
    public List<CColorElemento> Preguntas { get; set; }

    public CColoresPregunta()
    {
      CodigoElemento = "";
      ColorPregunta = ColoresParaPreguntas.Gris;
      Preguntas = new List<CColorElemento>();
    }

    public static ColoresParaPreguntas CompararColores(ColoresParaPreguntas C1,
          ColoresParaPreguntas C2)
    {
      if (C2 == ColoresParaPreguntas.Gris)
      {
        return C1;
      }

      switch (C1)
      {
        case ColoresParaPreguntas.Gris:
          return C2; // ColoresParaPreguntas.Gris;
        case ColoresParaPreguntas.Verde:
          return (C2 == ColoresParaPreguntas.Azul ? C1 : C2);
        case ColoresParaPreguntas.Amarillo:
          switch (C2)
          {
            case ColoresParaPreguntas.Azul:
            case ColoresParaPreguntas.Verde:
              return C1;
            default:
              return C2;
          }
        case ColoresParaPreguntas.Rojo:
          return C1; // (C2 == ColoresParaPreguntas.Gris ? C2 : C1);
        case ColoresParaPreguntas.Azul: return C2;
        default:
          return ColoresParaPreguntas.Gris;
      }
    }

    public void AjustarColor()
    {

      ColorPregunta = (Preguntas.Count == 0 ? ColoresParaPreguntas.Gris : Preguntas[0].ColorElemento);

      foreach (CColorElemento ColorLocal in Preguntas)
      {
        ColorPregunta = CompararColores(ColorLocal.ColorElemento, ColorPregunta);
      }
    }

  }

}
