# For.TemplateParser
高效能範本文字動態取代工具

不支援集合！！

# 使用方式
* 範本
```csharp
            var template = "Hi! {.Name}, your father name is {.Details.Father.Name}{.Details.Mother.Name}, your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}";
```
* 建立範本使用的模型
```csharp
            var obj = new TestModel()
            {
                Name = "Ricky",
                Age = 25,
                StandardDateTime = DateTime.Parse("2017/08/01"),
                OffsetDateTime = DateTimeOffset.Parse("2017/08/02"),
                Details = new Detail()
                {
                    Id = 0,
                    Mother = new Parent()
                    {
                        Name = "Mary",
                        Age = 50
                    },
                    Father = new Parent()
                    {
                        Name = "Eric",
                        Age = 51
                    }
                }
            };
```
* 宣告掃描器
```csharp
            var provider = new TemplateParser(new For.TemplateParser.Models.TemplateParserConfig()
            {
                DateTimeFormat = "yyyyMMdd",
                DateTimeOffsetFormat = "yyyy/MM/dd"
            });

```
* 執行並取得結果
```csharp
            var resultA = provider.BuildTemplate(obj, template);
```

# 更新紀錄
* V1.0.0 Preview
  ** 初版，目前將建立cache、取得結果做為同一個程式，未來將會以單一職責抽離
