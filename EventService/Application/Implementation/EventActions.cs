﻿using BookingService.Application.Grpc.Clients;
using Confluent.Kafka;
using EventService.Application.Interfaces;
using EventService.Application.Kafka;
using EventService.Application.Kafka.Interfaces;
using EventService.Application.SearchService;
using EventService.Data;
using EventService.Entities;
using EventService.Entities.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EventService.Application.Implementation
{
    public class EventActions : IEventActions
    {
        private readonly EventDbContext _context;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ICacheService _cache;
        private readonly BookingGrpcClient _bookingGrpcClient;
        private readonly ISearchServiceClient _searchServiceClient;
        private readonly ILogger<EventActions> _logger;
        private const string TICKET_RESERVE_PREFIX = "ticket-reserve:";

        public EventActions(
            EventDbContext context,
            IKafkaProducer kafkaProducer,
            ICacheService cache,
            BookingGrpcClient bookingGrpcClient,
            ISearchServiceClient searchServiceClient,
            ILogger<EventActions> logger
        )
        {
            _context = context;
            _kafkaProducer = kafkaProducer;
            _cache = cache;
            _bookingGrpcClient = bookingGrpcClient;
            _searchServiceClient = searchServiceClient;
            _logger = logger;
        }

        public async Task<Event> CreateEventAsync(EventDto eventDto)
        {
            try
            {
                var newEvent = new Event
                {
                    Id = Guid.NewGuid(),
                    Name = eventDto.NameEvent,
                    Description = eventDto.Description,
                    StartTime = eventDto.StartTime,
                    EndTime = eventDto.EndTime,
                    VenueId = Guid.NewGuid()
                };

                var newVenue = new Venue
                {
                    Id = newEvent.VenueId,
                    EventId = newEvent.Id,
                    Name = eventDto.NameVenue,
                    Location = eventDto.Location,
                    NumSeat = eventDto.NumSeat
                };

                await _context.Events.AddAsync(newEvent);
                await _context.Venues.AddAsync(newVenue);
                await _context.SaveChangesAsync();

                await _kafkaProducer.PublishAsync(KafkaTopic.EventCreated.ToString(), new EventCreatedSendDto { Id = newEvent.Id, NumSeat = newVenue.NumSeat });

                var createIndexElasticSearch = await _searchServiceClient.CreateEventAsync(newEvent);
                if (!createIndexElasticSearch)
                {
                    Console.WriteLine("Failed to sync event {EventId} to search service", newEvent.Id);
                }

                return newEvent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create event");
                throw new Exception("Failed to create event", ex);
            }
        }

        public async Task<bool> DeleteEventAsync(Guid id)
        {
            try
            {
                var cacheKey = $"event:{id}";
                var cachedEvent = await _cache.GetAsync<Event>(cacheKey);
                if (cachedEvent != null)
                {
                    await _cache.RemoveAsync(cacheKey);
                }

                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    return false;
                }

                // Remove event and its associated venue
                _context.Events.Remove(eventItem);

                var venue = await _context.Venues.FindAsync(eventItem.VenueId);
                if (venue != null)
                {
                    _context.Venues.Remove(venue);
                }

                // Sync deletion with search service
                var searchSyncSuccess = await _searchServiceClient.DeleteEventAsync(eventItem.Id);
                if (!searchSyncSuccess)
                {
                    _logger.LogError("Failed to sync deleted event {EventId} to search service", eventItem.Id);
                }

                // Delete tickets from booking service
                var ticketDeleteSuccess = await _bookingGrpcClient.DeleteTicketsByEventAsync(eventItem.Id, venue?.NumSeat ?? 0);
                if (!ticketDeleteSuccess)
                {
                    _logger.LogError("Failed to delete tickets for event ID {EventId} from booking service", eventItem.Id);
                    return false;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete event with ID {EventId}", id);
                return false;
            }
        }


        public async Task<List<Event>> GetEventsAsync()
        {
            try
            {
                var events = await _context.Events.OrderByDescending(d => d.StartTime).ToListAsync();
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve events");
                return new List<Event>();
            }

        }

        public async Task<Event?> GetEventByIdAsync(Guid eventId)
        {
            try
            {
                var cacheKey = $"event:{eventId}";
                var cachedEvent = await _cache.GetAsync<Event>(cacheKey);
                if (cachedEvent != null)
                {
                    return cachedEvent;
                }
                var eventItem = await _context.Events.FindAsync(eventId);
                await _cache.SetAsync(cacheKey, eventItem, TimeSpan.FromMinutes(2));
                return eventItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve event with ID {EventId}", eventId);
                return new Event();
            }
        }

        public async Task<Venue?> GetVenueByEventIdAsync(Guid eventId)
        {
            try
            {
                var cacheKey = $"venue:{eventId}";
                var cachedVenue = await _cache.GetAsync<Venue>(cacheKey);
                if (cachedVenue != null)
                {
                    return cachedVenue;
                }
                var venue = await _context.Venues.FirstOrDefaultAsync(v => v.EventId == eventId);
                await _cache.SetAsync(cacheKey, venue, TimeSpan.FromMinutes(2));
                return venue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve venue for event ID {EventId}", eventId);
                return new Venue();
            }
        }

        public async Task<List<TicketDto>> GetAllTicketsByEventIDAsync(Guid eventId)
        {
            try
            {
                var venue = await _context.Venues.FirstOrDefaultAsync(v => v.EventId == eventId);
                if (venue == null)
                {
                    _logger.LogWarning("No venue found for event ID {EventId}", eventId);
                    return new List<TicketDto>();
                }

                var cacheKey = $"tickets:{eventId}:1-{venue.NumSeat}";
                var cached = await _cache.GetAsync<List<TicketDto>>(cacheKey);
                if (cached is not null)
                {
                    foreach (var ticket in cached)
                    {
                        var reservedKey = $"{TICKET_RESERVE_PREFIX}{ticket.Id}";
                        if (await _cache.ExistsAsync(reservedKey))
                        {
                            ticket.Status = TicketStatus.Reserved;
                        }
                    }
                    return cached;
                }

                var listTickets = await _bookingGrpcClient.GetTicketsByEventAsync(eventId, venue.NumSeat);
                return listTickets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetEventsByIDAsync failed for event ID {EventId}", eventId);
                return new List<TicketDto>();
            }
        }


        public async Task<Event?> UpdateEventAsync(Guid id, EventDto eventDto)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(id);

                if (eventItem == null)
                {
                    return null;
                }

                eventItem.Name = eventDto.NameEvent;
                eventItem.Description = eventDto.Description;
                eventItem.StartTime = eventDto.StartTime;
                eventItem.EndTime = eventDto.EndTime;

                var venue = await _context.Venues.FindAsync(eventItem.VenueId);
                if (venue != null)
                {
                    venue.Name = eventDto.NameVenue;
                    venue.Location = eventDto.Location;
                    venue.NumSeat = eventDto.NumSeat;
                }

                await _context.SaveChangesAsync();

                var success = await _searchServiceClient.UpdateEventAsync(eventItem);
                if (!success)
                {
                    _logger.LogError("Failed to sync updated event {EventId} to search service", eventItem.Id);
                }

                return eventItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update event with ID {EventId}", id);
                return new Event();
            }
        }
    }
}
