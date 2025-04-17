using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.Models;

namespace LeaveManagementSystem
{
    // Extension methods for dynamic sorting
    public static class QueryableExtensions
    {
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string propertyName)
        {
            var entityType = typeof(T);
            var propertyInfo = entityType.GetProperty(propertyName);
            
            if (propertyInfo == null)
                return query; // Return unsorted if property doesn't exist
                
            var parameter = Expression.Parameter(entityType, "x");
            var property = Expression.Property(parameter, propertyInfo);
            var lambda = Expression.Lambda(property, parameter);
            
            var methodName = "OrderBy";
            var methodCallExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { entityType, propertyInfo.PropertyType },
                query.Expression,
                Expression.Quote(lambda));
                
            return query.Provider.CreateQuery<T>(methodCallExpression);
        }
        
        public static IQueryable<T> OrderByDescendingDynamic<T>(this IQueryable<T> query, string propertyName)
        {
            var entityType = typeof(T);
            var propertyInfo = entityType.GetProperty(propertyName);
            
            if (propertyInfo == null)
                return query; // Return unsorted if property doesn't exist
                
            var parameter = Expression.Parameter(entityType, "x");
            var property = Expression.Property(parameter, propertyInfo);
            var lambda = Expression.Lambda(property, parameter);
            
            var methodName = "OrderByDescending";
            var methodCallExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { entityType, propertyInfo.PropertyType },
                query.Expression,
                Expression.Quote(lambda));
                
            return query.Provider.CreateQuery<T>(methodCallExpression);
        }
    }
}

namespace LeaveManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly DataContext _context;

        public LeaveRequestsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/leaverequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetLeaveRequests()
        {
            return await _context.LeaveRequests.Include(lr => lr.Employee).ToListAsync();
        }

        // GET: api/leaverequests/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveRequest>> GetLeaveRequest(int id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null) return NotFound();
            return leaveRequest;
        }

        // POST: api/leaverequests
        [HttpPost]
        public async Task<ActionResult<LeaveRequest>> PostLeaveRequest(LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLeaveRequest), new { id = leaveRequest.Id }, leaveRequest);
        }

        // PUT: api/leaverequests/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLeaveRequest(int id, LeaveRequest leaveRequest)
        {
            if (id != leaveRequest.Id) return BadRequest();
            _context.Entry(leaveRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/leaverequests/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeaveRequest(int id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null) return NotFound();
            _context.LeaveRequests.Remove(leaveRequest);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // NEW ENDPOINT: GET /api/leaverequests/filter
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<object>>> FilterLeaveRequests(
            [FromQuery] int? employeeId,
            [FromQuery] LeaveType? leaveType,
            [FromQuery] LeaveStatus? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] string sortOrder = "asc")
        {
            var query = _context.LeaveRequests.AsQueryable();

            // Apply filters
            if (employeeId.HasValue) query = query.Where(lr => lr.EmployeeId == employeeId);
            if (leaveType.HasValue) query = query.Where(lr => lr.LeaveType == leaveType);
            if (status.HasValue) query = query.Where(lr => lr.Status == status);
            if (startDate.HasValue) query = query.Where(lr => lr.StartDate >= startDate);
            if (endDate.HasValue) query = query.Where(lr => lr.EndDate <= endDate);
            if (!string.IsNullOrEmpty(keyword)) query = query.Where(lr => lr.Reason.Contains(keyword));

            // Sorting
            query = sortOrder.ToLower() == "asc"
                ? query.OrderByDynamic(sortBy)
                : query.OrderByDescendingDynamic(sortBy);

            // Pagination
            var totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new { totalItems, items });
        }
    }
}