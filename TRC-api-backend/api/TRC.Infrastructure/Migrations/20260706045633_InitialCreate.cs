using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TRC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessPeriodData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PhoneProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    TotalSales = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPurchases = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VatDeclared = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessPeriodData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Commodities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HSCode = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    BenchmarkPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commodities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    WindowStart = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    WindowEnd = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    BookingCutoff = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    MaxBookings = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    TimesAssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationDays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Recipient = table.Column<string>(type: "text", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    TemplateKey = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ProviderMessageId = table.Column<string>(type: "text", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhoneProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PreferredLanguage = table.Column<int>(type: "integer", nullable: false),
                    MissedCount = table.Column<int>(type: "integer", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    BlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    ThresholdJson = table.Column<string>(type: "text", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    PhaseTag = table.Column<int>(type: "integer", nullable: false),
                    LastReviewed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Rationale = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    PreferredLanguage = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefreshTokenHash = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationDayId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingOrder = table.Column<int>(type: "integer", nullable: false),
                    AssignedStart = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    AssignedEnd = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_ConsultationDays_ConsultationDayId",
                        column: x => x.ConsultationDayId,
                        principalTable: "ConsultationDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_PhoneProfiles_PhoneProfileId",
                        column: x => x.PhoneProfileId,
                        principalTable: "PhoneProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtpCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpCodes_PhoneProfiles_PhoneProfileId",
                        column: x => x.PhoneProfileId,
                        principalTable: "PhoneProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuickChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BoeValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VariancePct = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RiskBand = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuickChecks_PhoneProfiles_PhoneProfileId",
                        column: x => x.PhoneProfileId,
                        principalTable: "PhoneProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Details = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Imports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Consignee = table.Column<string>(type: "text", nullable: false),
                    HSCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    InvoiceValueUsd = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CfrValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OtherCosts = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AssessableValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HasCommercialInvoice = table.Column<bool>(type: "boolean", nullable: false),
                    HasBillOfLading = table.Column<bool>(type: "boolean", nullable: false),
                    HasMushak = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Imports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RiskAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportId = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskScore = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    TriggeredRules = table.Column<string>(type: "jsonb", nullable: false),
                    NotEvaluable = table.Column<string>(type: "jsonb", nullable: false),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssessedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAssessments_Imports_ImportId",
                        column: x => x.ImportId,
                        principalTable: "Imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxBreakdowns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxType = table.Column<int>(type: "integer", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxBreakdowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxBreakdowns_Imports_ImportId",
                        column: x => x.ImportId,
                        principalTable: "Imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RiskRules",
                columns: new[] { "Id", "Code", "CreatedAt", "Enabled", "LastReviewed", "Name", "PhaseTag", "Rationale", "ThresholdJson", "Weight" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "R1", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Declared VAT > 10% below computed VAT", 1, "Approved default (TRC-RSK-002 Rev 1.1).", null, 15 },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "R2", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Return vs financial statements differ > 15%", 2, "Approved default (TRC-RSK-002 Rev 1.1).", null, 20 },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "R3", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Supplier on watchlist", 2, "Approved default (TRC-RSK-002 Rev 1.1).", null, 25 },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "R4", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Unit value < 90% of commodity benchmark", 2, "Approved default (TRC-RSK-002 Rev 1.1).", null, 20 },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "R5", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), "AV deviates > 50% from 12-month rolling average", 1, "Approved default (TRC-RSK-002 Rev 1.1).", null, 15 },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "R6", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Sales below purchases by > 20%", 2, "Approved default (TRC-RSK-002 Rev 1.1).", null, 10 },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "R7", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), "HS code inconsistent with description/history", 1, "Approved default (TRC-RSK-002 Rev 1.1).", null, 15 },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "R8", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), ">=3 of last 6 declarations below benchmark", 2, "Approved default (TRC-RSK-002 Rev 1.1).", null, 20 },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "R9", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Large turnover, no audit in 3 years", 2, "Approved default (TRC-RSK-002 Rev 1.1).", null, 10 },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "R10", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Round-number / repeated invoice patterns", 1, "Approved default (TRC-RSK-002 Rev 1.1).", null, 8 },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "R11", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Required document missing/inconsistent", 1, "Approved default (TRC-RSK-002 Rev 1.1).", null, 12 },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "R12", new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), "External intelligence hit", 2, "Approved default (TRC-RSK-002 Rev 1.1).", null, 30 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ConsultationDayId",
                table: "Appointments",
                column: "ConsultationDayId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PhoneProfileId",
                table: "Appointments",
                column: "PhoneProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPeriodData_UserId_PhoneProfileId_Year_Month",
                table: "BusinessPeriodData",
                columns: new[] { "UserId", "PhoneProfileId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Commodities_HSCode",
                table: "Commodities",
                column: "HSCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationDays_Date",
                table: "ConsultationDays",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Imports_Date",
                table: "Imports",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Imports_HSCode",
                table: "Imports",
                column: "HSCode");

            migrationBuilder.CreateIndex(
                name: "IX_Imports_UserId",
                table: "Imports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCodes_PhoneProfileId",
                table: "OtpCodes",
                column: "PhoneProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneProfiles_PhoneNumber",
                table: "PhoneProfiles",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuickChecks_PhoneProfileId",
                table: "QuickChecks",
                column: "PhoneProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_ImportId",
                table: "RiskAssessments",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskRules_Code",
                table: "RiskRules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxBreakdowns_ImportId",
                table: "TaxBreakdowns",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BusinessPeriodData");

            migrationBuilder.DropTable(
                name: "Commodities");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropTable(
                name: "OtpCodes");

            migrationBuilder.DropTable(
                name: "QuickChecks");

            migrationBuilder.DropTable(
                name: "RiskAssessments");

            migrationBuilder.DropTable(
                name: "RiskRules");

            migrationBuilder.DropTable(
                name: "TaxBreakdowns");

            migrationBuilder.DropTable(
                name: "ConsultationDays");

            migrationBuilder.DropTable(
                name: "PhoneProfiles");

            migrationBuilder.DropTable(
                name: "Imports");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
