using Stripe;
using SubscriptionSystem.Dtos;

namespace SubscriptionSystem.Interfaces
{
    public interface IStripeService
    {
        /// <summary>
        /// Asynchronously retrieves a list of all products from Stripe and maps them to a simplified DTO format.
        /// </summary>
        /// <param name="userId">The ID of the user requesting the product list.</param>
        /// <returns>A task representing the asynchronous operation, with a result of a list of simplified StripeProductDto objects.</returns>
        Task<List<StripeProductDto>> GetAllProductsAsync(string userId);

        /// <summary>
        /// Asynchronously creates a new customer in Stripe or retrieves the existing customer ID if the customer already exists.
        /// </summary>
        /// <param name="userId">The ID of the user for whom the Stripe customer is being created.</param>
        /// <returns>A task representing the asynchronous operation, with a result of the Stripe customer ID as a string, or an error message if an exception occurs.</returns>
        Task<string> CreateCustomerToStripeAsync(string userId);

        /// <summary>
        /// Asynchronously adds a Stripe customer to the database.
        /// </summary>
        /// <param name="customer">The Stripe customer object to be added to the database.</param>
        /// <param name="userId">The ID of the user associated with the Stripe customer.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddStipeCustomerToDbAsync(Customer customer, string userId);

        /// <summary>
        /// Asynchronously subscribes a user to a product plan in Stripe based on the provided payment request details.
        /// </summary>
        /// <param name="paymentRequest">The DTO containing payment request details, including the price ID and customer ID.</param>
        /// <param name="userId">The ID of the user subscribing to the product plan.</param>
        /// <returns>A task representing the asynchronous operation, with a result of a URL string indicating the success or checkout session URL, or an error message if an exception occurs.</returns>
        Task<string> SubscribeProductPlanToStipeAsync(StripePaymentRequestDto paymentRequest, string userId);

        /// <summary>
        /// Asynchronously adds a subscribed product plan to the database.
        /// </summary>
        /// <param name="subscription">The Stripe subscription object to be added to the database.</param>
        /// <param name="userId">The ID of the user associated with the subscription.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddSubscribedProductPlanToDbAsync(Subscription subscription, string userId);

        /// <summary>
        /// Asynchronously creates a billing portal session URL for a Stripe customer.
        /// </summary>
        /// <param name="customerId">The ID of the Stripe customer for whom the billing portal session is being created.</param>
        /// <returns>A task representing the asynchronous operation, with a result of the billing portal session URL as a string, or an error message if an exception occurs.</returns>
        Task<string> CustomerPortalAsync(string customerId);

        /// <summary>
        /// Asynchronously updates an existing subscribed product plan in the database.
        /// </summary>
        /// <param name="subscription">The Stripe subscription object containing the updated subscription details.</param>
        /// <param name="userId">The ID of the user associated with the subscription.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateSubscribedProductPlanToDbAsync(Subscription subscription, string userId);

        /// <summary>
        /// Asynchronously retrieves the active subscription details for a given Stripe customer.
        /// </summary>
        /// <param name="customerId">The ID of the Stripe customer whose subscription details are being retrieved.</param>
        /// <returns>A task representing the asynchronous operation, with a result of a SubscriptionDto object containing the subscription details, or an empty SubscriptionDto if an exception occurs.</returns>
        Task<SubscriptionDto> GetSubscriptionAsync(string customerId);

        /// <summary>
        /// Asynchronously creates a payment session for a product in Stripe and returns the session URL.
        /// </summary>
        /// <param name="payament">A DTO containing the payment details, including the customer ID, price, and quantity.</param>
        /// <param name="userId">The ID of the user making the payment.</param>
        /// <returns>A task representing the asynchronous operation, with a result of the session URL as a string, or an error message if an exception occurs.</returns>
        Task<string> PaymentProductAsync(PaymentDto payament, string userId);
    }
}
