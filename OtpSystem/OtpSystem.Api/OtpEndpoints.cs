namespace OtpSystem.Api;

public static class OtpEndpoints
{
    public static RouteGroupBuilder MapOtpEndPoints(this WebApplication app)
    {
        var group = app.MapGroup("/otp");

        //Request Otp
        group.MapPost(
            "/request",
            async (GenerateOtpRequest req, OtpServices _otp) =>
            {
                var result = await _otp.RequestNewOtpAsync(req);
                return Results.Ok(result);
            }
        );

        //Validate Otp
        group.MapPost(
            "/validate",
            async (
                ValidateOtpRequest req,
                OtpServices _otp,
                IValidator<ValidateOtpRequest> validator
            ) =>
            {
                var validationResult = await validator.ValidateAsync(req);

                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                var result = await _otp.ValidateOtpAsync(req);

                return result.Status switch
                {
                    OtpValidationStatus.Success => Results.Ok(
                        new
                        {
                            message = "OTP verified successfully.",
                            token = result.Token,
                            email = result.Email,
                        }
                    ),
                    OtpValidationStatus.NotFound => Results.NotFound(
                        new { message = "OTP not found, It may have expired or is invalid." }
                    ),
                    OtpValidationStatus.Inactive => Results.BadRequest(
                        new { message = "OTP has expired or is inactive." }
                    ),
                    OtpValidationStatus.EmailMismatch => Results.BadRequest(
                        new
                        {
                            message = "The email address does not match the original OTP request.",
                        }
                    ),
                    OtpValidationStatus.InvalidCode => Results.BadRequest(
                        new { message = "Invalid OTP code." }
                    ),
                    _ => Results.BadRequest(result.Status.ToString()),
                };
            }
        );

        //GetActive Otps
        group
            .MapGet(
                "/active",
                async (
                    string? email,
                    int pageNumber,
                    int pageSize,
                    OtpServices _otp,
                    ClaimsPrincipal user
                ) =>
                {
                    var claimsEmail = user.FindFirstValue(ClaimTypes.Email);

                    if (string.IsNullOrEmpty(claimsEmail))
                        return Results.Unauthorized();

                    if (pageNumber <= 0 || pageSize <= 0)
                    {
                        return Results.BadRequest("Invalid page");
                    }

                    var result = await _otp.ListActiveOtpsAsync(
                        new ListActiveOtpsRequest
                        {
                            Email = email,
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                        }
                    );

                    return Results.Ok(new { result.TotalCount, result.Items });
                }
            )
            .RequireAuthorization();

        //GetExpired Otps
        group
            .MapGet(
                "/expired",
                async (
                    string? email,
                    int pageNumber,
                    int pageSize,
                    OtpServices _otp,
                    ClaimsPrincipal user
                ) =>
                {
                    var claimsEmail = user.FindFirstValue(ClaimTypes.Email);

                    if (string.IsNullOrEmpty(claimsEmail))
                        return Results.Unauthorized();

                    if (pageNumber <= 0 || pageSize <= 0)
                    {
                        return Results.BadRequest("Invalid page");
                    }

                    var result = await _otp.ListExpiredOtpsAsync(
                        new ListExpiredOtpsRequest
                        {
                            Email = email,
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                        }
                    );

                    return Results.Ok(new { result.TotalCount, result.Items });
                }
            )
            .RequireAuthorization();

        return group;
    }
}
