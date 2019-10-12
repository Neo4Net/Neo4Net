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
namespace Org.Neo4j.Index
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using GenericNativeIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.GenericNativeIndexProvider;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.mebiBytes;

	public class SabotageNativeIndex : DatabaseRule.RestartAction
	{
		 private readonly Random _random;

		 internal SabotageNativeIndex( Random random )
		 {
			  this._random = random;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 public override void Run( FileSystemAbstraction fs, DatabaseLayout databaseLayout )
		 {
			  int files = ScrambleIndexFiles( fs, NativeIndexDirectoryStructure( databaseLayout ).rootDirectory() );
			  assertThat( "there is no index to sabotage", files, greaterThanOrEqualTo( 1 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int scrambleIndexFiles(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File fileOrDir) throws java.io.IOException
		 private int ScrambleIndexFiles( FileSystemAbstraction fs, File fileOrDir )
		 {
			  if ( fs.IsDirectory( fileOrDir ) )
			  {
					int count = 0;
					File[] children = fs.ListFiles( fileOrDir );
					if ( children != null )
					{
						 foreach ( File child in children )
						 {
							  count += ScrambleIndexFiles( fs, child );
						 }
					}
					return count;
			  }
			  else
			  {
					// Completely scramble file, assuming small files
					using ( StoreChannel channel = fs.Open( fileOrDir, OpenMode.READ_WRITE ) )
					{
						 if ( channel.size() > mebiBytes(10) )
						 {
							  throw new System.ArgumentException( "Was expecting small files here" );
						 }
						 sbyte[] bytes = new sbyte[( int ) channel.size()];
						 _random.NextBytes( bytes );
						 channel.WriteAll( ByteBuffer.wrap( bytes ) );
					}
					return 1;
			  }
		 }

		 internal static IndexDirectoryStructure NativeIndexDirectoryStructure( DatabaseLayout databaseLayout )
		 {
			  return IndexDirectoryStructure.directoriesByProvider( databaseLayout.DatabaseDirectory() ).forProvider(GenericNativeIndexProvider.DESCRIPTOR);
		 }
	}

}