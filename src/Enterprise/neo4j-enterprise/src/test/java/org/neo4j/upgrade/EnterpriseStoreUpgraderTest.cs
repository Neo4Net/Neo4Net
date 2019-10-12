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
namespace Neo4Net.upgrade
{
	using Parameterized = org.junit.runners.Parameterized;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using HighLimitV3_0_0 = Neo4Net.Kernel.impl.store.format.highlimit.v300.HighLimitV3_0_0;
	using StoreUpgraderTest = Neo4Net.Kernel.impl.storemigration.StoreUpgraderTest;
	using Unzip = Neo4Net.Test.Unzip;

	public class EnterpriseStoreUpgraderTest : StoreUpgraderTest
	{
		 public EnterpriseStoreUpgraderTest( RecordFormats recordFormats ) : base( recordFormats )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<org.neo4j.kernel.impl.store.format.RecordFormats> versions()
		 public static ICollection<RecordFormats> Versions()
		 {
			  return singletonList( HighLimitV3_0_0.RECORD_FORMATS );
		 }

		 protected internal override RecordFormats RecordFormats
		 {
			 get
			 {
				  return HighLimit.RECORD_FORMATS;
			 }
		 }

		 protected internal override string RecordFormatsName
		 {
			 get
			 {
				  return HighLimit.NAME;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void prepareSampleDatabase(String version, org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.io.layout.DatabaseLayout databaseLayout, java.io.File databaseDirectory) throws java.io.IOException
		 protected internal override void PrepareSampleDatabase( string version, FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout, File databaseDirectory )
		 {
			  File resourceDirectory = FindFormatStoreDirectoryForVersion( version, databaseDirectory );
			  File directory = databaseLayout.DatabaseDirectory();
			  fileSystem.DeleteRecursively( directory );
			  fileSystem.Mkdirs( directory );
			  fileSystem.CopyRecursively( resourceDirectory, directory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File findFormatStoreDirectoryForVersion(String version, java.io.File databaseDirectory) throws java.io.IOException
		 private static File FindFormatStoreDirectoryForVersion( string version, File databaseDirectory )
		 {
			  if ( version.Equals( HighLimitV3_0_0.STORE_VERSION ) )
			  {
					return HighLimit3_0Store( databaseDirectory );
			  }
			  else
			  {
					throw new System.ArgumentException( "Unknown enterprise store version." );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File highLimit3_0Store(java.io.File databaseDirectory) throws java.io.IOException
		 private static File HighLimit3_0Store( File databaseDirectory )
		 {
			  return Unzip.unzip( typeof( EnterpriseStoreUpgraderTest ), "upgradeTest30HighLimitDb.zip", databaseDirectory );
		 }
	}

}