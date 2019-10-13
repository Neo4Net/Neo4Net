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
namespace Neo4Net.Kernel.impl.storemigration
{

	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using MetaDataRecordFormat = Neo4Net.Kernel.impl.store.format.standard.MetaDataRecordFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.STORE_VERSION;
	using static Neo4Net.Kernel.impl.storemigration.StoreVersionCheck.Result.Outcome;

	public class StoreVersionCheck
	{
		 private readonly PageCache _pageCache;

		 public StoreVersionCheck( PageCache pageCache )
		 {
			  this._pageCache = pageCache;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Optional<String> getVersion(java.io.File neostoreFile) throws java.io.IOException
		 public virtual Optional<string> GetVersion( File neostoreFile )
		 {
			  long record = MetaDataStore.getRecord( _pageCache, neostoreFile, STORE_VERSION );
			  if ( record == MetaDataRecordFormat.FIELD_NOT_PRESENT )
			  {
					return null;
			  }
			  return MetaDataStore.versionLongToString( record );
		 }

		 public virtual Result HasVersion( File neostoreFile, string expectedVersion )
		 {
			  Optional<string> storeVersion;

			  try
			  {
					storeVersion = GetVersion( neostoreFile );
			  }
			  catch ( IOException )
			  {
					// since we cannot read let's assume the file is not there
					return new Result( Outcome.missingStoreFile, null, neostoreFile.Name );
			  }

			  return storeVersion.map( v => expectedVersion.Equals( v ) ? new Result( Outcome.ok, null, neostoreFile.Name ) : new Result( Outcome.unexpectedStoreVersion, v, neostoreFile.Name ) ).orElseGet( () => new Result(Outcome.storeVersionNotFound, null, neostoreFile.Name) );
		 }

		 public class Result
		 {
			  public readonly Outcome Outcome;
			  public readonly string ActualVersion;
			  public readonly string StoreFilename;

			  public Result( Outcome outcome, string actualVersion, string storeFilename )
			  {
					this.Outcome = outcome;
					this.ActualVersion = actualVersion;
					this.StoreFilename = storeFilename;
			  }

			  public sealed class Outcome
			  {
					public static readonly Outcome Ok = new Outcome( "Ok", InnerEnum.Ok, true );
					public static readonly Outcome MissingStoreFile = new Outcome( "MissingStoreFile", InnerEnum.MissingStoreFile, false );
					public static readonly Outcome StoreVersionNotFound = new Outcome( "StoreVersionNotFound", InnerEnum.StoreVersionNotFound, false );
					public static readonly Outcome UnexpectedStoreVersion = new Outcome( "UnexpectedStoreVersion", InnerEnum.UnexpectedStoreVersion, false );
					public static readonly Outcome AttemptedStoreDowngrade = new Outcome( "AttemptedStoreDowngrade", InnerEnum.AttemptedStoreDowngrade, false );
					public static readonly Outcome StoreNotCleanlyShutDown = new Outcome( "StoreNotCleanlyShutDown", InnerEnum.StoreNotCleanlyShutDown, false );

					private static readonly IList<Outcome> valueList = new List<Outcome>();

					static Outcome()
					{
						valueList.Add( Ok );
						valueList.Add( MissingStoreFile );
						valueList.Add( StoreVersionNotFound );
						valueList.Add( UnexpectedStoreVersion );
						valueList.Add( AttemptedStoreDowngrade );
						valueList.Add( StoreNotCleanlyShutDown );
					}

					public enum InnerEnum
					{
						Ok,
						MissingStoreFile,
						StoreVersionNotFound,
						UnexpectedStoreVersion,
						AttemptedStoreDowngrade,
						StoreNotCleanlyShutDown
					}

					public readonly InnerEnum innerEnumValue;
					private readonly string nameValue;
					private readonly int ordinalValue;
					private static int nextOrdinal = 0;

					internal Private readonly;

					internal Outcome( string name, InnerEnum innerEnum, bool success )
					{
						 this.Success = success;

						nameValue = name;
						ordinalValue = nextOrdinal++;
						innerEnumValue = innerEnum;
					}

					public bool Successful
					{
						get
						{
							 return this.Success;
						}
					}

				  public static IList<Outcome> values()
				  {
					  return valueList;
				  }

				  public int ordinal()
				  {
					  return ordinalValue;
				  }

				  public override string ToString()
				  {
					  return nameValue;
				  }

				  public static Outcome valueOf( string name )
				  {
					  foreach ( Outcome enumInstance in Outcome.valueList )
					  {
						  if ( enumInstance.nameValue == name )
						  {
							  return enumInstance;
						  }
					  }
					  throw new System.ArgumentException( name );
				  }
			  }
		 }
	}

}