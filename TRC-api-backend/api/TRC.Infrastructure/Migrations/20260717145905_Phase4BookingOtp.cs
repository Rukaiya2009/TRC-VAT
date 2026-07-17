using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase4BookingOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastVerifiedAt",
                table: "PhoneProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BufferMinutes",
                table: "ConsultationDays",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SessionMinutes",
                table: "ConsultationDays",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingLink",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SlotIndex",
                table: "Appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastVerifiedAt",
                table: "PhoneProfiles");

            migrationBuilder.DropColumn(
                name: "BufferMinutes",
                table: "ConsultationDays");

            migrationBuilder.DropColumn(
                name: "SessionMinutes",
                table: "ConsultationDays");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "MeetingLink",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "SlotIndex",
                table: "Appointments");
        }
    }
}
