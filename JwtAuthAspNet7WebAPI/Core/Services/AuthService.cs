using AutoMapper;
using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using JwtAuthAspNet7WebAPI.Core.OtherObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper, IConfiguration configuration, ILogger<AuthService> logger, IEmailService emailService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<AuthServiceResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                if (loginDto == null)
                {
                    _logger.LogWarning("Login attempt with null loginDto");
                    throw new ArgumentNullException(nameof(loginDto));
                }

                _logger.LogInformation("Login attempt for user: {UserName}", loginDto.UserName);
                
                var user = await _userManager.FindByNameAsync(loginDto.UserName);

                if (user is null)
                {
                    _logger.LogWarning("Login failed: User not found - {UserName}", loginDto.UserName);
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = false,
                        Message = "Invalid credentials"
                    };
                }

                var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);

                if (!isPasswordCorrect)
                {
                    _logger.LogWarning("Login failed: Invalid password for user - {UserName}", loginDto.UserName);
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = false,
                        Message = "Invalid credentials"
                    };
                }

                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("JWTID", Guid.NewGuid().ToString()),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName),
                    new Claim("DateOfBirth", user.DateOfBirth.ToString("yyyy-MM-dd")),
                    new Claim("Gender", user.Gender),
                    new Claim("BirthPlace", user.BirthPlace),
                    new Claim("Address", user.Address)
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GenerateNewJsonWebToken(authClaims);

                // Update last login timestamp
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Login successful for user: {UserName}", loginDto.UserName);
                
                return new AuthServiceResponseDto()
                {
                    IsSucceed = true,
                    Message = token,
                    Roles = userRoles.ToList(),
                    User = _mapper.Map<ApplicationUserDto>(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {UserName}", loginDto?.UserName);
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<AuthServiceResponseDto> MakeAdminAsync(UpdatePermissionDto updatePermissionDto)
        {
            try
            {
                if (updatePermissionDto == null)
                {
                    _logger.LogWarning("MakeAdmin attempt with null updatePermissionDto");
                    throw new ArgumentNullException(nameof(updatePermissionDto));
                }

                _logger.LogInformation("Attempting to make user admin: {UserName}", updatePermissionDto.UserName);
                
                var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

                if (user is null)
                {
                    _logger.LogWarning("MakeAdmin failed: User not found - {UserName}", updatePermissionDto.UserName);
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = false,
                        Message = "User not found"
                    };
                }

                var result = await _userManager.AddToRoleAsync(user, StaticUserRoles.ADMIN);
                
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed to add admin role to user: {UserName}. Errors: {Errors}", 
                        updatePermissionDto.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = false,
                        Message = "Failed to assign admin role"
                    };
                }

                _logger.LogInformation("Successfully made user admin: {UserName}", updatePermissionDto.UserName);
                
                return new AuthServiceResponseDto()
                {
                    IsSucceed = true,
                    Message = $"User '{user.UserName}' is now an admin"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making user admin: {UserName}", updatePermissionDto?.UserName);
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "An error occurred while assigning admin role"
                };
            }
        }

        public async Task<AuthServiceResponseDto> MakeUserAsync(UpdatePermissionDto updatePermissionDto)
        {
            try

            if (user is null)
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid User name!!!!!!!!"
                };

            await _userManager.AddToRoleAsync(user, StaticUserRoles.USER);

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "Guest " + user + " is now an User"
            };
        }

        public async Task<AuthServiceResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var isExistsUser = await _userManager.FindByNameAsync(registerDto.UserName);

            if (isExistsUser != null)
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "UserName Already Exists"
                };

            // Also check if email exists
            var isExistsEmail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (isExistsEmail != null)
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Email Already Exists"
                };

            // Generate email confirmation token
            var emailConfirmationToken = Guid.NewGuid().ToString();
            
            var newUser = new ApplicationUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                DateOfBirth = registerDto.DateOfBirth,
                Gender = registerDto.Gender,
                BirthPlace = registerDto.BirthPlace,
                Address = registerDto.Address,
                EmailConfirmed = false,
                IsEmailConfirmed = false,
                EmailConfirmationToken = emailConfirmationToken,
                EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };

            var createUserResult = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!createUserResult.Succeeded)
            {
                var errorString = "User Creation Failed Because: ";
                foreach (var error in createUserResult.Errors)
                {
                    errorString += " # " + error.Description;
                }
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = errorString
                };
            }

            await _userManager.AddToRoleAsync(newUser, StaticUserRoles.GUEST);

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "User Created Successfully"
            };
        }

        public async Task<AuthServiceResponseDto> SeedRolesAsync()
        {
            bool isAdminRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
            bool isUserRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);
            bool isGuestRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.GUEST);

            if (isAdminRoleExists && isUserRoleExists && isGuestRoleExists)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = true,
                    Message = "Roles Seeding is Already Done",
                    Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync()
                };
            }

            if (!isUserRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));
            }
            if (!isAdminRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
            }
            if (!isGuestRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.GUEST));
            }

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "Role Seeding Done Successfully",
                Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync()
            };
        }

        private string GenerateNewJsonWebToken(List<Claim> claims)
        {
            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var tokenObject = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(1),
                    claims: claims,
                    signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256)
                );

            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

            return token;
        }

        public async Task<List<ApplicationUserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var userDtos = new List<ApplicationUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new ApplicationUserDto
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return userDtos;
        }

        public async Task<List<string>> GetAllUserNamesAsync()
        {
            var userNames = _userManager.Users.Select(user => user.UserName).ToList();
            return userNames;
        }

        public async Task<AuthServiceResponseDto> RemoveAdminAsync(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid User name " + user
                };
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, StaticUserRoles.ADMIN);

            if (!isAdmin)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "User " + user + " is not an admin"
                };
            }

            var removeAdminResult = await _userManager.RemoveFromRoleAsync(user, StaticUserRoles.ADMIN);

            if (!removeAdminResult.Succeeded)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Failed to remove admin role for " + user
                };
            }

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "User " + user + "  is no longer an admin"
            };
        }

        public async Task<AuthServiceResponseDto> RemoveUserAsync(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid User name " + user
                };
            }

            var isUser = await _userManager.IsInRoleAsync(user, StaticUserRoles.USER);

            if (!isUser)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "User " + user + " is not a USER"
                };
            }

            var removeUserResult = await _userManager.RemoveFromRoleAsync(user, StaticUserRoles.USER);

            if (!removeUserResult.Succeeded)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Failed to remove USER role for " + user
                };
            }

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "User " + user + "  is no longer a USER"
            };
        }

        public async Task<AuthServiceResponseDto> RemoveGuestAsync(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid User name " + user
                };
            }

            var isGuest = await _userManager.IsInRoleAsync(user, StaticUserRoles.GUEST);

            if (!isGuest)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "User " + user + " is not a GUEST"
                };
            }

            var removeGuestResult = await _userManager.RemoveFromRoleAsync(user, StaticUserRoles.GUEST);

            if (!removeGuestResult.Succeeded)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Failed to remove GUEST role for " + user
                };
            }

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "User " + user + "  is no longer a GUEST"
            };
        }

        public async Task<AuthServiceResponseDto> ActivateEmailAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("ActivateEmailAsync called with empty token");
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = false,
                        Message = "Invalid activation token"
                    };
                }

                _logger.LogInformation("Attempting to activate email with token: {Token}", token);

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);

                if (user == null)
                {
                    _logger.LogWarning("No user found with activation token: {Token}", token);
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = false,
                        Message = "Invalid activation token"
                    };
                }

                if (user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("Activation token expired for user: {UserId}", user.Id);
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = false,
                        Message = "Activation token has expired. Please request a new activation email."
                    };
                }

                if (user.IsEmailConfirmed)
                {
                    _logger.LogInformation("Email already confirmed for user: {UserId}", user.Id);
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = true,
                        Message = "Email is already confirmed"
                    };
                }

                // Activate the user
                user.IsEmailConfirmed = true;
                user.EmailConfirmed = true;
                user.EmailConfirmationToken = null;
                user.EmailConfirmationTokenExpiry = null;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to activate email for user: {UserId}. Errors: {Errors}", 
                        user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = false,
                        Message = "Failed to activate email"
                    };
                }

                // Send welcome email
                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.UserName);
                    _logger.LogInformation("Welcome email sent to activated user: {Email}", user.Email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send welcome email to {Email}", user.Email);
                    // Don't fail activation if welcome email fails
                }

                _logger.LogInformation("Email activated successfully for user: {UserId}", user.Id);
                return new AuthServiceResponseDto()
                {
                    IsSucceed = true,
                    Message = "Email activated successfully! Welcome to ByteBuddy!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating email with token: {Token}", token);
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "An error occurred during email activation"
                };
            }
        }

        public async Task<AuthServiceResponseDto> ResendActivationEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("ResendActivationEmailAsync called with empty email");
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = false,
                        Message = "Email is required"
                    };
                }

                _logger.LogInformation("Resending activation email to: {Email}", email);

                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning("No user found with email: {Email}", email);
                    // Don't reveal if email exists or not for security
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = true,
                        Message = "If the email exists in our system, an activation link has been sent."
                    };
                }

                if (user.IsEmailConfirmed)
                {
                    _logger.LogInformation("Email already confirmed for user: {Email}", email);
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = true,
                        Message = "Email is already confirmed"
                    };
                }

                // Generate new activation token
                var newToken = Guid.NewGuid().ToString();
                user.EmailConfirmationToken = newToken;
                user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update activation token for user: {Email}", email);
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = false,
                        Message = "Failed to resend activation email"
                    };
                }

                // Send activation email
                try
                {
                    await _emailService.SendActivationEmailAsync(user.Email, user.UserName, newToken);
                    _logger.LogInformation("Activation email resent successfully to: {Email}", email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to resend activation email to {Email}", email);
                    return new AuthServiceResponseDto()
                    {
                        IsSucceed = false,
                        Message = "Failed to send activation email"
                    };
                }

                return new AuthServiceResponseDto()
                {
                    IsSucceed = true,
                    Message = "Activation email sent successfully. Please check your email."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending activation email to: {Email}", email);
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "An error occurred while resending activation email"
                };
            }
        }
    }
}
