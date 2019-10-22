using System;
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
namespace Neo4Net.Kernel.Internal
{

	using Format = Neo4Net.Helpers.Format;
	using DiagnosticsPhase = Neo4Net.Internal.Diagnostics.DiagnosticsPhase;
	using DiagnosticsProvider = Neo4Net.Internal.Diagnostics.DiagnosticsProvider;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using Logger = Neo4Net.Logging.Logger;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

	public abstract class KernelDiagnostics : DiagnosticsProvider
	{
		 public class Versions : KernelDiagnostics
		 {
			  internal readonly DatabaseInfo DatabaseInfo;
			  internal readonly StoreId StoreId;

			  public Versions( DatabaseInfo databaseInfo, StoreId storeId )
			  {
					this.DatabaseInfo = databaseInfo;
					this.StoreId = storeId;
			  }

			  internal override void Dump( Logger logger )
			  {
					logger.Log( "Graph Database: " + DatabaseInfo + " " + StoreId );
					logger.Log( "Kernel version: " + Version.KernelVersion );
			  }
		 }

		 public class StoreFiles : KernelDiagnostics
		 {
			  internal readonly DatabaseLayout DatabaseLayout;
			  internal static string FormatDateIso = "yyyy-MM-dd'T'HH:mm:ssZ";
			  internal readonly SimpleDateFormat DateFormat;

			  public StoreFiles( DatabaseLayout databaseLayout )
			  {
					this.DatabaseLayout = databaseLayout;
					TimeZone tz = TimeZone.Default;
					DateFormat = new SimpleDateFormat( FormatDateIso );
					DateFormat.TimeZone = tz;
			  }

			  internal override void Dump( Logger logger )
			  {
					logger.Log( GetDiskSpace( DatabaseLayout ) );
					logger.Log( "Storage files: (filename : modification date - size)" );
					MappedFileCounter mappedCounter = new MappedFileCounter( DatabaseLayout );
					long totalSize = LogStoreFiles( logger, "  ", DatabaseLayout.databaseDirectory(), mappedCounter );
					logger.Log( "Storage summary: " );
					logger.Log( "  Total size of store: " + Format.bytes( totalSize ) );
					logger.Log( "  Total size of mapped files: " + Format.bytes( mappedCounter.Size ) );
			  }

			  internal virtual long LogStoreFiles( Logger logger, string prefix, File dir, MappedFileCounter mappedCounter )
			  {
					if ( !dir.Directory )
					{
						 return 0;
					}
					File[] files = dir.listFiles();
					if ( files == null )
					{
						 logger.Log( prefix + "<INACCESSIBLE>" );
						 return 0;
					}
					long total = 0;

					// Sort by name
					IList<File> fileList = Arrays.asList( files );
					fileList.sort( System.Collections.IComparer.comparing( File.getName ) );

					foreach ( File file in fileList )
					{
						 long size;
						 string filename = file.Name;
						 if ( file.Directory )
						 {
							  logger.Log( prefix + filename + ":" );
							  size = LogStoreFiles( logger, prefix + "  ", file, mappedCounter );
							  filename = "- Total";
						 }
						 else
						 {
							  size = file.length();
							  mappedCounter.AddFile( file );
						 }

						 string fileModificationDate = GetFileModificationDate( file );
						 string bytes = Format.bytes( size );
						 string fileInformation = string.Format( "{0}{1}: {2} - {3}", prefix, filename, fileModificationDate, bytes );
						 logger.Log( fileInformation );

						 total += size;
					}
					return total;
			  }

			  internal virtual string GetFileModificationDate( File file )
			  {
					DateTime modifiedDate = new DateTime( file.lastModified() );
					return DateFormat.format( modifiedDate );
			  }

			  internal static string GetDiskSpace( DatabaseLayout databaseLayout )
			  {
					File directory = databaseLayout.DatabaseDirectory();
					long free = directory.FreeSpace;
					long total = directory.TotalSpace;
					long percentage = total != 0 ? ( free * 100 / total ) : 0;
					return string.Format( "Disk space on partition (Total / Free / Free %): {0} / {1} / {2}", total, free, percentage );
			  }

			  private class MappedFileCounter
			  {
					internal readonly DatabaseLayout Layout;
					internal readonly FileFilter MappedIndexFilter;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
					internal long SizeConflict;
					internal readonly IList<File> MappedCandidates;

					internal MappedFileCounter( DatabaseLayout layout )
					{
						 this.Layout = layout;
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
						 MappedCandidates = java.util.org.Neo4Net.kernel.impl.store.StoreType.values().Where(StoreType::isRecordStore).Select(StoreType::getDatabaseFile).flatMap(layout.file).ToList();
						 MappedIndexFilter = new NativeIndexFileFilter( layout.DatabaseDirectory() );
					}

					internal virtual void AddFile( File file )
					{
						 if ( CanBeManagedByPageCache( file ) || MappedIndexFilter.accept( file ) )
						 {
							  SizeConflict += file.length();
						 }
					}

					public virtual long Size
					{
						get
						{
							 return SizeConflict;
						}
					}

					/// <summary>
					/// Returns whether or not store file by given file name should be managed by the page cache.
					/// </summary>
					/// <param name="storeFile"> file of the store file to check. </param>
					/// <returns> Returns whether or not store file by given file name should be managed by the page cache. </returns>
					internal virtual bool CanBeManagedByPageCache( File storeFile )
					{
						 bool isLabelScanStore = Layout.labelScanStore().Equals(storeFile);
						 return isLabelScanStore || MappedCandidates.Contains( storeFile );
					}
			  }
		 }

		 public virtual string DiagnosticsIdentifier
		 {
			 get
			 {
				  return this.GetType().DeclaringType.SimpleName + ":" + this.GetType().Name;
			 }
		 }

		 public override void AcceptDiagnosticsVisitor( object visitor )
		 {
			  // nothing visits ConfigurationLogging
		 }

		 public override void Dump( DiagnosticsPhase phase, Logger log )
		 {
			  if ( phase.Initialization || phase.ExplicitlyRequested )
			  {
					Dump( log );
			  }
		 }
		 internal abstract void Dump( Logger logger );
	}

}