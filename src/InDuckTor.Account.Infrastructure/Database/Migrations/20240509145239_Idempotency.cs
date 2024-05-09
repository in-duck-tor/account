using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InDuckTor.Account.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Idempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdempotencyRecord",
                schema: "account",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestMatch = table.Column<string>(type: "text", nullable: false),
                    CachedResponse_ResponseCode = table.Column<int>(type: "integer", nullable: true),
                    CachedResponse_Headers = table.Column<KeyValuePair<string, string>[]>(type: "jsonb", nullable: true),
                    CachedResponse_ResponseBody = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyRecord", x => x.Key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdempotencyRecord",
                schema: "account");
        }
    }
}
