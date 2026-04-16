using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Contoso.OrderSystem
{
    /// <summary>
    /// Handles payment processing through the QuickPay gateway.
    /// Originally integrated in 2015.
    /// </summary>
    public class PaymentService
    {
        /// <summary>
        /// Charge a customer via the external payment gateway.
        /// Returns true if the charge succeeded.
        /// </summary>
        public bool ChargeCustomer(string customerEmail, decimal amount, string paymentToken)
        {
            string gatewayUrl = ConfigurationManager.AppSettings["PaymentGatewayUrl"];
            string merchantId = ConfigurationManager.AppSettings["MerchantId"];
            string merchantSecret = GetMerchantSecret();

            if (!IsAllowedGateway(gatewayUrl))
            {
                LogSecurityError("Blocked payment request because the configured gateway URL is not HTTPS or is not allowlisted.");
                return false;
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string postData = string.Format(
                "merchant_id={0}&secret={1}&email={2}&amount={3}&token={4}&currency=USD",
                merchantId,
                merchantSecret,
                Uri.EscapeDataString(customerEmail),
                amount.ToString("F2"),
                Uri.EscapeDataString(paymentToken));

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(gatewayUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 30000;

                byte[] data = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string body = reader.ReadToEnd();
                            return body.StartsWith("APPROVED");
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                LogSecurityError("Payment gateway error: " + ex.Message);
            }
            catch (Exception ex)
            {
                LogSecurityError("Unexpected payment error: " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Issue a refund for a previous charge. Added in 2019.
        /// </summary>
        public bool RefundCharge(string transactionId, decimal amount)
        {
            string gatewayUrl = ConfigurationManager.AppSettings["PaymentGatewayUrl"] + "/refund";
            string merchantId = ConfigurationManager.AppSettings["MerchantId"];
            string merchantSecret = GetMerchantSecret();

            if (!IsAllowedGateway(gatewayUrl))
            {
                LogSecurityError("Blocked refund request because the configured gateway URL is not HTTPS or is not allowlisted.");
                return false;
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string postData = string.Format(
                "merchant_id={0}&secret={1}&transaction_id={2}&amount={3}",
                merchantId, merchantSecret, transactionId, amount.ToString("F2"));

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(gatewayUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 30000;

                byte[] data = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string body = reader.ReadToEnd();
                        return body.StartsWith("REFUNDED");
                    }
                }
            }
            catch (Exception ex)
            {
                LogSecurityError("Refund error: " + ex.Message);
            }

            return false;
        }

        private static string GetMerchantSecret()
        {
            return Environment.GetEnvironmentVariable("ORDER_SYSTEM_MERCHANT_SECRET")
                   ?? ConfigurationManager.AppSettings["MerchantSecret"];
        }

        private static bool IsAllowedGateway(string gatewayUrl)
        {
            Uri gatewayUri;
            if (!Uri.TryCreate(gatewayUrl, UriKind.Absolute, out gatewayUri))
            {
                return false;
            }

            if (!string.Equals(gatewayUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var allowedHosts = (ConfigurationManager.AppSettings["PaymentGatewayAllowedHosts"] ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(host => host.Trim())
                .ToArray();

            return allowedHosts.Contains(gatewayUri.Host, StringComparer.OrdinalIgnoreCase);
        }

        private static void LogSecurityError(string message)
        {
            System.Diagnostics.EventLog.WriteEntry(
                "OrderSystem",
                "event=security_error component=PaymentService message=\"" + message + "\"",
                System.Diagnostics.EventLogEntryType.Error);
        }
    }
}
