import pandas as pd
import pyodbc #Permite conectar con la base de datos de SQL Server

#Conectar con la base de datos
ConnectionString = (
    r"DRIVER={ODBC Driver 17 for SQL Server};"
    r"SERVER=PP-WALL-E\SQLEXPRESS;"    # <-- cambia por tu servidor si procede
    r"DATABASE=Ultrasound;"
    r"Trusted_Connection=yes;"
)

def ObtenerLecturasBalanceDeBD(usuarioId = 1, numeroPruebas = 1):
    query_ConsultaLecturasBalance = f'SELECT Id, UsuarioId, NumeroPruebas, TopLeft, TopRight, BottomLeft, BottomRight, COP_X, COP_Y, Total, TimeStamp FROM LecturasBalanceBoard where UsuarioId = {usuarioId} and NumeroPruebas = {numeroPruebas} ORDER BY TimeStamp ASC;'
    
    """Conecta a SQL Server y devuelve un DataFrame con todas las filas ordenadas por TimeStamp asc."""
    
    conn = None
    try:
        conn = pyodbc.connect(ConnectionString, timeout=10)
        # pd.read_sql_query detecta y parsea TimeStamp a datetime automáticamente si puede
        df = pd.read_sql_query(query_ConsultaLecturasBalance, conn, parse_dates=['TimeStamp'])
        # Asegurar el tipo datetime y ordenar por si acaso
        df['TimeStamp'] = pd.to_datetime(df['TimeStamp'])
        df = df.sort_values('TimeStamp', ascending=True).reset_index(drop=True)
        if df is None:
            print("No se pudo leer la tabla.")
        else:
            print(f"Filas leídas: {len(df)}")
            print("Primeras 10 filas:")
            print(df.head(10))
        return df
    except Exception as e:
        print("Error al conectar o leer datos:", e)
        return None
    finally:
        
        if conn is not None:
            conn.close()

def Insertar_dataframe(df, table_name, usuarioId, numeroPruebas):
    conn = pyodbc.connect(ConnectionString, timeout=10)
    cursor = conn.cursor()

    # Las columnas estan en null, por lo que se asigna estos valores a todas las filas
    df["UsuarioId"] = usuarioId
    df["NumeroPruebas"] = numeroPruebas
    

    # Seleccionamos solo las columnas que existen en la tabla destino
    cols = [
        "UsuarioId", "NumeroPruebas", "COP_X", "COP_Y", "Total", "TimeStamp", "TimeStampSegundos"
    ]
    # Verificamos que existan en el DataFrame
    df_to_insert = df[[c for c in cols if c in df.columns]].copy()

    # Construimos la query base
    placeholders = ", ".join(["?"] * len(df_to_insert.columns))
    columns_str = ", ".join(df_to_insert.columns)
    sql = f"INSERT INTO {table_name} ({columns_str}) VALUES ({placeholders})"

    # Insertamos por lotes para mayor rendimiento
    data_tuples = [tuple(x) for x in df_to_insert.to_numpy()]
    cursor.fast_executemany = True
    cursor.executemany(sql, data_tuples)

    conn.commit()
    cursor.close()
    conn.close()
    print(f"✅ Insertadas {len(df_to_insert)} filas en {table_name}")

def Insertar_metricas(metricas):
    conn = pyodbc.connect(ConnectionString, timeout=10)
    cursor = conn.cursor()

    sql = """
    INSERT INTO Evaluaciones (UsuarioId, NumeroPruebas, Mean_COPX, Mean_COPY,
                              LongitudTrayectoria, AreaRectangulo, RMS, TimeStamp)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?)
    """

    values = (
        metricas["UsuarioId"],
        metricas["NumeroPruebas"],
        metricas["Mean_COPX"],
        metricas["Mean_COPY"],
        metricas["LongitudTrayectoria"],
        metricas["AreaRectangulo"],
        metricas["RMS"],
        metricas["TimeStamp"]
    )

    cursor.execute(sql, values)
    conn.commit()
    cursor.close()
    conn.close()
    print("✅ Métricas insertadas en Evaluaciones")

def Actualizar_tiempos_balanceboard(df, table_name):
    conn = pyodbc.connect(ConnectionString, timeout=10)

    # Calcular segundos
    df["TimeStampSegundos"] = (df["TimeStamp"] - df["TimeStamp"].iloc[0]).dt.total_seconds()

    # Actualizar uno por uno (seguro pero más lento)
    cursor = conn.cursor()
    for _, row in df.iterrows():
        cursor.execute(
            f'UPDATE {table_name} SET TimeStampSegundos = ? WHERE Id = ?',
            (row["TimeStampSegundos"], row["Id"])
        )
    conn.commit()
    cursor.close()
    conn.close()
    print(f"✅ Actualizados {len(df)} registros en LecturasBalanceBoard")










