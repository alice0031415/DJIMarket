using System;
using DJIMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DJIMarket.Infrastructure.Data.Migrations;

[DbContext(typeof(DJIMarket.Infrastructure.Data.AppDbContext))]
[Migration("202609240001_InitialCreate")]
partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "10.0.12");
        modelBuilder.Entity("DJIMarket.Domain.Entities.Category", b => { b.Property<Guid>("Id").HasColumnType("uuid"); b.Property<string>("Name").IsRequired().HasMaxLength(120).HasColumnType("character varying(120)"); b.HasKey("Id"); b.HasIndex("Name").IsUnique(); b.ToTable("categories"); });
        modelBuilder.Entity("DJIMarket.Domain.Entities.Customer", b => { b.Property<Guid>("Id").HasColumnType("uuid"); b.Property<string>("Company").IsRequired().HasMaxLength(160).HasColumnType("character varying(160)"); b.Property<string>("Name").IsRequired().HasMaxLength(120).HasColumnType("character varying(120)"); b.Property<string>("Segment").IsRequired().HasMaxLength(40).HasColumnType("character varying(40)"); b.HasKey("Id"); b.HasIndex("Segment"); b.ToTable("customers"); });
        modelBuilder.Entity("DJIMarket.Domain.Entities.Manager", b => { b.Property<Guid>("Id").HasColumnType("uuid"); b.Property<string>("AvatarSeed").IsRequired().HasMaxLength(80).HasColumnType("character varying(80)"); b.Property<bool>("IsActive").HasColumnType("boolean"); b.Property<string>("Name").IsRequired().HasMaxLength(120).HasColumnType("character varying(120)"); b.Property<string>("Position").IsRequired().HasMaxLength(80).HasColumnType("character varying(80)"); b.Property<string>("Team").IsRequired().HasMaxLength(80).HasColumnType("character varying(80)"); b.HasKey("Id"); b.HasIndex("IsActive", "Team"); b.ToTable("managers"); });
        modelBuilder.Entity("DJIMarket.Domain.Entities.Product", b => { b.Property<Guid>("Id").HasColumnType("uuid"); b.Property<Guid>("CategoryId").HasColumnType("uuid"); b.Property<bool>("IsActive").HasColumnType("boolean"); b.Property<string>("Name").IsRequired().HasMaxLength(160).HasColumnType("character varying(160)"); b.HasKey("Id"); b.HasIndex("CategoryId", "IsActive"); b.ToTable("products"); });
        modelBuilder.Entity("DJIMarket.Domain.Entities.Sale", b => { b.Property<Guid>("Id").HasColumnType("uuid"); b.Property<Guid>("CustomerId").HasColumnType("uuid"); b.Property<Guid>("ManagerId").HasColumnType("uuid"); b.Property<DateTimeOffset>("SoldAt").HasColumnType("timestamp with time zone"); b.Property<string>("Status").IsRequired().HasMaxLength(20).HasColumnType("character varying(20)"); b.HasKey("Id"); b.HasIndex("CustomerId"); b.HasIndex("ManagerId", "SoldAt", "Status"); b.HasIndex("SoldAt", "Status"); b.ToTable("sales"); });
        modelBuilder.Entity("DJIMarket.Domain.Entities.SaleItem", b => { b.Property<Guid>("Id").HasColumnType("uuid"); b.Property<decimal>("CostPrice").HasPrecision(18, 2).HasColumnType("numeric(18,2)"); b.Property<Guid>("ProductId").HasColumnType("uuid"); b.Property<int>("Quantity").HasColumnType("integer"); b.Property<Guid>("SaleId").HasColumnType("uuid"); b.Property<decimal>("SalePrice").HasPrecision(18, 2).HasColumnType("numeric(18,2)"); b.HasKey("Id"); b.HasIndex("ProductId"); b.HasIndex("SaleId"); b.ToTable("sale_items"); });
        modelBuilder.Entity("DJIMarket.Domain.Entities.Product", b => b.HasOne("DJIMarket.Domain.Entities.Category", "Category").WithMany("Products").HasForeignKey("CategoryId").OnDelete(DeleteBehavior.Restrict).IsRequired());
        modelBuilder.Entity("DJIMarket.Domain.Entities.Sale", b => { b.HasOne("DJIMarket.Domain.Entities.Customer", "Customer").WithMany("Sales").HasForeignKey("CustomerId").OnDelete(DeleteBehavior.Restrict).IsRequired(); b.HasOne("DJIMarket.Domain.Entities.Manager", "Manager").WithMany("Sales").HasForeignKey("ManagerId").OnDelete(DeleteBehavior.Restrict).IsRequired(); });
        modelBuilder.Entity("DJIMarket.Domain.Entities.SaleItem", b => { b.HasOne("DJIMarket.Domain.Entities.Product", "Product").WithMany("SaleItems").HasForeignKey("ProductId").OnDelete(DeleteBehavior.Restrict).IsRequired(); b.HasOne("DJIMarket.Domain.Entities.Sale", "Sale").WithMany("Items").HasForeignKey("SaleId").OnDelete(DeleteBehavior.Cascade).IsRequired(); });
#pragma warning restore 612, 618
    }
}
