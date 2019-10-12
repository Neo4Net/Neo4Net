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
namespace Org.Neo4j.Commandline
{

	using CommandFailed = Org.Neo4j.Commandline.admin.CommandFailed;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using StoreLayout = Org.Neo4j.Io.layout.StoreLayout;
	using StoreLockException = Org.Neo4j.Kernel.StoreLockException;
	using GlobalStoreLocker = Org.Neo4j.Kernel.@internal.locker.GlobalStoreLocker;
	using StoreLocker = Org.Neo4j.Kernel.@internal.locker.StoreLocker;

	public class Util
	{
		 private Util()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.nio.file.Path canonicalPath(java.nio.file.Path path) throws IllegalArgumentException
		 public static Path CanonicalPath( Path path )
		 {
			  return canonicalPath( path.toFile() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.nio.file.Path canonicalPath(String path) throws IllegalArgumentException
		 public static Path CanonicalPath( string path )
		 {
			  return CanonicalPath( new File( path ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.nio.file.Path canonicalPath(java.io.File file) throws IllegalArgumentException
		 public static Path CanonicalPath( File file )
		 {
			  try
			  {
					return Paths.get( file.CanonicalPath );
			  }
			  catch ( IOException e )
			  {
					throw new System.ArgumentException( "Unable to parse path: " + file, e );
			  }
		 }

		 public static bool IsSameOrChildFile( File parent, File candidate )
		 {
			  Path canonicalCandidate = CanonicalPath( candidate );
			  Path canonicalParentPath = CanonicalPath( parent );
			  return canonicalCandidate.startsWith( canonicalParentPath );
		 }

		 public static bool IsSameOrChildPath( Path parent, Path candidate )
		 {
			  Path normalizedCandidate = candidate.normalize();
			  Path normalizedParent = parent.normalize();
			  return normalizedCandidate.startsWith( normalizedParent );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void checkLock(org.neo4j.io.layout.StoreLayout storeLayout) throws org.neo4j.commandline.admin.CommandFailed
		 public static void CheckLock( StoreLayout storeLayout )
		 {
			  try
			  {
					  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), StoreLocker storeLocker = new GlobalStoreLocker(fileSystem, storeLayout) )
					  {
						storeLocker.CheckLock();
					  }
			  }
			  catch ( StoreLockException e )
			  {
					throw new CommandFailed( "the database is in use -- stop Neo4j and try again", e );
			  }
			  catch ( IOException e )
			  {
					WrapIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void wrapIOException(java.io.IOException e) throws org.neo4j.commandline.admin.CommandFailed
		 public static void WrapIOException( IOException e )
		 {
			  throw new CommandFailed( format( "unable to load database: %s: %s", e.GetType().Name, e.Message ), e );
		 }

		 /// <returns> the version of Neo4j as defined during the build </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static String neo4jVersion()
		 public static string Neo4jVersion()
		 {
			  Properties props = new Properties();
			  try
			  {
					LoadProperties( props );
					return props.getProperty( "neo4jVersion" );
			  }
			  catch ( IOException e )
			  {
					// This should never happen
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void loadProperties(java.util.Properties props) throws java.io.IOException
		 private static void LoadProperties( Properties props )
		 {
			  using ( Stream resource = typeof( Util ).getResourceAsStream( "/org/neo4j/commandline/build.properties" ) )
			  {
					props.load( resource );
			  }
		 }
	}

}