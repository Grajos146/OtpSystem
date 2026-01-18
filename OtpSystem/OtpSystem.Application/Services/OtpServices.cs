namespace OtpSystem.Application.Services;

public class OtpServices(
    IOtpRepository otpRepository,
    ITokenService tokenService,
    IEmailServices emailService,
    IUserRepository userRepository
    )
{
    private readonly IOtpRepository _otpRepository = otpRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IEmailServices _emailService = emailService;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<object> RequestNewOtpAsync(GenerateOtpRequest req)
    {
        var latestOtp = await _otpRepository.GetLatestActiveOtpsByEmailAsync(req.Email);

        var user = await _userRepository.GetUserByEmailAsync(req.Email);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = req.Email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };

            await _userRepository.AddAsync(user);
        }

        if (latestOtp is not null)
        {
            var elapsed = DateTime.UtcNow - latestOtp.CreatedAt;

            if (elapsed < TimeSpan.FromMinutes(1))
            {
                var remaining = TimeSpan.FromMinutes(1) - elapsed;
                return new
                {
                    Message = $"Please wait {Math.Ceiling(remaining.TotalSeconds)} seconds",
                    req.CorrelationId,
                };
            }
        }

        var plainCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

        var existingOtp = await _otpRepository.GetLatestActiveOtpsByEmailAsync(req.Email);

        if (existingOtp != null && existingOtp.CorrelationId == req.CorrelationId)
            return req.CorrelationId;

        var otp = Otp.Create(user.Id, req.CorrelationId, plainCode);

        await _otpRepository.AddAsync(otp);

        try
        {
            await _emailService.SendOtpEmailAsync(req.Email, plainCode);
        }
        catch
        {
            // This should invalidate the otp if email fails
            otp.InvalidatedAt = DateTime.UtcNow;
            await _otpRepository.UpdateAsync(otp);
            throw;
        }

        return (
            new { Message = "OTP sent successfully, please check your email.", req.CorrelationId }
        );
    }

    public async Task<AuthResponse> ValidateOtpAsync(ValidateOtpRequest req)
    {
        var otp = await _otpRepository.GetOtpByCorrelationIdAsync(req.CorrelationId);

        //This is a basic check
        if (otp is null)
        {
            return new AuthResponse { Status = OtpValidationStatus.NotFound };
        }

        //Checking if the otp is Active
        if (!otp.IsActive())
        {
            return new AuthResponse { Status = OtpValidationStatus.Inactive };
        }

        //this is to verify the email of the user requesting the otp
        if (otp.User?.Email != req.Email)
        {
            return new AuthResponse { Status = OtpValidationStatus.EmailMismatch };
        }

        //Verifying the otp...basic check
        if (otp.Verify(req.Code))
        {
            otp.MarkAsUsed();
            await _otpRepository.UpdateAsync(otp);
            return new AuthResponse
            {
                Status = OtpValidationStatus.Success,
                Token = _tokenService.GenerateToken(otp.User!),
                Email = otp.User!.Email,
            };
        }
        else
        {
            otp.RegisterWrongAttempt();
            await _otpRepository.UpdateAsync(otp);

            return new AuthResponse { Status = OtpValidationStatus.InvalidCode };
        }
    }

    // public async Task<bool> AddAttemptAsync(AttemptedOtpReq req)
    // {
    //     var attemptedOtp = await _otpRepository.GetOtpAttemptByOtpIdAsync(req.OtpId);

    //     if (attemptedOtp is not null)
    //     {
    //         return false;
    //     }

    // }

    public async Task<(IEnumerable<OtpResponseDto> Items, int TotalCount)> ListActiveOtpsAsync(
        ListActiveOtpsRequest req
    )
    {
        var (items, totalCount) = await _otpRepository.GetActiveOtpsAsync(
            req.Email,
            req.PageNumber,
            req.PageSize,
            DateTime.UtcNow
        );

        var res = items.Select(Mapper.MapToDto);

        return (res, totalCount);
    }

    public async Task<(IEnumerable<OtpResponseDto> Items, int TotalCount)> ListExpiredOtpsAsync(
        ListExpiredOtpsRequest req
    )
    {
        var (items, totalCount) = await _otpRepository.GetExpiredOtpsAsync(
            req.Email,
            req.PageNumber,
            req.PageSize
        );

        var res = items.Select(Mapper.MapToDto);

        return (res, totalCount);
    }
}
