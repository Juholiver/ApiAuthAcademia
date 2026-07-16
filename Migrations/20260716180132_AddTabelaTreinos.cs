using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ApiAuth.Migrations
{
    /// <inheritdoc />
    public partial class AddTabelaTreinos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId1",
                table: "RefreshTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Treinos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    ExercicioId = table.Column<int>(type: "integer", nullable: false),
                    Divisao = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    NomeExercicio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Series = table.Column<int>(type: "integer", nullable: false),
                    Repeticoes = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Carga = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descanso = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treinos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsuarioId1",
                table: "RefreshTokens",
                column: "UsuarioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Usuarios_UsuarioId1",
                table: "RefreshTokens",
                column: "UsuarioId1",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Usuarios_UsuarioId1",
                table: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Treinos");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UsuarioId1",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "RefreshTokens");
        }
    }
}
