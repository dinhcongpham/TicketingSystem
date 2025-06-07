using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventService.Migrations
{
    /// <inheritdoc />
    public partial class SeedRealEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    NumSeat = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "Description", "EndTime", "Name", "StartTime", "VenueId" },
                values: new object[,]
                {
                    { new Guid("0d1e2f3a-4b5c-6d7e-8f9a-0b1c2d3e4f5a"), "Consumer Electronics Show", new DateTime(2025, 1, 10, 18, 0, 0, 0, DateTimeKind.Utc), "CES 2025", new DateTime(2025, 1, 7, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("6f5e4d3c-2b1a-09f8-e7d6-c5b4a3f2e1d0") },
                    { new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), "Live music concert by Coldplay", new DateTime(2025, 7, 15, 23, 0, 0, 0, DateTimeKind.Utc), "Coldplay World Tour", new DateTime(2025, 7, 15, 20, 0, 0, 0, DateTimeKind.Utc), new Guid("d6c5b4a3-f2e1-a0b9-c8d7-e6f5a4b3c2d1") },
                    { new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"), "Google's annual developer conference", new DateTime(2025, 5, 14, 17, 0, 0, 0, DateTimeKind.Utc), "Google I/O 2025", new DateTime(2025, 5, 14, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("e7d6c5b4-a3f2-e1d0-c9b8-a7f6e5d4c3b2") },
                    { new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), "Annual fan convention for comics and pop culture", new DateTime(2025, 7, 21, 18, 0, 0, 0, DateTimeKind.Utc), "Comic-Con International", new DateTime(2025, 7, 18, 9, 0, 0, 0, DateTimeKind.Utc), new Guid("f8e7d6c5-b4a3-f2e1-d0c9-b8a7f6e5d4c3") },
                    { new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), "Final game of the NBA Championship", new DateTime(2025, 6, 20, 23, 30, 0, 0, DateTimeKind.Utc), "NBA Finals Game 7", new DateTime(2025, 6, 20, 20, 30, 0, 0, DateTimeKind.Utc), new Guid("09f8e7d6-c5b4-a3f2-e1d0-c9b8a7f6e5d4") },
                    { new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"), "Technology, Entertainment, and Design conference", new DateTime(2025, 4, 10, 17, 0, 0, 0, DateTimeKind.Utc), "TED Global 2025", new DateTime(2025, 4, 10, 9, 0, 0, 0, DateTimeKind.Utc), new Guid("1a09f8e7-d6c5-b4a3-f2e1-d0c9b8a7f6e5") },
                    { new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"), "Electronic dance music festival", new DateTime(2025, 7, 28, 2, 0, 0, 0, DateTimeKind.Utc), "Tomorrowland", new DateTime(2025, 7, 26, 16, 0, 0, 0, DateTimeKind.Utc), new Guid("2b1a09f8-e7d6-c5b4-a3f2-e1d0c9b8a7f6") },
                    { new Guid("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"), "Apple Worldwide Developers Conference", new DateTime(2025, 6, 3, 17, 0, 0, 0, DateTimeKind.Utc), "Apple WWDC 2025", new DateTime(2025, 6, 3, 9, 0, 0, 0, DateTimeKind.Utc), new Guid("3c2b1a09-f8e7-d6c5-b4a3-f2e1d0c9b8a7") },
                    { new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"), "Kickoff for the Summer Olympics", new DateTime(2025, 7, 24, 23, 0, 0, 0, DateTimeKind.Utc), "Olympic Games Opening Ceremony", new DateTime(2025, 7, 24, 20, 0, 0, 0, DateTimeKind.Utc), new Guid("4d3c2b1a-09f8-e7d6-c5b4-a3f2e1d0c9b8") },
                    { new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"), "Final match of the FIFA World Cup", new DateTime(2025, 12, 18, 21, 0, 0, 0, DateTimeKind.Utc), "FIFA World Cup Final", new DateTime(2025, 12, 18, 18, 0, 0, 0, DateTimeKind.Utc), new Guid("5e4d3c2b-1a09-f8e7-d6c5-b4a3f2e1d0c9") }
                });

            migrationBuilder.InsertData(
                table: "Venues",
                columns: new[] { "Id", "EventId", "Location", "Name", "NumSeat" },
                values: new object[,]
                {
                    { new Guid("09f8e7d6-c5b4-a3f2-e1d0-c9b8a7f6e5d4"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), "Boston, MA", "TD Garden", 60 },
                    { new Guid("1a09f8e7-d6c5-b4a3-f2e1-d0c9b8a7f6e5"), new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"), "Vancouver, Canada", "Vancouver Convention Centre", 70 },
                    { new Guid("2b1a09f8-e7d6-c5b4-a3f2-e1d0c9b8a7f6"), new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"), "Boom, Belgium", "Boom Recreation Area", 50 },
                    { new Guid("3c2b1a09-f8e7-d6c5-b4a3-f2e1d0c9b8a7"), new Guid("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"), "Cupertino, CA", "Apple Park", 80 },
                    { new Guid("4d3c2b1a-09f8-e7d6-c5b4-a3f2e1d0c9b8"), new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"), "Paris, France", "Stade de France", 10 },
                    { new Guid("5e4d3c2b-1a09-f8e7-d6c5-b4a3f2e1d0c9"), new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"), "Lusail, Qatar", "Lusail Stadium", 50 },
                    { new Guid("6f5e4d3c-2b1a-09f8-e7d6-c5b4a3f2e1d0"), new Guid("0d1e2f3a-4b5c-6d7e-8f9a-0b1c2d3e4f5a"), "Las Vegas, NV", "Las Vegas Convention Center", 1000 },
                    { new Guid("d6c5b4a3-f2e1-a0b9-c8d7-e6f5a4b3c2d1"), new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), "London, UK", "Wembley Stadium", 50 },
                    { new Guid("e7d6c5b4-a3f2-e1d0-c9b8-a7f6e5d4c3b2"), new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"), "Mountain View, CA", "Shoreline Amphitheatre", 100 },
                    { new Guid("f8e7d6c5-b4a3-f2e1-d0c9-b8a7f6e5d4c3"), new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), "San Diego, CA", "San Diego Convention Center", 150 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Venues");
        }
    }
}
