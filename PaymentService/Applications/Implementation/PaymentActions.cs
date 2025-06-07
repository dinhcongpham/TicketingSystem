using MongoDB.Driver;
using PaymentService.Application.Kafka.Interfaces;
using PaymentService.Applications.Interfaces;
using PaymentService.Applications.Kafka;
using PaymentService.Entities;
using PaymentService.Entities.Records;

namespace PaymentService.Applications.Implementation
{
    public class PaymentActions : IPaymentActions
    {
        private readonly IMongoCollection<Payment> _payments;
        private readonly ILogger<PaymentActions> _logger;
        private readonly IKafkaProducer _kafkaProducer;

        public PaymentActions(
            IMongoDatabase database, 
            ILogger<PaymentActions> logger,
            IKafkaProducer kafkaProducer)
        {
            _payments = database.GetCollection<Payment>("Payments");
            _logger = logger;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            try
            {
                return await _payments.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all payments");
                throw;
            }
        }

        public async Task HandleBookingCreatedAsync(BookingCreatedMessage message)
        {
            try
            {
                _logger.LogInformation("Handling booking created for booking: {BookingId}", message.BookingId);

                var payment = await CreatePaymentAsync(message);

                _logger.LogInformation("Payment entry created for booking: {BookingId}, Payment: {PaymentId}",
                    message.BookingId, payment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling booking created for booking: {BookingId}", message.BookingId);
                throw;
            }
        }

        public async Task HandleBookingExpriedAsync(BookingExpiredMessage message)
        {
            try
            {
                _logger.LogInformation("Handling booking expired for booking: {BookingId}", message.BookingId);
                // Get payment by booking ID
                var payment = await GetPaymentByBookingIdAsync(message.BookingId);
                if (payment == null)
                {
                    _logger.LogWarning("No payment found for booking: {BookingId}", message.BookingId);
                    return;
                }
                // Update payment status to expired
                payment.Status = PaymentStatus.Expired;
                await UpdatePaymentAsync(payment);
                _logger.LogInformation("Payment status updated to expired for booking: {BookingId}, Payment: {PaymentId}",
                    message.BookingId, payment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling booking expired for booking: {BookingId}", message.BookingId);
                throw;
            }
        }
        public async Task<ProcessPaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing payment for booking: {BookingId}", request.BookingId);

                // Get payment by booking ID
                var payment = await GetPaymentByBookingIdAsync(request.BookingId);
                if (payment == null)
                {
                    return new ProcessPaymentResponse
                    {
                        Success = false,
                        Message = "Payment not found for this booking"
                    };
                }

                if (payment.Status != PaymentStatus.Processing)
                {
                    return new ProcessPaymentResponse
                    {
                        Success = false,
                        Message = $"Payment is in {payment.Status} status and cannot be processed"
                    };
                }

                // Update payment status to paid
                payment.Status = PaymentStatus.Paid;
                await UpdatePaymentAsync(payment);

                // Phase 5: Send payment success message to booking service
                var paymentSuccessMessage = new PaymentSuccessMessage
                {
                    BookingId = payment.BookingId,
                    PaymentId = payment.Id,
                    Amount = payment.Amount,
                    PaidAt = DateTime.UtcNow,
                    NumSeats = request.NumSeats,
                };

                await _kafkaProducer.PublishAsync(KafkaTopic.PaymentSuccess.ToString(), paymentSuccessMessage);

                _logger.LogInformation("Payment processed successfully for booking: {BookingId}, Payment: {PaymentId}",
                    request.BookingId, payment.Id);

                return new ProcessPaymentResponse
                {
                    Success = true,
                    Message = "Payment processed successfully",
                    PaymentId = payment.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for booking: {BookingId}", request.BookingId);

                // Update payment status to failed
                try
                {
                    var payment = await GetPaymentByBookingIdAsync(request.BookingId);
                    if (payment != null && payment.Status == PaymentStatus.Processing)
                    {
                        payment.Status = PaymentStatus.Failed;
                        await UpdatePaymentAsync(payment);
                    }
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Error updating payment status to failed for booking: {BookingId}", request.BookingId);
                }

                return new ProcessPaymentResponse
                {
                    Success = false,
                    Message = "Payment processing failed. Please try again."
                };
            }
        }
        public async Task<Payment> GetPaymentByBookingIdAsync(Guid bookingId)
        {
            try
            {
                var filter = Builders<Payment>.Filter.Eq(p => p.BookingId, bookingId);
                return await _payments.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment by booking ID: {BookingId}", bookingId);
                throw;
            }
        }
        public async Task<Payment> CreatePaymentAsync(BookingCreatedMessage message)
        {
            try
            {
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    BookingId = message.BookingId,
                    Amount = message.TotalAmount,
                    Email = message.Email,
                    Status = PaymentStatus.Processing
                };

                await _payments.InsertOneAsync(payment);
                _logger.LogInformation("Payment created: {PaymentId}", payment.Id);
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for bookingId: {PaymentId}", message.BookingId);
                throw;
            }
        }
        public async Task<Payment?> GetPaymentByIdAsync(Guid paymentId)
        {
            try
            {
                var filter = Builders<Payment>.Filter.Eq(p => p.Id, paymentId);
                return await _payments.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment by ID: {PaymentId}", paymentId);
                throw;
            }
        }
        public async Task<Payment?> UpdatePaymentAsync(Payment payment)
        {
            try
            {
                var filter = Builders<Payment>.Filter.Eq(p => p.Id, payment.Id);
                await _payments.ReplaceOneAsync(filter, payment);
                _logger.LogInformation("Payment updated: {PaymentId}", payment.Id);
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment: {PaymentId}", payment.Id);
                throw;
            }
        }
    }
}
