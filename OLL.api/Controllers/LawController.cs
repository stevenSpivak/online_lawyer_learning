using DAL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace OLL.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LawsController : ControllerBase
{
    private readonly LawCatalog lawCatalog;

    public LawsController(LawCatalog lawCatalog)
    {
        this.lawCatalog = lawCatalog;
    }

    // GET: /api/laws
    // GET: /api/laws?search=student
    // GET: /api/laws?categoryId=2
    [HttpGet]
    public async Task<ActionResult<List<Law>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null)
    {
        var laws = await lawCatalog.GetAllAsync(
            search, categoryId);

        return Ok(laws);
    }

    // GET: /api/laws/categories
    [HttpGet("categories")]
    public async Task<ActionResult<List<LawCategory>>>
        GetCategories()
    {
        var categories =
            await lawCatalog.GetCategoriesAsync();

        return Ok(categories);
    }

    // GET: /api/laws/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Law>> GetById(int id)
    {
        Law? law =
            await lawCatalog.GetByIdAsync(id);

        if (law is null)
        {
            return NotFound();
        }

        return Ok(law);
    }
}