using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JobTrail.Data;
using JobTrail.DTOs;
using JobTrail.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JobTrail.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;
    
    public AuthService(IConfiguration configuration, AppDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }
    
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
         //1. Check if email already exits
         var existingUser =  await _dbContext.Users.FirstOrDefaultAsync(u=>u.Email == dto.Email);
         if (existingUser != null) 
             throw new InvalidOperationException("Email already registered.");
         
         //2. Create user with hashed password
         var user = new User
         {
             Id = Guid.NewGuid(),
             Email = dto.Email.ToLower().Trim(),
             PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
             CreatedOn = DateTime.UtcNow,
         };
         _dbContext.Users.Add(user);
         await _dbContext.SaveChangesAsync();
         
         //3.Generate token pair and return
         return await GenerateTokenPairAsync(user);
    }

    private async Task<AuthResponseDto> GenerateTokenPairAsync(User user)
    {
        // Generate JWT access token
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(
            double.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"]!));
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );
        
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Generate refresh token
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(
                double.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]!))
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = expiry
        };

    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        // 1. Find user by email (normalize it)
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        // 2. If not found → throw (same message as "invalid credentials", don't reveal which field was wrong)
        if (existingUser == null)
            throw new InvalidOperationException("Invalid email or password.");
        
        // 3. Verify password hash with BCrypt.Verify()
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, existingUser.PasswordHash))
            throw new InvalidOperationException("Invalid email or password.");
        
        // 5. Call GenerateTokenPairAsync and return
        return await GenerateTokenPairAsync(existingUser);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshRequestDto dto)
    {
        //1. Find the refresh token in the DB by the token string
        var storedToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);
        
        //2. Check it exists           → if not, throw "Invalid refresh token"
        if (storedToken == null)
            throw new InvalidOperationException("Invalid refresh token.");
    
        //3. Check it's not expired    → if ExpiresOn < UtcNow, throw "Refresh token expired"
        if (storedToken.ExpiresOn < DateTime.UtcNow)
            throw new InvalidOperationException("Refresh token has expired.");
        
        //4. Check it's not revoked    → if RevokedOn != null, throw "Refresh token revoked"
        if (storedToken.RevokedOn != null)
            throw new InvalidOperationException("Refresh token has been revoked.");
        
        //5. Revoke the old token      → set RevokedOn = UtcNow, save
        storedToken.RevokedOn = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(); 
        
        //6. Generate and return new token pair
        return await GenerateTokenPairAsync(storedToken.User);
    }

    public async Task RevokeAsync(Guid userId)
    {
        //1. Find all active refresh tokens for this userId
         //   → active means RevokedOn == null
         var refreshTokens = await _dbContext.RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedOn == null)
             .ToListAsync();
         
         //2. Set RevokedOn = UtcNow on each one
         foreach (var refreshToken in refreshTokens)
         {
             refreshToken.RevokedOn = DateTime.UtcNow;
         }
         
         //3. Save changes
         await _dbContext.SaveChangesAsync();
    }
}