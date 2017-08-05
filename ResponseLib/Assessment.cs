/*    This file is part of LibreClass.
 *
 *    LibreClass is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    LibreClass is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with LibreClass.  If not, see <http://www.gnu.org/licenses/>.
 */

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
