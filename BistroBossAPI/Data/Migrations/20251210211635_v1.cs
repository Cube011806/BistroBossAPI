using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BistroBossAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataUrodzenia",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Imie",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KoszykId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Nazwisko",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Dostawy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Miejscowosc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ulica = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    NumerBudynku = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    KodPocztowy = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dostawy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kategorie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nazwa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategorie", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Opinie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Komentarz = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Ocena = table.Column<byte>(type: "tinyint", nullable: false),
                    ZamowienieId = table.Column<int>(type: "int", nullable: false),
                    UzytkownikId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opinie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opinie_AspNetUsers_UzytkownikId",
                        column: x => x.UzytkownikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Produkty",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nazwa = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cena = table.Column<float>(type: "real", nullable: false),
                    CzasPrzygotowania = table.Column<int>(type: "int", nullable: false),
                    Zdjecie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produkty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produkty_Kategorie_KategoriaId",
                        column: x => x.KategoriaId,
                        principalTable: "Kategorie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Zamowienia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataZamowienia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CenaCalkowita = table.Column<float>(type: "real", nullable: false),
                    PrzewidywanyCzasRealizacji = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Miejscowosc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ulica = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    NumerBudynku = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    KodPocztowy = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SposobDostawy = table.Column<bool>(type: "bit", nullable: false),
                    Imie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nazwisko = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumerTelefonu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpiniaId = table.Column<int>(type: "int", nullable: true),
                    UzytkownikId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zamowienia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zamowienia_AspNetUsers_UzytkownikId",
                        column: x => x.UzytkownikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Zamowienia_Opinie_OpiniaId",
                        column: x => x.OpiniaId,
                        principalTable: "Opinie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Koszyki",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UzytkownikId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ZamowienieId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Koszyki", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Koszyki_AspNetUsers_UzytkownikId",
                        column: x => x.UzytkownikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Koszyki_Zamowienia_ZamowienieId",
                        column: x => x.ZamowienieId,
                        principalTable: "Zamowienia",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ZamowieniaProdukty",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZamowienieId = table.Column<int>(type: "int", nullable: false),
                    ProduktId = table.Column<int>(type: "int", nullable: false),
                    Ilosc = table.Column<int>(type: "int", nullable: false),
                    Cena = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZamowieniaProdukty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZamowieniaProdukty_Produkty_ProduktId",
                        column: x => x.ProduktId,
                        principalTable: "Produkty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZamowieniaProdukty_Zamowienia_ZamowienieId",
                        column: x => x.ZamowienieId,
                        principalTable: "Zamowienia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KoszykProdukty",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KoszykId = table.Column<int>(type: "int", nullable: false),
                    ProduktId = table.Column<int>(type: "int", nullable: false),
                    Ilosc = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KoszykProdukty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KoszykProdukty_Koszyki_KoszykId",
                        column: x => x.KoszykId,
                        principalTable: "Koszyki",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KoszykProdukty_Produkty_ProduktId",
                        column: x => x.ProduktId,
                        principalTable: "Produkty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kategorie_Nazwa",
                table: "Kategorie",
                column: "Nazwa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Koszyki_UzytkownikId",
                table: "Koszyki",
                column: "UzytkownikId",
                unique: true,
                filter: "[UzytkownikId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Koszyki_ZamowienieId",
                table: "Koszyki",
                column: "ZamowienieId");

            migrationBuilder.CreateIndex(
                name: "IX_KoszykProdukty_KoszykId",
                table: "KoszykProdukty",
                column: "KoszykId");

            migrationBuilder.CreateIndex(
                name: "IX_KoszykProdukty_ProduktId",
                table: "KoszykProdukty",
                column: "ProduktId");

            migrationBuilder.CreateIndex(
                name: "IX_Opinie_UzytkownikId",
                table: "Opinie",
                column: "UzytkownikId");

            migrationBuilder.CreateIndex(
                name: "IX_Produkty_KategoriaId",
                table: "Produkty",
                column: "KategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Zamowienia_OpiniaId",
                table: "Zamowienia",
                column: "OpiniaId",
                unique: true,
                filter: "[OpiniaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Zamowienia_UzytkownikId",
                table: "Zamowienia",
                column: "UzytkownikId");

            migrationBuilder.CreateIndex(
                name: "IX_ZamowieniaProdukty_ProduktId",
                table: "ZamowieniaProdukty",
                column: "ProduktId");

            migrationBuilder.CreateIndex(
                name: "IX_ZamowieniaProdukty_ZamowienieId",
                table: "ZamowieniaProdukty",
                column: "ZamowienieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dostawy");

            migrationBuilder.DropTable(
                name: "KoszykProdukty");

            migrationBuilder.DropTable(
                name: "ZamowieniaProdukty");

            migrationBuilder.DropTable(
                name: "Koszyki");

            migrationBuilder.DropTable(
                name: "Produkty");

            migrationBuilder.DropTable(
                name: "Zamowienia");

            migrationBuilder.DropTable(
                name: "Kategorie");

            migrationBuilder.DropTable(
                name: "Opinie");

            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DataUrodzenia",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Imie",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "KoszykId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Nazwisko",
                table: "AspNetUsers");
        }
    }
}
