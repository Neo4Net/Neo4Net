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
namespace Neo4Net.metrics.diagnostics
{

	using DiagnosticsOfflineReportProvider = Neo4Net.Diagnostics.DiagnosticsOfflineReportProvider;
	using DiagnosticsReportSource = Neo4Net.Diagnostics.DiagnosticsReportSource;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.diagnostics.DiagnosticsReportSources.newDiagnosticsFile;

	public class MetricsDiagnosticsOfflineReportProvider : DiagnosticsOfflineReportProvider
	{
		 private FileSystemAbstraction _fs;
		 private Config _config;

		 public MetricsDiagnosticsOfflineReportProvider() : base("metrics", "metrics")
		 {
		 }

		 public override void Init( FileSystemAbstraction fs, Config config, File storeDirectory )
		 {
			  this._fs = fs;
			  this._config = config;
		 }

		 protected internal override IList<DiagnosticsReportSource> ProvideSources( ISet<string> classifiers )
		 {
			  File metricsDirectory = _config.get( MetricsSettings.csvPath );
			  if ( _fs.fileExists( metricsDirectory ) && _fs.isDirectory( metricsDirectory ) )
			  {
					IList<DiagnosticsReportSource> files = new List<DiagnosticsReportSource>();
					foreach ( File file in _fs.listFiles( metricsDirectory ) )
					{
						 Files.Add( newDiagnosticsFile( "metrics/" + file.Name, _fs, file ) );
					}
					return files;
			  }
			  return Collections.emptyList();
		 }
	}

}