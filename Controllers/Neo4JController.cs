using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using System.Threading.Tasks;

namespace Neo4J.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly Neo4jService _neo4jService;

        public PersonController(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePerson(string name, int age)
        {
            await using var session = _neo4jService.GetSession();
            var result = await session.WriteTransactionAsync(async tx =>
            {
                var cursor = await tx.RunAsync("CREATE (p:Person {name: $name, age: $age}) RETURN p", new { name, age });
                return await cursor.SingleAsync();
            });

            var person = result["p"].As<INode>();

            return Ok(new
            {
                Name = person.Properties["name"].As<string>(),
                Age = person.Properties["age"].As<int>()
            });
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetPerson(string name)
        {
            await using var session = _neo4jService.GetSession();

            var records = await session.ReadTransactionAsync(async tx =>
            {
                var cursor = await tx.RunAsync("MATCH (p:Person {name: $name}) RETURN p", new { name });
                return await cursor.ToListAsync();
            });

            if (records.Count == 0)
            {
                return NotFound();
            }

            var person = records[0]["p"].As<INode>();

            return Ok(new
            {
                Name = person.Properties["name"].As<string>(),
                Age = person.Properties["age"].As<int>()
            });
        }

        [HttpPut("{name}")]
        public async Task<IActionResult> UpdatePerson(string name, int age)
        {
            await using var session = _neo4jService.GetSession();
            var result = await session.WriteTransactionAsync(async tx =>
            {
                var cursor = await tx.RunAsync("MATCH (p:Person {name: $name}) SET p.age = $age RETURN p", new { name, age });
                return await cursor.SingleAsync();
            });

            var person = result["p"].As<INode>();

            return Ok(new
            {
                Name = person.Properties["name"].As<string>(),
                Age = person.Properties["age"].As<int>()
            });
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> DeletePerson(string name)
        {
            await using var session = _neo4jService.GetSession();
            var result = await session.WriteTransactionAsync(async tx =>
            {
                var cursor = await tx.RunAsync("MATCH (p:Person {name: $name}) DELETE p RETURN COUNT(p) AS count", new { name });
                return await cursor.SingleAsync();
            });

            var count = result["count"].As<int>();

            return Ok(new { DeletedCount = count });
        }
    }
}
