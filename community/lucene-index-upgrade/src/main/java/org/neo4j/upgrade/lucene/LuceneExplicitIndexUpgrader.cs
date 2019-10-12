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
namespace Org.Neo4j.Upgrade.Lucene
{

	using JarLoaderSupplier = Org.Neo4j.Upgrade.Loader.JarLoaderSupplier;

	/// <summary>
	/// Lucene index upgrader that will try to migrate all indexes in specified index root directory.
	/// <para>
	/// Currently index migration has 2 steps:
	/// <ol>
	/// <li>Migration to format supported by Lucene 4</li>
	/// <li>Migration to format supported by Lucene 5</li>
	/// </ol>
	/// Migration performed by using native lucene's IndexUpgraders from corresponding versions. For details see Lucenes
	/// migration guide.
	/// </para>
	/// <para>
	/// In case if one of the indexes can not be migrated migration is terminated and corresponding exception is thrown.
	/// </para>
	/// </summary>
	public class LuceneExplicitIndexUpgrader
	{
		 public interface Monitor
		 {
			  /// <summary>
			  /// Upgrade is starting.
			  /// </summary>
			  /// <param name="count"> number of indexes to migrate. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void starting(int count)
	//		  {
	//		  }

			  /// <summary>
			  /// Called after an index has been migrated, called for each migrated index.
			  /// </summary>
			  /// <param name="name"> name of the index. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void migrated(String name)
	//		  {
	//		  }
		 }

		 public static readonly Monitor NO_MONITOR = new MonitorAnonymousInnerClass();

		 private class MonitorAnonymousInnerClass : Monitor
		 {
		 }

		 private const string LIBRARY_DIRECTORY = "lib";
		 private const string RESOURCE_SEPARATOR = "/";
		 private const string LUCENE4_CORE_JAR_NAME = "lucene-core-4.10.4.jar";
		 private const string LUCENE5_CORE_JAR_NAME = "lucene-core-5.5.0.jar";
		 private const string LUCENE5_BACKWARD_CODECS_NAME = "lucene-backward-codecs-5.5.0.jar";
		 private const string SEGMENTS_FILE_NAME_PREFIX = "segments";

		 private readonly Path _indexRootPath;
		 private readonly Monitor _monitor;

		 public LuceneExplicitIndexUpgrader( Path indexRootPath, Monitor monitor )
		 {
			  this._monitor = monitor;
			  if ( Files.exists( indexRootPath ) && !Files.isDirectory( indexRootPath ) )
			  {
					throw new System.ArgumentException( "Index path should be a directory" );
			  }
			  this._indexRootPath = indexRootPath;
		 }

		 /// <summary>
		 /// Perform index migrations </summary>
		 /// <exception cref="ExplicitIndexMigrationException"> in case of exception during index migration </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void upgradeIndexes() throws ExplicitIndexMigrationException
		 public virtual void UpgradeIndexes()
		 {
			  try
			  {
					if ( !Files.exists( _indexRootPath ) )
					{
						 return;
					}
					_monitor.starting( ( int ) Files.walk( _indexRootPath ).count() );
					using ( Stream<Path> pathStream = Files.walk( _indexRootPath ), IndexUpgraderWrapper lucene4Upgrader = CreateIndexUpgrader( Lucene4JarPaths ), IndexUpgraderWrapper lucene5Upgrader = CreateIndexUpgrader( Lucene5JarPaths ) )
					{
						 IList<Path> indexPaths = pathStream.filter( IndexPathFilter ).collect( Collectors.toList() );
						 foreach ( Path indexPath in indexPaths )
						 {
							  try
							  {
									lucene4Upgrader.UpgradeIndex( indexPath );
									lucene5Upgrader.UpgradeIndex( indexPath );
									_monitor.migrated( indexPath.toFile().Name );
							  }
							  catch ( Exception e )
							  {
									throw new ExplicitIndexMigrationException( indexPath.FileName.ToString(), "Migration of explicit index at path:" + indexPath + " failed.", e );
							  }
						 }
					}
			  }
			  catch ( ExplicitIndexMigrationException ime )
			  {
					throw ime;
			  }
			  catch ( Exception e )
			  {
					throw new Exception( "Lucene explicit indexes migration failed.", e );
			  }
		 }

		 internal virtual IndexUpgraderWrapper CreateIndexUpgrader( string[] jars )
		 {
			  return new IndexUpgraderWrapper( JarLoaderSupplier.of( jars ) );
		 }

		 private static string[] Lucene5JarPaths
		 {
			 get
			 {
				  return GetJarsFullPaths( LUCENE5_CORE_JAR_NAME, LUCENE5_BACKWARD_CODECS_NAME );
			 }
		 }

		 private static string[] Lucene4JarPaths
		 {
			 get
			 {
				  return GetJarsFullPaths( LUCENE4_CORE_JAR_NAME );
			 }
		 }

		 private static string[] GetJarsFullPaths( params string[] jars )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.of( jars ).map( LuceneExplicitIndexUpgrader.getJarPath ).toArray( string[]::new );
		 }

		 private static string GetJarPath( string library )
		 {
			  return LIBRARY_DIRECTORY + RESOURCE_SEPARATOR + library;
		 }

		 private static System.Predicate<Path> IndexPathFilter
		 {
			 get
			 {
				  return path =>
				  {
					try
					{
						 return Files.isDirectory( path ) && IsIndexDirectory( path );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
				  };
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static boolean isIndexDirectory(java.nio.file.Path path) throws java.io.IOException
		 private static bool IsIndexDirectory( Path path )
		 {
			  using ( Stream<Path> pathStream = Files.list( path ) )
			  {
					return pathStream.anyMatch( child => child.FileName.ToString().StartsWith(SEGMENTS_FILE_NAME_PREFIX, StringComparison.Ordinal) );
			  }
		 }

	}

}