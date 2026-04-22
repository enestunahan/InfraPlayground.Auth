using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InfraPlayground.Auth.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Isbn = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    PublicationYear = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Description", "Isbn", "Price", "PublicationYear", "Title" },
                values: new object[,]
                {
                    { new Guid("22fdfd36-83fd-45b6-aa09-c19f453f4fb2"), "Karmaşık domain problemleri için modelleme yaklaşımı.", "9780321125217", 55.00m, 2003, "Domain-Driven Design" },
                    { new Guid("bb3dcb8d-f4c0-4bb2-87a4-b3db9fd11b7f"), "Pratik yazılım geliştirme alışkanlıkları ve mühendislik bakışı.", "9780201616224", 39.90m, 1999, "The Pragmatic Programmer" },
                    { new Guid("d6a6fd4f-8a61-47b2-87e4-c7d8f934c1d1"), "Yazılım geliştirmede okunabilirlik ve sürdürülebilirlik prensipleri.", "9780132350884", 42.50m, 2008, "Clean Code" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_Isbn",
                table: "Books",
                column: "Isbn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
