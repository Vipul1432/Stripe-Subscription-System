using Stripe.Checkout;
using Stripe;
using SubscriptionSystem.Dtos;
using SubscriptionSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SubscriptionSystem.Services
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly string _stripeSecretKey;
        private readonly Stripe.ProductService _productService;
        private readonly ILogger _logger;
        private readonly SubscriptionService _subscriptionService;
        public StripeService(IConfiguration configuration, Stripe.ProductService productService, ILogger logger, SubscriptionService subscriptionService)
        {
            _configuration = configuration;
            _productService = productService;
            _logger = logger;
            _subscriptionService = subscriptionService;
            _stripeSecretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrEmpty(_stripeSecretKey))
            {
                _logger.LogCritical("Stripe secret key is not configured.");
                throw new InvalidOperationException("Stripe secret key is not configured.");
            }

        }

        /// <summary>
        /// Asynchronously retrieves a list of all products from Stripe and maps them to a simplified DTO format.
        /// 
        /// This method initializes the Stripe API configuration with the secret key, creates a list options object to
        /// include the default price data in the product details, and then fetches the list of products from the 
        /// Stripe service. It iterates through the products, mapping each product to a simplified DTO containing
        /// the default price ID, name, and description. In case of an exception, the method logs the error and 
        /// returns an empty list.
        /// 
        /// Parameters:
        /// - userId: The ID of the user requesting the product list.
        /// 
        /// Returns:
        /// - A task representing the asynchronous operation, with a result of a list of simplified StripeProductDto objects.
        /// </summary>
        public async Task<List<StripeProductDto>> GetAllProductsAsync(string userId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretKey;
                var options = new Stripe.ProductListOptions { Expand = new List<string>() { "data.default_price" } };

                var products = await _productService.ListAsync(options);
                var simplifiedProducts = new List<StripeProductDto>();

                foreach (var product in products)
                {
                    simplifiedProducts.Add(new StripeProductDto
                    {
                        DefaultPriceId = product.DefaultPriceId,
                        Name = product.Name,
                        Description = product.Description,
                    });
                }
                return simplifiedProducts;

            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx.Message.ToString());
                return new List<StripeProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
                return new List<StripeProductDto>();
            }
        }

        /// <summary>
        /// Asynchronously creates a new customer in Stripe or retrieves the existing customer ID if the customer already exists.
        /// 
        /// This method initializes the Stripe API configuration with the secret key, checks if the customer exists in the 
        /// database, and if not, creates a new customer in Stripe with the provided user ID, name, email, and metadata. 
        /// If successful, the method returns the newly created customer's ID. In case of a Stripe-related or general exception, 
        /// the method logs the error and returns the error message.
        /// 
        /// Parameters:
        /// - userId: The ID of the user for whom the Stripe customer is being created.
        /// 
        /// Returns:
        /// - A task representing the asynchronous operation, with a result of the Stripe customer ID as a string, or an error message if an exception occurs.
        /// </summary>
        public async Task<string> CreateCustomerToStripeAsync(string userId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretKey;
                // fetch from database if customer exist then return their customerId
                if (false) { }
                else
                {
                    var options = new CustomerCreateOptions
                    {
                        Name = "Vipul Kumar",
                        Email = "vipulupadhyay563@gmail.com",
                        Metadata = new Dictionary<string, string>
                        {
                            { "UserId", userId }
                        }
                    };

                    var service = new CustomerService();
                    var customer = await service.CreateAsync(options);
                    return customer.Id;
                }
            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx.Message.ToString());
                return stripeEx.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return ex.Message;
            }
        }

        /// <summary>
        /// Asynchronously adds a Stripe customer to the database.
        /// 
        /// This method attempts to add a given Stripe customer to the database using the provided user ID. If an exception occurs 
        /// during the database operation, the method logs the error.
        /// 
        /// Parameters:
        /// - customer: The Stripe customer object to be added to the database.
        /// - userId: The ID of the user associated with the Stripe customer.
        /// 
        /// Returns:
        /// - A task representing the asynchronous operation.
        /// </summary>
        public async Task AddStipeCustomerToDbAsync(Customer customer, string userId)
        {
            try
            {
               // Add to database
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while updating the database.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Asynchronously subscribes a user to a product plan in Stripe based on the provided payment request details.
        /// 
        /// This method initializes the Stripe API configuration with the secret key and retrieves the price details using the provided price ID. 
        /// If the price amount is zero and the currency is USD, it creates a subscription directly without requiring credit card information. 
        /// Otherwise, it creates a checkout session requiring credit card details for the subscription. The method returns the success URL 
        /// if the subscription is created directly or the checkout session URL if payment details are needed. In case of an exception, 
        /// the method logs the error and returns the error message.
        /// 
        /// Parameters:
        /// - paymentRequest: The DTO containing payment request details, including the price ID and customer ID.
        /// - userId: The ID of the user subscribing to the product plan.
        /// 
        /// Returns:
        /// - A task representing the asynchronous operation, with a result of a URL string indicating the success or checkout session URL, or an error message if an exception occurs.
        /// </summary>
        public async Task<string> SubscribeProductPlanToStipeAsync(StripePaymentRequestDto paymentRequest, string userId)
        {
            try
            {
                string host = _configuration["Host"];
                StripeConfiguration.ApiKey = _stripeSecretKey;
                var priceService = new PriceService();
                var price = await priceService.GetAsync(paymentRequest.PriceId);
                string productId = price.ProductId;
                if (price.UnitAmount == 0 && price.Currency == "usd")
                {
                    // It directly subscribe without Asking for credit card information 
                    var subscriptionOptions = new SubscriptionCreateOptions
                    {
                        Customer = paymentRequest.CustomerId,
                        Items = new List<SubscriptionItemOptions>
                        {
                            new SubscriptionItemOptions
                            {
                                Price = paymentRequest.PriceId,
                                Quantity = 1
                            }
                        },
                        Metadata = new Dictionary<string, string>
                        {
                            { "UserId", userId }
                        }

                    };
                    var subscriptionService = new SubscriptionService();
                    Stripe.Subscription subscription = await subscriptionService.CreateAsync(subscriptionOptions);
                    return $"{host}/payment-success";
                }
                else
                {
                    // It ask credit card details for payment when user enter credit card details and payment the amount then subscription created 
                    var options = new SessionCreateOptions
                    {
                        LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = paymentRequest.PriceId,
                            Quantity = 1
                        }
                    },
                        Mode = "subscription",
                        SuccessUrl = $"{host}/payment-success",
                        CancelUrl = $"{host}/payment-fail",
                        Metadata = new Dictionary<string, string>
                        {
                            { "UserId", userId }
                        }

                    };

                    options.Customer = paymentRequest.CustomerId;

                    var service = new SessionService();
                    Session session = await service.CreateAsync(options);
                    return session.Url;
                }

            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx.Message.ToString());
                return stripeEx.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
                return ex.Message;
            }
        }

        /// <summary>
        /// Asynchronously adds a subscribed product plan to the database.
        /// 
        /// This method attempts to add the provided Stripe subscription to the database using the associated user ID. 
        /// If an exception occurs during the database operation, the method logs the error.
        /// 
        /// Parameters:
        /// - subscription: The Stripe subscription object to be added to the database.
        /// - userId: The ID of the user associated with the subscription.
        /// 
        /// Returns:
        /// - A task representing the asynchronous operation.
        /// </summary>
        public async Task AddSubscribedProductPlanToDbAsync(Subscription subscription, string userId)
        {
            try
            {
               // Add Subscription to database

            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while updating the database.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Asynchronously creates a billing portal session URL for a Stripe customer.
        /// 
        /// This method initializes the Stripe API configuration with the secret key and creates a billing portal session 
        /// for the provided customer ID. The session is configured to return to the host URL specified in the configuration 
        /// after the customer finishes in the billing portal. If successful, the method returns the billing portal session URL. 
        /// In case of a Stripe-related or general exception, the method logs the error and returns the error message.
        /// 
        /// Parameters:
        /// - customerId: The ID of the Stripe customer for whom the billing portal session is being created.
        /// 
        /// Returns:
        /// - A task representing the asynchronous operation, with a result of the billing portal session URL as a string, or an error message if an exception occurs.
        /// </summary>
        public async Task<string> CustomerPortalAsync(string customerId)
        {
            try
            {
                string host = _configuration["Host"];
                StripeConfiguration.ApiKey = _stripeSecretKey;
                var options = new Stripe.BillingPortal.SessionCreateOptions
                {
                    Customer = customerId,
                    ReturnUrl = host,
                };
                var service = new Stripe.BillingPortal.SessionService();
                var session = await service.CreateAsync(options);
                return session.Url;
            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx.Message.ToString());
                return stripeEx.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return ex.Message;
            }
        }

        /// <summary>
        /// Asynchronously updates an existing subscribed product plan in the database.
        /// 
        /// This method attempts to update the provided Stripe subscription in the database using the associated user ID. 
        /// If an exception occurs during the database operation, the method logs the error.
        /// 
        /// Parameters:
        /// - subscription: The Stripe subscription object containing the updated subscription details.
        /// - userId: The ID of the user associated with the subscription.
        /// 
        /// Returns:
        /// - A task representing the asynchronous operation.
        /// </summary>
        public async Task UpdateSubscribedProductPlanToDbAsync(Subscription subscription, string userId)
        {
            try
            {
                //Update existing subcription in databse

            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while updating the database.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Asynchronously retrieves the active subscription details for a given Stripe customer.
        /// 
        /// This method initializes the Stripe API configuration with the secret key and lists active subscriptions for the provided customer ID. 
        /// It returns a DTO containing the subscription details, including the subscription ID, status, and current period start and end dates. 
        /// If no active subscription is found, it returns a DTO indicating that the customer is not subscribed to any plan. 
        /// In case of a Stripe-related or general exception, the method logs the error and returns an empty subscription DTO.
        /// 
        /// Parameters:
        /// - customerId: The ID of the Stripe customer whose subscription details are being retrieved.
        /// 
        /// Returns:
        /// - A task representing the asynchronous operation, with a result of a SubscriptionDto object containing the subscription details, or an empty SubscriptionDto if an exception occurs.
        /// </summary>
        public async Task<SubscriptionDto> GetSubscriptionAsync(string customerId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretKey;
                var options = new SubscriptionListOptions
                {
                    Customer = customerId,
                    Status = "active"
                };

                StripeList<Stripe.Subscription> subscriptions = await _subscriptionService.ListAsync(options);
                SubscriptionDto subscriptionDto = new SubscriptionDto();
                var activeSubscription = subscriptions.FirstOrDefault();

                if (activeSubscription == null)
                {
                    subscriptionDto.SubscriptionStatus = "not subscribed any plan";
                }
                else
                {
                    subscriptionDto.SubscriptionId = activeSubscription.Id;
                    subscriptionDto.SubscriptionStatus = activeSubscription.Status;
                    subscriptionDto.CurrentPeriodStart = activeSubscription.CurrentPeriodStart;
                    subscriptionDto.CurrentPeriodEnd = activeSubscription.CurrentPeriodEnd;
                }
                return subscriptionDto;
            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx.Message.ToString());
                return new SubscriptionDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
                return new SubscriptionDto();
            }
        }

        /// <summary>
        /// Asynchronously creates a payment session for a product in Stripe and returns the session URL.
        /// 
        /// This method initializes the Stripe API configuration with the secret key and sets up a checkout session for a payment. 
        /// The session includes details such as the success and cancel URLs, line items with price and product data, and customer information. 
        /// It then creates the session using the Stripe SessionService and returns the session URL. 
        /// In case of a Stripe-related or general exception, the method logs the error and returns the error message.
        /// 
        /// Parameters:
        /// - payament: A DTO containing the payment details, including the customer ID, price, and quantity.
        /// - userId: The ID of the user making the payment.
        /// 
        /// Returns:
        /// - A task representing the asynchronous operation, with a result of the session URL as a string, or an error message if an exception occurs.
        /// </summary>
        public async Task<string> PaymentProductAsync(PaymentDto payament, string userId) 
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretKey;
                string host = _configuration["Host"];

                var option = new SessionCreateOptions
                {
                    SuccessUrl = $"{host}/payment-success",
                    CancelUrl = $"{host}/payment-fail",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    Customer = payament.CustomerId,
                    Metadata = new Dictionary<string, string>
                    {
                        { "UserId", userId }
                    }

                };
                var sessionListItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = Convert.ToInt64(payament.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Additional Task",
                            Description = $"You will get {payament.Quantity} task in ${payament.Price * payament.Quantity} and you can add maximum 5 tasks"
                        },
                    },
                    Quantity = payament.Quantity,
                };
                option.LineItems.Add(sessionListItem);

                var service = new SessionService();
                Session session = await service.CreateAsync(option);

                return session.Url;
            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx.Message.ToString());
                return stripeEx.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
                return ex.Message;
            }
        }
    }
}
