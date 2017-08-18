﻿using PerceptiveDialogBasedAgent.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.Knowledge
{
    class Database
    {
        /// <summary>
        /// Constraint used for results fetching.
        /// </summary>
        internal readonly DbConstraint Fetch = new DbConstraint();

        /// <summary>
        /// Data contained in the database.
        /// </summary>
        private readonly List<DbEntry> _data = new List<DbEntry>();

        /// <summary>
        /// Index of entities contained in _data.
        /// </summary>
        private readonly HashSet<string> _entities = new HashSet<string>();

        /// <summary>
        /// Queries DB according to given constraint.
        /// </summary>
        /// <param name="constraint">Constraint for results retrieving the DB.</param>
        internal IEnumerable<DbResult> Query(DbConstraint constraint)
        {
            var result = new List<DbResult>();
            foreach (var entity in _entities)
            {
                //TODO allow variables
                if (meetsConstraints(entity, constraint))
                    result.Add(new DbResult(Fetch, entity));
            }

            return result;
        }

        internal IEnumerable<string> GetAnswers(string subject, string question)
        {
            foreach (var entry in _data)
            {
                if (entry.Subject == subject && entry.Question == question)
                    yield return entry.Answer;
            }
        }

        internal void AddFact(string subject, string question, string answer)
        {
            _entities.Add(subject);
            _entities.Add(answer);

            _data.Add(new DbEntry(subject, question, answer));
        }

        private bool meetsConstraints(string entity, DbConstraint constraint)
        {
            if (constraint == null)
                //there is no constraint on the entity
                return true;

            if (constraint.NameConstraint != null && entity != constraint.NameConstraint)
                return false;

            foreach (var questionConstraint in constraint.SubjectConstraints)
            {
                if (!holds(entity, questionConstraint.Question, questionConstraint.Answer))
                    return false;
            }

            foreach (var answerConstraint in constraint.AnswerConstraints)
            {
                if (!holds(answerConstraint.Subject, answerConstraint.Question, entity))
                    return false;
            }

            return true;
        }

        private bool holds(string entity, string question, DbConstraint answerConstraint)
        {
            foreach (var answer in getAnswers(entity, question))
            {
                if (meetsConstraints(answer, answerConstraint))
                    return true;
            }

            return false;
        }

        private bool holds(DbConstraint subjectConstraint, string question, string entity)
        {
            foreach (var subjectEntity in getSubjects(question, entity))
            {
                if (meetsConstraints(subjectEntity, subjectConstraint))
                    return true;
            }

            return false;
        }

        private IEnumerable<string> getAnswers(string subject, string question)
        {
            foreach (var entry in _data)
            {
                if (entry.Subject == subject && entry.Question == question)
                    yield return entry.Answer;
            }
        }

        private IEnumerable<string> getSubjects(string question, string answer)
        {
            foreach (var entry in _data)
            {
                if (entry.Answer == answer && entry.Question == question)
                    yield return entry.Subject;
            }
        }
    }
}
