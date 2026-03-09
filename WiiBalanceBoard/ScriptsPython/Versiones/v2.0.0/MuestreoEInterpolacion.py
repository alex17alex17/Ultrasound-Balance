import numpy as np
import pandas as pd
from scipy.interpolate import interp1d

def Interpolar(df_real, fs = 100):
    df = df_real.copy()

    """
    Interpola los datos del DataFrame a 100 Hz (cada 0.01 segundos)
    basado en la columna TimeStamp (datetime).
    """
    # Convertimos el tiempo a segundos relativos. Se toma el primer instante de tiempo, ya que
    # están ordenados de menor a mayor instante de tiempo.
    t0 = df["TimeStamp"].iloc[0]
    #A todos los instantes de tiempo se les resta el primer instante de tiempo y se toma los segundos
    df["t_seg"] = (df["TimeStamp"] - t0).dt.total_seconds()

    df = df.drop_duplicates(subset="t_seg", keep="first")

    # Creamos un eje temporal uniforme a 100 Hz. Se coge el primer instante de tiempo y luego
    # el último, y a parte de ahí se crea el intervalo de  0.00, 0.01, 0.02, etc
    t_uniforme = np.arange(df["t_seg"].iloc[0], df["t_seg"].iloc[-1], 1/fs)

    # Interpolamos columnas clave
    interp_copx = interp1d(df["t_seg"], df["COP_X"], kind='linear', fill_value="extrapolate")
    interp_copy = interp1d(df["t_seg"], df["COP_Y"], kind='linear', fill_value="extrapolate")
    interp_total= interp1d(df["t_seg"], df["Total"], kind='linear', fill_value="extrapolate")

    # Calculamos los valores interpolados
    df_interp = pd.DataFrame({
        "TimeStamp": [t0 + pd.to_timedelta(t, unit="s") for t in t_uniforme], #Convierte los instantes de tiempo 0.00, 0.01, 0.02 en TimeStamps
        "Total": interp_total(t_uniforme),
        "COP_X": interp_copx(t_uniforme),
        "COP_Y": interp_copy(t_uniforme)
    })

    print(f"✅ Interpoladas {len(df_interp)} muestras uniformes ({fs} Hz).")
    return df_interp
