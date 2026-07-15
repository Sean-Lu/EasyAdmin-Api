using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data;
using System.Reflection;

namespace EasyAdmin.Test;

[TestClass]
public class NoteCategoryTransactionTests
{
    [TestMethod]
    public async Task DefaultCategoryCreation_UsesProvidedTransaction()
    {
        var categoryRepository = new Mock<INoteCategoryRepository>();
        categoryRepository.Setup(repository => repository.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<NoteCategoryEntity, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<NoteCategoryEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((NoteCategoryEntity?)null);
        categoryRepository.Setup(repository => repository.AddAsync(
                It.IsAny<NoteCategoryEntity>(),
                It.IsAny<bool>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<NoteCategoryEntity, object>>>(),
                It.IsAny<IDbTransaction>()))
            .Callback<NoteCategoryEntity, bool, System.Linq.Expressions.Expression<Func<NoteCategoryEntity, object>>, IDbTransaction>((entity, _, _, _) => entity.Id = 9)
            .ReturnsAsync(true);

        TenantContextHolder.UserInfo = new JwtUserModel { TenantId = 1, UserId = 2 };
        try
        {
            var service = CreateService(categoryRepository.Object);
            var transaction = new Mock<IDbTransaction>().Object;
            var method = typeof(NoteService).GetMethod("NormalizeCategoryIdAsync", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            Assert.AreEqual(2, method.GetParameters().Length, "默认分类解析必须接收外层事务");

            var task = (Task<long>)method.Invoke(service, new object?[] { 0L, transaction })!;
            var categoryId = await task;

            Assert.AreEqual(9, categoryId);
            categoryRepository.Verify(repository => repository.AddAsync(
                It.IsAny<NoteCategoryEntity>(),
                It.IsAny<bool>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<NoteCategoryEntity, object>>>(),
                transaction), Times.Once);
        }
        finally
        {
            TenantContextHolder.Clear();
        }
    }

    private static NoteService CreateService(INoteCategoryRepository categoryRepository)
    {
        return new NoteService(
            Mock.Of<ILogger<NoteService>>(),
            Mock.Of<IMapper>(),
            Mock.Of<INoteRepository>(),
            categoryRepository,
            Mock.Of<INoteTagRepository>(),
            Mock.Of<INoteTagRelationRepository>(),
            Mock.Of<INoteTagService>(),
            Mock.Of<INotePasswordService>(),
            Mock.Of<INoteMarkdownService>(),
            Mock.Of<IFileService>(),
            Mock.Of<EasyAdmin.Infrastructure.Storage.IFileStorageFactory>());
    }
}