using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MariaDigital.Migrations
{
	/// <inheritdoc />
	public partial class Createstudentstable : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Students",
				columns: table => new
				{
					Id = table
						.Column<long>(type: "bigint", nullable: false)
						.Annotation(
							"Npgsql:ValueGenerationStrategy",
							NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
						),
					Name = table.Column<string>(type: "text", nullable: false),
					Level = table.Column<int>(type: "integer", nullable: false),
					DateOfBirth = table.Column<DateTime>(
						type: "timestamp with time zone",
						nullable: true
					),
					DateAndPlaceOfBaptism = table.Column<string>(type: "text", nullable: true),
					DateAndPlaceOfConfirmation = table.Column<string>(type: "text", nullable: true),
					DateAndPlaceOfFirstCommunion = table.Column<string>(
						type: "text",
						nullable: true
					),
					FathersName = table.Column<string>(type: "text", nullable: true),
					MothersName = table.Column<string>(type: "text", nullable: true),
					AreParentsMarriedByChurch = table.Column<bool>(type: "boolean", nullable: true),
					DoParentsLiveTogether = table.Column<bool>(type: "boolean", nullable: true),
					Address = table.Column<string>(type: "text", nullable: true),
					Cellphone = table.Column<string>(type: "text", nullable: true),
					Email = table.Column<string>(type: "text", nullable: true),
					GodparentName = table.Column<string>(type: "text", nullable: true),
					SchoolName = table.Column<string>(type: "text", nullable: true),
					SchoolGrade = table.Column<string>(type: "text", nullable: true),
					ExtracurricularActivities = table.Column<string>(type: "text", nullable: true),
					IllnessesOrAllergies = table.Column<string>(type: "text", nullable: true),
					MedicationTaken = table.Column<string>(type: "text", nullable: true),
					HealthInstitution = table.Column<int>(type: "integer", nullable: true),
					AuthorizedPickupPerson = table.Column<string>(type: "text", nullable: true),
					WishToServeInChoir = table.Column<bool>(type: "boolean", nullable: true),
					WishToServeAsAcolyte = table.Column<bool>(type: "boolean", nullable: true),
					FinancialContribution = table.Column<decimal>(type: "numeric", nullable: true),
					CatechistName = table.Column<string>(type: "text", nullable: true),
					CreatedAt = table.Column<DateTime>(
						type: "timestamp with time zone",
						nullable: false
					),
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Students", x => x.Id);
				}
			);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(name: "Students");
		}
	}
}
