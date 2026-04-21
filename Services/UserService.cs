using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace eCommerceMotoRepuestos.Services;

public class UserService(
    GenericRepository<User> _userRepository,
    IPasswordHasher<User> _passwordHasher)
{
    public async Task<UserViewModel> Login(LoginViewModel loginVM)
    {
        var conditions = new List<Expression<Func<User, bool>>>()
        {
            x => x.Email == loginVM.Email,
        };

        var found = await _userRepository.GetByFilter(conditions: conditions.ToArray());

        var userVM = new UserViewModel();

        if (found != null)
        {
            var verificationResult = _passwordHasher.VerifyHashedPassword(found, found.Password, loginVM.Password);
            var isSuccess = verificationResult == PasswordVerificationResult.Success ||
                            verificationResult == PasswordVerificationResult.SuccessRehashNeeded;

            if (isSuccess)
            {
                userVM.UserId = found.UserId;
                userVM.FullName = found.FullName;
                userVM.Email = found.Email;
                userVM.Type = found.Type;
            }
        }

        return userVM;
    }


    public async Task Register(UserViewModel userVM)
    {
        if (userVM.Password != userVM.RepeatPassword)
            throw new InvalidOperationException("Las contraseñas no coinciden");

        var conditions = new List<Expression<Func<User, bool>>>()
        {
            x => x.Email == userVM.Email
        };

        var foundEmail = await _userRepository.GetByFilter(conditions: conditions.ToArray());

        if (foundEmail != null)
            throw new InvalidOperationException("La dirección de email ya se encuentra registrada");


        var entity = new User()
        {
            FullName = userVM.FullName,
            Email = userVM.Email,
            Type = userVM.Type,
            Password = string.Empty,
        };

        entity.Password = _passwordHasher.HashPassword(entity, userVM.Password);

        await _userRepository.AddAsync(entity);
    }

    public async Task<EditProfileViewModel> GetProfileForEditAsync(int userId)
    {
        var found = await _userRepository.GetByIdAsync(userId);
        if (found == null)
            throw new InvalidOperationException("Usuario no encontrado");

        return new EditProfileViewModel
        {
            UserId = found.UserId,
            FullName = found.FullName,
            Email = found.Email,
            Password = string.Empty,
            RepeatPassword = string.Empty
        };
    }

    public async Task<UserViewModel> UpdateProfileAsync(EditProfileViewModel userVM)
    {
        var hasPasswordInput =
            !string.IsNullOrWhiteSpace(userVM.Password) ||
            !string.IsNullOrWhiteSpace(userVM.RepeatPassword);

        if (hasPasswordInput && userVM.Password != userVM.RepeatPassword)
            throw new InvalidOperationException("Las contraseñas no coinciden");

        var currentUser = await _userRepository.GetByIdAsync(userVM.UserId);

        if (currentUser == null)
            throw new InvalidOperationException("Usuario no encontrado");

        if (!string.IsNullOrWhiteSpace(userVM.Email) &&
            !string.Equals(userVM.Email, currentUser.Email, StringComparison.OrdinalIgnoreCase))
        {
            var conditions = new List<Expression<Func<User, bool>>>()
            {
                x => x.Email == userVM.Email,
                x => x.UserId != userVM.UserId
            };

            var foundEmail = await _userRepository.GetByFilter(conditions: conditions.ToArray());
            if (foundEmail != null)
                throw new InvalidOperationException("La dirección de email ya se encuentra registrada");

            currentUser.Email = userVM.Email;
        }

        if (!string.IsNullOrWhiteSpace(userVM.FullName))
            currentUser.FullName = userVM.FullName;

        if (hasPasswordInput && !string.IsNullOrWhiteSpace(userVM.Password))
            currentUser.Password = _passwordHasher.HashPassword(currentUser, userVM.Password);

        await _userRepository.EditAsync(currentUser);

        return new UserViewModel
        {
            UserId = currentUser.UserId,
            FullName = currentUser.FullName,
            Email = currentUser.Email,
            Password = currentUser.Password,
            RepeatPassword = currentUser.Password,
            Type = currentUser.Type
        };
    }
}
