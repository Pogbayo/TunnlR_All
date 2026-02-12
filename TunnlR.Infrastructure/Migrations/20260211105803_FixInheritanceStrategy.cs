using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TunnlR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixInheritanceStrategy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseEntity_BaseEntity_TunnelId",
                table: "BaseEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseEntity_BaseEntity_TunnelTraffic_TunnelId",
                table: "BaseEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseEntity_Users_UserId",
                table: "BaseEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BaseEntity",
                table: "BaseEntity");

            migrationBuilder.DropIndex(
                name: "IX_BaseEntity_TunnelId",
                table: "BaseEntity");

            migrationBuilder.DropIndex(
                name: "IX_BaseEntity_TunnelTraffic_TunnelId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "BytesReceived",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "BytesSent",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "ClientIp",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "ConnectedAt",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "DisconnectedAt",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "TunnelId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "TunnelTraffic_TunnelId",
                table: "BaseEntity");

            migrationBuilder.RenameTable(
                name: "BaseEntity",
                newName: "Tunnels");

            migrationBuilder.RenameIndex(
                name: "IX_BaseEntity_UserId",
                table: "Tunnels",
                newName: "IX_Tunnels_UserId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Tunnels",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Tunnels",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "Tunnels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PublicUrl",
                table: "Tunnels",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(70)",
                oldMaxLength: 70,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Protocol",
                table: "Tunnels",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LocalPort",
                table: "Tunnels",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DashboardUrl",
                table: "Tunnels",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(70)",
                oldMaxLength: 70,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tunnels",
                table: "Tunnels",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TunnelConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TunnelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    ConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisconnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BytesSent = table.Column<long>(type: "bigint", nullable: false),
                    BytesReceived = table.Column<long>(type: "bigint", nullable: false),
                    IsCreated = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TunnelConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TunnelConnections_Tunnels_TunnelId",
                        column: x => x.TunnelId,
                        principalTable: "Tunnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TunnelLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TunnelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsCreated = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TunnelLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TunnelLogs_Tunnels_TunnelId",
                        column: x => x.TunnelId,
                        principalTable: "Tunnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TunnelConnections_TunnelId",
                table: "TunnelConnections",
                column: "TunnelId");

            migrationBuilder.CreateIndex(
                name: "IX_TunnelLogs_TunnelId",
                table: "TunnelLogs",
                column: "TunnelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tunnels_Users_UserId",
                table: "Tunnels",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tunnels_Users_UserId",
                table: "Tunnels");

            migrationBuilder.DropTable(
                name: "TunnelConnections");

            migrationBuilder.DropTable(
                name: "TunnelLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tunnels",
                table: "Tunnels");

            migrationBuilder.RenameTable(
                name: "Tunnels",
                newName: "BaseEntity");

            migrationBuilder.RenameIndex(
                name: "IX_Tunnels_UserId",
                table: "BaseEntity",
                newName: "IX_BaseEntity_UserId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "BaseEntity",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "BaseEntity",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "BaseEntity",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "PublicUrl",
                table: "BaseEntity",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(70)",
                oldMaxLength: 70);

            migrationBuilder.AlterColumn<string>(
                name: "Protocol",
                table: "BaseEntity",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<int>(
                name: "LocalPort",
                table: "BaseEntity",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DashboardUrl",
                table: "BaseEntity",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(70)",
                oldMaxLength: 70);

            migrationBuilder.AddColumn<long>(
                name: "BytesReceived",
                table: "BaseEntity",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BytesSent",
                table: "BaseEntity",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientIp",
                table: "BaseEntity",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConnectedAt",
                table: "BaseEntity",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisconnectedAt",
                table: "BaseEntity",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "BaseEntity",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "BaseEntity",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "BaseEntity",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "BaseEntity",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TunnelId",
                table: "BaseEntity",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TunnelTraffic_TunnelId",
                table: "BaseEntity",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BaseEntity",
                table: "BaseEntity",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_TunnelId",
                table: "BaseEntity",
                column: "TunnelId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_TunnelTraffic_TunnelId",
                table: "BaseEntity",
                column: "TunnelTraffic_TunnelId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEntity_BaseEntity_TunnelId",
                table: "BaseEntity",
                column: "TunnelId",
                principalTable: "BaseEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEntity_BaseEntity_TunnelTraffic_TunnelId",
                table: "BaseEntity",
                column: "TunnelTraffic_TunnelId",
                principalTable: "BaseEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEntity_Users_UserId",
                table: "BaseEntity",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
