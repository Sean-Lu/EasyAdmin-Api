using EasyAdmin.Application.Dtos;
using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Test;

[TestClass]
public class NoteMarkdownExportRequestTests
{
    [TestMethod]
    public void Request_IsValid_WithoutExportType()
    {
        var request = new NoteMarkdownExportReqDto { Id = 1 };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.IsTrue(isValid);
        Assert.IsFalse(validationResults.Any(result => result.MemberNames.Contains("ExportType")));
    }
}
