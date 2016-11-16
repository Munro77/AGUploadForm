using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AGUploadForm.Data.Migrations
{
    public partial class latest_update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Job",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountNumber = table.Column<string>(nullable: true),
                    ContactAddress = table.Column<string>(nullable: true),
                    ContactAddressCity = table.Column<string>(nullable: true),
                    ContactAddressPostalCode = table.Column<string>(nullable: true),
                    ContactAddressProvince = table.Column<string>(nullable: true),
                    ContactAddressUnitNumber = table.Column<string>(nullable: true),
                    ContactCompanyName = table.Column<string>(nullable: true),
                    ContactEmail = table.Column<string>(nullable: true),
                    ContactName = table.Column<string>(nullable: true),
                    ContactPhoneNumber = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    DepartmentName = table.Column<string>(nullable: true),
                    DueDateTime = table.Column<string>(nullable: true),
                    Instructions = table.Column<string>(nullable: true),
                    OfficeName = table.Column<string>(nullable: true),
                    ProjectNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Job", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Job");
        }
    }
}
