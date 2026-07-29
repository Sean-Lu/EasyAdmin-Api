using System.Text;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Converter;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Serialization;

namespace EasyAdmin.Test;

[TestClass]
public class AiChatControllerTests
{
    [TestMethod]
    public async Task Stream_UsesCamelCaseAndSerializesLongIdsAsStrings()
    {
        var chatService = new Mock<IAiChatService>();
        chatService
            .Setup(item => item.StreamAsync(
                It.IsAny<AiChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .Returns(StreamEvents(
                new AiStreamEventDto
                {
                    Type = "sources",
                    Data = new List<AiStreamSourceDto>
                    {
                        new AiStreamSourceDto
                        {
                            Number = 1,
                            SourceType = AiSourceType.Note,
                            SourceId = 9007199254740993,
                            Title = "source"
                        }
                    }
                }));
        var responseBody = new MemoryStream();
        var jsonOptions = new MvcNewtonsoftJsonOptions();
        jsonOptions.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        jsonOptions.SerializerSettings.Converters.Add(new JsonLongToStringConverter());
        var controller = new AiChatController(
            Mock.Of<IAiConversationService>(),
            chatService.Object,
            Mock.Of<IAiToolService>(),
            Options.Create(jsonOptions))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.Response.Body = responseBody;

        await controller.Stream(new AiChatRequestDto { ConversationId = 1, Prompt = "hello" });

        var response = Encoding.UTF8.GetString(responseBody.ToArray());
        StringAssert.Contains(response, "\"number\":1");
        StringAssert.Contains(response, "\"sourceId\":\"9007199254740993\"");
        Assert.IsFalse(response.Contains("\"SourceId\"", StringComparison.Ordinal));
    }

    private static async IAsyncEnumerable<AiStreamEventDto> StreamEvents(
        params AiStreamEventDto[] events)
    {
        foreach (var item in events)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }
}
