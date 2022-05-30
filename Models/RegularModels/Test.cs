using TestBaza.Extensions;
using TestBaza.Models.DTOs;
using TestBaza.Models.Summaries;
using TestBaza.Models.JsonModels;
// ReSharper disable ValueParameterNotUsed
// ReSharper disable UnusedMember.Global

namespace TestBaza.Models.RegularModels;

public class Test
{
    public int TestId { get; set; }

    public string? TestName { get; set; }
    public string? Description { get; set; } 
    public string? ImageRoute { get; set; }

    public bool HasImage
    {
        get => !string.IsNullOrEmpty(ImageRoute);
        set { }
    }

    public string? ImagePhysicalPath { get; set; }
    public DateTime TimeCreated { get; set; }
    public bool AreAttemptsLimited { get; set; }
    public int AllowedAttempts { get; set; } = 1;
    public string? Link { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsTimeLimited { get; set; }
    public bool IsPublished { get; set; }

    public bool IsBrowsable
    {
        get => IsPublished && !IsPrivate;
        set { }
    }

    public bool AreAnswersManuallyChecked { get; set; }

    /// <summary>
    ///     Временное ограничение на прохождение теста, выраженное в секундах
    /// </summary>
    public int TimeLimit { get; set; }

    public IEnumerable<Question> Questions { get; set; } = new List<Question>();
    public IEnumerable<Rate> Rates { get; set; } = new List<Rate>();
    public IEnumerable<PassingInfo> PassingInfos { get; set; } = new List<PassingInfo>();
    public string? CreatorId { get; set; }
    public User? Creator { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is Test test)
            return test.TestId == TestId
                   && test.TestName == TestName
                   && test.TimeCreated == TimeCreated;
        return false;
    }

    //ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => TestId.GetHashCode();
    

    public TestJsonModel ToJsonModel(bool includeAnswers = true)
    {
        return new TestJsonModel
        {
            TestId = TestId,
            TestName = TestName,
            Description = Description,
            ImageRoute = ImageRoute,
            HasImage = HasImage,
            AuthorName = Creator!.UserName,
            IsPrivate = IsPrivate,
            TimeInfo = new TimeInfo(IsTimeLimited, TimeLimit),
            Questions = Questions.Select(q => q.ToJsonModel(includeAnswers)),
            AllowedAttempts = AllowedAttempts,
            AreAttemptsLimited = AreAttemptsLimited,
            AreAnswersManuallyChecked = AreAnswersManuallyChecked
        };
    }

    public TestSummary ToSummary()
    {
        return new TestSummary
        {
            TestId = TestId,
            TestName = TestName,
            AuthorName = Creator!.UserName,
            Link = Link,
            QuestionsCount = Questions.Count(),
            TimeCreated = TimeCreated.ToMskTimeString(),
            RatesCount = Rates.Count(),
            AverageRate = Rates.Any() ? Rates.Select(r => r.Value).Average() : 0,
            IsBrowsable = IsBrowsable,
            IsPublished = IsPublished,
            TimeInfo = new TimeInfo(IsTimeLimited, TimeLimit),
            AllowedAttempts = AllowedAttempts,
            AreAttemptsLimited = AreAttemptsLimited,
            AreAnswersManuallyChecked = AreAnswersManuallyChecked
        };
    }

    public void Update(UpdateTestDto model)
    {
        TestName = model.TestName;
        Description = model.Description;
        IsPrivate = model.IsPrivate;
        AllowedAttempts = model.AllowedAttempts;
        AreAttemptsLimited = model.AreAttemptsLimited;
        AreAnswersManuallyChecked = model.AreAnswersManuallyChecked;

        var timeInfo = model.TimeInfo;
        IsTimeLimited = timeInfo.IsTimeLimited;
        TimeLimit = timeInfo.ConvertToSeconds();
    }

    public async void UpdateImage(IFormFile? image, IWebHostEnvironment environment)
    {
        if (HasImage)
        {
            if (File.Exists(ImagePhysicalPath)) File.Delete(ImagePhysicalPath);

            if (image is null)
            {
                ImageRoute = null;
                ImagePhysicalPath = null;
                return;
            }

            await using var stream = new FileStream(ImagePhysicalPath, FileMode.Create);
            await image.CopyToAsync(stream);
        }
        else
        {
            if (image is null) return;
            
            var fileRoute = Guid.NewGuid().ToString()[..7] + image.FileName;
            
            var imageLink = $"/images/tests/{fileRoute}";
            
            var pathToImage = Path.Combine(
                environment.WebRootPath,
                "images",
                "tests",
                fileRoute
                );
            
            ImageRoute = imageLink;
            ImagePhysicalPath = pathToImage;
            
            await using var stream = new FileStream(pathToImage, FileMode.Create);
            await image.CopyToAsync(stream);
        }
    }
}