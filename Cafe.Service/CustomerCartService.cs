using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Cafe.Service
{
    public class CustomerCartService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OrderService _orderService;
        private readonly SettingService _settingService;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public CustomerCartService(
            ApplicationDbContext dbContext,
            OrderService orderService,
            SettingService settingService)
        {
            _dbContext = dbContext;
            _orderService = orderService;
            _settingService = settingService;
        }

        /// <summary>
        /// Validates cart items against database stock and recalculates official server-side pricing & taxes.
        /// </summary>
        public async Task<ValidateCartResponseDto> ValidateAndPriceCartAsync(ValidateCartRequestDto request)
        {
            var response = new ValidateCartResponseDto();
            var settings = await _settingService.GetSettingsAsync();

            decimal taxRate = settings.Financial?.TaxRate ?? 8.0m;
            string currency = settings.Financial?.Currency ?? "USD";
            string currencySymbol = GetCurrencySymbol(currency);

            decimal subtotal = 0;
            bool canProceed = true;

            foreach (var item in request.Items)
            {
                if (!int.TryParse(item.ProductId, out int pid))
                {
                    response.Warnings.Add($"Invalid product ID: {item.ProductId}");
                    canProceed = false;
                    continue;
                }

                var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pid);
                if (product == null)
                {
                    response.Warnings.Add($"Product #{pid} is no longer available.");
                    canProceed = false;
                    continue;
                }

                bool isAvailable = product.Status.Equals("Active", StringComparison.OrdinalIgnoreCase);
                string? warningMessage = null;

                if (!isAvailable)
                {
                    warningMessage = product.Status.Equals("Out of Stock", StringComparison.OrdinalIgnoreCase)
                        ? $"{product.Name} is currently sold out."
                        : $"{product.Name} is unavailable.";
                    canProceed = false;
                    response.Warnings.Add(warningMessage);
                }

                // Calculate verified price
                decimal unitPrice = product.BasePrice;

                // Add customization option modifiers if provided
                if (item.Options != null && item.Options.Count > 0 && !string.IsNullOrWhiteSpace(product.CustomizationGroupsJson))
                {
                    try
                    {
                        var groups = JsonSerializer.Deserialize<List<CustomizationGroupDto>>(product.CustomizationGroupsJson, JsonOptions);
                        if (groups != null)
                        {
                            foreach (var optEntry in item.Options)
                            {
                                var group = groups.FirstOrDefault(g => g.Name.Equals(optEntry.Key, StringComparison.OrdinalIgnoreCase));
                                if (group != null)
                                {
                                    var option = group.Options.FirstOrDefault(o => o.Name.Equals(optEntry.Value, StringComparison.OrdinalIgnoreCase));
                                    if (option != null)
                                    {
                                        unitPrice += option.PriceAdjustment;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Fallback to client's supplied unit price if deserialization fails
                        if (item.UnitPrice.HasValue && item.UnitPrice.Value >= product.BasePrice)
                        {
                            unitPrice = item.UnitPrice.Value;
                        }
                    }
                }

                int qty = Math.Max(1, item.Quantity);
                decimal itemTotal = unitPrice * qty;
                subtotal += itemTotal;

                response.Items.Add(new ValidatedCartItemDto
                {
                    ProductId = product.Id.ToString(),
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Quantity = qty,
                    UnitPrice = unitPrice,
                    TotalPrice = itemTotal,
                    Options = item.Options,
                    Notes = item.Notes,
                    IsAvailable = isAvailable,
                    WarningMessage = warningMessage
                });
            }

            decimal tax = Math.Round(subtotal * (taxRate / 100m), 2);
            decimal total = subtotal + tax;

            response.Subtotal = subtotal;
            response.TaxRate = taxRate;
            response.Tax = tax;
            response.Total = total;
            response.CurrencySymbol = currencySymbol;
            response.CanProceed = canProceed && response.Items.Count > 0;

            return response;
        }

        /// <summary>
        /// Validates cart items and places a verified order directly into the database.
        /// </summary>
        public async Task<OrderDto> CheckoutCartAsync(CartCheckoutRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                throw new ArgumentException("Customer name is required.");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                throw new InvalidOperationException("Cart is empty. Please add items to order.");
            }

            // 1. Validate items and compute server-verified prices
            var validation = await ValidateAndPriceCartAsync(new ValidateCartRequestDto
            {
                Items = request.Items,
                TableNumber = request.TableNumber,
                ServiceType = request.ServiceType
            });

            if (!validation.CanProceed)
            {
                var issues = string.Join("; ", validation.Warnings);
                throw new InvalidOperationException($"Cannot place order: {issues}");
            }

            // 2. Prepare Order Items
            var orderItems = new List<CreateOrderItemDto>();
            foreach (var item in validation.Items)
            {
                var customizations = new List<string>();
                if (item.Options != null)
                {
                    foreach (var kvp in item.Options)
                    {
                        customizations.Add($"{kvp.Key}: {kvp.Value}");
                    }
                }

                orderItems.Add(new CreateOrderItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Customizations = customizations
                });
            }

            var combinedNotes = string.Join(" | ", validation.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.Notes))
                .Select(i => $"{i.ProductName}: {i.Notes}"));

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                combinedNotes = string.IsNullOrWhiteSpace(combinedNotes)
                    ? request.Notes
                    : $"{combinedNotes} | General: {request.Notes}";
            }

            if (!string.IsNullOrWhiteSpace(request.GuestId))
            {
                var guestTag = $"[GuestId: {request.GuestId.Trim()}]";
                combinedNotes = string.IsNullOrWhiteSpace(combinedNotes) ? guestTag : $"{combinedNotes} {guestTag}";
            }

            // 3. Delegate to OrderService to generate order number and persist
            string? tableNumber = CleanTableNumber(request.TableNumber);
            if (string.IsNullOrWhiteSpace(tableNumber) && !string.IsNullOrWhiteSpace(request.TableId) && int.TryParse(request.TableId, out int tId))
            {
                var tbl = await _dbContext.Tables.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tId);
                tableNumber = tbl != null ? tbl.Number.ToString() : request.TableId.Trim();
            }

            var createOrderDto = new CreateOrderDto
            {
                CustomerName = request.CustomerName.Trim(),
                TableNumber = tableNumber,
                GuestId = request.GuestId?.Trim(),
                ServiceType = string.IsNullOrWhiteSpace(request.ServiceType) ? "Dine-In" : request.ServiceType.Trim(),
                PaymentStatus = string.IsNullOrWhiteSpace(request.PaymentStatus) ? "Pending" : request.PaymentStatus.Trim(),
                PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Online" : request.PaymentMethod.Trim(),
                TransactionId = request.TransactionId,
                Subtotal = validation.Subtotal,
                Tax = validation.Tax,
                Total = validation.Total,
                Notes = string.IsNullOrWhiteSpace(combinedNotes) ? null : combinedNotes,
                Items = orderItems
            };

            return await _orderService.CreateOrderAsync(createOrderDto);
        }

        /// <summary>
        /// Retrieves settings necessary for cart and checkout calculations.
        /// </summary>
        public async Task<CheckoutSettingsDto> GetCheckoutSettingsAsync()
        {
            var settings = await _settingService.GetSettingsAsync();
            var financial = settings.Financial ?? new FinancialSettingsDto();

            var paymentMethods = new List<string> { "Online" };
            if (financial.AcceptCash) paymentMethods.Add("Cash");
            if (financial.AcceptCard) paymentMethods.Add("Card at Counter");
            if (financial.AcceptUpi) paymentMethods.Add("UPI at Counter");

            var (isOpen, statusText) = EvaluateOpenStatus(settings.Hours);
            string currency = financial.Currency ?? "USD";

            return new CheckoutSettingsDto
            {
                Currency = currency,
                CurrencySymbol = GetCurrencySymbol(currency),
                TaxRate = financial.TaxRate,
                PaymentMethods = paymentMethods,
                IsOpen = isOpen,
                OperatingStatusText = statusText
            };
        }

        private static (bool IsOpen, string StatusText) EvaluateOpenStatus(List<OperatingHourDto>? hours)
        {
            if (hours == null || hours.Count == 0)
            {
                return (true, "Open");
            }

            var now = DateTime.UtcNow;
            var currentDayName = now.DayOfWeek.ToString();

            var todayHour = hours.FirstOrDefault(h => h.Day.Equals(currentDayName, StringComparison.OrdinalIgnoreCase));
            if (todayHour == null || !todayHour.IsOpen)
            {
                return (false, "Closed Today");
            }

            if (TimeSpan.TryParse(todayHour.OpenTime, out var open) && TimeSpan.TryParse(todayHour.CloseTime, out var close))
            {
                var currentTime = now.TimeOfDay;
                if (currentTime >= open && currentTime <= close)
                {
                    return (true, $"Open until {DateTime.Today.Add(close):hh:mm tt}");
                }
                else if (currentTime < open)
                {
                    return (false, $"Opens today at {DateTime.Today.Add(open):hh:mm tt}");
                }
                else
                {
                    return (false, "Closed for the day");
                }
            }

            return (true, "Open");
        }

        private static string GetCurrencySymbol(string? currency)
        {
            return (currency?.ToUpperInvariant()) switch
            {
                "EUR" => "€",
                "GBP" => "£",
                "INR" => "₹",
                "CAD" => "CA$",
                "AUD" => "AU$",
                "JPY" => "¥",
                _ => "$"
            };
        }

        public static string? CleanTableNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var clean = input.Trim();
            if (clean.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ||
                clean.Equals("Table Unknown", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            if (clean.StartsWith("Table ", StringComparison.OrdinalIgnoreCase))
            {
                clean = clean[6..].Trim();
            }
            else if (clean.StartsWith("Tbl ", StringComparison.OrdinalIgnoreCase))
            {
                clean = clean[4..].Trim();
            }
            else if (clean.StartsWith('#'))
            {
                clean = clean.TrimStart('#').Trim();
            }
            return string.IsNullOrWhiteSpace(clean) ? null : clean;
        }
    }
}
