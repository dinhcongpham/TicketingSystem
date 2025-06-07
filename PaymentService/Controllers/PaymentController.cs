using Microsoft.AspNetCore.Mvc;
using PaymentService.Applications.Interfaces;
using PaymentService.Entities;
using PaymentService.Entities.Records;

namespace PaymentService.Controllers
{

   [Controller]
   [Route("api/[controller]")]
   public class PaymentController : ControllerBase
    {
        private readonly IPaymentActions _paymentActions;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentActions paymentActions, ILogger<PaymentController> logger)
        {
            _paymentActions = paymentActions;
            _logger = logger;
        }

        [HttpPost("process/{id}")]
        public async Task<ActionResult<ProcessPaymentResponse>> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ProcessPayment request: {ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                var result = await _paymentActions.ProcessPaymentAsync(request);

                if (!result.Success)
                {
                    _logger.LogWarning("Payment processing failed: {Message}", result.Message);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessPayment endpoint");
                return StatusCode(500, new ProcessPaymentResponse
                {
                    Success = false,
                    Message = "An error occurred while processing payment"
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Payment>>> GetAllPayments()
        {
            try
            {
                var payments = await _paymentActions.GetAllPaymentsAsync();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllPayments endpoint");
                return StatusCode(500, "An error occurred while retrieving payments");
            }
        }
    }
}
