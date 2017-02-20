﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.DataCollection;

using WebBackend.AnswerExtraction;

namespace WebBackend.DialogProvider
{
    class AnswerExtractionWebConsole : WebConsoleBase
    {
        private readonly string _databasePath;

        private readonly QuestionCollection _questions;

        private readonly ExtractionKnowledge _knowledge;

        internal AnswerExtractionWebConsole(string databasePath, QuestionCollection questions, ExtractionKnowledge knowledge)
        {
            _databasePath = databasePath;
            _questions = questions;
            _knowledge = knowledge;
        }

        protected override IInputDialogManager createDialogManager()
        {
            return new AnswerExtractionManager(_questions, _knowledge);
        }
    }
}
