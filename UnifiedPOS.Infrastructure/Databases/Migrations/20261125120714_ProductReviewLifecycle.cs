using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifiedPOS.Infrastructure.Databases.Migrations
{
    /// <inheritdoc />
    public partial class ProductReviewLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PendingReview",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReviewReason",
                table: "Product",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedBy",
                table: "Product",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedOn",
                table: "Product",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedAtLocationId",
                table: "Product",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingReview",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ReviewReason",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ReviewedOn",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "SubmittedAtLocationId",
                table: "Product");
        }
    }
}
