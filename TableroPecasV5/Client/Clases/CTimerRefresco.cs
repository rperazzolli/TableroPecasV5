using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace TableroPecasV5.Client.Clases
{
  public class CTimerRefresco
  {
    public delegate void FncRefrescar();
    public event FncRefrescar AlRefrescar;
    private Timer mTimer = null;
    private DateTime mFechaHoraTimer = Rutinas.CRutinas.FechaMaxima();
    private static bool gGraficando = false;
    private static object OBJ_LOCK = new object();
    private static object OBJ_LOCK_TIMER = new object();

    public void RefrescarPedido()
    {
      lock (OBJ_LOCK_TIMER)
      {
        mFechaHoraTimer = DateTime.Now.AddMilliseconds(200);
        if (mTimer == null)
        {
          mTimer = new Timer(new TimerCallback(_ =>
          {
            lock (OBJ_LOCK)
            {
              if (gGraficando)
              {
                return;
              }
              else
              {
                gGraficando = true;
              }
            }
            try
            {
              if (DateTime.Now >= mFechaHoraTimer)
              {
                if (AlRefrescar != null)
                {
                  AlRefrescar();
                }

                mTimer.Dispose();
                mTimer = null;
              }
            }
            finally
            {
              gGraficando = false;
            }
          }), null, 200, 200);
        }
      }
    }

  }
}
