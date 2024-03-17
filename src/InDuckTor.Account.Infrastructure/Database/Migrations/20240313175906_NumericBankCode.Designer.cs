﻿// <auto-generated />
using System;
using System.Collections.Generic;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InDuckTor.Account.Infrastructure.Database.Migrations
{
    [DbContext(typeof(AccountsDbContext))]
    [Migration("20240313175906_NumericBankCode")]
    partial class NumericBankCode
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("account")
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.HasSequence("account_personal_code_seq");

            modelBuilder.Entity("InDuckTor.Account.Domain.Account", b =>
                {
                    b.Property<string>("Number")
                        .HasColumnType("text");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<int>("BankCode")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("CreatedBy")
                        .HasColumnType("integer");

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomComment")
                        .HasColumnType("text");

                    b.Property<List<GrantedAccountUser>>("GrantedUsers")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("OwnerId")
                        .HasColumnType("integer");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Number");

                    b.HasIndex("BankCode");

                    b.HasIndex("CurrencyCode");

                    b.ToTable("Account", "account");
                });

            modelBuilder.Entity("InDuckTor.Account.Domain.BankInfo", b =>
                {
                    b.Property<int>("BankCode")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("BankCode");

                    b.ToTable("BankInfo", "account");
                });

            modelBuilder.Entity("InDuckTor.Account.Domain.Currency", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<int>("NumericCode")
                        .HasColumnType("integer");

                    b.Property<decimal>("RateToRuble")
                        .HasColumnType("numeric");

                    b.HasKey("Code");

                    b.HasIndex("NumericCode")
                        .IsUnique();

                    b.ToTable("Currency", "account");
                });

            modelBuilder.Entity("InDuckTor.Account.Domain.FundsReservation", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<long?>("TransactionId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TransactionId");

                    b.ToTable("FundsReservation", "account");
                });

            modelBuilder.Entity("InDuckTor.Account.Domain.Transaction", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("AutoCloseAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("FinishedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("InitiatedBy")
                        .HasColumnType("integer");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Transaction", "account");
                });

            modelBuilder.Entity("InDuckTor.Account.Domain.Account", b =>
                {
                    b.HasOne("InDuckTor.Account.Domain.BankInfo", "BankInfo")
                        .WithMany()
                        .HasForeignKey("BankCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("InDuckTor.Account.Domain.Currency", "Currency")
                        .WithMany()
                        .HasForeignKey("CurrencyCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BankInfo");

                    b.Navigation("Currency");
                });

            modelBuilder.Entity("InDuckTor.Account.Domain.FundsReservation", b =>
                {
                    b.HasOne("InDuckTor.Account.Domain.Transaction", null)
                        .WithMany("Reservations")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("InDuckTor.Account.Domain.Transaction", b =>
                {
                    b.OwnsOne("InDuckTor.Account.Domain.TransactionTarget", "DepositOn", b1 =>
                        {
                            b1.Property<long>("TransactionId")
                                .HasColumnType("bigint");

                            b1.Property<string>("AccountNumber")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<decimal>("Amount")
                                .HasColumnType("numeric");

                            b1.Property<int>("BankCode")
                                .HasColumnType("integer");

                            b1.Property<string>("CurrencyCode")
                                .HasColumnType("text");

                            b1.HasKey("TransactionId");

                            b1.HasIndex("AccountNumber");

                            b1.HasIndex("BankCode");

                            b1.ToTable("Transaction", "account");

                            b1.HasOne("InDuckTor.Account.Domain.BankInfo", "BankInfo")
                                .WithMany()
                                .HasForeignKey("BankCode")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.WithOwner()
                                .HasForeignKey("TransactionId");

                            b1.Navigation("BankInfo");
                        });

                    b.OwnsOne("InDuckTor.Account.Domain.TransactionTarget", "WithdrawFrom", b1 =>
                        {
                            b1.Property<long>("TransactionId")
                                .HasColumnType("bigint");

                            b1.Property<string>("AccountNumber")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<decimal>("Amount")
                                .HasColumnType("numeric");

                            b1.Property<int>("BankCode")
                                .HasColumnType("integer");

                            b1.Property<string>("CurrencyCode")
                                .HasColumnType("text");

                            b1.HasKey("TransactionId");

                            b1.HasIndex("AccountNumber");

                            b1.HasIndex("BankCode");

                            b1.ToTable("Transaction", "account");

                            b1.HasOne("InDuckTor.Account.Domain.BankInfo", "BankInfo")
                                .WithMany()
                                .HasForeignKey("BankCode")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.WithOwner()
                                .HasForeignKey("TransactionId");

                            b1.Navigation("BankInfo");
                        });

                    b.Navigation("DepositOn");

                    b.Navigation("WithdrawFrom");
                });

            modelBuilder.Entity("InDuckTor.Account.Domain.Transaction", b =>
                {
                    b.Navigation("Reservations");
                });
#pragma warning restore 612, 618
        }
    }
}
