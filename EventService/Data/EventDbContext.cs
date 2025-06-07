using EventService.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventService.Data
{
    public class EventDbContext : DbContext
    {
        public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }

        public DbSet<Event> Events => Set<Event>();
        public DbSet<Venue> Venues => Set<Venue>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define static GUIDs for your seed data
            // *** These are VALID GUIDs. You should generate your own unique ones. ***

            var coldplayEventId = Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d");
            var wembleyVenueId = Guid.Parse("d6c5b4a3-f2e1-a0b9-c8d7-e6f5a4b3c2d1");

            var googleIoEventId = Guid.Parse("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e");
            var shorelineVenueId = Guid.Parse("e7d6c5b4-a3f2-e1d0-c9b8-a7f6e5d4c3b2");

            var comicConEventId = Guid.Parse("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f");
            var sanDiegoVenueId = Guid.Parse("f8e7d6c5-b4a3-f2e1-d0c9-b8a7f6e5d4c3");

            var nbaFinalsEventId = Guid.Parse("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a");
            var tdGardenVenueId = Guid.Parse("09f8e7d6-c5b4-a3f2-e1d0-c9b8a7f6e5d4");

            var tedGlobalEventId = Guid.Parse("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b");
            var vancouverVenueId = Guid.Parse("1a09f8e7-d6c5-b4a3-f2e1-d0c9b8a7f6e5");

            var tomorrowlandEventId = Guid.Parse("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c");
            var boomVenueId = Guid.Parse("2b1a09f8-e7d6-c5b4-a3f2-e1d0c9b8a7f6");

            var appleWwdcEventId = Guid.Parse("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d");
            var appleParkVenueId = Guid.Parse("3c2b1a09-f8e7-d6c5-b4a3-f2e1d0c9b8a7");

            var olympicEventId = Guid.Parse("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e");
            var stadeDeFranceVenueId = Guid.Parse("4d3c2b1a-09f8-e7d6-c5b4-a3f2e1d0c9b8");

            var fifaWorldCupEventId = Guid.Parse("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f");
            var lusailStadiumVenueId = Guid.Parse("5e4d3c2b-1a09-f8e7-d6c5-b4a3f2e1d0c9");

            var cesEventId = Guid.Parse("0d1e2f3a-4b5c-6d7e-8f9a-0b1c2d3e4f5a");
            var vegasConventionVenueId = Guid.Parse("6f5e4d3c-2b1a-09f8-e7d6-c5b4a3f2e1d0");


            var events = new List<Event>();
            var venues = new List<Venue>();

            // Directly add to lists to avoid anonymous type, but main thing is using static GUIDs
            events.Add(new Event
            {
                Id = coldplayEventId,
                Name = "Coldplay World Tour",
                Description = "Live music concert by Coldplay",
                VenueId = wembleyVenueId,
                StartTime = new DateTime(2025, 7, 15, 20, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 7, 15, 23, 0, 0, DateTimeKind.Utc)
            });
            venues.Add(new Venue
            {
                Id = wembleyVenueId,
                EventId = coldplayEventId,
                Name = "Wembley Stadium",
                Location = "London, UK",
                NumSeat = 50
            });

            events.Add(new Event
            {
                Id = googleIoEventId,
                Name = "Google I/O 2025",
                Description = "Google's annual developer conference",
                VenueId = shorelineVenueId,
                StartTime = new DateTime(2025, 5, 14, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 5, 14, 17, 0, 0, DateTimeKind.Utc)
            });
            venues.Add(new Venue
            {
                Id = shorelineVenueId,
                EventId = googleIoEventId,
                Name = "Shoreline Amphitheatre",
                Location = "Mountain View, CA",
                NumSeat = 100
            });
            // ... and so on for the rest of your data, using the correct static GUIDs
            events.Add(new Event
            {
                Id = comicConEventId,
                Name = "Comic-Con International",
                Description = "Annual fan convention for comics and pop culture",
                VenueId = sanDiegoVenueId,
                StartTime = new DateTime(2025, 7, 18, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 7, 21, 18, 0, 0, DateTimeKind.Utc)
            });
            venues.Add(new Venue
            {
                Id = sanDiegoVenueId,
                EventId = comicConEventId,
                Name = "San Diego Convention Center",
                Location = "San Diego, CA",
                NumSeat = 150
            });

            events.Add(new Event
            {
                Id = nbaFinalsEventId,
                Name = "NBA Finals Game 7",
                Description = "Final game of the NBA Championship",
                VenueId = tdGardenVenueId,
                StartTime = new DateTime(2025, 6, 20, 20, 30, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 20, 23, 30, 0, DateTimeKind.Utc)
            });
            venues.Add(new Venue
            {
                Id = tdGardenVenueId,
                EventId = nbaFinalsEventId,
                Name = "TD Garden",
                Location = "Boston, MA",
                NumSeat = 60
            });

            events.Add(new Event
            {
                Id = tedGlobalEventId,
                Name = "TED Global 2025",
                Description = "Technology, Entertainment, and Design conference",
                VenueId = vancouverVenueId,
                StartTime = new DateTime(2025, 4, 10, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 4, 10, 17, 0, 0, DateTimeKind.Utc)
            });
            venues.Add(new Venue
            {
                Id = vancouverVenueId,
                EventId = tedGlobalEventId,
                Name = "Vancouver Convention Centre",
                Location = "Vancouver, Canada",
                NumSeat = 70
            });

            events.Add(new Event
            {
                Id = tomorrowlandEventId,
                Name = "Tomorrowland",
                Description = "Electronic dance music festival",
                VenueId = boomVenueId,
                StartTime = new DateTime(2025, 7, 26, 16, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 7, 28, 2, 0, 0, DateTimeKind.Utc)
            });
            venues.Add(new Venue
            {
                Id = boomVenueId,
                EventId = tomorrowlandEventId,
                Name = "Boom Recreation Area",
                Location = "Boom, Belgium",
                NumSeat = 50
            });

            events.Add(new Event
            {
                Id = appleWwdcEventId,
                Name = "Apple WWDC 2025",
                Description = "Apple Worldwide Developers Conference",
                VenueId = appleParkVenueId,
                StartTime = new DateTime(2025, 6, 3, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 3, 17, 0, 0, DateTimeKind.Utc)
            });
            venues.Add(new Venue
            {
                Id = appleParkVenueId,
                EventId = appleWwdcEventId,
                Name = "Apple Park",
                Location = "Cupertino, CA",
                NumSeat = 80
            });

            events.Add(new Event
            {
                Id = olympicEventId,
                Name = "Olympic Games Opening Ceremony",
                Description = "Kickoff for the Summer Olympics",
                VenueId = stadeDeFranceVenueId,
                StartTime = new DateTime(2025, 7, 24, 20, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 7, 24, 23, 0, 0, DateTimeKind.Utc)
            });
            venues.Add(new Venue
            {
                Id = stadeDeFranceVenueId,
                EventId = olympicEventId,
                Name = "Stade de France",
                Location = "Paris, France",
                NumSeat = 10
            });

            events.Add(new Event
            {
                Id = fifaWorldCupEventId,
                Name = "FIFA World Cup Final",
                Description = "Final match of the FIFA World Cup",
                VenueId = lusailStadiumVenueId,
                StartTime = new DateTime(2025, 12, 18, 18, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 12, 18, 21, 0, 0, DateTimeKind.Utc)
            });
            venues.Add(new Venue
            {
                Id = lusailStadiumVenueId,
                EventId = fifaWorldCupEventId,
                Name = "Lusail Stadium",
                Location = "Lusail, Qatar",
                NumSeat = 50
            });

            events.Add(new Event
            {
                Id = cesEventId,
                Name = "CES 2025",
                Description = "Consumer Electronics Show",
                VenueId = vegasConventionVenueId,
                StartTime = new DateTime(2025, 1, 7, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 1, 10, 18, 0, 0, DateTimeKind.Utc)
            });
            venues.Add(new Venue
            {
                Id = vegasConventionVenueId,
                EventId = cesEventId,
                Name = "Las Vegas Convention Center",
                Location = "Las Vegas, NV",
                NumSeat = 1000
            });

            modelBuilder.Entity<Event>().HasData(events);
            modelBuilder.Entity<Venue>().HasData(venues);
        }
    }
}