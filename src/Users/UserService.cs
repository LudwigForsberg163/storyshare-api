using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace StoryShare.Api.Users;

public class UserService
{
	private readonly IConfiguration _config;

	public UserService(IConfiguration config)
	{
		_config = config;
	}

	public string GenerateJwtToken(User user)
	{
		var jwtKey = _config["Jwt:Key"];
		if (string.IsNullOrEmpty(jwtKey))
			throw new InvalidOperationException("JWT-nyckel är inte konfigurerad.");

		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.UTF8.GetBytes(jwtKey);
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.Name, user.Username),
				new Claim("userId", user.Id.ToString())
			}),
			Expires = DateTime.UtcNow.AddHours(1),
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(key),
				SecurityAlgorithms.HmacSha256Signature)
		};
		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken(token);
	}
}
