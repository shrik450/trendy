﻿// <auto-generated />
namespace Trendy.Migrations

open System
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Infrastructure
open Microsoft.EntityFrameworkCore.Metadata
open Microsoft.EntityFrameworkCore.Migrations
open Microsoft.EntityFrameworkCore.Storage.ValueConversion
open Trendy.Contexts

[<DbContext(typeof<LinksContext.LinksContext>)>]
type LinksContextModelSnapshot() =
    inherit ModelSnapshot()

    override this.BuildModel(modelBuilder: ModelBuilder) =
        modelBuilder
            .HasAnnotation("ProductVersion", "5.0.7")
            |> ignore

        modelBuilder.Entity("Trendy.Models.Link+Link", (fun b ->

            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("INTEGER") |> ignore
            b.Property<string>("Notes")
                .HasColumnType("TEXT") |> ignore
            b.Property<string>("Url")
                .HasColumnType("TEXT") |> ignore

            b.HasKey("Id") |> ignore

            b.ToTable("Links") |> ignore

        )) |> ignore

        modelBuilder.Entity("Trendy.Models.User+User", (fun b ->

            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("INTEGER") |> ignore
            b.Property<int>("AccessFailedCount")
                .IsRequired()
                .HasColumnType("INTEGER") |> ignore
            b.Property<string>("ConcurrencyStamp")
                .HasColumnType("TEXT") |> ignore
            b.Property<string>("Email")
                .HasColumnType("TEXT") |> ignore
            b.Property<bool>("EmailConfirmed")
                .IsRequired()
                .HasColumnType("INTEGER") |> ignore
            b.Property<bool>("LockoutEnabled")
                .IsRequired()
                .HasColumnType("INTEGER") |> ignore
            b.Property<Nullable<DateTimeOffset>>("LockoutEnd")
                .HasColumnType("TEXT") |> ignore
            b.Property<string>("NormalizedEmail")
                .HasColumnType("TEXT") |> ignore
            b.Property<string>("NormalizedUserName")
                .HasColumnType("TEXT") |> ignore
            b.Property<string>("PasswordHash")
                .HasColumnType("TEXT") |> ignore
            b.Property<string>("PhoneNumber")
                .HasColumnType("TEXT") |> ignore
            b.Property<bool>("PhoneNumberConfirmed")
                .IsRequired()
                .HasColumnType("INTEGER") |> ignore
            b.Property<string>("SecurityStamp")
                .HasColumnType("TEXT") |> ignore
            b.Property<bool>("TwoFactorEnabled")
                .IsRequired()
                .HasColumnType("INTEGER") |> ignore
            b.Property<string>("UserName")
                .HasColumnType("TEXT") |> ignore

            b.HasKey("Id") |> ignore

            b.ToTable("Users") |> ignore

        )) |> ignore

