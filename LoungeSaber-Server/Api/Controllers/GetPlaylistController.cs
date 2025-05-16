using System.Net;
using System.Text;
using LoungeSaber_Server.Models.Maps;
using LoungeSaber_Server.SQL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoungeSaber_Server.Api.Controllers;

[ApiController]
[Route("api/playlist")]
public class GetPlaylistController
{
    [HttpGet("all")]
    public IResult GetAllSongs()
    {
        var playlistSongs = MapData.Instance.GetAllMaps().Select(i => i.GetPlaylistMap());
        
        var playlistObject = new JObject
        {
            {"playlistTitle","LoungeSaber Maps"},
            {"playlistAuthor", "LoungeSaber Team"},
            {"playlistDescription", "All maps that can be played in LoungeSaber"},
            {"songs", JArray.FromObject(playlistSongs)},
            {"syncURL", "https://localhost:7198/api/playlist/all"}
        };

        return Results.File(Encoding.UTF8.GetBytes(playlistObject.ToString()), "application/json", "loungesaber-playlist.bplist");
    }
}