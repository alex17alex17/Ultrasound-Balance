CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Edad INT NULL,
    Genero NVARCHAR(10) NULL,
    Altura FLOAT NULL,
    Peso FLOAT NULL,
    NumeroPruebas INT DEFAULT 0,
    TipoEfecto nvarchar(100) NULL,
    Descripcion nvarchar(500) NULL,
    TimeStamp DATETIME NOT NULL
);

CREATE TABLE LecturasBalanceBoard (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NULL,
    NumeroPruebas INT NULL,
    TopLeft FLOAT NOT NULL,
    TopRight FLOAT NOT NULL,
    BottomLeft FLOAT NOT NULL,
    BottomRight FLOAT NOT NULL,
    COP_X FLOAT NULL,
    COP_Y FLOAT NULL,
    Total FLOAT NOT NULL,
    TimeStamp DATETIME NOT NULL,
    TimeStampSegundos FLOAT NULL,
    CONSTRAINT FK_LecturasBalanceBoard_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

CREATE TABLE LecturasInterpoladas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NULL,
    NumeroPruebas INT NULL,
    TopLeft FLOAT NULL,
    TopRight FLOAT NULL,
    BottomLeft FLOAT NULL,
    BottomRight FLOAT NULL,
    COP_X FLOAT NULL,
    COP_Y FLOAT NULL,
    Total FLOAT NULL,
    TimeStamp DATETIME NOT NULL,
    TimeStampSegundos FLOAT NULL,
    CONSTRAINT FK_LecturasInterpoladas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

CREATE TABLE LecturasConFiltro (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NULL,
    NumeroPruebas INT NULL,
    TopLeft FLOAT NULL,
    TopRight FLOAT NULL,
    BottomLeft FLOAT NULL,
    BottomRight FLOAT NULL,
    COP_X FLOAT NULL,
    COP_Y FLOAT NULL,
    Total FLOAT NULL,
    TimeStamp DATETIME NOT NULL,
    TimeStampSegundos FLOAT NULL,
    CONSTRAINT FK_LecturasConFiltro_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

CREATE TABLE LecturasConMediaMovil (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NULL,
    NumeroPruebas INT NULL,
    TopLeft FLOAT NULL,
    TopRight FLOAT NULL,
    BottomLeft FLOAT NULL,
    BottomRight FLOAT NULL,
    COP_X FLOAT NULL,
    COP_Y FLOAT NULL,
    Total FLOAT NULL,
    TimeStamp DATETIME NOT NULL,
    TimeStampSegundos FLOAT NULL,
    CONSTRAINT FK_LecturasConMediaMovil_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

CREATE TABLE Evaluaciones (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NULL,
    NumeroPruebas INT NULL,
    Mean_COPX FLOAT NULL,
    Mean_COPY FLOAT NULL,
    LongitudTrayectoria FLOAT NULL,
    AreaRectangulo FLOAT NULL,
    RMS FLOAT NULL,
    TimeStamp DATETIME NOT NULL,
    CONSTRAINT FK_Evaluaciones_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
