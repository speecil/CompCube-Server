using LoungeSaber_Server.MatchRoom;
using LoungeSaber_Server.SkillDivision;
using Microsoft.AspNetCore.Mvc;

namespace LoungeSaber_Server.Api.Controllers;

[ApiController]
[Route("/api/divisions")]
public class GetDivisionsController
{
    [HttpGet]
    public ActionResult<Division[]> GetDivisions() => DivisionManager.Divisions;
}