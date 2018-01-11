// #addin nuget:?package=Cake.Git&version=0.16.1
// #addin nuget:?package=Cake.Handlebars&version=0.2.0
#load "local:?path=build/pantry.cake"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Build-Templates");
var configuration = Argument("configuration", "Release");
var model = new {
    source = "created with Cake for Visual Studio",
    cake = new {
      version = "0.24.0",
      target = "netstandard1.6;net46",
      frosting = new {
        context = "<%= settingsType %>",
        lifetime = "<%= lifetimeType %>",
        tasks = new[] {
          new {
            name = "Default",
            skipRun = true,
            runBody = $"// If you don't inherit from FrostingTask<MySettings>{Environment.NewLine}    // the standard ICakeContext will be provided.{Environment.NewLine}",
            dependencies = new[] { "<%= taskName %>" }
          },
          new {
            name = "<%= taskName %>",
            skipRun = false,
            runBody = "", // this is needed for anonymous types to not bork out. Templates don't *need* this property
            dependencies = new string[0] // as above
          }
        }
      }
    },
    project = new {
        name = "$safeprojectname$",
        target = "" //just used for Frosting
    },
    test = new {}
  };

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
  SetTemplatesDirectoryPath("./.templates");
  CleanTemplatesDirectory();
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Render-Script-Template")
.IsDependentOn("Clone-Templates")
.Does(() => {
  GenerateTemplateFile("build.cake.hbs", model, "./template/ItemTemplate/build.cake");
//   WriteTemplateToFile(template, "./template/ItemTemplate/build.cake");
});

Task("Render-Addin-Template")
.IsDependentOn("Clone-Templates")
.Does(() => {
  var csTemplate = RenderTemplateFromFile("./.templates/template/addin/Aliases.cs.hbs", model);
  WriteTemplateToFile(csTemplate, "./template/AddinTemplate/Aliases.cs");
  var projTemplate = RenderTemplateFromFile("./.templates/template/addin/CakeAddin.csproj.hbs", model);
  WriteTemplateToFile(projTemplate, "./template/AddinTemplate/AddinTemplate.csproj");
});

Task("Render-Addin-Test-Template")
.IsDependentOn("Clone-Templates")
.Does(() => {
  GenerateTemplateFile("./.templates/template/addin-test/AddinTests.cs.hbs", model, "./template/AddinTestTemplate/AddinTests.cs");
  GenerateTemplateFile("./.templates/template/addin-test/CakeFixture.cs.hbs", model, "./template/AddinTestTemplate/CakeFixture.cs");
  GenerateTemplateFile("./.templates/template/addin-test/CakeTool.cs.hbs", model, "./template/AddinTestTemplate/CakeTool.cs");
  GenerateTemplateFile("./.templates/template/addin-test/", model, "./template/AddinTestTemplate/CakeToolSettings.cs");
  GenerateTemplateFile("./.templates/template/addin/CakeAddin.csproj.hbs", model, "./template/AddinTemplate/ProjectTemplate.csproj");
});

Task("Render-Addin-TestBasic-Template")
.IsDependentOn("Clone-Templates")
.Does(() => {
  GenerateTemplateFile("addin-test-basic/AddinTests.cs.hbs", model, "./template/AddinTestBasicTemplate/AddinTests.cs");
  GenerateTemplateFile("addin-test-basic/CakeAddin.Tests.csproj.hbs", model, "./template/AddinTestBasicTemplate/ProjectTemplate.csproj");
});

Task("Render-Addin-Test-Templates")
.IsDependentOn("Render-Addin-Test-Template")
.IsDependentOn("Render-Addin-TestBasic-Template");

Task("Render-Modules-Template")
.IsDependentOn("Clone-Templates")
.Does(() => {
  GenerateTemplateFile("module/ReverseLog.cs.hbs", model, "./template/ModuleTemplate/ReverseLog.cs");
  GenerateTemplateFile("module/SampleLogModule.cs.hbs", model, "./template/ModuleTemplate/SampleLogModule.cs");
  GenerateTemplateFile("module/Cake.Sample.Module.cs.hbs", model, "./template/ModuleTemplate/ProjectTemplate.csproj");
});

Task("Build-Templates")
.IsDependentOn("Render-Script-Template")
.IsDependentOn("Render-Addin-Template")
.IsDependentOn("Render-Addin-Test-Templates")
.IsDependentOn("Render-Modules-Template");

Task("Default")
.IsDependentOn("Build-Templates");

RunTarget(target);