using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABC_construction.Migrations
{
    /// <inheritdoc />
    public partial class AddGeorgianContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryKa",
                table: "Projects",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChallengesKa",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionKa",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DurationKa",
                table: "Projects",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialsUsedKa",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionKa",
                table: "Projects",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimelineKa",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleKa",
                table: "Projects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BiographyKa",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullNameKa",
                table: "Employees",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionKa",
                table: "Employees",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressKa",
                table: "CompanyInformation",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionKa",
                table: "CompanyInformation",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CompanyInformation",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AddressKa", "DescriptionKa" },
                values: new object[] { null, null });

            TranslateInitialContent(migrationBuilder);
        }

        /// <summary>
        /// Gives databases seeded before the Georgian site existed the same
        /// Georgian texts a new database gets from InitialContent. Each update
        /// matches on the untouched English seed text, so a record an admin has
        /// since edited is left alone. On a new database the tables are still
        /// empty at this point and these updates change nothing.
        /// </summary>
        private static void TranslateInitialContent(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Category",
                keyValue: Data.InitialContent.Category,
                column: "CategoryKa",
                value: Data.InitialContent.CategoryKa);

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "ShortDescription",
                keyValue: Data.InitialContent.ShortDescription,
                column: "ShortDescriptionKa",
                value: Data.InitialContent.ShortDescriptionKa);

            foreach (var (title, description, descriptionKa) in Data.InitialContent.ProjectRows)
            {
                migrationBuilder.UpdateData(
                    table: "Projects",
                    keyColumns: new[] { "Title", "Description" },
                    keyValues: new object[] { title, description },
                    column: "DescriptionKa",
                    value: descriptionKa);
            }

            foreach (var (position, positionKa) in Data.InitialContent.PositionsKa)
            {
                migrationBuilder.UpdateData(
                    table: "Employees",
                    keyColumn: "Position",
                    keyValue: position,
                    column: "PositionKa",
                    value: positionKa);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryKa",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ChallengesKa",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DescriptionKa",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DurationKa",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "MaterialsUsedKa",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ShortDescriptionKa",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TimelineKa",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TitleKa",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "BiographyKa",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "FullNameKa",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PositionKa",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AddressKa",
                table: "CompanyInformation");

            migrationBuilder.DropColumn(
                name: "DescriptionKa",
                table: "CompanyInformation");
        }
    }
}
