using System;

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
namespace Neo4Net.Kernel.impl.transaction.log.files
{

	internal class TransactionLogFilesHelper
	{
		 private const string REGEX_DEFAULT_NAME = "neostore\\.transaction\\.db";
		 private const string VERSION_SUFFIX = ".";
		 private const string REGEX_VERSION_SUFFIX = "\\.";

		 internal static readonly FilenameFilter DefaultFilenameFilter = new LogicalLogFilenameFilter( REGEX_DEFAULT_NAME );

		 private readonly File _logBaseName;
		 private readonly FilenameFilter _logFileFilter;

		 internal TransactionLogFilesHelper( File directory, string name )
		 {
			  this._logBaseName = new File( directory, name );
			  this._logFileFilter = new LogicalLogFilenameFilter( name );
		 }

		 internal virtual File GetLogFileForVersion( long version )
		 {
			  return new File( _logBaseName.Path + VERSION_SUFFIX + version );
		 }

		 internal virtual long GetLogVersion( string historyLogFilename )
		 {
			  int index = historyLogFilename.LastIndexOf( VERSION_SUFFIX, StringComparison.Ordinal );
			  if ( index == -1 )
			  {
					throw new Exception( "Invalid log file '" + historyLogFilename + "'" );
			  }
			  return long.Parse( historyLogFilename.Substring( index + VERSION_SUFFIX.Length ) );
		 }

		 internal virtual File ParentDirectory
		 {
			 get
			 {
				  return _logBaseName.ParentFile;
			 }
		 }

		 internal virtual FilenameFilter LogFilenameFilter
		 {
			 get
			 {
				  return _logFileFilter;
			 }
		 }

		 private sealed class LogicalLogFilenameFilter : FilenameFilter
		 {
			  internal readonly Pattern LogFilenamePattern;

			  internal LogicalLogFilenameFilter( string name )
			  {
					LogFilenamePattern = compile( name + REGEX_VERSION_SUFFIX + ".*" );
			  }

			  public override bool Accept( File dir, string name )
			  {
					return LogFilenamePattern.matcher( name ).matches();
			  }
		 }
	}

}