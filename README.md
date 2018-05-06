# For.TemplateEngine
高效能範本文字動態取代工具<br>
可使用在任何需要預先或是讓使用者自行編輯的案例底下<br>
例如：簡單的操作Log紀錄、Mail Template、Notification Template...等

**不支援集合！！**

# 使用方式
* 範本
```csharp
            var template = "Hi! {.Name}, your father name is {.Details.Father.Name}{.Details.Mother.Name}, your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}";
```
* 宣告掃描器
```csharp
            var provider = new TemplateEngine(new For.TemplateEngine.Models.TemplateEngineConfig()
            {
                DateTimeFormat = "yyyyMMdd",
                DateTimeOffsetFormat = "yyyy/MM/dd"
            });
```
* 註冊範本與模型
```csharp
            provider.RegisterTemplate<TestModel>(template);
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

* 執行並取得結果
```csharp
            var resultA = provider.Render(obj);
```

# 更新紀錄
* v1.1.2
    * 加入動態註冊方式，可使用匿名型別註冊同時取回結果
* v1.1.1
    * 轉為 .net standard 專案，目前僅相容於.net framework
* V1.1.0
    * 注冊與執行，分開使用
    
* V1.0.0 Preview
    * 初版，目前將建立cache、取得結果做為同一個程式，未來將打算以單一職責抽離(真的就打算而已)
