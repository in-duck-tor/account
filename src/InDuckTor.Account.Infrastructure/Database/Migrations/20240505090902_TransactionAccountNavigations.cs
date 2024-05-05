using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InDuckTor.Account.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class TransactionAccountNavigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Допускаем неизвестные номера счётов, расценивая как внешние 
            // migrationBuilder.AddForeignKey(
            //     name: "FK_Transaction_Account_DepositOn_AccountNumber",
            //     schema: "account",
            //     table: "Transaction",
            //     column: "DepositOn_AccountNumber",
            //     principalSchema: "account",
            //     principalTable: "Account",
            //     principalColumn: "Number");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_Transaction_Account_WithdrawFrom_AccountNumber",
            //     schema: "account",
            //     table: "Transaction",
            //     column: "WithdrawFrom_AccountNumber",
            //     principalSchema: "account",
            //     principalTable: "Account",
            //     principalColumn: "Number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_Transaction_Account_DepositOn_AccountNumber",
            //     schema: "account",
            //     table: "Transaction");
            //
            // migrationBuilder.DropForeignKey(
            //     name: "FK_Transaction_Account_WithdrawFrom_AccountNumber",
            //     schema: "account",
            //     table: "Transaction");
        }
    }
}
