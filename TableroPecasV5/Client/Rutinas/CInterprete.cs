using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;

namespace TableroPecasV5.Client.Rutinas
{

  public enum Operadores
  {
    PLUS,
    MINUS,
    TIMES,
    DIVIDE,
    EXP,
    CIERRE
  }

  public class CTermino
  {
    public double Valor;
    public Operadores Funcion;
    public Int32 Nivel;
  }

  public class CInterprete
  {

    // mis definiciones de constantes
    private const Int32 MAX_DIG = 25; // maxima cantidad de digitos
    private const Int32 MAX_ELEM = 15; // maximo elementos en un nivel de una formula
    public const string DIV_CERO = "Divide por cero";
    public const string PARENTESIS_DESB = "Paréntesis desbalanceados";
    public const string ERROR_FORMULA = "Error en fórmula";
    public const string LARGO_CONSTANTE = "Excede largo de constante";
    public const string CONDICION_IN = "Condición incorrecta";
    public const string POT_NEGATIVA = "Potencia de número negativo";
    // hasta aca

    private string mszMsgErr;

    private const Int32 FALSO = 0;
    private const Int32 VERDADERO = 1;

    public CInterprete()
    {
      mszMsgErr = "";
    }

    public static string Funciones(int i)
    {
      switch (i)
      {
        case 0: return "ABS";
        case 1: return "ACOS";
        case 2: return "ASIN";
        case 3: return "ATAN";
        case 4: return "COSH";
        case 5: return "COS";
        case 6: return "EXP";
        case 7: return "LOG10";
        case 8: return "LOG";
        case 9: return "ROUND";
        case 10: return "POW10";
        case 11: return "SINH";
        case 12: return "SIN";
        case 13: return "SQRT";
        case 14: return "SQR";
        case 15: return "TANH";
        case 16: return "TAN";
        case 17: return "TRUNC";
        default: return "$%$%";
      }
    }

    private static string Opers(int i)
    {
      switch (i)
      {
        case 0: return ">";
        case 1: return ">=";
        case 2: return "<";
        case 3: return "<=";
        case 4: return "=";
        case 5: return "<>";
        default: return "::";
      }
    }

    public string MensajeError
    {
      get
      {
        return mszMsgErr;
      }
    }

    private void LimpiarPuntos(ref string Frm)
    {
      if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
      {
        Frm = Frm.Replace(",", "");
      }
      else
      {
        Frm = Frm.Replace(".", "");
        Frm = Frm.Replace(",", ".");
      }
    }

    public bool Interpreta(string Formula, ref double Respuesta)
    {
      string szForm = Formula.Trim().ToUpper();
      LimpiarPuntos(ref szForm);
      string MsgError = "";
      Respuesta = SubParse(szForm, ref MsgError);
      mszMsgErr = MsgError;
      return (MsgError.Length == 0);
    }

    /// <summary>
    /// Extrae una porcion de una expresion logica.
    /// 0: primera 1: segunda 2: tercera
    /// @SI(12>1;1.5;12*125)
    /// </summary>
    /// <param name="A"></param>
    /// <returns></returns>
    public static string SubExtraeExpre(string Expresion, ref string Extraida, Int32 Posicion)
    {

      Int32 Nivel = -1;
      Int32 Pos = 0;
      Int32 Inicio = 0;
      string szRefe;

      szRefe = Expresion;
      for (Int32 i = 0; i < szRefe.Length; i++)
      {
        switch (szRefe.Substring(i, 1))
        {
          case "(":
            Nivel++;
            if (Nivel == 0)
            {
              Inicio++;
            }
            break;
          case ")":
            if (Nivel == 0)
            { // esta en el nivel de abajo
              if (Pos == Posicion)
              {
                if (i == szRefe.Length)
                {
                  Extraida = Expresion.Substring(Inicio, i - Inicio);
                  return "";
                }
                else
                {
                  return PARENTESIS_DESB;
                }
              }
              else
              {
                return "";
              }
            }
            Nivel--;
            break;
          case ";":
            if (Nivel == 0)
            {
              if (Pos == Posicion)
              {
                Extraida = Expresion.Substring(Inicio, i - Inicio);
                return "";
              }
              Pos++;
              Inicio++;
            }
            break;
        }
      }
      if (Nivel == -1)
      {
        return "";
      }
      else
      {
        return PARENTESIS_DESB;
      }
    }

    /// <summary>
    /// Interpreta la formula s y en MsgError retorna el codigo de error.
    /// </summary>
    /// <param name="?"></param>
    /// <returns></returns>
    private static double SubParse(string FormulaOriginal, ref string MsgError)
    {
      try
      {
        List<CTermino> Elementos = new List<CTermino>();
        Int32 Curr = 0;
        Int32 er;
        string Formula = "";
        //    string Input;
        string Anterior = " ";
        string Condicion = "";

        MsgError = "";

        Formula = FormulaOriginal.Trim().ToUpper();

        if (Formula.StartsWith("@SI"))
        {
          MsgError = SubExtraeExpre(Formula, ref Condicion, 0);
          if (MsgError.Length > 0)
          {
            return 0;
          }
          if (Condicion.Length == 0)
          {
            MsgError = CONDICION_IN;
            return 0;
          }

          er = SubCondicion(Condicion);
          if (er == -1)
          {
            MsgError = CONDICION_IN;
            return 0;
          }

          if (SubExtraeExpre(Formula, ref Condicion, er).Length > 0)
          {
            MsgError = CONDICION_IN;
            return 0;
          }

          return SubParse(Condicion, ref MsgError);

        }

        if (Formula.StartsWith("-"))
        {
          CTermino Agregado = new CTermino();
          Agregado.Nivel = 2;
          Agregado.Funcion = Operadores.TIMES;
          Agregado.Valor = -1;
          Elementos.Add(Agregado);
          Formula = Formula.Substring(1).Trim();
          Curr++;
        }

        while (Formula.Length > 0)
        {

          string Input = Formula.Substring(0, 1);
          if (Input == " ")
          {
            continue;
          }

          CTermino Elemento = new CTermino();

          if ("0123456789.".Contains(Input) ||
              ("+*/^".Contains(Anterior) && Input == "-"))
          {
            Elemento.Valor = SubProcNro(ref Formula, ref MsgError);
          }
          else
          {
            if (Input == "(")
            { // abre parentesis
              Elemento.Valor = SubDetRango(ref Formula, ref MsgError);
            }
            else
            { // funcion
              if (!EsFunc(ref Formula, ref Elemento))
              {
                MsgError = ERROR_FORMULA;
              }
            }
          }

          if (MsgError.Length > 0)
          {
            return 0;
          }

          Formula = Formula.Trim(); // porque se puede haber modificado

          if (Formula.Length > 0)
          {
            Input = Formula.Substring(0, 1);
          }
          else
          {
            Input = "";
          }
          Anterior = Input;

          switch (Input)
          {
            case "": // ' fin de la formula
              Elemento.Funcion = Operadores.CIERRE;
              Elemento.Nivel = -1;
              Curr++;
              Elementos.Add(Elemento);
              return SubEvaluar(ref Elementos, ref Curr, ref MsgError);
            case "+":
              Elemento.Nivel = 1;
              Elemento.Funcion = Operadores.PLUS;
              break;
            case "-":
              Elemento.Nivel = 1;
              Elemento.Funcion = Operadores.MINUS;
              break;
            case "*":
              Elemento.Nivel = 2;
              Elemento.Funcion = Operadores.TIMES;
              break;
            case "/":
              Elemento.Nivel = 2;
              Elemento.Funcion = Operadores.DIVIDE;
              break;
            case "^":
              Elemento.Nivel = 3;
              Elemento.Funcion = Operadores.EXP;
              break;
            default:
              MsgError = ERROR_FORMULA;
              return 0;
          }
          Curr++;
          Elementos.Add(Elemento);
          SubEvaluar(ref Elementos, ref Curr, ref MsgError);
          if (MsgError.Length > 0)
          {
            return 0;
          }

          Formula = Formula.Substring(1);

        }

        // aca llega cuando se completo
        CTermino Elemento2 = new CTermino();
        Elemento2.Funcion = Operadores.CIERRE;
        Elemento2.Nivel = -1;
        Elementos.Add(Elemento2);
        Curr++;
        return SubEvaluar(ref Elementos, ref Curr, ref MsgError);
      }
      catch (Exception ex)
      {
        MsgError = CRutinas.MostrarMensajeError(ex);
        return 0;
      }
    }

    /// <summary>
    /// Evaluar reduce los terminos desde derecha a izquierda mientras
    /// se encuentre con nivel >= que el ultimo.
    /// SIEMPRE QUEDAN COLAS CON NIVELES CRECIENTES, YA QUE AL INCORPORAR
    /// UN ELEMENTO, SI SU NIVEL DECRECE SE COMPRIME.
    /// </summary>
    /// <param name="Elementos"></param>
    /// <param name="Curr"></param>
    /// <param name="MsgError"></param>
    /// <returns></returns>
    private static double SubEvaluar(ref List<CTermino> Elementos, ref Int32 Curr, ref string MsgError)
    {
      switch (Curr)
      {
        case 0:
          return 0;
        case 1:
          return Elementos[0].Valor;
        default:
          SubComprimir(ref Elementos, ref Curr, ref MsgError);
          return Elementos[Curr - 1].Valor;
      }
    }

    private static void SubComprimir(ref List<CTermino> Elementos, ref Int32 Curr, ref string MsgError)
    {
      // curr: cantidad de elementos
      Int32 Niv0;
      Operadores Func0;

      Niv0 = Elementos[Curr - 1].Nivel;
      Func0 = Elementos[Curr - 1].Funcion;

      for (Int32 i = Curr - 2; i >= 0; i--)
      {
        if (Elementos[i].Nivel >= Niv0)
        {
          switch (Elementos[i].Funcion)
          {
            case Operadores.MINUS:
              Elementos[i].Valor -= Elementos[i + 1].Valor;
              break;
            case Operadores.PLUS:
              Elementos[i].Valor += Elementos[i + 1].Valor;
              break;
            case Operadores.TIMES:
              Elementos[i].Valor *= Elementos[i + 1].Valor;
              break;
            case Operadores.DIVIDE:
              if (Elementos[i + 1].Valor == 0)
              {
                MsgError = DIV_CERO;
                return;
              }
              Elementos[i].Valor /= Elementos[i + 1].Valor;
              break;
            case Operadores.EXP:
              if (Elementos[i + 1].Valor < 0)
              {
                MsgError = POT_NEGATIVA;
                return;
              }
              try
              {
                Elementos[i].Valor = Math.Pow(Elementos[i].Valor, Elementos[i + 1].Valor);
              }
              catch (Exception)
              {
                MsgError = ERROR_FORMULA;
                return;
              }
              break;
            default:
              MsgError = ERROR_FORMULA;
              return;
          }
          Curr--;
          Elementos.RemoveAt(Curr);
          Elementos[i].Nivel = Niv0;
          Elementos[i].Funcion = Func0;
          if (Curr < 2)
          {
            return;
          }
        }
        else
        {
          return;
        }
      }

    }

    /// <summary>
    /// Determina el valor correspondiente al interior de un rango
    /// entre parentesis y devuelve input apuntando al caracter posterior
    /// (originalmente viene apuntando al parentesis inicial).
    /// </summary>
    /// <param name="Input"></param>
    /// <param name="MsgError"></param>
    /// <returns></returns>
    private static double SubDetRango(ref string Input, ref string MsgError)
    {

      string Inp00;
      string Eval;
      Int32 Largo;
      Int32 Nivel;

      Input = Input.Substring(1).Trim();

      Inp00 = Input.Substring(0, 1);

      // barre hasta encontrar el parentesis de cierre
      Nivel = 0;
      Largo = 0;
      for (Int32 i = 0; i < Input.Length; i++)
      {
        switch (Input.Substring(i, 1))
        {
          case "(":
            Nivel++;
            break;
          case ")":
            Nivel--;
            break;
        }
        Largo++;
        if (Nivel < 0)
        {
          break;
        }
      }

      if (Largo < 2)
      {
        return 0;
      }

      if (Nivel >= 0)
      {
        return 0;
      }

      // elimina el parentesis que cerro
      Largo--;

      Eval = Input.Substring(0, Largo);

      Input = Input.Substring(Largo + 1);

      return SubParse(Eval, ref MsgError);

    }


    /// <summary>
    /// Verifica si una expresion a partir de input es una funcion de las
    /// soportadas y cuando lo es determina el valor y devuelve TRUE, poniendo
    /// en la estructura refe el valor de la funcion y dejando input apuntando
    /// al caracter posterior el parentesis de cierre.
    /// </summary>
    /// <param name="Input"></param>
    /// <param name="Elemento"></param>
    /// <returns></returns>
    private static bool EsFunc(ref string Input, ref CTermino Elemento)
    {

      Int32 Funcion = -1;
      double R;

      for (Int32 i = 0; i < 18; i++)
      {
        if (Input.StartsWith(Funciones(i)))
        {
          Input = Input.Substring(Funciones(i).Length).Trim();
          Funcion = i + 1;
        }
      }

      if (Funcion < 0)
      {
        return false;
      }

      if (!Input.StartsWith("("))
      {
        return false;
      }

      string MsgError = "";
      R = SubDetRango(ref Input, ref MsgError);
      if (MsgError.Length > 0)
      {
        return false;
      }

      try
      {
        switch (Funcion)
        {
          case 1:
            Elemento.Valor = Math.Abs(R);
            break;
          case 2:
            Elemento.Valor = Math.Acos(R);
            break;
          case 3:
            Elemento.Valor = Math.Asin(R);
            break;
          case 4:
            Elemento.Valor = Math.Atan(R);
            break;
          case 5:
            Elemento.Valor = Math.Cosh(R);
            break;
          case 6:
            Elemento.Valor = Math.Cos(R);
            break;
          case 7:
            if (R < 0)
            {
              Elemento.Valor = 0;
              return false;
            }
            else
            {
              Elemento.Valor = Math.Pow(2.71828182, R);
              break;
            }
          case 8:
            if (R < 0)
            {
              Elemento.Valor = 0;
              return false;
            }
            else
            {
              Elemento.Valor = Math.Log10(R);
              break;
            }
          case 9:
            if (R < 0)
            {
              Elemento.Valor = 0;
              return false;
            }
            else
            {
              Elemento.Valor = Math.Log(R);
              break;
            }
          case 10:
            Elemento.Valor = Math.Floor(R + 0.5);
            break;
          case 11:
            Elemento.Valor = Math.Pow(10, R);
            break;
          case 12:
            Elemento.Valor = Math.Sinh(R);
            break;
          case 13:
            Elemento.Valor = Math.Sin(R);
            break;
          case 14:
            if (R < 0)
            {
              Elemento.Valor = 0;
              return false;
            }
            else
            {
              Elemento.Valor = Math.Sqrt(R);
              break;
            }
          case 15:
            Elemento.Valor = R * R;
            break;
          case 16:
            Elemento.Valor = Math.Tanh(R);
            break;
          case 17:
            Elemento.Valor = Math.Tan(R);
            break;
          case 18:
            Elemento.Valor = Math.Floor(R);
            break;
        }

        return true;
      }

      catch (Exception)
      {
        return false;
      }

    }

    /// <summary>
    /// Procesa un string a partir de input0 y devuelve el valor
    /// correspondiente o 0 y pone error en att.
    /// Deja input0 apuntando al caracter posterior a la constante.
    /// </summary>
    /// <param name="Input0"></param>
    /// <param name="MsgError"></param>
    /// <returns></returns>
    private static double SubProcNro(ref string Input0, ref string MsgError)
    {

      MsgError = "";

      Input0 = Input0.Trim();
      string Input;
      string NumString = "";

      for (Int32 i = 0; i < Input0.Length; i++)
      {
        Input = Input0.Substring(i, 1);
        if (i == 0)
        {
          if (!"-.,0123456789".Contains(Input))
          {
            break;
          }
        }
        else
        {
          if (!".,0123456789".Contains(Input))
          {
            break;
          }
        }
        NumString += Input;
      }
      Input0 = Input0.Substring(NumString.Length).Trim();

      try
      {
        NumString = NumString.Trim();
        if (NumString.Length == 0)
        {
          throw new Exception("");
        }
        return CRutinas.StrVFloat(NumString);
      }
      catch (Exception)
      {
        MsgError = ERROR_FORMULA;
        return 0;
      }

    }

    /// <summary>
    /// Recibe una expresion logica en Condicion y devuelve
    /// 1 si verifica, 0 si no verifica y -1 si es incorrecta.
    /// </summary>
    /// <param name="Condicion"></param>
    /// <returns></returns>
    private static Int32 SubCondicion(string Condicion)
    {

      Int32 Actual;
      string Copia;
      string Input;

      Copia = Condicion.Trim().ToUpper();

      // elimina parentesis extremos
      while (Copia.StartsWith("(") && Copia.EndsWith(")"))
      {
        Copia = Copia.Substring(1, Copia.Length - 2).Trim();
      }

      Actual = 1;
      Int32 Opcion = -1;

      Int32 i = 0;

      while (i < (Copia.Length - 1))
      {
        i++;
        Input = Copia.Substring(i, 1);
        if (Input == " ")
        {
          continue;
        }

        Actual = Busca_AND_OR(ref Copia, Actual, ref Opcion);
        if (Actual == -1)
        {
          break;
        }
      }

      return Actual;

    }

    /// <summary>
    /// Antes: como estaba antes (1:true 0:false -1:error).
    /// Opcion:
    ///    1: AND
    ///    0: OR
    ///    -1: primer termino (ni AND ni OR).
    /// </summary>
    /// <param name="S"></param>
    /// <param name="Antes"></param>
    /// <param name="Opcion"></param>
    /// <returns></returns>
    private static Int32 Busca_AND_OR(ref string S, Int32 Antes, ref Int32 Opcion)
    {

      string Posic = S.Trim().ToUpper();
      string Buffer;
      string MsgError = "";
      double R;
      double R1;

      while (Posic.StartsWith("(") && Posic.EndsWith(")"))
      {
        Posic = Posic.Substring(1, Posic.Length - 2).Trim();
      }

      Int32 Largo = Posic.Length;

      bool And_Or = (Posic.IndexOf("AND") >= 0 || Posic.IndexOf("OR") >= 0);

      Int32 i = -1;
      Int32 ii = 0;
      Int32 iOp;
      Int32 Nivel = 0;

      while (i < (Posic.Length - 1))
      {

        i++;

        if ("()".Contains(Posic.Substring(i, 1)))
        {
          Nivel += (Posic.Substring(i, 1) == "(" ? 1 : -1);
          i++;
          continue;
        }

        // ***********************************************************
        if (Nivel == 0 && i != 0)
        { // si se cerraron los parentesis
          // si es AND or OR evalua la expresion
          // y sale retornando el codigo (AND or OR) y la evaluacion de
          // la expresion hasta alli
          if (And_Or)
          {
            if (Posic.Substring(i, 3) == "AND" || Posic.Substring(i, 2) == "OR")
            {
              Buffer = Posic.Substring(0, i);
              ii = SubCondicion(Buffer);
              if (Opcion != -1)
              {
                if (Antes == -1 || ii == -1)
                {
                  ii = -1;
                }
                else
                {
                  if (Opcion == 1)
                  { // AND
                    if (Antes == 1 && ii == 1)
                    {
                      ii = 1;
                    }
                    else
                    {
                      i = 0;
                    }
                  }
                  else
                  { // OR
                    if (Antes == 1 || ii == 1)
                    {
                      ii = 1;
                    }
                    else
                    {
                      i = 0;
                    }
                  }
                }
              }

              if (Posic.Substring(i, 1) == "A")
              {
                i += 3;
                Opcion = 1;
              }
              else
              {
                i += 2;
                Opcion = 0;
              }

              S = Posic.Substring(i);

              return ii;
            }
            else
            {
              continue;
            } // cierra AND OR
          }

          // *********************************************
          // verifica si es un operador ( > < >= <= = <> )
          // o si se termino la expresion

          iOp = EsOper(Posic.Substring(i), ii);
          if (iOp != 0)
          {
            if (iOp < 0)
            {
              return -1;
            }
            Buffer = Posic.Substring(0, i); // no incluye el operador
            R = SubParse(Buffer, ref MsgError);
            if (MsgError.Length > 0)
            {
              return -1;
            }
            i += ii;

            // evalua el termino del extremo derecho
            R1 = SubParse(Posic.Substring(i), ref MsgError);
            if (MsgError.Length > 0)
            {
              return -1;
            }

            ii = 0;
            switch (iOp)
            {
              case 1: // >
                if (R > R1)
                {
                  ii = 1;
                }
                break;
              case 2: // >=
                if (R >= R1)
                {
                  ii = 1;
                }
                break;
              case 3: // <
                if (R < R1)
                {
                  ii = 1;
                }
                break;
              case 4: // <=
                if (R <= R1)
                {
                  ii = 1;
                }
                break;
              case 5: // =
                if (R == R1)
                {
                  ii = 1;
                }
                break;
              case 6: // <>
                if (R != R1)
                {
                  ii = 1;
                }
                break;
            }

            if (Opcion != -1)
            {
              if (Antes == -1 || ii == -1)
              {
                ii = -1;
              }
              else
              {
                if (Opcion == 1)
                { // AND
                  if (Antes == 1 && ii == 1)
                  {
                    ii = 1;
                  }
                  else
                  {
                    ii = 0;
                  }
                }
              }
            }
            else
            { // OR
              if (Antes == 1 || ii == 1)
              {
                ii = 1;
              }
              else
              {
                ii = 0;
              }
            }

            S = Posic.Substring(Largo); // actualiza el pointer a la posicion corriente

            return ii; // retorna la condicion actual

          } // cierra IF IOP
        } // cierra IF nivel=0
      } // cierra loop de barrido de la expresion

      return -1; // retorna expresion incorrecta

    }

    private static Int32 EsOper(string A, Int32 l)
    {
      for (Int32 i = 0; i < 5; i++)
      {
        if (A.StartsWith(Opers(i)))
        {
          l = Opers(i).Length;
          return i + 1;
        }
      }

      return 0;

    }

    /*     Shared Function SubParsH(ByVal A As String, ByRef H As Single, ByRef Att As Integer) As Double

           Dim i As Integer
           Dim Form As String

           Form = UCase(Trim(A))

           i = 1
           While i < Len(Form)
             If Mid$(Form, i, 1) = "HM" Then
               Form = Left$(Form, i - 1) + Trim(CStr(H)) + Mid$(Form, i + 2)
             End If
           End While

           Return SubParse(Form, Att)

         End Function

       End Class */
  }

}

