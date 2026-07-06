using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseriesNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedAvailabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailablePlaces",
                table: "Nurseries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "Bookings",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailablePlaces",
                table: "Nurseries");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Bookings");
        }
    }
}
