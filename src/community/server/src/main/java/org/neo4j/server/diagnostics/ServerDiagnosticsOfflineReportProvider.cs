using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Server.diagnostics
{

	using DiagnosticsOfflineReportProvider = Neo4Net.Diagnostics.DiagnosticsOfflineReportProvider;
	using DiagnosticsReportSource = Neo4Net.Diagnostics.DiagnosticsReportSource;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.diagnostics.DiagnosticsReportSources.newDiagnosticsRotatingFile;

	public class ServerDiagnosticsOfflineReportProvider : DiagnosticsOfflineReportProvider
	{
		 private FileSystemAbstraction _fs;
		 private Config _config;

		 public ServerDiagnosticsOfflineReportProvider() : base("server", "logs")
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
					File httpLog = _config.get( ServerSettings.http_log_path );
					if ( _fs.fileExists( httpLog ) )
					{
						 return newDiagnosticsRotatingFile( "logs/http.log", _fs, httpLog );
					}
			  }
			  return Collections.emptyList();
		 }
	}

}