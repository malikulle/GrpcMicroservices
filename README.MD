# Getting started
- This project is developed by `Mehmet Ali Külle` to understand GRPCMicroservices architecture with `IdentityServer` and `GRPC`

## Code Overview
- `Authentication/IdentityServer` is Authentication layer. This layer protect project.
    - `Config.cs`  is the configuration file.

- `Discount/DiscountGrpc` is Discount API. This api return discounts.
    - `Protos/discount.proto` is discount proto.

- `Product/ProductGrpc` is Product API. This api contains product CRUD operation
    - `Protos/product.proto` is product proto.

- `Product/ProductGrpc.Client` is client for Product API to do CURD operation.

- `Product/ProductWorkerService` is Product Worker service.

- `ShoppingCart/ShoppingCartGrpc` is Shopping Cart API. This API returns cart.
    - `Protos/shoppingcart.proto` is shopping cart proto.

- `ShoppingCart/ShoppingCartWorkerService` is shopping cart worker service. This worker service send request to `IdentityServer` and get token to use in ShoppingCart API.