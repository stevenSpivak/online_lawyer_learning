using DAL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace OLL.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassesController : ControllerBase
    {
        private readonly ClassDB classDB;

        public ClassesController(ClassDB classDB)
        {
            this.classDB = classDB;
        }

        // GET: /api/classes/teacher/5
        // מחזיר את כל הכיתות של מורה מסוים.
        [HttpGet("teacher/{teacherId:int}")]
        public async Task<ActionResult<List<Classroom>>> GetTeacherClasses(
            int teacherId)
        {
            return Ok(
                await classDB.GetClassesForTeacherAsync(
                    teacherId));
        }

        // POST: /api/classes
        // יוצר כיתה חדשה.
        [HttpPost]
        public async Task<ActionResult<Classroom>> Create(
            [FromBody] Classroom classroom)
        {
            if (string.IsNullOrWhiteSpace(classroom.Name))
            {
                return BadRequest(
                    "Class name is required.");
            }

            string name = classroom.Name.Trim();

            if (name.Length < 3)
            {
                return BadRequest(
                    "Class name must contain at least 3 characters.");
            }

            if (await classDB.ClassNameExistsAsync(
                    name,
                    classroom.TeacherId))
            {
                return Conflict(
                    "A class with this name already exists.");
            }

            int id =
                await classDB.CreateClassAsync(
                    name,
                    classroom.TeacherId);

            return Ok(new
            {
                id,
                name
            });
        }

        // DELETE: /api/classes/5?teacherId=2
        // מוחק כיתה השייכת למורה.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id,
            [FromQuery] int teacherId)
        {
            bool deleted =
                await classDB.DeleteClassAsync(
                    id,
                    teacherId);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}