# 代码生成器使用说明文档

> EasyAdmin 代码生成器 2.0：三种生成模式，灵活适配任意场景。

***

## 一、三种模式对比分析

| 对比维度        | 数据库模式 (DbFirst) | 代码解析模式 (CodeFirst)                             | 配置模式 (Config)  |
| ----------- | --------------- | ---------------------------------------------- | -------------- |
| **元数据来源**   | 数据库表结构          | Entity 源码解析                                    | 用户手动填写         |
| **适用场景**    | 已有数据库，需要生成全套代码  | 已有 Entity 类定义，生成 Repository/Service/Controller | 快速原型、不依赖任何外部资源 |
| **需要什么**    | 数据库连接 + 已存在的表   | C# 或 Java Entity 源码                            | 什么都不需要         |
| **列信息获取**   | 自动查询数据库元数据      | 自动解析 Entity 属性                                 | 手动配置（可选）       |
| **上手难度**    | 需配置数据库连接        | 需粘贴/上传源码                                       | 最低，即填即用        |
| **精确度**     | 100%（数据库说了算）    | 高（取决于解析器准确性）                                   | 100%（用户说了算）    |
| **灵活性**     | 低               | 中                                              | 最高             |
| **对现有功能影响** | 无               | 无                                              | 无              |

***

## 二、统一架构流程图

```
┌──────────────────────────────────────────────────────────────────────┐
│                         代码生成器 - 三模式统一架构                      │
│                                                                       │
│  ┌─────────────────┐   ┌──────────────────┐   ┌──────────────────┐   │
│  │  数据库模式       │   │  代码解析模式      │   │  配置模式         │   │
│  │  (DbFirst)       │   │  (CodeFirst)      │   │  (Config)        │   │
│  │                  │   │                   │   │                   │   │
│  │ DB连接 → SQL查询  │   │ .cs/.java源码     │   │ 用户直接填写       │   │
│  │       │          │   │       │           │   │       │           │   │
│  │       ▼          │   │       ▼           │   │       ▼           │   │
│  │ DbTableInfoDto   │   │ CSharpEntityParser │  │ CodeGenConfigReq  │   │
│  │ DbColumnInfoDto  │   │ JavaEntityParser  │   │ Dto               │   │
│  └────────┬─────────┘   └────────┬──────────┘   └────────┬──────────┘   │
│           │                      │                        │              │
│           └──────────────────────┼────────────────────────┘              │
│                                  ▼                                       │
│                       ┌────────────────────┐                             │
│                       │  统一模板变量上下文   │                             │
│                       │  { ClassName,        │                             │
│                       │    TableName,        │                             │
│                       │    InstanceName,     │                             │
│                       │    Columns[],        │                             │
│                       │    PackageName,      │                             │
│                       │    ModuleName,       │                             │
│                       │    Author, Date }    │                             │
│                       └──────────┬───────────┘                            │
│                                  │                                       │
│                                  ▼                                       │
│                       ┌────────────────────┐                             │
│                       │  Handlebars 模板引擎 │                             │
│                       └──────────┬───────────┘                            │
│                                  │                                       │
│                                  ▼                                       │
│                       ┌────────────────────┐                             │
│                       │  生成代码文件列表    │                             │
│                       └────────────────────┘                             │
└──────────────────────────────────────────────────────────────────────┘
```

***

## 三、三种模式详细介绍

### 3.1 数据库模式 (DbFirst)

#### 适用场景

- 数据库已存在，需要为表生成 Entity、Repository、Service、Controller 等全套代码
- 适合传统开发流程：先设计数据库 → 再写代码

#### 操作流程

1. 选择「数据库模式」标签
2. 左侧面板：选择/刷新数据库连接配置 → 选择要生成代码的数据表
3. 右侧面板：选择模板分类 → 勾选需要的代码模板 → 配置包名/模块名/作者/表前缀
4. 点击「生成代码」
5. 查看生成结果，可逐个下载或打包下载 ZIP

***

### 3.2 代码解析模式 (CodeFirst)

#### 适用场景

- 已经定义了 Entity 类（C# 或 Java），需要根据 Entity 生成配套的 Repository、Service、Controller
- Entity 类可以是来自任意项目：本项目、其他 C# 项目、Java 项目，没有框架限制
- 适合 CodeFirst 开发流程：先定义 Entity → 再生成配套基础设施代码

#### 操作流程

1. 选择「代码解析模式」标签
2. 选择语言（C# / Java）
3. **粘贴源码**或**上传 .cs/.java 文件**
4. 点击「解析预览」，系统将展示解析结果：
   - 类名、表名、命名空间
   - 属性列表（属性名、类型、描述、是否主键等）
5. 上方选择模板分类和模板
6. 配置包名/模块名/作者（无需表前缀）
7. 点击「生成代码」
8. 查看生成结果并下载

#### 解析器覆盖的 Entity 特性/注解

##### C# Entity

- `[Table("tableName")]` / `[TableAttribute]` → 自定义表名
- `[Key]` / `[KeyAttribute]` → 主键标记
- `[DatabaseGenerated]` / `[DatabaseGeneratedAttribute]` → 自增标识
- `<summary>` XML 文档注释 → 表/列描述
- Nullable 引用类型 → 可空标记
- 自动过滤 `EntityBase` 基类属性（如 `Id`, `CreateTime`, `UpdateTime` 等）

##### Java Entity

- `@Table(name = "table_name")` → 自定义表名
- `@Id` → 主键标记
- `Javadoc` 注释 → 类描述
- 单行 `//` 注释 → 字段描述
- 自动过滤基类字段（`id`, `createTime`, `updateTime`, `isDelete` 等）

***

### 3.3 配置模式 (Config)

#### 适用场景

- 最灵活的生成方式，不依赖任何外部资源（无需数据库、无需 Entity 源码）
- 适合快速原型开发
- 模板只用到部分变量时，只需填写会用到的几个变量即可

#### 操作流程

1. 选择「配置模式」标签
2. 填写模板变量：

| 变量                 | 说明                      | 必填 |
| ------------------ | ----------------------- | -- |
| 类名 (ClassName)     | 类名，如 `User`、`Product`   | 是  |
| 实例名 (InstanceName) | 首字母小写的实例名，默认自动推导        | 否  |
| 表名 (TableName)     | 数据库表名，默认使用类名            | 否  |
| 表注释 (TableComment) | 表注释/描述，默认使用类名           | 否  |
| 包名 (PackageName)   | 包名/命名空间，如 `com.example` | 否  |
| 模块名 (ModuleName)   | 模块名，如 `system`          | 否  |
| 作者 (Author)        | 代码作者                    | 否  |

1. **列配置（可选）**：如果模板用到了 `Columns` 变量（如 Entity 模板、数据库相关模板），点击「添加列」配置属性：
   - 属性名、字段名
   - C# 类型：`string`、`int`、`long`、`decimal`、`bool`、`DateTime`、`Guid`（及其可空版本）
   - Java 类型：`String`、`Integer`、`Long`、`BigDecimal`、`Boolean`、`Date`
   - 描述、是否主键
2. 上方选择模板分类和模板
3. 点击「生成代码」
4. 查看结果并下载

#### 最小配置示例

如果模板只需要 `ClassName` 变量（比如生成一个简单的 DTO 模板），只需要填入类名即可生成，其他变量留空系统会使用默认值：

```
类名: User
→ 自动推导: InstanceName=user, TableName=User, TableComment=User
```

***

## 四、模板体系

代码生成器使用 **Handlebars** 模板引擎。新增了以下模板，分布在现有分类中：

### 模板变量一览

| 变量名                 | 说明         | 示例            |
| ------------------- | ---------- | ------------- |
| `{{ClassName}}`     | 类名         | `User`        |
| `{{InstanceName}}`  | 实例名（首字母小写） | `user`        |
| `{{TableName}}`     | 数据表名       | `user`        |
| `{{TableComment}}`  | 表注释        | `用户表`         |
| `{{PackageName}}`   | 包名 / 命名空间  | `com.example` |
| `{{ModuleName}}`    | 模块名        | `system`      |
| `{{Author}}`        | 作者         | `Sean`        |
| `{{Date}}`          | 当前日期       | `2026-05-20`  |
| `{{#each Columns}}` | 列遍历        | 每列包含以下字段      |
| `{{PropertyName}}`  | 属性名        | `Name`        |
| `{{FieldName}}`     | 字段名（驼峰）    | `name`        |
| `{{ColumnName}}`    | 数据库列名      | `name`        |
| `{{ColumnComment}}` | 列注释        | `用户名称`        |
| `{{CSharpType}}`    | C# 类型      | `string`      |
| `{{JavaType}}`      | Java 类型    | `String`      |
| `{{IsNullable}}`    | 是否可空       | `true/false`  |
| `{{IsKey}}`         | 是否主键       | `true/false`  |
| `{{IsIdentity}}`    | 是否自增       | `true/false`  |

### 模板分类

| 分类           | 模板              | 路径示例                                               |
| ------------ | --------------- | -------------------------------------------------- |
| **Java项目模板** | 实体类             | `entity/{{ClassName}}.java`                        |
| <br />       | Service接口       | `service/I{{ClassName}}Service.java`               |
| <br />       | Service实现       | `service/impl/{{ClassName}}ServiceImpl.java`       |
| <br />       | Controller      | `controller/{{ClassName}}Controller.java`          |
| <br />       | Repository接口（新） | `repository/{{ClassName}}Repository.java`          |
| <br />       | Repository实现（新） | `repository/impl/{{ClassName}}RepositoryImpl.java` |
| **C#项目模板**   | 实体类             | `Entities/{{ClassName}}Entity.cs`                  |
| <br />       | Repository接口（新） | `Contracts/I{{ClassName}}Repository.cs`            |
| <br />       | Repository实现（新） | `Repositories/{{ClassName}}Repository.cs`          |
| <br />       | Service接口（新）    | `Contracts/I{{ClassName}}Service.cs`               |
| <br />       | Service实现（新）    | `Services/{{ClassName}}Service.cs`                 |
| <br />       | Controller（新）   | `Controllers/{{ClassName}}Controller.cs`           |

***

## 五、关键技术说明

### 后端架构

- 核心框架：.NET 8.0
- 模板引擎：Handlebars.Net
- C# 源码解析：Microsoft.CodeAnalysis.CSharp (Roslyn)
- Java 源码解析：正则表达式模式匹配

### 前端架构

- 框架：React 18
- UI 组件库：Ant Design 6.3.6
- 语言：TypeScript

### 设计原则

1. **对现有功能零影响**：DbFirst 模式所有代码保持不变
2. **模板引擎完全复用**：三种模式共享同一套 Handlebars 模板渲染管线
3. **解析与生成分离**：代码解析模式先解析预览，用户确认后再生成
4. **最简配置原则**：配置模式只需填写必要的变量，未填的自动推导默认值

