using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransporteCargaPesada.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialTransporte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TipoCategoria = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.CheckConstraint("CK_Categorias_TipoCategoria", "TipoCategoria IN ('VEHICULO', 'TIPO_CARGA', 'TIPO_ENTREGA', 'ESTADO_CARGA', 'UNIDAD_MEDIDA', 'TIPO_OBJETO')");
                });

            migrationBuilder.CreateTable(
                name: "Cargas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Origen = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Destino = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PesoTotalKg = table.Column<decimal>(type: "TEXT", nullable: false),
                    VolumenTotalM3 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TipoCargaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoVehiculoId = table.Column<int>(type: "INTEGER", nullable: true),
                    TipoEntregaId = table.Column<int>(type: "INTEGER", nullable: false),
                    EstadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargas", x => x.Id);
                    table.CheckConstraint("CK_Cargas_PesoTotalKg", "CAST(PesoTotalKg AS REAL) > 0");
                    table.CheckConstraint("CK_Cargas_VolumenTotalM3", "VolumenTotalM3 IS NULL OR CAST(VolumenTotalM3 AS REAL) > 0");
                    table.ForeignKey(
                        name: "FK_Cargas_Categorias_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cargas_Categorias_TipoCargaId",
                        column: x => x.TipoCargaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cargas_Categorias_TipoEntregaId",
                        column: x => x.TipoEntregaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cargas_Categorias_TipoVehiculoId",
                        column: x => x.TipoVehiculoId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parametros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    TipoDato = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Valor = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UnidadMedida = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: true),
                    EsObligatorio = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametros", x => x.Id);
                    table.CheckConstraint("CK_Parametros_TipoDato", "TipoDato IN ('ENTERO', 'DECIMAL', 'TEXTO', 'BOOLEANO', 'FECHA')");
                    table.ForeignKey(
                        name: "FK_Parametros_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Objetos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CargaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    PesoUnitarioKg = table.Column<decimal>(type: "TEXT", nullable: false),
                    VolumenUnitarioM3 = table.Column<decimal>(type: "TEXT", nullable: true),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objetos", x => x.Id);
                    table.CheckConstraint("CK_Objetos_Cantidad", "Cantidad > 0");
                    table.CheckConstraint("CK_Objetos_PesoUnitarioKg", "CAST(PesoUnitarioKg AS REAL) > 0");
                    table.CheckConstraint("CK_Objetos_VolumenUnitarioM3", "VolumenUnitarioM3 IS NULL OR CAST(VolumenUnitarioM3 AS REAL) > 0");
                    table.ForeignKey(
                        name: "FK_Objetos_Cargas_CargaId",
                        column: x => x.CargaId,
                        principalTable: "Cargas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Objetos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cargas_Codigo",
                table: "Cargas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cargas_EstadoId",
                table: "Cargas",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargas_TipoCargaId",
                table: "Cargas",
                column: "TipoCargaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargas_TipoEntregaId",
                table: "Cargas",
                column: "TipoEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargas_TipoVehiculoId",
                table: "Cargas",
                column: "TipoVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_TipoCategoria_Codigo",
                table: "Categorias",
                columns: new[] { "TipoCategoria", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Objetos_CargaId",
                table: "Objetos",
                column: "CargaId");

            migrationBuilder.CreateIndex(
                name: "IX_Objetos_CategoriaId",
                table: "Objetos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Parametros_CategoriaId",
                table: "Parametros",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Parametros_Codigo",
                table: "Parametros",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Objetos");

            migrationBuilder.DropTable(
                name: "Parametros");

            migrationBuilder.DropTable(
                name: "Cargas");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
