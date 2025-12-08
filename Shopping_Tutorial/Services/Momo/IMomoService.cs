using Shopping_Tutorial.Models.MoMo;
using Shopping_Tutorial.Models;

namespace Shopping_Tutorial.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfo model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
