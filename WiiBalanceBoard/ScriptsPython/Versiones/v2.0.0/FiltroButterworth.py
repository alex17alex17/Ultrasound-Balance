from scipy.signal import butter, filtfilt

def Butterworth_Filtro_PasoBajo(data, fc=10, fs=100, order=4):
    """
    Aplica un filtro Butterworth paso bajo a una señal 1D.
    
    Parámetros:
        data: array-like, señal de entrada
        fc: frecuencia de corte (Hz)
        fmuestreo: frecuencia de muestreo (Hz)
        order: orden del filtro
        
    Devuelve:
        Señal filtrada
    """

    # Es la frecuencia máxima de una señal que se puede representar sin distorsión.
    # Por encima de esa señal, el filtro causará distorsión (aliasing).
    # Cuando se define un filtro, este necesita conocer la frecuencia de corte relativa a la frecuencia nyquist
    # fc = 10 Hz / Fnyquist = 50 HZ --> significa que deje pasar hasta el 20% de la frecuencia máxima posible
    # que es la frecuencia nyquist.

    f_nyquist = 0.5 * fs # el filtro se elige en función de la frecuencia nyquist
    fc_normalizada = fc / f_nyquist
    b, a = butter(order, fc_normalizada, btype='low', analog=False) # si se pone la frecuencia de muestreo fs = 100Hz, no es necesario normalizar la fc.
    y = filtfilt(b, a, data)  # filtra hacia adelante y hacia atrás para evitar desfase
    return y

def Aplicar_filtro_butterworth(df, fc=10, fs=100):
    df_filtrado = df.copy()
    
    columnas = ["Total", "COP_X", "COP_Y"]
    
    for col in columnas:
        df_filtrado[col] = Butterworth_Filtro_PasoBajo(df[col], fc=fc, fs=fs, order=4)
    
    print(f"✅ Filtro Butterworth aplicado ({len(df_filtrado)} muestras, fc={fc} Hz, orden 4)")
    return df_filtrado