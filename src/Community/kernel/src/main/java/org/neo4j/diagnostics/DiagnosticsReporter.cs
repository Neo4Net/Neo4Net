using System;
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

	using Format = Neo4Net.Helpers.Format;
	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;

	public class DiagnosticsReporter
	{
		 private readonly IList<DiagnosticsOfflineReportProvider> _providers = new List<DiagnosticsOfflineReportProvider>();
		 private readonly ISet<string> _availableClassifiers = new SortedSet<string>();
		 private readonly IDictionary<string, IList<DiagnosticsReportSource>> _additionalSources = new Dictionary<string, IList<DiagnosticsReportSource>>();

		 public virtual void RegisterOfflineProvider( DiagnosticsOfflineReportProvider provider )
		 {
			  _providers.Add( provider );
			  _availableClassifiers.addAll( provider.FilterClassifiers );
		 }

		 public virtual void RegisterSource( string classifier, DiagnosticsReportSource source )
		 {
			  _availableClassifiers.Add( classifier );
			  _additionalSources.computeIfAbsent( classifier, c => new List<>() ).add(source);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void dump(java.util.Set<String> classifiers, java.nio.file.Path destination, DiagnosticsReporterProgress progress, boolean force) throws java.io.IOException
		 public virtual void Dump( ISet<string> classifiers, Path destination, DiagnosticsReporterProgress progress, bool force )
		 {
			  // Collect sources
			  IList<DiagnosticsReportSource> sources = new List<DiagnosticsReportSource>();
			  foreach ( DiagnosticsOfflineReportProvider provider in _providers )
			  {
					( ( IList<DiagnosticsReportSource> )sources ).AddRange( provider.GetDiagnosticsSources( classifiers ) );
			  }

			  // Add additional sources
			  foreach ( KeyValuePair<string, IList<DiagnosticsReportSource>> classifier in _additionalSources.SetOfKeyValuePairs() )
			  {
					if ( classifiers.Contains( "all" ) || classifiers.Contains( classifier.Key ) )
					{
						 ( ( IList<DiagnosticsReportSource> )sources ).AddRange( classifier.Value );
					}
			  }

			  // Make sure target directory exists
			  Path destinationFolder = destination.Parent;
			  Files.createDirectories( destinationFolder );

			  // Estimate an upper bound of the final size and make sure it will fit, if not, end reporting
			  EstimateSizeAndCheckAvailableDiskSpace( destination, progress, sources, destinationFolder, force );

			  // Compress all files to destination
			  IDictionary<string, object> env = new Dictionary<string, object>();
			  env["create"] = "true";
			  env["useTempFile"] = true;

			  // NOTE: we need the toUri() in order to handle windows file paths
			  URI uri = URI.create( "jar:file:" + destination.toAbsolutePath().toUri().RawPath );

			  using ( FileSystem fs = FileSystems.newFileSystem( uri, env ) )
			  {
					progress.TotalSteps = sources.Count;
					for ( int i = 0; i < sources.Count; i++ )
					{
						 DiagnosticsReportSource source = sources[i];
						 Path path = fs.getPath( source.DestinationPath() );
						 if ( path.Parent != null )
						 {
							  Files.createDirectories( path.Parent );
						 }

						 progress.Started( i + 1, path.ToString() );
						 try
						 {
							  source.AddToArchive( path, progress );
						 }
						 catch ( Exception e )
						 {
							  progress.Error( "Step failed", e );
							  continue;
						 }
						 progress.Finished();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void estimateSizeAndCheckAvailableDiskSpace(java.nio.file.Path destination, DiagnosticsReporterProgress progress, java.util.List<DiagnosticsReportSource> sources, java.nio.file.Path destinationFolder, boolean force) throws java.io.IOException
		 private void EstimateSizeAndCheckAvailableDiskSpace( Path destination, DiagnosticsReporterProgress progress, IList<DiagnosticsReportSource> sources, Path destinationFolder, bool force )
		 {
			  if ( force )
			  {
					return;
			  }

			  long estimatedFinalSize = 0;
			  foreach ( DiagnosticsReportSource source in sources )
			  {
					estimatedFinalSize += source.EstimatedSize( progress );
			  }

			  long freeSpace = destinationFolder.toFile().FreeSpace;
			  if ( estimatedFinalSize > freeSpace )
			  {
					string message = string.Format( "Free available disk space for {0} is {1}, worst case estimate is {2}. To ignore add '--force' to the command.", destination.FileName, Format.bytes( freeSpace ), Format.bytes( estimatedFinalSize ) );
					throw new Exception( message );
			  }
		 }

		 public virtual ISet<string> AvailableClassifiers
		 {
			 get
			 {
				  return _availableClassifiers;
			 }
		 }

		 public virtual void RegisterAllOfflineProviders( Config config, File storeDirectory, FileSystemAbstraction fs )
		 {
			  foreach ( DiagnosticsOfflineReportProvider provider in Service.load( typeof( DiagnosticsOfflineReportProvider ) ) )
			  {
					provider.Init( fs, config, storeDirectory );
					RegisterOfflineProvider( provider );
			  }
		 }
	}

}