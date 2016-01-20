﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MicroKernel;
using Dapper.Contrib.Extensions;
using DbAdvance.Host.Commands.Steps.FolderRunStrategy;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Package;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Steps
{
    public class ApplyScriptsStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly DatabaseConnectorFactory _factory;
        private readonly List<ScriptRunResult> _results = new List<ScriptRunResult>();

        public Action<ScriptsRunInfo> OnScriptInfoRecorded;
        public Action<ScriptsRunErrorInfo> OnScriptInfoErrorRecorded;

        public bool UseRollbackScripts { get; set; }

        public ApplyScriptsStep(IKernel kernel, DatabaseConnectorFactory factory) : base(kernel)
        {
            _factory = factory;
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();

            Logger.Info("STAGE: Upgrading database via pending scripts");

            var connector = _factory.Create();

            connector.OnScriptExecuted += OnScriptExecuted;

            ExecuteScriptsAsDeltas(connector, context);

            connector.OnScriptExecuted -= OnScriptExecuted;

            ApplyResultsToInfoTables(context);

            Logger.WriteBanner();
        }

        private void ExecuteScriptsAsDeltas(
            IDatabaseConnector connector, CommandPipelineContext context)
        {
            foreach (var delta in context.FolderDeltas)
            {
                var step = new Step {Scripts = delta.Scripts};
                connector.Apply(step);
            }
        }

        private void OnScriptExecuted(ScriptRunResult result)
        {
            if (_results.Any(r => r.Script.ToString() == result.Script.ToString())) return;

            _results.Add(result);

            if (result.HasErrors())
            {
                Logger.ErrorFormat("'{0}' script application failure. Reason(s): {1}", result.Script.ToString(),
                    string.Join(Environment.NewLine, result.Errors));
            }
            else
            {
                Logger.InfoFormat("'{0}' script applied.", result.Script.ToString());
            }
        }

        private void ApplyResultsToInfoTables(CommandPipelineContext context)
        {
            RecordAllScriptsToInfoTable(context);
            RecordAllFailuresToErrorInfoTable(context);
        }

        private void RecordAllScriptsToInfoTable(CommandPipelineContext context)
        {
            var scripts = _results
                .Select(r => new ScriptsRunInfo
                {
                    EntryDate = System.DateTime.Now,
                    IsRollbackScript = UseRollbackScripts,
                    ScriptName = Path.GetFileName(r.Script.GetFullPath()),
                    ScriptText = r.Script.Read(),
                    Tag = r.Script.Tag ?? string.Empty,
                    Version = context.ToVersion
                })
                .Distinct()
                .ToList();

            if (!scripts.Any()) return;

            using (var connection = _factory.GetConnection())
            {
                foreach (var script in scripts)
                {
                    connection.Insert(script);
                    RaiseOnScriptInfoRecorded(script);
                }
            }
        }

        private void RecordAllFailuresToErrorInfoTable(CommandPipelineContext context)
        {
            var failures = _results
                .Where(r => r.HasErrors())
                .Select(r => new ScriptsRunErrorInfo
                {
                    EntryDate = System.DateTime.Now,
                    ScriptName = r.Script.ToString(),
                    ScriptText = r.Script.Read(),
                    ScriptError = string.Join(Environment.NewLine, r.Errors),
                    Version = context.ToVersion
                })
                .Distinct()
                .ToList();

            if (!failures.Any()) return;

            using (var connection = _factory.GetConnection())
            {
                foreach (var failure in failures)
                {
                    connection.Insert(failure);
                    RaiseOnScriptInfoErrorRecorded(failure);
                }
            }
        }

        private void RaiseOnScriptInfoRecorded(ScriptsRunInfo info)
        {
            var evt = OnScriptInfoRecorded;
            if (evt == null) return;
            evt(info);
        }

        private void RaiseOnScriptInfoErrorRecorded(ScriptsRunErrorInfo info)
        {
            var evt = OnScriptInfoErrorRecorded;
            if (evt == null) return;
            evt(info);
        }
    }
}