using Microsoft.AspNetCore.Mvc;
using SlowPokeIMS.Web.Controllers.Attributes;

namespace SlowPokeIMS.Web.Controllers;


[ShowInNavBar("Start")]
public class StartController : Controller
{
    [ShowInNavBar("New Text Note"), HttpGet("new/text-note")]
    public ActionResult NewTextNote() => RedirectToAction(nameof(TextDocController), "TextDoc");
    
    // [ShowInNavBar("New Image Note"), HttpGet("new/image-note")]
    // public ActionResult NewImageNote() => RedirectToAction(nameof(ImageDocController), "TextDoc");
    
    // [ShowInNavBar("New Sound Note"), HttpGet("new/sound-note")]
    // public ActionResult NewSoundNote() => RedirectToAction(nameof(SoundDocController), "TextDoc");
}