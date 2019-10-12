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
namespace Neo4Net.Kernel.impl.storemigration
{

	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using FormatFamily = Neo4Net.Kernel.impl.store.format.FormatFamily;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using DatabaseNotCleanlyShutDownException = Neo4Net.Kernel.impl.storemigration.StoreUpgrader.DatabaseNotCleanlyShutDownException;
	using UnexpectedUpgradingStoreFormatException = Neo4Net.Kernel.impl.storemigration.StoreUpgrader.UnexpectedUpgradingStoreFormatException;
	using UnexpectedUpgradingStoreVersionException = Neo4Net.Kernel.impl.storemigration.StoreUpgrader.UnexpectedUpgradingStoreVersionException;
	using UpgradeMissingStoreFilesException = Neo4Net.Kernel.impl.storemigration.StoreUpgrader.UpgradeMissingStoreFilesException;
	using UpgradingStoreVersionNotFoundException = Neo4Net.Kernel.impl.storemigration.StoreUpgrader.UpgradingStoreVersionNotFoundException;
	using Result = Neo4Net.Kernel.impl.storemigration.StoreVersionCheck.Result;
	using Outcome = Neo4Net.Kernel.impl.storemigration.StoreVersionCheck.Result.Outcome;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;

	/// <summary>
	/// Logic to check whether a database version is upgradable to the current version. It looks at the
	/// version information found in the store files themselves.
	/// </summary>
	public class UpgradableDatabase
	{
		 private readonly StoreVersionCheck _storeVersionCheck;
		 private readonly RecordFormats _format;
		 private readonly LogTailScanner _tailScanner;

		 public UpgradableDatabase( StoreVersionCheck storeVersionCheck, RecordFormats format, LogTailScanner tailScanner )
		 {
			  this._storeVersionCheck = storeVersionCheck;
			  this._format = format;
			  this._tailScanner = tailScanner;
		 }

		 /// <summary>
		 /// Assumed to only be called if <seealso cref="hasCurrentVersion(DatabaseLayout)"/> returns {@code false}.
		 /// </summary>
		 /// <param name="dbDirectoryLayout"> database directory structure. </param>
		 /// <returns> the <seealso cref="RecordFormats"/> the current store (which is upgradable) is currently in. </returns>
		 /// <exception cref="UpgradeMissingStoreFilesException"> if store cannot be upgraded due to some store files are missing. </exception>
		 /// <exception cref="UpgradingStoreVersionNotFoundException"> if store cannot be upgraded due to store
		 /// version cannot be determined. </exception>
		 /// <exception cref="UnexpectedUpgradingStoreVersionException"> if store cannot be upgraded due to an unexpected store
		 /// version found. </exception>
		 /// <exception cref="UnexpectedUpgradingStoreFormatException"> if store cannot be upgraded due to an unexpected store
		 /// format found. </exception>
		 /// <exception cref="DatabaseNotCleanlyShutDownException"> if store cannot be upgraded due to not being cleanly shut down. </exception>
		 public virtual RecordFormats CheckUpgradable( DatabaseLayout dbDirectoryLayout )
		 {
			  File neostoreFile = dbDirectoryLayout.MetadataStore();
			  Result result = _storeVersionCheck.hasVersion( neostoreFile, _format.storeVersion() );
			  if ( result.Outcome.Successful )
			  {
					// This store already has the format that we want
					// Although this method should not have been called in this case.
					return _format;
			  }

			  RecordFormats fromFormat;
			  try
			  {
					fromFormat = RecordFormatSelector.selectForVersion( result.ActualVersion );

					// If we are trying to open an enterprise store when configured to use community format, then inform the user
					// of the config setting to change since downgrades aren't possible but the store can still be opened.
					if ( FormatFamily.isLowerFamilyFormat( _format, fromFormat ) )
					{
						 throw new StoreUpgrader.UnexpectedUpgradingStoreFormatException();
					}

					if ( FormatFamily.isSameFamily( fromFormat, _format ) && ( fromFormat.Generation() > _format.generation() ) )
					{
						 // Tried to downgrade, that isn't supported
						 result = new Result( Result.Outcome.attemptedStoreDowngrade, fromFormat.StoreVersion(), neostoreFile.AbsolutePath );
					}
					else
					{
						 result = CheckCleanShutDownByCheckPoint();
						 if ( result.Outcome.Successful )
						 {
							  return fromFormat;
						 }
					}
			  }
			  catch ( System.ArgumentException )
			  {
					result = new Result( Result.Outcome.unexpectedStoreVersion, result.ActualVersion, result.StoreFilename );
			  }

			  switch ( result.Outcome.innerEnumValue )
			  {
			  case Result.Outcome.InnerEnum.missingStoreFile:
					throw new StoreUpgrader.UpgradeMissingStoreFilesException( GetPathToStoreFile( dbDirectoryLayout, result ) );
			  case Result.Outcome.InnerEnum.storeVersionNotFound:
					throw new StoreUpgrader.UpgradingStoreVersionNotFoundException( GetPathToStoreFile( dbDirectoryLayout, result ) );
			  case Result.Outcome.InnerEnum.attemptedStoreDowngrade:
					throw new StoreUpgrader.AttemptedDowngradeException();
			  case Result.Outcome.InnerEnum.unexpectedStoreVersion:
					throw new StoreUpgrader.UnexpectedUpgradingStoreVersionException( result.ActualVersion, _format.storeVersion() );
			  case Result.Outcome.InnerEnum.storeNotCleanlyShutDown:
					throw new StoreUpgrader.DatabaseNotCleanlyShutDownException();
			  default:
					throw new System.ArgumentException( "Unexpected outcome: " + result.Outcome.name() );
			  }
		 }

		 private Result CheckCleanShutDownByCheckPoint()
		 {
			  // check version
			  try
			  {
					if ( !_tailScanner.TailInformation.commitsAfterLastCheckpoint() )
					{
						 return new Result( Result.Outcome.ok, null, null );
					}
			  }
			  catch ( Exception )
			  {
					// ignore exception and return db not cleanly shutdown
			  }

			  return new Result( Result.Outcome.storeNotCleanlyShutDown, null, null );
		 }

		 private static string GetPathToStoreFile( DatabaseLayout directoryLayout, Result result )
		 {
			  return directoryLayout.File( result.StoreFilename ).AbsolutePath;
		 }

		 internal virtual bool HasCurrentVersion( DatabaseLayout dbDirectoryLayout )
		 {
			  File neoStore = dbDirectoryLayout.MetadataStore();
			  Result result = _storeVersionCheck.hasVersion( neoStore, _format.storeVersion() );
			  switch ( result.Outcome.innerEnumValue )
			  {
			  case Result.Outcome.InnerEnum.ok:
			  case Result.Outcome.InnerEnum.missingStoreFile: // let's assume the db is empty
					return true;
			  case Result.Outcome.InnerEnum.storeVersionNotFound:
			  case Result.Outcome.InnerEnum.unexpectedStoreVersion:
			  case Result.Outcome.InnerEnum.attemptedStoreDowngrade:
					return false;
			  default:
					throw new System.ArgumentException( "Unknown outcome: " + result.Outcome.name() );
			  }
		 }

		 public virtual string CurrentVersion()
		 {
			  return _format.storeVersion();
		 }
	}

}