using System.Collections.Generic;
using InDuckTor.Account.Domain;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InDuckTor.Account.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class NumericBankCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropPrimaryKey("PK_BankInfo", "BankInfo", "account");
            migrationBuilder.Sql("""ALTER TABLE account."BankInfo" DROP CONSTRAINT "PK_BankInfo" CASCADE;""");
            MigrateColumn("BankInfo", "BankCode", addForingKey: false);
            migrationBuilder.AddPrimaryKey("PK_BankInfo", "BankInfo", "BankCode", "account");
            
            MigrateColumn("Transaction", "WithdrawFrom_BankCode");

            MigrateColumn("Transaction", "DepositOn_BankCode");

            MigrateColumn("Account", "BankCode", nullable: false);
           
            
            migrationBuilder.DropForeignKey(
                name: "FK_FundsReservation_Transaction_TransactionId",
                schema: "account",
                table: "FundsReservation");

            migrationBuilder.AlterColumn<List<GrantedAccountUser>>(
                name: "GrantedUsers",
                schema: "account",
                table: "Account",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FundsReservation_Transaction_TransactionId",
                schema: "account",
                table: "FundsReservation",
                column: "TransactionId",
                principalSchema: "account",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            void MigrateColumn(string tableName, string colunmName,bool nullable = true, bool addForingKey = true)
            {
                migrationBuilder.AddColumn<int>($"{colunmName}_Temp", tableName, schema: "account", nullable: true);
                migrationBuilder.Sql($"""UPDATE account."{tableName}" SET "{colunmName}_Temp" = "{colunmName}"::integer; """);
                migrationBuilder.DropColumn(colunmName, tableName, schema: "account");
                migrationBuilder.RenameColumn($"{colunmName}_Temp", tableName, colunmName, schema: "account");
                
                if (!nullable)
                    migrationBuilder.AlterColumn<int>(colunmName, tableName, schema: "account", nullable: false, oldNullable: true);
                
                if (addForingKey)
                {
                    migrationBuilder.AddForeignKey($"FK_{tableName}_BankInfo_{colunmName}", tableName, colunmName, "BankInfo", schema: "account", principalSchema: "account");
                    migrationBuilder.CreateIndex($"IX_{tableName}_{colunmName}", tableName, colunmName, schema: "account");
                }
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GrantedUsers",
                schema: "account",
                table: "Account",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(List<GrantedAccountUser>),
                oldType: "jsonb");

            migrationBuilder.DropForeignKey(
                name: "FK_FundsReservation_Transaction_TransactionId",
                schema: "account",
                table: "FundsReservation");
            
            migrationBuilder.AddForeignKey(
                name: "FK_FundsReservation_Transaction_TransactionId",
                schema: "account",
                table: "FundsReservation",
                column: "TransactionId",
                principalSchema: "account",
                principalTable: "Transaction",
                principalColumn: "Id");
        }
    }
}
