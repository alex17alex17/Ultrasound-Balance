import numpy as np
import pandas as pd
from datetime import datetime

def Calcular_metricas(df, usuarioId, numeroPruebas):
    """
    Calcula métricas de equilibrio a partir de los datos del COP suavizados.
    Devuelve un diccionario con los valores listos para insertar en la tabla Evaluaciones.
    """

    # La media cuandoe existen número negativos se calcula de la misma manera de siempre.
    # Se suman todos y se divide entre el número total de elementos.
    mean_copx = df["COP_X"].mean()
    mean_copy = df["COP_Y"].mean()

    # Calcula la diferencia de cada elemento del COP con su siguiente.
    # Si son número negativos también los tiene en cuenta. Es decir, de -2 a 1 se obtiene 3 y de -1 a -3 se obtiene -2, ya que a
    # -1 se le debe restar -2 para obtener -3. Además, el signo no importa ua que luego se hace el cuadrado
    # al menos en la longitud de la trayectoria.
    dx = np.diff(df["COP_X"])
    dy = np.diff(df["COP_Y"])
    longitud_trayectoria = np.sum(np.sqrt(dx**2 + dy**2))

    # Si se coge el menor de todos y es negativo, entonces se hace positivo y la diferencia es mayor. 4 - (-3) entonces la diferencia es 7
    # En el caso de que sean los dos positivos (7 - 3), la diferencia es 4.
    # El problema está en que si los dos números son negativos (-3 - (-7)) esto es 4, lo cual sería correcto también.
    area_rectangulo = (df["COP_X"].max() - df["COP_X"].min()) * (df["COP_Y"].max() - df["COP_Y"].min())

    # Al calcular la diferencia de -3 con la media 10, entonces la distancia del COP_X con su media es -13
    # Como despues se realiza el cuadrado, el negativo siempre se va
    # Si se tiene 3 - 10, entonces da 7 la diferencia.
    # por otro lado, 
    rms = np.sqrt(np.mean((df["COP_X"] - mean_copx)**2 + (df["COP_Y"] - mean_copy)**2))

    metricas = {
        "UsuarioId": usuarioId,
        "NumeroPruebas": numeroPruebas,
        "Mean_COPX": mean_copx,
        "Mean_COPY": mean_copy,
        "LongitudTrayectoria": longitud_trayectoria,
        "AreaRectangulo": area_rectangulo,
        "RMS": rms,
        "TimeStamp": datetime.now()
    }

    print("✅ Métricas calculadas:")
    for k, v in metricas.items():
        print(f"   {k}: {v}")

    return metricas