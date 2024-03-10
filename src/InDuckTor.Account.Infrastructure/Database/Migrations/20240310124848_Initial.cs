using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InDuckTor.Account.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "account");

            migrationBuilder.CreateSequence(
                name: "account_personal_code_seq",
                schema: "account");

            migrationBuilder.CreateTable(
                name: "BankInfo",
                schema: "account",
                columns: table => new
                {
                    BankCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankInfo", x => x.BankCode);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                schema: "account",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    NumericCode = table.Column<int>(type: "integer", nullable: false),
                    RateToRuble = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                schema: "account",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AutoCloseAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DepositOn_Amount = table.Column<decimal>(type: "numeric", nullable: true),
                    DepositOn_AccountNumber = table.Column<string>(type: "text", nullable: true),
                    DepositOn_CurrencyCode = table.Column<string>(type: "text", nullable: true),
                    DepositOn_BankCode = table.Column<string>(type: "text", nullable: true),
                    WithdrawFrom_Amount = table.Column<decimal>(type: "numeric", nullable: true),
                    WithdrawFrom_AccountNumber = table.Column<string>(type: "text", nullable: true),
                    WithdrawFrom_CurrencyCode = table.Column<string>(type: "text", nullable: true),
                    WithdrawFrom_BankCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_BankInfo_DepositOn_BankCode",
                        column: x => x.DepositOn_BankCode,
                        principalSchema: "account",
                        principalTable: "BankInfo",
                        principalColumn: "BankCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transaction_BankInfo_WithdrawFrom_BankCode",
                        column: x => x.WithdrawFrom_BankCode,
                        principalSchema: "account",
                        principalTable: "BankInfo",
                        principalColumn: "BankCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Account",
                schema: "account",
                columns: table => new
                {
                    Number = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BankCode = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    CustomComment = table.Column<string>(type: "text", nullable: true),
                    GrantedUsers = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.Number);
                    table.ForeignKey(
                        name: "FK_Account_BankInfo_BankCode",
                        column: x => x.BankCode,
                        principalSchema: "account",
                        principalTable: "BankInfo",
                        principalColumn: "BankCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Account_Currency_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalSchema: "account",
                        principalTable: "Currency",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundsReservation",
                schema: "account",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    AccountNumber = table.Column<string>(type: "text", nullable: false),
                    TransactionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundsReservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundsReservation_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "account",
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_BankCode",
                schema: "account",
                table: "Account",
                column: "BankCode");

            migrationBuilder.CreateIndex(
                name: "IX_Account_CurrencyCode",
                schema: "account",
                table: "Account",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Currency_NumericCode",
                schema: "account",
                table: "Currency",
                column: "NumericCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundsReservation_TransactionId",
                schema: "account",
                table: "FundsReservation",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_DepositOn_AccountNumber",
                schema: "account",
                table: "Transaction",
                column: "DepositOn_AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_DepositOn_BankCode",
                schema: "account",
                table: "Transaction",
                column: "DepositOn_BankCode");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_WithdrawFrom_AccountNumber",
                schema: "account",
                table: "Transaction",
                column: "WithdrawFrom_AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_WithdrawFrom_BankCode",
                schema: "account",
                table: "Transaction",
                column: "WithdrawFrom_BankCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Account",
                schema: "account");

            migrationBuilder.DropTable(
                name: "FundsReservation",
                schema: "account");

            migrationBuilder.DropTable(
                name: "Currency",
                schema: "account");

            migrationBuilder.DropTable(
                name: "Transaction",
                schema: "account");

            migrationBuilder.DropTable(
                name: "BankInfo",
                schema: "account");

            migrationBuilder.DropSequence(
                name: "account_personal_code_seq",
                schema: "account");
        }
    }
}
