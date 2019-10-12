using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Diagnostics
{


	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;

	/// <summary>
	/// Base class for a provider of offline reports. Offline reports does not require a running instance of the database
	/// and is intended to be use as a way to gather information even if the database cannot be started. All implementing
	/// classes is service loaded and initialized through the
	/// <seealso cref="DiagnosticsOfflineReportProvider.init(FileSystemAbstraction, Config, File)"/> method.
	/// </summary>
	public abstract class DiagnosticsOfflineReportProvider : Service
	{
		 private readonly ISet<string> _filterClassifiers;

		 /// <summary>
		 /// A provider needs to know all the available classifiers in advance. A classifier is a group in the context of
		 /// diagnostics reporting, e.g. 'logs', 'config' or 'threaddump'.
		 /// </summary>
		 /// <param name="identifier"> a unique identifier for tagging during service loading, used for useful debug information. </param>
		 /// <param name="classifier"> one </param>
		 /// <param name="classifiers"> or more classifiers have to be provided. </param>
		 protected internal DiagnosticsOfflineReportProvider( string identifier, string classifier, params string[] classifiers ) : base( identifier )
		 {
			  _filterClassifiers = new HashSet<string>( Arrays.asList( classifiers ) );
			  _filterClassifiers.Add( classifier );
		 }

		 /// <summary>
		 /// Called after service loading to initialize the class.
		 /// </summary>
		 /// <param name="fs"> filesystem to use for file access. </param>
		 /// <param name="config"> configuration file in use. </param>
		 /// <param name="storeDirectory"> directory of the database files. </param>
		 public abstract void Init( FileSystemAbstraction fs, Config config, File storeDirectory );

		 /// <summary>
		 /// Returns a list of source that matches the given classifiers.
		 /// </summary>
		 /// <param name="classifiers"> a set of classifiers to filter on. </param>
		 /// <returns> a list of sources, empty if nothing matches. </returns>
		 protected internal abstract IList<DiagnosticsReportSource> ProvideSources( ISet<string> classifiers );

		 internal ISet<string> FilterClassifiers
		 {
			 get
			 {
				  return _filterClassifiers;
			 }
		 }

		 internal IList<DiagnosticsReportSource> GetDiagnosticsSources( ISet<string> classifiers )
		 {
			  if ( classifiers.Contains( "all" ) )
			  {
					return ProvideSources( _filterClassifiers );
			  }
			  else
			  {
					return ProvideSources( classifiers );
			  }
		 }
	}

}