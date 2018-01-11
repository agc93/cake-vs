// NOTE: This file should only remain local while pantry is in development. Once released, replace this with a NuGet reference
#addin nuget:?package=Cake.Git&version=0.16.1
#addin nuget:?package=Cake.Handlebars&version=0.2.0

static DirectoryPath _templatesDirPath = "./.templates";

void SetTemplatesDirectoryPath(DirectoryPath templateDirectoryPath) {
    _templatesDirPath = templateDirectoryPath;
}

void CleanTemplatesDirectory() {
    if (DirectoryExists(_templatesDirPath)) {
        var files = GetFiles(_templatesDirPath + "/**/*");
        foreach (var file in files) {
            System.IO.File.SetAttributes(file.FullPath, FileAttributes.Normal);
        }
        DeleteDirectory(_templatesDirPath, new DeleteDirectorySettings { Force = true, Recursive = true });
    }
}

void CloneTemplates() {
    GitClone(
        "https://github.com/cake-build/pantry.git",
        _templatesDirPath
    );
}

void WriteTemplateToFile(string template, FilePath targetFile) {
  using (var writer = new StreamWriter(Context.FileSystem.GetFile(targetFile).OpenWrite())) {
    writer.Write(template);
  }
}

void GenerateTemplateFile(FilePath templateFilePath, object data, FilePath targetFile) {
    templateFilePath = FileExists(templateFilePath)
        ? templateFilePath
        : templateFilePath.ToTemplatePath();
    var template = RenderTemplateFromFile(templateFilePath, model);
    WriteTemplateToFile(template, targetFile);
}

static FilePath ToTemplatePath(this FilePath templateFilePath) {
    return _templatesDirPath.Combine("./template").CombineWithFilePath(templateFilePath);
}

Task("Clone-Templates")
.Does(() => {
    CloneTemplates ();
});