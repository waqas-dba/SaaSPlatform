using CoreKit.SharedKernel.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.SharedKernel.Interfaces;

public interface IProductOrderInfoProvider
{
    Task<ProductOrderInfo?> GetProductInfoAsync(Guid productId, CancellationToken ct = default);
    Task<ProductVariantOrderInfo?> GetVariantInfoAsync(Guid variantId, CancellationToken ct = default);
}
