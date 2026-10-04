using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSampleRosterSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Admins]
                WHERE (([Id] = 1 AND [StaffId] = 'ADM-001' AND [Email] = 'ama.serwaa@university.edu')
                    OR ([Id] = 2 AND [StaffId] = 'ADM-002' AND [Email] = 'kwame.mensah@university.edu'))
                    AND NOT EXISTS (SELECT 1 FROM [Complaints] WHERE [AssignedToId] = [Admins].[Id])
                    AND NOT EXISTS (SELECT 1 FROM [Notifications] WHERE [UserId] = CONCAT('Admin-', [Admins].[Id]));

                DELETE FROM [Students]
                WHERE (([Id] = 1 AND [IndexNumber] = '10982341' AND [Email] = 'collins@student.university.edu')
                    OR ([Id] = 2 AND [IndexNumber] = '10982342' AND [Email] = 'jessica@student.university.edu'))
                    AND NOT EXISTS (SELECT 1 FROM [Complaints] WHERE [StudentId] = [Students].[Id])
                    AND NOT EXISTS (SELECT 1 FROM [Notifications] WHERE [UserId] = CONVERT(nvarchar(450), [Students].[Id]));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
