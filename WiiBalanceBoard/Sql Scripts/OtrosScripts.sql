select * from LecturasBalanceBoard where Id = 11411 order by TimeStamp asc;
select * from LecturasInterpoladas order by TimeStamp asc;
select * from LecturasConFiltro order by TimeStamp asc;
select * from LecturasConMediaMovil order by TimeStamp asc;
select * from Evaluaciones order by TimeStamp asc;
select * from Usuarios;

/*
ALTER TABLE LecturasBalanceBoard
ADD TimeStampSegundos FLOAT NULL;

ALTER TABLE LecturasInterpoladas
ADD TimeStampSegundos FLOAT NULL;

ALTER TABLE LecturasConFiltro
ADD TimeStampSegundos FLOAT NULL;

ALTER TABLE LecturasConMediaMovil
ADD TimeStampSegundos FLOAT NULL;

ALTER TABLE Usuarios
ADD TipoEfecto nvarchar(100) NULL;

ALTER TABLE Usuarios
ADD Descripcion nvarchar(500) NULL;
*/



/*
delete LecturasInterpoladas 
delete LecturasConFiltro 
delete LecturasConMediaMovil
delete Evaluaciones
delete LecturasBalanceBoard;
delete Usuarios
*/