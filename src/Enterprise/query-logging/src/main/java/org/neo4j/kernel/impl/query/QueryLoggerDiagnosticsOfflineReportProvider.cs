using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.impl.query
{

	using DiagnosticsOfflineReportProvider = Neo4Net.Diagnostics.DiagnosticsOfflineReportProvider;
	using DiagnosticsReportSource = Neo4Net.Diagnostics.DiagnosticsReportSource;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.diagnostics.DiagnosticsReportSources.newDiagnosticsRotatingFile;

	public class QueryLoggerDiagnosticsOfflineReportProvider : DiagnosticsOfflineReportProvider
	{
		 private FileSystemAbstraction _fs;
		 private Config _config;

		 public QueryLoggerDiagnosticsOfflineReportProvider() : base("query-logger", "logs")
		 {
		 }

		 public override void Init( FileSystemAbstraction fs, Config config, File storeDirectory )
		 {
			  this._fs = fs;
			  this._config = config;
		 }

		 protected internal override IList<DiagnosticsReportSource> ProvideSources( ISet<string> classifiers )
		 {
			  if ( classifiers.Contains( "logs" ) )
			  {
					File queryLog = _config.get( GraphDatabaseSettings.log_queries_filename );
					if ( _fs.fileExists( queryLog ) )
					{
						 return newDiagnosticsRotatingFile( "logs/query.log", _fs, queryLog );
					}
			  }
			  return Collections.emptyList();
		 }
	}

}