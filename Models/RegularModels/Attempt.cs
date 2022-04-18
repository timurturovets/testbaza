﻿using TestBaza.Extensions;

namespace TestBaza.Models
{
    public class Attempt
    {
        public int AttemptId { get; set; }

        public DateTime TimeStarted { get; set; }
        public DateTime TimeEnded { get; set; }
        public bool IsEnded { get; set; }
        public IEnumerable<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        public int PassingInfoId { get; set; }
        public PassingInfo? PassingInfo { get; set; }

        public AttemptJsonModel ToJsonModel()
        {
            return new AttemptJsonModel
            {
                TimeLeft = PassingInfo!.Test!.TimeLimit - (int)(DateTime.Now - TimeStarted).TotalSeconds,
                UserAnswers = UserAnswers.Select(a => a.ToJsonModel()),
                Test = PassingInfo?.Test?.ToJsonModel(includeAnswers: false)
            };
        }

        public DetailedPassedTest ToDetailedTest()
        {
            return new DetailedPassedTest
            {
                TestName = PassingInfo!.Test!.TestName,
                UserAnswers = UserAnswers.Select(a => a.ToJsonModel()),
                Questions = PassingInfo!.Test!.Questions.Select(q => q.ToJsonModel())
            };
        }

        public UserStatSummary ToStatSummary() 
            => new (PassingInfo!.User!.UserName, 
                AttemptId, 
                TimeEnded.ToMskTimeString());
        
    }
}