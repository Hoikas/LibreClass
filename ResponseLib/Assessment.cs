using System;
using System.Collections.Generic;
using System.Linq;

namespace LibreClass.Response {
    public enum AnswerFormat {
        Alpha,
        Numeric,
    }

    public enum AssessmentType {
        None,
        /// <summary>
        /// Teacher managed assessment (single question)
        /// </summary>
        TMA,
        /// <summary>
        /// Student managed assessment (self paced, multiple questions)
        /// </summary>
        SMA,
    }

    public enum QuestionType {
        INVALID,
        TrueFalse,
        MultipleChoice,
        Numeric,
        Essay,
    }

    public class Assessment {
        public List<IQuestion> Questions { get; }
    }

    public interface IQuestion {
        AnswerFormat AnswerFormat { get; set; }
        QuestionType QuestionType { get; set; }
        string CorrectAnswer { get; set; }
        int NumChoices { get; set; }
    }

    /// <summary>
    /// Programmatic multiple choice question
    /// </summary>
    public sealed class MCQuestion : IQuestion {
        public AnswerFormat AnswerFormat {
            get { return AnswerFormat.Alpha; }
            set { throw new NotSupportedException(); }
        }

        public QuestionType QuestionType {
            get { return QuestionType.MultipleChoice; }
            set { throw new NotSupportedException(); }
        }

        public string CorrectAnswer { get; set; }

        public int NumChoices { get; set; } = 4;
    }
}
