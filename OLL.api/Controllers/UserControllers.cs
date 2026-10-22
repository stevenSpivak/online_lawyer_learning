using DAL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace OLL.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserDB userDB;

        public UsersController(UserDB userDB)
        {
            this.userDB = userDB;
        }

        // GET: /api/users
        // Gets all users from the database.
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            return Ok(await userDB.GetAllUsersAsync());
        }

        // GET: /api/users/1
        // Gets one user by their ID.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            User? user = await userDB.GetUserByIdAsync(id);

            // CHANGED:
            // Added braces so it is easier to see exactly what happens
            // when the user does not exist.
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: /api/users/exists/admin
        // Checks whether a username already exists.
        [HttpGet("exists/{username}")]
        public async Task<ActionResult<bool>> UsernameExists(string username)
        {
            return Ok(await userDB.UsernameExistsAsync(username));
        }

        // GET: /api/users/email-exists/admin@example.com
        // Checks whether an email already exists.
        [HttpGet("email-exists/{email}")]
        public async Task<ActionResult<bool>> EmailExists(string email)
        {
            return Ok(await userDB.EmailExistsAsync(email));
        }

        // GET: /api/users/count-by-role
        // Counts users according to their role.
        [HttpGet("count-by-role")]
        public async Task<ActionResult<Dictionary<string, int>>> CountByRole()
        {
            return Ok(await userDB.CountUsersByRoleAsync());
        }

        // POST: /api/users
        // Creates a new user.
        [HttpPost]
        public async Task<ActionResult<User>> Add(
            [FromBody] User user,
            [FromQuery] string roleName = "Student")
        {
            // Checks if the username is already being used.
            if (await userDB.UsernameExistsAsync(user.Username))
            {
                return Conflict("Username already exists.");
            }

            // Checks if the email is already being used.
            if (await userDB.EmailExistsAsync(user.Email))
            {
                return Conflict("Email already exists.");
            }

            try
            {
                // Adds the user to the database.
                await userDB.AddUserAsync(user, roleName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            // Returns HTTP 201 and the newly-created user.
            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                user);
        }

        // POST: /api/users/login
        // Checks the username and password.
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(
            [FromBody] User credentials)
        {
            User? user = await userDB.CheckLoginAsync(
                credentials.Username,
                credentials.PasswordHash);

            // If the login details are wrong, return 401.
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(user);
        }

        // PUT: /api/users/1/role?roleName=Teacher
        // Changes a user's role.
        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> UpdateRole(
            int id,
            [FromQuery] string roleName)
        {
            bool updated =
                await userDB.UpdateUserRoleAsync(id, roleName);

            // If no user with this ID was found, return 404.
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: /api/users/1/password
        // Changes a user's password.
        [HttpPut("{id:int}/password")]
        public async Task<IActionResult> UpdatePassword(
            int id,
            [FromBody] string newPasswordHash)
        {
            bool updated =
                await userDB.UpdatePasswordAsync(
                    id,
                    newPasswordHash);

            // If no user with this ID was found, return 404.
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: /api/users/1/email
        // Changes a user's email.
        [HttpPut("{id:int}/email")]
        public async Task<IActionResult> UpdateEmail(
            int id,
            [FromBody] string newEmail)
        {
            // CHANGED:
            // Don't allow the email to be changed to an email
            // that already belongs to another user.
            if (await userDB.EmailExistsForAnotherUserAsync(id, newEmail))
            {
                return Conflict("Email already exists.");
            }

            bool updated =
                await userDB.UpdateEmailAsync(id, newEmail);

            // If no user with this ID was found, return 404.
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: /api/users/1/status?isActive=false
        // Changes whether a user is active.
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromQuery] bool isActive)
        {
            bool updated =
                await userDB.UpdateUserStatusAsync(
                    id,
                    isActive);

            // If no user with this ID was found, return 404.
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: /api/users/1
        // Deletes a user.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted =
                await userDB.DeleteUserAsync(id);

            // If no user with this ID was found, return 404.
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}