using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;
using System.Linq.Expressions;

namespace eCommerceMotoRepuestos.Services;

public class UserService(GenericRepository<User> _userRepository)
{
    public async Task<UserViewModel> Login(LoginViewModel loginVM)
    {
        var conditions = new List<Expression<Func<User, bool>>>()
        {
            x => x.Email == loginVM.Email,
            x => x.Password == loginVM.Password,
        };

        var found = await _userRepository.GetByFilter(conditions: conditions.ToArray());

        var userVM = new UserViewModel();
        if (found != null)
        {
            userVM.UserId = found.UserId;
            userVM.FullName = found.FullName;
            userVM.Email = found.Email;
            userVM.Type = found.Type;
        }

        return userVM;
    }


    public async Task Register(UserViewModel userVM)
    {
        if (userVM.Password != userVM.RepeatPassword)
            throw new InvalidOperationException("The passwords are not the same");

        var conditions = new List<Expression<Func<User, bool>>>()
        {
            x => x.Email == userVM.Email
        };

        var foundEmail = await _userRepository.GetByFilter(conditions: conditions.ToArray());

        if (foundEmail != null)
            throw new InvalidOperationException("the email address is already registered");


        var entity = new User()
        {
            FullName = userVM.FullName,
            Email = userVM.Email,
            Type = userVM.Type,
            Password = userVM.Password,
        };

        await _userRepository.AddAsync(entity);
    }
}
