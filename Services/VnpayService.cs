using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using TestVnPay.Ultils.Helpers;
using TestVnPay.VnPay.Config;
using TestVnPay.VnPay.Lib;
using TestVnPay.DTOs.Request;
using TestVnPay.DTOs.Response;
using TestVnPay.Models;
using AutoMapper;
using Azure.Core;
using TestVnPay.DTOs;

namespace TestVnPay.Services
{
    public class VnpayService
    {
        private VnpayPayResponse _vnpayPayResponse;
        private readonly VnpayConfig _vnpayConfig;
        private VnpayPayRequest _vnpayPayRequest;
        private readonly TestVnPayContext _context;
        private readonly IMapper _mapper;

        public VnpayService(VnpayPayResponse vnpayPayResponse, IOptions<VnpayConfig> vnpayConfig, VnpayPayRequest vnpayPayRequest, TestVnPayContext context, IMapper mapper)
        {
            _vnpayPayResponse = vnpayPayResponse;
            _vnpayConfig = vnpayConfig.Value;
            _vnpayPayRequest = vnpayPayRequest;
            _context = context;
            _mapper = mapper;
        }

        public SortedList<string, string> responseData
           = new SortedList<string, string>(new VnpayCompare());

        //hàm này để sắp các string theo thứ tự trừ trên xuống theo bảng chữ cái
        public SortedList<string, string> requestData
            = new SortedList<string, string>(new VnpayCompare());

        //Tạo link thanh toán VNPAY
        public string GetLink(string baseUrl, string secretKey)
        {
            MakeRequestData();
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string result = baseUrl + "?" + data.ToString();
            var secureHash = HashHelper.HmacSHA512(secretKey, data.ToString().Remove(data.Length - 1, 1));
            return result += "vnp_SecureHash=" + secureHash;
        }

        //Check data if it is not null then add to requestData
        public void MakeRequestData()
        {
            if (_vnpayPayRequest.vnp_Amount != null)
                requestData.Add("vnp_Amount", _vnpayPayRequest.vnp_Amount.ToString() ?? string.Empty);
            if (_vnpayPayRequest.vnp_Command != null)
                requestData.Add("vnp_Command", _vnpayPayRequest.vnp_Command);
            if (_vnpayPayRequest.vnp_CreateDate != null)
                requestData.Add("vnp_CreateDate", _vnpayPayRequest.vnp_CreateDate);
            if (_vnpayPayRequest.vnp_CurrCode != null)
                requestData.Add("vnp_CurrCode", _vnpayPayRequest.vnp_CurrCode);
            if (_vnpayPayRequest.vnp_BankCode != null)
                requestData.Add("vnp_BankCode", _vnpayPayRequest.vnp_BankCode);
            if (_vnpayPayRequest.vnp_IpAddr != null)
                requestData.Add("vnp_IpAddr", _vnpayPayRequest.vnp_IpAddr);
            if (_vnpayPayRequest.vnp_Locale != null)
                requestData.Add("vnp_Locale", _vnpayPayRequest.vnp_Locale);
            if (_vnpayPayRequest.vnp_OrderInfo != null)
                requestData.Add("vnp_OrderInfo", _vnpayPayRequest.vnp_OrderInfo);
            if (_vnpayPayRequest.vnp_OrderType != null)
                requestData.Add("vnp_OrderType", _vnpayPayRequest.vnp_OrderType);
            if (_vnpayPayRequest.vnp_ReturnUrl != null)
                requestData.Add("vnp_ReturnUrl", _vnpayPayRequest.vnp_ReturnUrl);
            if (_vnpayPayRequest.vnp_TmnCode != null)
                requestData.Add("vnp_TmnCode", _vnpayPayRequest.vnp_TmnCode);
            if (_vnpayPayRequest.vnp_ExpireDate != null)
                requestData.Add("vnp_ExpireDate", _vnpayPayRequest.vnp_ExpireDate);
            if (_vnpayPayRequest.vnp_TxnRef != null)
                requestData.Add("vnp_TxnRef", _vnpayPayRequest.vnp_TxnRef);
            if (_vnpayPayRequest.vnp_Version != null)
                requestData.Add("vnp_Version", _vnpayPayRequest.vnp_Version);
        }



        //Check Signature response from VNPAY
        public bool IsValidSignature(string secretKey)
        {
            MakeResponseData();
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in responseData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string checkSum = HashHelper.HmacSHA512(secretKey,
                data.ToString().Remove(data.Length - 1, 1));
            return checkSum.Equals(_vnpayPayResponse.vnp_SecureHash, StringComparison.InvariantCultureIgnoreCase);
        }

        public void MakeResponseData()
        {
            if (_vnpayPayResponse.vnp_Amount != null)
                responseData.Add("vnp_Amount", _vnpayPayResponse.vnp_Amount.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(_vnpayPayResponse.vnp_TmnCode))
                responseData.Add("vnp_TmnCode", _vnpayPayResponse.vnp_TmnCode.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(_vnpayPayResponse.vnp_BankCode))
                responseData.Add("vnp_BankCode", _vnpayPayResponse.vnp_BankCode.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(_vnpayPayResponse.vnp_BankTranNo))
                responseData.Add("vnp_BankTranNo", _vnpayPayResponse.vnp_BankTranNo.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(_vnpayPayResponse.vnp_CardType))
                responseData.Add("vnp_CardType", _vnpayPayResponse.vnp_CardType.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(_vnpayPayResponse.vnp_OrderInfo))
                responseData.Add("vnp_OrderInfo", _vnpayPayResponse.vnp_OrderInfo.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(_vnpayPayResponse.vnp_TransactionNo))
                responseData.Add("vnp_TransactionNo", _vnpayPayResponse.vnp_TransactionNo.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(_vnpayPayResponse.vnp_TransactionStatus))
                responseData.Add("vnp_TransactionStatus", _vnpayPayResponse.vnp_TransactionStatus.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(_vnpayPayResponse.vnp_TxnRef))
                responseData.Add("vnp_TxnRef", _vnpayPayResponse.vnp_TxnRef.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(_vnpayPayResponse.vnp_PayDate))
                responseData.Add("vnp_PayDate", _vnpayPayResponse.vnp_PayDate.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(_vnpayPayResponse.vnp_ResponseCode))
                responseData.Add("vnp_ResponseCode", _vnpayPayResponse.vnp_ResponseCode ?? string.Empty);
        }

        //Create payment (save PaymentDtos to Database with Mapper)
        public string CreatePayment(PaymentDtos paymentDtos, string? IpAddress, string? UserId)
        {
            var payment = _mapper.Map<Payments>(paymentDtos);
            var resultPayment = _context.Payments.Add(payment);
            var result = _context.SaveChanges();
            var paymentUrl = string.Empty;
            var test = _vnpayConfig;
            if (result > 0)
            {
                
                _vnpayPayRequest = new VnpayPayRequest(_vnpayConfig.Version,
                _vnpayConfig.TmnCode, DateTime.Now, "127.0.0.1" ?? string.Empty, paymentDtos.RequiredAmount ?? 0, paymentDtos.PaymentCurrency ?? string.Empty,
                                "other", paymentDtos.PaymentContent ?? string.Empty, _vnpayConfig.ReturnUrl, resultPayment.Entity.PaymentId!.ToString() ?? string.Empty);
                paymentUrl = GetLink(_vnpayConfig.PaymentUrl, _vnpayConfig.HashSecret);
                return paymentUrl;
            }
            return "Lỗi rồi";
        }
    }
}
