using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InDuckTor.Account.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class TransactionCurrencyForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transaction_DepositOn_CurrencyCode",
                schema: "account",
                table: "Transaction",
                column: "DepositOn_CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_WithdrawFrom_CurrencyCode",
                schema: "account",
                table: "Transaction",
                column: "WithdrawFrom_CurrencyCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Currency_DepositOn_CurrencyCode",
                schema: "account",
                table: "Transaction",
                column: "DepositOn_CurrencyCode",
                principalSchema: "account",
                principalTable: "Currency",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Currency_WithdrawFrom_CurrencyCode",
                schema: "account",
                table: "Transaction",
                column: "WithdrawFrom_CurrencyCode",
                principalSchema: "account",
                principalTable: "Currency",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Currency_DepositOn_CurrencyCode",
                schema: "account",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Currency_WithdrawFrom_CurrencyCode",
                schema: "account",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_DepositOn_CurrencyCode",
                schema: "account",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_WithdrawFrom_CurrencyCode",
                schema: "account",
                table: "Transaction");
        }
    }
}
