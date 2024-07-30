using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using API.Dto;
using API.Data.Entities;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using API.Context;
using SixLabors.ImageSharp;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using API.Model;
using elFinder.NetCore.Helpers;

namespace API.Controllers
{
    [Route("api/soas")]
    [ApiController]
    [Authorize]
    public class SoaController : ControllerBase
    {
        private readonly SoftGED_DBContext _db;
        private readonly string _connectionString;
        private readonly IMapper _mapper;

        public SoaController(IMapper mapper, SoftGED_DBContext db, IConfiguration configuration)
        {
            _mapper = mapper;
            _db = db;
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
        }

        [HttpGet("")]
        public async Task<ActionResult> Get()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var query = _db.Soas.Where(x => x.DeletionDate == null);

            var projects = await query.ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var soa = await _db.Soas.FirstOrDefaultAsync(project => project.Id == id && project.DeletionDate == null);
            return Ok(soa);
        }

        [HttpPost("")]
        public async Task<ActionResult> ProjectToAdd(SoaToAdd projectToAdd)
        {
            if (_db.Soas.Any(project => project.Name == projectToAdd.Name && project.DeletionDate == null))
                return BadRequest("Le SOA existe déjà!");

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO Soas (Name, CreationDate)
                VALUES (@name, @CreationDate)
            ", conn);

            cmd.Parameters.AddWithValue("@name", projectToAdd.Name);
            cmd.Parameters.AddWithValue("@CreationDate", DateTime.Now);

            await cmd.ExecuteNonQueryAsync();

            return Ok();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(int id, SoaToUpdate projectToUpdate)
        {
            if (_db.Soas.Any(project => project.Name == projectToUpdate.Name && project.DeletionDate == null))
                return BadRequest("Le SOA existe déjà!");

            var project = await _db.Soas.FirstOrDefaultAsync(project => project.Id == id && project.DeletionDate == null);

            project.Name = projectToUpdate.Name;

            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Soas SET DeletionDate = GETDATE()
                WHERE Id = @Id
            ", conn);

            cmd.Parameters.AddWithValue("@Id", id);

            await cmd.ExecuteNonQueryAsync();

            return Ok();
        }
    }
}
