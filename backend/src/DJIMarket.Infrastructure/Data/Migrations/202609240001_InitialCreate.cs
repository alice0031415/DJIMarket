using System;
using DJIMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DJIMarket.Infrastructure.Data.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "categories", columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
        }, constraints: table => table.PrimaryKey("PK_categories", x => x.Id));

        migrationBuilder.CreateTable(name: "customers", columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
            Company = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
            Segment = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
        }, constraints: table => table.PrimaryKey("PK_customers", x => x.Id));

        migrationBuilder.CreateTable(name: "managers", columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
            Team = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
            Position = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
            IsActive = table.Column<bool>(type: "boolean", nullable: false),
            AvatarSeed = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
        }, constraints: table => table.PrimaryKey("PK_managers", x => x.Id));

        migrationBuilder.CreateTable(name: "products", columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
            CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
            IsActive = table.Column<bool>(type: "boolean", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_products", x => x.Id);
            table.ForeignKey("FK_products_categories_CategoryId", x => x.CategoryId, "categories", "Id", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateTable(name: "sales", columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            ManagerId = table.Column<Guid>(type: "uuid", nullable: false),
            CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
            SoldAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_sales", x => x.Id);
            table.ForeignKey("FK_sales_customers_CustomerId", x => x.CustomerId, "customers", "Id", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_sales_managers_ManagerId", x => x.ManagerId, "managers", "Id", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateTable(name: "sale_items", columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            SaleId = table.Column<Guid>(type: "uuid", nullable: false),
            ProductId = table.Column<Guid>(type: "uuid", nullable: false),
            Quantity = table.Column<int>(type: "integer", nullable: false),
            SalePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
            CostPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_sale_items", x => x.Id);
            table.ForeignKey("FK_sale_items_products_ProductId", x => x.ProductId, "products", "Id", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_sale_items_sales_SaleId", x => x.SaleId, "sales", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateIndex(name: "IX_categories_Name", table: "categories", column: "Name", unique: true);
        migrationBuilder.CreateIndex(name: "IX_customers_Segment", table: "customers", column: "Segment");
        migrationBuilder.CreateIndex(name: "IX_managers_IsActive_Team", table: "managers", columns: new[] { "IsActive", "Team" });
        migrationBuilder.CreateIndex(name: "IX_products_CategoryId_IsActive", table: "products", columns: new[] { "CategoryId", "IsActive" });
        migrationBuilder.CreateIndex(name: "IX_sales_CustomerId", table: "sales", column: "CustomerId");
        migrationBuilder.CreateIndex(name: "IX_sales_ManagerId_SoldAt_Status", table: "sales", columns: new[] { "ManagerId", "SoldAt", "Status" });
        migrationBuilder.CreateIndex(name: "IX_sales_SoldAt_Status", table: "sales", columns: new[] { "SoldAt", "Status" });
        migrationBuilder.CreateIndex(name: "IX_sale_items_ProductId", table: "sale_items", column: "ProductId");
        migrationBuilder.CreateIndex(name: "IX_sale_items_SaleId", table: "sale_items", column: "SaleId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "sale_items");
        migrationBuilder.DropTable(name: "products");
        migrationBuilder.DropTable(name: "sales");
        migrationBuilder.DropTable(name: "categories");
        migrationBuilder.DropTable(name: "customers");
        migrationBuilder.DropTable(name: "managers");
    }
}
