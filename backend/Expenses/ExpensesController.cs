using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Expenses;

[ApiController]
[Route("api/Expenses")]
public class ExpensesController : ControllerBase
{
    private readonly ExpensesContext _context;

    public ExpensesController(ExpensesContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync()
    {
        var expenses = await _context.Expenses
            .Include(e => e.Category).ToListAsync();
        return Ok(expenses);
    }
}