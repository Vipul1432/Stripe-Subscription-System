using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using SubscriptionSystem.Dtos;
using SubscriptionSystem.Interfaces;
using System;

namespace SubscriptionSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly ILogger _logger;
        public StripeController(IStripeService stripeService, ILogger logger)
        {
            _stripeService = stripeService;
            _logger = logger;
        }

        /// <summary>
        /// Asynchronously retrieves all products associated with the authenticated user.
        /// </summary>
        /// <remarks>
        /// This method first authorizes the user to get their userId. It then calls the Stripe service to fetch all products 
        /// associated with the user. If products are successfully retrieved, it returns an Ok result with the list of products. 
        /// If no products are found or an error occurs during the process, it returns a BadRequest or Internal Server Error response, 
        /// respectively, along with an appropriate error message.
        /// </remarks>
        /// <returns>An asynchronous action result representing the operation's status and, optionally, the fetched products.</returns>
        [HttpGet("get-all-products")]
        public async Task<IActionResult> GetAllProductsAsync()
        {
            try
            {
                // Authorize to get this userId
                var userId = User.FindFirst("Id")?.Value;
                var products = await _stripeService.GetAllProductsAsync(userId);
                if (products == null || !products.Any())
                {
                    return BadRequest("Unable to fetch the product details");
                }
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500, $"An error occurred while fetching products");
            }
        }

        /// <summary>
        /// Asynchronously creates a new customer in Stripe for the specified user.
        /// </summary>
        /// <remarks>
        /// This method calls the Stripe service to create a new customer associated with the provided user ID. 
        /// If the customer creation is successful, it returns an Ok result with the customer ID. 
        /// If an error occurs during the process, it returns a BadRequest response with an appropriate error message.
        /// </remarks>
        /// <param name="userId">The ID of the user for whom the customer is being created.</param>
        /// <returns>An asynchronous action result representing the operation's status and, optionally, the created customer ID.</returns>
        [HttpPost("create-customer")]
        public async Task<IActionResult> CreateCustomerAsync(string userId)
        {
            try
            {
                var customerId = await _stripeService.CreateCustomerToStripeAsync(userId);
                return Ok(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("Unable to create the customer");
            }
        }

        /// <summary>
        /// Asynchronously subscribes the authenticated user to a product plan in Stripe.
        /// </summary>
        /// <remarks>
        /// This method first authorizes the user to get their userId. It then calls the Stripe service to subscribe the user 
        /// to the specified product plan using the provided payment request details. If the subscription process is successful, 
        /// it returns an Ok result with the subscription URL. If an error occurs during the process, it returns a 
        /// Internal Server Error response with an appropriate error message.
        /// </remarks>
        /// <param name="stripePaymentRequestDto">The DTO containing payment request details, including the price ID and customer ID.</param>
        /// <returns>An asynchronous action result representing the operation's status and, optionally, the subscription URL.</returns>
        [HttpPost("make-payment")]
        public async Task<IActionResult> SubscribeProductAsync([FromBody] StripePaymentRequestDto stripePaymentRequestDto)
        {
            try
            {
                // Authorize to get this
                var userId = User.FindFirst("Id")?.Value;
                var url = await _stripeService.SubscribeProductPlanToStipeAsync(stripePaymentRequestDto, userId);
                return Ok(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500, $"An error occurred while making payment");
            }
        }

        /// <summary>
        /// Asynchronously creates a billing portal session URL for the specified customer in Stripe.
        /// </summary>
        /// <remarks>
        /// This method calls the Stripe service to create a billing portal session URL for the provided customer ID. 
        /// If the billing portal session URL is successfully generated, it returns an Ok result with the URL. 
        /// If an error occurs during the process, it returns a Internal Server Error response with an appropriate error message.
        /// </remarks>
        /// <param name="customerId">The ID of the customer for whom the billing portal session URL is being created.</param>
        /// <returns>An asynchronous action result representing the operation's status and, optionally, the billing portal session URL.</returns>
        [HttpPost("create-customer-portal")]
        public async Task<IActionResult> CreateCustomerPortalAsync(string customerId)
        {
            try
            {
                var url = await _stripeService.CustomerPortalAsync(customerId);
                return Ok(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500, $"An error occurred while accessing customer portal");
            }
        }

        /// <summary>
        /// Asynchronously retrieves the subscription details of the authenticated user from Stripe.
        /// </summary>
        /// <remarks>
        /// This method first authorizes the user to get their userId. It then calls the Stripe service to retrieve the subscription details 
        /// associated with the user. If subscription details are successfully fetched, it returns an Ok result with the subscription details. 
        /// If an error occurs during the process, it returns a Internal Server Error response with an appropriate error message.
        /// </remarks>
        /// <returns>An asynchronous action result representing the operation's status and, optionally, the fetched subscription details.</returns>
        [HttpGet("get-customer-id")]
        public async Task<IActionResult> GetCustomerSubscriptionDetailsAsync()
        {
            try
            {
                // Authorize to get this
                var userId = User.FindFirst("Id")?.Value;
                var stripeCustomer = await _stripeService.GetSubscriptionAsync(userId);
                return Ok(stripeCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500, $"An error occurred while fetching customers");
            }
        }

        /// <summary>
        /// Asynchronously processes a payment for a product on behalf of the authenticated user.
        /// </summary>
        /// <remarks>
        /// This method first authorizes the user to get their userId. It then calls the Stripe service to process a payment for 
        /// the specified product using the provided payment details. If the payment process is successful, it returns an Ok result 
        /// with the payment details. If an error occurs during the process, it returns a Internal Server Error response with an 
        /// appropriate error message.
        /// </remarks>
        /// <param name="paymentDto">A DTO containing the payment details, including the customer ID, price, and quantity.</param>
        /// <returns>An asynchronous action result representing the operation's status and, optionally, the payment details.</returns>
        [HttpGet("payment-product")]
        public async Task<IActionResult> PaymentForProductAsync(PaymentDto paymentDto)
        {
            try
            {
                // Authorize to get this
                var userId = User.FindFirst("Id")?.Value;
                var stripeCustomer = await _stripeService.PaymentProductAsync(paymentDto, userId);
                return Ok(stripeCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500, $"An error occurred while fetching customers");
            }
        }

        /// <summary>
        /// Asynchronously handles incoming webhook events from Stripe.
        /// </summary>
        /// <remarks>
        /// This method reads the incoming webhook payload and verifies the signature using the provided webhook secret. 
        /// It then processes different types of webhook events, such as customer creation, subscription creation, subscription update, 
        /// and checkout session completion. For each event type, it extracts the relevant user ID and delegates the corresponding 
        /// processing logic to the Stripe service. If an error occurs during the processing of webhook events, it returns a 
        /// Internal Server Error response with an appropriate error message.
        /// </remarks>
        /// <returns>An asynchronous action result representing the operation's status.</returns>
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhookAsync()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    "webhook-secret",
                    throwOnApiVersionMismatch: false
                );

                string userId = String.Empty;
                switch (stripeEvent.Type)
                {
                    case Stripe.Events.CustomerCreated:
                        var customer = stripeEvent.Data.Object as Customer;
                        userId = customer.Metadata["UserId"];
                        await _stripeService.AddStipeCustomerToDbAsync(customer, userId);
                        break;
                    case Stripe.Events.CustomerSubscriptionCreated:
                        var createdSubscription = stripeEvent.Data.Object as Subscription;
                        userId = createdSubscription.Metadata["UserId"];
                        await _stripeService.AddSubscribedProductPlanToDbAsync(createdSubscription, userId);
                        break;
                    case Stripe.Events.CustomerSubscriptionUpdated:
                        var updatedSubscription = stripeEvent.Data.Object as Subscription;
                        userId = updatedSubscription.Metadata["UserId"];
                        await _stripeService.UpdateSubscribedProductPlanToDbAsync(updatedSubscription, userId);
                        break;
                    case Stripe.Events.CheckoutSessionCompleted:
                        var session = stripeEvent.Data.Object as Session;
                        userId = session.Metadata["UserId"];
                        // Buy single product logic
                        break;
                    default:
                        break;
                }

                return Ok();
            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx, "An error occurred while processing the Stripe payment.");
                return StatusCode(500, "An error occurred while processing the payment. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing the request.");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}
